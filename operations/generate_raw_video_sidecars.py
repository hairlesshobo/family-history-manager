
import pathlib
from typing import Callable
from modules import media_scanning, shared
from modules.config import Config


class GenerateRawVideoSidecars:
    """This class contains the operation used to generate metadata for raw video source files"""
    def __init__(self):
        self.Simlate = False

    def process_source_tree(self) -> None:
        media_scanning.scan_raw_media(self.__handle_media_file)

    def __handle_media_file(self, file_path: str, path: pathlib.Path, depth: int, rename_dir: Callable) -> None:
        print(file_path)
        
    def process_file(self, file_path: str) -> None:
        pass