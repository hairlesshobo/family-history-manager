#!/usr/bin/env python3

import click
import logging

from cli_commands import clean_raw, generate_cams, generate_raw_video_sidecars, organize_raw, validate_profiles
from modules.config import Config

logging.basicConfig(level=logging.ERROR)


@click.group()
def cli():
    Config.load_config()
    pass


cli.add_command(clean_raw.command)
cli.add_command(generate_cams.command)
cli.add_command(generate_raw_video_sidecars.command)
cli.add_command(organize_raw.command)
cli.add_command(validate_profiles.command)

# Main entrypoint
if __name__ == '__main__':
    cli()
