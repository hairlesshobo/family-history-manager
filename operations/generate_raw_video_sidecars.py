
from typing import Callable
from classes.media_file import MediaFile
from classes.raw_media_file_sidecar import RawMediaFileSidecar
from modules import media_scanning, shared

import yaml


class GenerateRawVideoSidecars:
    """This class contains the operation used to generate metadata for raw video source files"""
    def __init__(self):
        self.Simulate = False

    def process_source_tree(self) -> None:
        media_scanning.scan_raw_media(self.__handle_media_file)

    def __handle_media_file(self, media_file: MediaFile, depth: int, rename_dir: Callable[[str], None]) -> bool:
        sidecar_info = RawMediaFileSidecar()
        sidecar_info.media_info = media_scanning.read_media_info(media_file.path)

        print(yaml.dump(sidecar_info.dump()))

        return False
