import logging
import os
import yaml

from datetime import datetime
from typing import Any
from modules.cam_profile import CamProfile

from modules.hint import Hint


class DirConfig:
    video_root: str
    raw_footage_root: str
    web_footage_root: str
    final_footage_root: str
    ignore_paths: list[str]
    include_paths: list[str]


class Config:
    _loaded = False

    project_root_path: str = os.path.abspath(os.path.join(os.path.realpath(
        os.path.dirname(__file__)), '../'))

    config_file_path = os.path.join(project_root_path, 'config', 'config.yml')

    directories = DirConfig
    cam_profiles: dict[str, CamProfile]
    known_extensions: list[str]
    cam_ids: list[str]
    cam_name_mappings: dict[str, str]

    @staticmethod
    def load_config():
        if Config._loaded:
            logging.warning('Config already loaded, not reloading')
            return

        Config._loaded = True

        if not os.path.exists(Config.config_file_path):
            raise Exception('Config file does not exist!')

        with open(Config.config_file_path, 'r') as file:
            cyaml = yaml.safe_load(file)

        Config.directories.video_root = cyaml['directories']['video_root']
        Config.directories.raw_footage_root = cyaml['directories']['raw_footage_root']
        Config.directories.web_footage_root = cyaml['directories']['web_footage_root']
        Config.directories.final_footage_root = cyaml['directories']['final_footage_root']
        Config.directories.ignore_paths = list(cyaml['directories']['ignore_paths']) if 'ignore_paths' in cyaml['directories'] else list()
        Config.directories.include_paths = list(cyaml['directories']['include_paths']) if 'include_paths' in cyaml['directories'] else list()

        Config.cam_profiles = Config._load_cam_profiles()
        Config.known_extensions = Config._get_known_extensions()
        Config.cam_ids = list(Config.cam_profiles.keys())
        Config.cam_name_mappings = Config._load_cam_name_mappings()

    @staticmethod
    def _load_cam_name_mappings() -> dict[str, str]:
        mappings: dict[str, str] = dict()

        for cam_id in Config.cam_profiles:
            mappings[cam_id] = Config.cam_profiles[cam_id].name

        return mappings

    @staticmethod
    def _get_known_extensions() -> list[str]:
        extensions: list[str] = list()

        for cam_id in Config.cam_profiles:
            cam = Config.cam_profiles[cam_id]

            for hint in cam.hints:
                if hint.type == 'file_extension':
                    if hint.value not in extensions:
                        extensions.append(hint.value)

        return extensions

    @staticmethod
    def _load_cam_profiles() -> dict[str, CamProfile]:
        cams_path = os.path.join(Config.project_root_path,
                                 'config', 'cam_profiles')

        cams: dict[str, CamProfile] = dict()

        for entry in sorted(os.listdir(cams_path)):
            if entry.endswith('.yml'):
                with open(os.path.join(cams_path, entry), "r") as file:
                    cyaml = yaml.safe_load(file)

                    cam_profile = CamProfile(cyaml)
                    cams[cyaml['id']] = cam_profile

        return cams
