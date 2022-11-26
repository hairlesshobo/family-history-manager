import os
import logging
import yaml

from . import shared

weights = {
    'ulow': 0.5,
    'low': 1,
    'medium': 2,
    'high': 3
}

class CamSignature:
    __cams = dict()
    __script_path = os.path.realpath(os.path.dirname(__file__))
    __known_extensions = list()

    def __init__(self):
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
            cams_path = os.path.join(
                self.__script_path, '../', 'config', 'cams.yml')

            with open(cams_path) as file:
                self.__cams = yaml.safe_load(file)

    def cams(self):
        return self.__cams['cameras']

    def new_scorecard(self) -> dict[str, float]:
        scorecard = dict()

        for cam_id in list(self.__cams['cameras'].keys()):
            scorecard[cam_id] = 0.0

        return scorecard

    def process_hints(self, cam_id: str, type: str, tracks, all_hints, scorecard: dict[str, float]) -> None:
        logging.info(f'[{cam_id}] {type} >> enter')

        # Audio hints
        track = list(filter(lambda x: x['@type'] == type, tracks))[0]
        hints = list(filter(lambda x: x['type'] == 'mediainfo' and x['section'] == type.lower(), all_hints))

        logging.info(f'[{cam_id}] {type} >> hints: {list(map(lambda x: x["key"], hints))}')

        for hint in hints:
            hint_key = hint['key']
            hint_value = str(hint['value'])

            track_entry_value = None

            is_extra = hint_key[:6] == "extra/"

            if is_extra:
                key_name = hint_key[6:]
                
                if 'extra' in track and key_name in track['extra']:
                    track_entry_value = str(track['extra'][key_name])
            else:
                if hint_key in track:
                    track_entry_value = str(track[hint_key])

            if track_entry_value != None:
                is_match = track_entry_value == hint_value
                
                logging.info(f'[{cam_id}] {type}/{hint_key}:  {track_entry_value} is {hint_value}?  {is_match}')

                if is_match:
                    scorecard[cam_id] += weights[hint['weight']]
                    # print(f'{hint_key} - MEOW YAY!')
