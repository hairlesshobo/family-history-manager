import click
import logging
import os
import yaml

from modules import shared
from modules.config import Config

from typing import Dict


@click.command('generate-cams')
@click.option('--force', '-f', default=False, is_flag=True, help="If the output file exists, automatically overwrite")
def command(force) -> None:
    """Generate cams.yaml file from cams_raw.yaml"""

    do_continue = True

    cams_path = os.path.join(Config.project_root_path, 'yaml', 'cams.yaml')

    if os.path.exists(cams_path):
        if force:
            logging.warning('cams.yaml already exists, but overwriting due to --force parameter')
        else:
            do_continue = click.confirm('cams.yaml already exists, overwrite?')

    if not do_continue:
        logging.info('generate_cams() operation aborted')
        return

    logging.info('Reading cams_raw.yaml...')
    raw_cams = shared.read_raw_cams()
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
            logging.info(f'  {cam.ljust(45)}   =>   {cam_id.ljust(18)} / {file_basename.ljust(28)} / {file_ext}')

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

    print()
    print('Writing to cams.yaml')
    with open(cams_path, 'w') as file:
        yaml.dump(new_cameras_container, file)


def new_hint(section: str, key: str, value: str) -> Dict[str, str]:
    return {
        'type': 'mediainfo',
        'section': section.lower(),
        'key': key,
        'value': value,
        'weight': 'undetermined',
    }
