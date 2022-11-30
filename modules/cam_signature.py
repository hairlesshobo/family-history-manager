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
        self.__get_known_extensions()

        # cams = self.cams()

        # self.__known_extensions = list(
        #     shared.distinct(list(map(lambda y: list(filter(lambda x: 'type' in x and x['type'] == 'file_extension',
        #                                                    cams[y]['hints']))[0]['value'],
        #                              cams)))
        # )

    def __get_known_extensions(self) -> None:
        self.__known_extensions = list()

        cams = self.cams()

        for cam_id in cams:
            cam = cams[cam_id]

            # make sure hints are defined and not empty
            if 'hints' in cam and cam['hints']:
                for hint in cam['hints']:
                    if hint['type'] == 'file_extension':
                        if hint['value'] not in self.__known_extensions:
                            self.__known_extensions.append(hint['value'])


    def known_extensions(self) -> list[str]:
        return self.__known_extensions


    def load_cams(self) -> None:
        if len(self.__cams.keys()) == 0:
            cams_path = os.path.join(Config.project_root_path, 'config', 'cam_profiles')

            for entry in sorted(os.listdir(cams_path)):
                if entry.endswith('.yml'):
                    with open(os.path.join(cams_path, entry), "r") as file:
                        cyaml = yaml.safe_load(file)
                        self.__cams[cyaml['id']] = cyaml

    def cams(self):
        return self.__cams


    def new_scorecard(self) -> Scorecard:
        # TODO: exclude camera profiles that have no hints
        # TODO: print a warning if a camera has hints but the total score does not equal the max score
        return Scorecard(list(self.__cams.keys()))
