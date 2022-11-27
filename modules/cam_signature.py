import os
import logging
import yaml

from modules.config import Config

from . import shared
from .scorecard import Scorecard

class CamSignature:
    def __init__(self):
        self.__cams = dict()

        self.load_cams()

        cams = self.cams()

        self.__known_extensions = list(
            shared.distinct(list(map(lambda y: list(filter(lambda x: x['type'] == 'file_extension',
                                                           cams[y]['hints']))[0]['value'],
                                     cams)))
        )

    def known_extensions(self) -> list[str]:
        return self.__known_extensions

    def load_cams(self) -> None:
        if len(self.__cams.keys()) == 0:
            cams_path = os.path.join(Config.project_root_path, 'config', 'cams.yml')

            with open(cams_path) as file:
                self.__cams = yaml.safe_load(file)

    def cams(self):
        return self.__cams['cameras']

    def new_scorecard(self) -> Scorecard:
        return Scorecard(list(self.__cams['cameras'].keys()))
