import logging
import os
import yaml


class DirConfig:
    video_root: str
    raw_footage_root: str
    web_footage_root: str
    final_footage_root: str
    ignore_paths: list[str]
    include_paths: list[str]

class Config:
    __loaded = False

    project_root_path: str = os.path.abspath(
            os.path.join(os.path.realpath(
                os.path.dirname(__file__)), '../'))
    
    config_file_path = os.path.join(project_root_path, 'config', 'config.yml')

    directories = DirConfig

    @staticmethod
    def load_config():
        if Config.__loaded:
            logging.warning('Config already loaded, not reloading')
            return
        
        Config.__loaded = True

        if not os.path.exists(Config.config_file_path):
            raise Exception('Config file does not exist!')
        
        with open(Config.config_file_path, 'r') as file:
            cyaml = yaml.safe_load(file)

        Config.directories.video_root = cyaml['directories']['video_root']
        Config.directories.raw_footage_root = cyaml['directories']['raw_footage_root']
        Config.directories.web_footage_root = cyaml['directories']['web_footage_root']
        Config.directories.final_footage_root = cyaml['directories']['final_footage_root']
        Config.directories.ignore_paths = list(cyaml['directories']['ignore_paths'])
        Config.directories.include_paths = list(cyaml['directories']['include_paths'])
        
