import os


def distinct(sequence):
    seen = set()
    for s in sequence:
        if not s in seen:
            seen.add(s)
            yield s

def scantree(path):
    """Recursively yield DirEntry objects for given directory."""

    for entry in os.scandir(path):
        if entry.is_dir(follow_symlinks=False):
            yield from scantree(entry.path)  # see below for Python 2.x
        else:
            yield entry