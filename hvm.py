#!/usr/bin/env python3

import click
import json
import logging
import os
import pprint
import subprocess
import yaml

from modules import cam_signature

from typing import Dict
from typing import Any

logging.basicConfig(level=logging.INFO)

__script_path = os.path.realpath(os.path.dirname(__file__))

def read_raw_cams() -> Dict[str, Any]:
    """Read the contents of cams_raw.yml and return as dict"""

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



@cli.command()
@click.argument('root_dir', required=True, type=str)
def identify(root_dir) -> None:
    """Run a scan to identify camera using mediainfo metadata"""

    csig = cam_signature.CamSignature()
    cams = csig.cams()
    known_extensions = csig.known_extensions()

    with os.scandir(root_dir) as file_it:
        for file in file_it:
            ext_idx = len(file.name) - file.name[::-1].index('.')
            extension = file.name[ext_idx:]

            if extension not in known_extensions:
                continue

            click.echo(f'Analyzing {file}...')
            full_path = os.path.join(root_dir, file)

            click.echo('  Reading mediainfo')
            proc = subprocess.run(['mediainfo', '--Output=JSON', full_path], 
                                    stdout=subprocess.PIPE)

            minfo = json.loads(proc.stdout)
            scorecard = csig.new_scorecard()

            tracks = minfo['media']['track']

            click.echo("  Generating camera scorecard")
            for cam_id in cams:
                cam = cams[cam_id]
                hints = cam['hints']


                csig.process_hints(cam_id, 'General', tracks, hints, scorecard)
                csig.process_hints(cam_id, 'Video', tracks, hints, scorecard)
                csig.process_hints(cam_id, 'Audio', tracks, hints, scorecard)
                    # print(hint_key)

                # break

            # pprint.pprint(gtrack)

            # print(pprint.pformat(gtrack))

            click.echo('Camera scores by ')

            for score_entry in sorted(scorecard.items(), key=lambda x: x[1], reverse=True):
                print(f'{score_entry[0].rjust(25)}: {score_entry[1]}')

            # print(full_path)

if __name__ == '__main__':
    cli()
