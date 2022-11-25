#!/usr/bin/env python3

import click
import os
import pprint
import yaml

from typing import Dict
from typing import Any

__script_path = os.path.realpath(os.path.dirname(__file__))

# cameras:
#   akaso_brave7_le:
#     name: Akaso Brave 7 LE
#     commission_date: '2021-05-16'
#     decommission_date: 
#     hints:
#       - type: file_extension
#         value: MOV
#         weight: medium
#       - type: file_pattern
#         value: '^\d{8}_\d{6}$'
#         weight: medium
#       - type: mediainfo
#         section: general
#         key: Format
#         value: 'MPEG-4'
#         weight: low


# cameras:
#   akaso_brave_7_le20210521_110655.MOV:
#     Audio:
#       Codec ID: sowt
#       Format: PCM
#     General:
#       Format: MPEG-4
#       Format profile: QuickTime
#     Video:
#       Codec ID: avc1
#       Codec configuration box: avcC
#       Color range: Limited
#       Format: AVC
#       Format level: 5.1
#       Format profile: High
#       Format settings, CABAC: true
#       Format settings, GOP: M=1, N=8
#       Format settings, Referenc: 2 frames
#       Matrix coefficients: BT.601
#       Muxing mode: Container profile=Main@5.1
#       Scan type: Progressive

def read_raw_cams() -> Dict[str, Any]:
    raw_path = os.path.join(__script_path, 'yaml', 'cams_raw.yml')

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

    cleaned_path = os.path.join(__script_path, 'yaml', 'cams_cleaned.yml')
    
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

    cams_path = os.path.join(__script_path, 'yaml', 'cams.yml')

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

if __name__ == '__main__':
    cli()
