import logging
import os
import pathlib

from os import DirEntry
from typing import Any, Callable, Dict, Generator

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


def scanmedia(
    root_path: str,
    process_file: Callable[[DirEntry, int, Callable[[], None]], None],
    extensions: list[str] = list(),
    include_paths: list[str] = list(),
    exclude_paths: list[str] = list(),
    depth: int = 0
) -> None:
    """This is meant to be a reusable function for walking media directory trees"""

    def dir_renamed() -> None:
        print('hey')

    for entry in os.scandir(root_path):
        if entry.is_dir(follow_symlinks=False):
            scanmedia(entry.path, process_file, extensions, include_paths, exclude_paths, depth + 1)
        else:
            # if a list of allowed extensions was provided, we skip any files that do not in the approved list
            if len(extensions) > 0:
                path = pathlib.Path(entry.path)

                extension = path.suffix[1:]
                # file_basename = path.name[:-len(extension) - 1]

                if extension not in extensions:
                    continue

            # skip any paths that are defined in our exclude_paths directive
            if any(entry.path.startswith(x) for x in exclude_paths):
                continue

            # if a list of include paths is configued, we require the current path to be in the list
            # else we skip the current path
            if len(include_paths) > 0:
                if not any(entry.path.startswith(x) for x in include_paths):
                    continue

            print(entry.path)
                
            # if we made it this far, we execute the callback to process the file
            process_file(entry, depth, dir_renamed)
