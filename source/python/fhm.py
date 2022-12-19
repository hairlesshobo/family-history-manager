#!/usr/bin/env python3

import click
import logging

from commands import identify_camera
# from modules.config import Config

logging.basicConfig(level=logging.DEBUG)


@click.group()
def cli():
    # Config.load_config()
    pass


cli.add_command(identify_camera.command)

# Main entrypoint
if __name__ == '__main__':
    cli()
