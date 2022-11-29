#!/usr/bin/env python3

import click
import json
import logging
import os
import pprint
import subprocess
import yaml
from modules import shared, static

from modules.cam_signature import CamSignature
from modules.config import Config
from modules.hint import Hint

from typing import Dict
from typing import Any

logging.basicConfig(level=logging.DEBUG)

Config.load_config()

print(Config.directories.video_root)

def read_raw_cams() -> Dict[str, Any]:
    """Read the contents of cams_raw.yml and return as dict"""

    raw_path = os.path.join(Config.project_root_path, 'yaml', 'cams_raw.yml')

    with open(raw_path) as file:
        try:
            cams_raw = yaml.safe_load(file)
            return cams_raw
        except yaml.YAMLError as exc:
            print(exc)
            return {}


@click.group()
def cli():
    pass

@cli.command()
def clean_raw() -> None:
    """Reads cams_raw.yml file, clean and output to cams_cleaned.yml"""

    cams_raw = read_raw_cams()

    cleaned_path = os.path.join(Config.project_root_path, 'yaml', 'cams_cleaned.yml')
    
    with open(cleaned_path, 'w') as file:
        yaml.dump(cams_raw, file)

def new_hint(section: str, key: str, value: str) -> Dict[str, str]:
    return {
        'type': 'mediainfo',
        'section': section.lower(),
        'key': key,
        'value': value,
        'weight': 'undetermined',
    }

@cli.command()
@click.option('--force', '-f', default=False, is_flag=True, help="If the output file exists, automatically overwrite")
def generate_cams(force) -> None:
    """Generate cams.yml file from cams_raw.yml"""

    do_continue = True

    cams_path = os.path.join(Config.project_root_path, 'yaml', 'cams.yml')

    if os.path.exists(cams_path):
        if force:
           click.echo('cams.yml already exists, but overwriting due to --force parameter') 
        else:
            do_continue = click.confirm('cams.yml already exists, overwrite?')

    if not do_continue:
        click.echo('generate_cams() operation aborted')
        return

    click.echo('Reading cams_raw.yml...')
    raw_cams = read_raw_cams()
    new_cameras_container = dict()
    new_cameras = new_cameras_container['cameras'] = dict()

    if 'cameras' in raw_cams:
        for cam in list(raw_cams['cameras'].keys()):
            cam_obj = raw_cams['cameras'][cam]

            new_camera = {
                'friendly_name': None,
                'commission_date': None,
                'decommission_date': None,
                'hints': list()
            }

            (cam_id, file_name) = cam.split('-', 1)
            (file_basename, file_ext) = file_name.split('.', 1)
            click.echo(f'  {cam.ljust(45)}   =>   {cam_id.ljust(18)} / {file_basename.ljust(28)} / {file_ext}')

            new_camera['hints'].append({
                'type': 'file_extension',
                'value': file_ext,
                'weight': 'medium'
            })

            new_camera['hints'].append({
                'type': 'file_pattern',
                'value': file_basename,
                'weight': 'medium'
            })

            def process_section(section: str) -> None:
                for ghint in cam_obj[section]:
                    new_camera['hints'].append(new_hint(section, ghint, cam_obj[section][ghint]))

            process_section('General')
            process_section('Video')
            process_section('Audio')

            new_cameras[cam_id] = new_camera

    click.echo()
    click.echo('Writing to cams.yml')
    with open(cams_path, 'w') as file:
        yaml.dump(new_cameras_container, file)


@cli.command()
def validate_profiles() -> None:
    """Validate all known camera profiles"""

    csig = CamSignature()
    cams = csig.cams()

    click.echo(f"{'Camera'.ljust(20)} {'Hints'.ljust(8)} {'Max Score'.ljust(9)} {'Deviation'.ljust(10)}")
    click.echo(f"------------------------------------------------------------------------");
    for cam_id in cams:
        cam = cams[cam_id]
        total_score = sum(static.hint_weights[x['weight']] for x in cam['hints'])
        deviation = total_score - static.max_score

        if deviation > 0:
            deviation = '+' + str(deviation)
        elif deviation == 0.0:
            deviation = str(0)
        else:
            deviation = str(deviation)

        click.echo(f"{cam_id.ljust(20)} {str(len(cam['hints'])).ljust(8)} {str(total_score).ljust(9)} {deviation.ljust(10)}")



@cli.command()
# @click.argument('root_dir', required=True, type=str)
# def identify(root_dir) -> None:
def identify() -> None:
    """Run a scan to identify camera using mediainfo metadata"""

    csig = CamSignature()
    cams = csig.cams()
    known_extensions = csig.known_extensions()

    for file in shared.scantree(Config.directories.raw_footage_root):
        if file.is_file():
            file_path_friendly = "raw:" + file.path.removeprefix(Config.directories.raw_footage_root)
            ext_idx = len(file.name) - file.name[::-1].index('.')
            extension = file.name[ext_idx:]
            file_basename = file.name[:ext_idx-1]

            # skip any extensions that are not defined by a camera profile
            if extension not in known_extensions:
                continue

            # skip any paths that are defined in our ignore_paths directive
            if any(file.path.startswith(x) for x in Config.directories.ignore_paths):
                continue

            # if a list of include paths is configued, we require the path to be in the list
            # else we skip the current path
            if len(Config.directories.include_paths) > 0:
                if not any(file.path.startswith(x) for x in Config.directories.include_paths):
                    continue

            click.echo(f'Analyzing {file_path_friendly}...')

            click.echo('  Reading mediainfo')
            proc = subprocess.run(['mediainfo', '--Output=JSON', file.path], 
                                    stdout=subprocess.PIPE)

            minfo = json.loads(proc.stdout)
            scorecard = csig.new_scorecard()

            tracks = minfo['media']['track']

            click.echo("  Generating camera scorecard")
            for cam_id in cams:
                cam = cams[cam_id]
                hints = Hint.to_list(cam['hints'])

                # TODO: use provided (de)commission dates as binary qualifiers

                scorecard.process_section_hints(cam_id, 'General', tracks, hints)
                scorecard.process_section_hints(cam_id, 'Video', tracks, hints)
                scorecard.process_section_hints(cam_id, 'Audio', tracks, hints)
                scorecard.process_file_extension(cam_id, extension, hints)
                scorecard.process_file_pattern(cam_id, file_basename, hints)


            click.echo('Camera score results')

            scorecard.calc_confidence()

            # show top 3 cameras
            total = 0
            max = 0
            for score_entry in sorted(scorecard.get_scores().items(), key=lambda x: x[1], reverse=True)[0:3]:
                total += score_entry[1]

                if score_entry[1] > max:
                    max = score_entry[1]

                click.echo(f'{score_entry[0].rjust(25)}: {score_entry[1]}')

            confidence = 100 / (total / max)

            click.echo()
            click.echo(f'Confidence: {round(confidence, 1)}%')

            # show all cameras
            # for score_entry in sorted(scorecard.get_scores().items(), key=lambda x: x[1], reverse=True):
            #     print(f'{score_entry[0].rjust(25)}: {score_entry[1]}')

            break


## Main entrypoint
if __name__ == '__main__':
    cli()
