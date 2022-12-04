import click
import json
import os
import pathlib
import subprocess

from modules import shared, static
from modules.cam_signature import CamSignature
from modules.config import Config
from rich import print


# TODO: Add flag to force re-scan of known cam dirs
@click.command('organize-raw')
# @click.argument('root_dir', required=True, type=str)
# def identify(root_dir) -> None:
def command() -> None:
    """Run a scan to identify camera using mediainfo metadata"""

    rescan_known_cam_dirs = True
    stats: dict[str, int] = {
        'located_at_invalid_level': 0,
        'in_proper_directory': 0,
        'in_proper_directory_trusted': 0,
        'in_cam_dir_not_correct_cam': 0,
        'not_in_dir_movable': 0,
        'not_in_dir_not_movable': 0,
        'in_unknown_cam_dir_movable': 0,
        'in_unknown_cam_dir_not_movable': 0
    }

    csig = CamSignature()

    # depths
    # 0 = "Cross" folder
    # 1 = year folder, should be 4 digits and less or equal to current year
    # 2 = scene folder, should be named "^(\d{4}-\d{2}-\d{2}) -- (\w+)$"
    # 3 = camera folder
    # 4 or higher = invalid

    for (file, depth) in shared.scantree(Config.directories.raw_footage_root):
        if file.is_file():
            path = pathlib.Path(file.path)

            extension = path.suffix[1:]
            file_basename = path.name[:-len(extension) - 1]

            # skip any extensions that are not defined by a camera profile
            if extension not in Config.known_extensions:
                continue

            # skip any paths that are defined in our ignore_paths directive
            if any(file.path.startswith(x) for x in Config.directories.ignore_paths):
                continue

            # if a list of include paths is configued, we require the path to be in the list
            # else we skip the current path
            if len(Config.directories.include_paths) > 0:
                if not any(file.path.startswith(x) for x in Config.directories.include_paths):
                    continue

            file_path_friendly = f"{depth}_raw:" + file.path.removeprefix(Config.directories.raw_footage_root)
            print('')
            print(f'>> [magenta]File[/magenta]        : [dark_blue]{file_path_friendly}[/dark_blue]')

            # now we build some logic to to determine if the video is organized properly
            # into a known camera folder. camera folders currently live at a depth of 3
            in_cam_directory = depth == 3
            dir_is_known_cam = False
            cam_dir_name = ''

            # if we are deemed to be in a cam folder, lets check to see if it is a known camera
            if in_cam_directory:
                cam_dir_name = path.parent.name
                dir_is_known_cam = cam_dir_name in Config.cam_name_mappings.values()

            print(f'   [magenta]Folder Stats[/magenta]: in cam folder: {in_cam_directory} / '
                  f'cam dir name: \'{cam_dir_name}\' / known_cam: {dir_is_known_cam}')

            if dir_is_known_cam and not rescan_known_cam_dirs:
                print_ident_result('Known camera directory, nothing to do', 'green')
                continue

            # Read metadata using `mediainfo` tool
            proc = subprocess.run(['mediainfo', '--Output=JSON', file.path], stdout=subprocess.PIPE)

            minfo = json.loads(proc.stdout)
            scorecard = csig.new_scorecard()

            tracks = minfo['media']['track']

            for cam_id in Config.cam_profiles:
                cam = Config.cam_profiles[cam_id]

                # TODO: use provided (de)commission dates as binary qualifiers

                scorecard.process_section_hints(cam_id, 'General', tracks, cam.hints)
                scorecard.process_section_hints(cam_id, 'Video', tracks, cam.hints)
                scorecard.process_section_hints(cam_id, 'Audio', tracks, cam.hints)
                scorecard.process_file_extension(cam_id, extension, cam.hints)
                scorecard.process_file_pattern(cam_id, file_basename, cam.hints)

            top_scores = scorecard.get_top_scores(2)
            confidence = scorecard.calc_confidence()
            confidence_pass = confidence > static.required_confidence

            c1 = top_scores[0]
            c1_score = round(100 * (c1[1] / static.max_score), 0)
            m1 = f'{c1[0]} / C: {round(confidence, 1)}% / S: {c1_score}%'
            m2 = ''

            if len(top_scores) > 1:
                c2 = top_scores[1]
                c2_score = round(100 * (c2[1] / static.max_score), 0)
                m2 = f'[dim][2nd: {c2[0]} / S: {c2_score}%][/]'

            print(f'   [magenta]Cam Profile[/magenta] : {m1} {m2}   Pass: {confidence_pass}')

            identified_cam_name = Config.cam_name_mappings[top_scores[0][0]]


            # Analyze directory structure and take action as needed to organize raw media
            if depth not in [2, 3]:
                stats['located_at_invalid_level'] += 1
                print_ident_result(f'File exists at unknown depth \'{depth}\'', 'red')
                continue

            # The file was located in a camera directory, which may or may not be known
            if in_cam_directory:

                # the cam directory is a known camera profile
                if dir_is_known_cam:

                    # The scanner confidently identified the camera of the file
                    if confidence_pass:

                        # The identified camera matches the name of the dir, all done here!
                        if identified_cam_name == cam_dir_name:
                            stats['in_proper_directory'] += 1
                            print_ident_result('File in proper directory, nothing to do', 'green')

                        # TODO: Do we want to auto rename if the dir is named wrong but we confidently know the cam profile?
                        # The identified camera does NOT match the name of the dir, that means we have a mis-named directory
                        # that needs to be corrected manually by the user
                        else:
                            stats['in_cam_dir_not_correct_cam'] += 1
                            print_ident_result('File in known cam dir, but cam not same as identified', 'red')

                    # The file is in a known camera dir but our scanner is not confident enough to confirm it. This means
                    # the file was organized manually by the user and the resuls must be trusted
                    else:
                        stats['in_proper_directory_trusted'] += 1
                        print_ident_result('File in known cam directory but no confidence match, assume correct and nothing to do.', 'blue')

                # the cam directory is NOT a known camera profile
                else:

                    # The camera scanner confidence is high enough to rename the directory automatically
                    if confidence_pass:
                        stats['in_unknown_cam_dir_movable'] += 1
                        # print_ident_result('File in unknown cam directory.. dir can be renamed automatically', 'yellow')
                        new_dir_name = identified_cam_name
                        new_full_path = os.path.join(path.parent.parent.resolve(), new_dir_name)

                        # rename the parent (directory) to the new name
                        # TODO: Enable renaming once the directory scanning logic has been integrated directly into the identify function
                        # path.parent.rename(new_full_path)
                        print_ident_result(f'File in unknown cam directory.. dir renamed to \'{new_dir_name}\'', 'yellow')

                    # the camera scanner confidence is NOT high enough to take any automatic action. manual user intervention required
                    else:
                        stats['in_unknown_cam_dir_not_movable'] += 1
                        print_ident_result('File in unknown cam directory.. dir cannot be renamed automatically', 'red')

            # Discovered file not located in an appropriately named camera directory.. action needs to be taken
            else:

                # We are confident of our camera identification, move the file to an appropriately named folder
                if confidence_pass:
                    stats['not_in_dir_movable'] += 1
                    print_ident_result('File not in cam directory, can automatically move to cam dir', 'yellow')

                # Our camera confidence is not high enough to automatically move the file, user must move manually
                else:
                    stats['not_in_dir_not_movable'] += 1
                    print_ident_result('File not in cam directory, and must be moved to cam dir manually', 'red')

            # TODO: create list with any manual action that must be taken to show at end of process
            # break


    print("Summary:")
    print("----------------------------------------------------")
    print(f"      located_at_invalid_level: {stats['located_at_invalid_level']}")
    print(f"           in_proper_directory: {stats['in_proper_directory']}")
    print(f"   in_proper_directory_trusted: {stats['in_proper_directory_trusted']}")
    print(f"    in_cam_dir_not_correct_cam: {stats['in_cam_dir_not_correct_cam']}")
    print(f"    in_unknown_cam_dir_movable: {stats['in_unknown_cam_dir_movable']}")
    print(f"in_unknown_cam_dir_not_movable: {stats['in_unknown_cam_dir_not_movable']}")
    print(f"            not_in_dir_movable: {stats['not_in_dir_movable']}")
    print(f"        not_in_dir_not_movable: {stats['not_in_dir_not_movable']}")


def print_ident_result(message: str, color: str = 'green') -> None:
    print(f'   [bright_yellow]Result[/bright_yellow]      : [{color}]{message}[/{color}]')
