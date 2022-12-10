import click
import os
import yaml

from modules import shared
from modules.config import Config


@click.command('clean-raw')
def command() -> None:
    """Reads cams_raw.yaml file, clean and output to cams_cleaned.yaml"""

    cams_raw = shared.read_raw_cams()

    cleaned_path = os.path.join(Config.project_root_path, 'yaml', 'cams_cleaned.yaml')

    with open(cleaned_path, 'w') as file:
        yaml.dump(cams_raw, file)
