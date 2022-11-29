from modules.hint import Hint
from . import static

import logging
import re


class Scorecard:
    def __init__(self, cam_ids: list[str]):
        # TODO: create class for storing cam scores that includes cam_id, score, and confidence
        self.__scores: dict[str, float] = dict()

        for cam_id in cam_ids:
            self.__scores[cam_id] = 0.0


    def get_scores(self) -> dict[str, float]:
        return self.__scores

    
    def calc_confidence(self) -> None:
        """Calculate the current camera confidence levels"""

        # add the top three scores together
        # ex: 20 + 12 + 2 = 34

        # divice by the current camera score
        # ex: 34 / 20 = 1.7

        # divice 100 by the above factor
        # ex 100/1.7 = 58.8% confidence


    def process_section_hints(self, cam_id: str, type: str, tracks, all_hints: list[Hint]) -> None:
        """Search for any hints for the given section and add to the camera score if any match"""

        logging.debug(f'[{cam_id}] {type} >> enter')

        if cam_id not in list(self.__scores.keys()):
            raise Exception(f'{cam_id} does not exist in scorecard')

        # Audio hints
        track = list(filter(lambda x: x['@type'] == type, tracks))[0]
        hints = list(filter(lambda x: x.type == 'mediainfo' and x.section == type.lower(), all_hints))

        logging.debug(f'[{cam_id}] {type} >> hints: {list(map(lambda x: x.key, hints))}')

        for hint in hints:
            hint_key = hint.key
            hint_value = hint.value

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
                # TODO: handle startswith, endswith, contains, or similar
                is_match = track_entry_value == hint_value
                
                logging.debug(f'[{cam_id}] {type}/{hint_key}:  {track_entry_value} is {hint_value}?  {is_match}')

                if is_match:
                    self.__scores[cam_id] += static.hint_weights[hint.weight]
                    # print(f'{hint_key} - MEOW YAY!')
            else:
                logging.debug(f'[{cam_id}] {type}/{hint_key}:  NOT FOUND')

    
    def process_file_extension(self, cam_id: str, file_extension: str, hints: list[Hint]) -> None:
        """Search for the 'file_extension' hint and to the camera score if it matches"""

        logging.debug(f'[{cam_id}] process_file_extension >> enter')

        if cam_id not in list(self.__scores.keys()):
            raise Exception(f'{cam_id} does not exist in scorecard')

        extension_hint = next(filter(lambda x: x.type == 'file_extension', hints), None)

        if extension_hint:
            if file_extension == extension_hint.value:
                self.__scores[cam_id] += static.hint_weights[extension_hint.weight]


    def process_file_pattern(self, cam_id: str, file_basename: str, hints: list[Hint]) -> None:
        """Search for the "file_basename" hint and to the camera score if the pattern matches"""

        logging.debug(f'[{cam_id}] process_file_pattern >> enter')

        if cam_id not in list(self.__scores.keys()):
            raise Exception(f'{cam_id} does not exist in scorecard')

        pattern_hint = next(filter(lambda x: x.type == 'file_pattern', hints), None)

        if pattern_hint:
            if re.search(pattern_hint.value, file_basename):
                self.__scores[cam_id] += static.hint_weights[pattern_hint.weight]
            