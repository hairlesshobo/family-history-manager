import os

from os import DirEntry
from typing import Generator


def distinct(sequence):
    seen = set()
    for s in sequence:
        if not s in seen:
            seen.add(s)
            yield s

def scantree(path, depth=0) -> Generator[tuple[DirEntry[str], int], None, None]:
    """Recursively yield DirEntry objects for given directory."""

    for entry in os.scandir(path):
        if entry.is_dir(follow_symlinks=False):
            yield from scantree(entry.path, depth+1)
        else:
            yield (entry, depth)