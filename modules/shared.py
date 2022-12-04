import logging
import os

from os import DirEntry
from typing import Any, Dict, Generator

import yaml

from modules.config import Config


def distinct(sequence):
    seen = set()
    for s in sequence:
        if s not in seen:
            seen.add(s)
            yield s


def read_raw_cams() -> Dict[str, Any]:
    """Read the contents of cams_raw.yml and return as dict"""

    raw_path = os.path.join(Config.project_root_path, 'yaml', 'cams_raw.yml')

    with open(raw_path) as file:
        try:
            cams_raw = yaml.safe_load(file)
            return cams_raw
        except yaml.YAMLError as exc:
            logging.error(exc)
            return {}

            
# TODO: move this logic to the identify command so that it can intelligently handle renaming of directories
# TODO: can i use a callback function to inject core functionality into this reusable scantree function?
def scantree(path, depth=0) -> Generator[tuple[DirEntry[str], int], None, None]:
    """Recursively yield DirEntry objects for given directory."""

    for entry in os.scandir(path):
        if entry.is_dir(follow_symlinks=False):
            yield from scantree(entry.path, depth + 1)
        else:
            if os.path.exists(entry.path):
                yield (entry, depth)
