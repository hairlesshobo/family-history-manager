from __future__ import annotations

import os
import pathlib

from typing import Any, Callable, Dict

from modules.config import Config

def scan_raw_media(
    process_file: Callable[[str, pathlib.Path, int, Callable[[], None]], None]
) -> None:
    return scan_media(
        Config.directories.raw.root,
        process_file,
        Config.known_extensions,
        Config.directories.raw.include,
        Config.directories.raw.exclude
    )

def scan_media(
    root_path: str,
    process_file: Callable[[str, pathlib.Path, int, Callable[[], None]], None],
    extensions: list[str] = list(),
    include_paths: list[str] = list(),
    exclude_paths: list[str] = list(),
    depth: int = 0
) -> None:
    """This is meant to be a reusable function for walking media directory trees"""

    # if depth == 0:
    #     print(f'extensions: {extensions}')

    dir_paths: list[str] = list()
    file_paths: list[str] = list()

    for entry in os.scandir(root_path):
        if entry.is_dir(follow_symlinks=False):
            dir_paths.append(entry.path)
        else:
            file_paths.append(entry.path)

    dir_paths.sort()
    file_paths.sort()


    def rename_dir(new_dir_name: str) -> None:
        nonlocal file_paths, root_path

        path = pathlib.Path(root_path)

        new_root_path = os.path.join(path.parent.resolve(), new_dir_name)

        if os.path.exists(new_root_path):
            raise Exception(f'Destination path already exists: {new_root_path}')
            
        path.rename(new_root_path)

        for i in range(len(file_paths)):
            file_paths[i] = file_paths[i].replace(root_path, new_root_path)

        root_path = new_root_path



    for dir_path in dir_paths:
        # print(f'   DIR: {dir_path}')
        scan_media(dir_path, process_file, extensions, include_paths, exclude_paths, depth + 1)
    
    for file_path in file_paths:
        # if a list of allowed extensions was provided, we skip any files that do not in the approved list
        if len(extensions) > 0:
            path = pathlib.Path(file_path)

            extension = path.suffix[1:]
            # file_basename = path.name[:-len(extension) - 1]

            if extension not in extensions:
                # print(f'SKIP-E: {file_path}')
                continue

        # skip any paths that are defined in our exclude_paths directive
        if any(file_path.startswith(x) for x in exclude_paths):
            # print(f'  EXCL: {file_path}')
            continue

        # if a list of include paths is configued, we require the current path to be in the list
        # else we skip the current path
        if len(include_paths) > 0:
            if not any(file_path.startswith(x) for x in include_paths):
                # print(f'N-INCL: {file_path}')
                continue
        
        # print(f'  FILE: {file_path}')


        # if we made it this far, we execute the callback to process the file
        process_file(file_path, path, depth, rename_dir)
