
import os
import pathlib


class MediaFile:
    def __init__(self):
        self.path: str
        self.path_info: pathlib.Path
        self.directory: str
        self.name: str
        self.extension: str
        self.base_name: str
        self.sidecar_files: list[str]

    def parse_file(self, file_path: str) -> None:
        self.path = file_path
        self.path_info = pathlib.Path(file_path)
        self.directory = self.path_info.parent.resolve()
        self.name = self.path_info.name
        self.extension = self.path_info.suffix[1:]
        self.base_name = self.name[:-len(self.extension) - 1]
