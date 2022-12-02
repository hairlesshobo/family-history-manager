from modules.hint import Hint
from . import static

import logging
import re


class Scorecard:
    def __init__(self, cam_ids: list[str]):
        self._scores: dict[str, float] = dict()
        self._cam_ids = cam_ids

        for cam_id in cam_ids:
            self._scores[cam_id] = 0.0

    def get_cam_ids(self) -> list[str]:
        return self._cam_ids

    def get_scores(self) -> dict[str, float]:
        return self._scores

    
    def get_top_scores(self, count=0) -> list[tuple[str, float]]:
        if count <= 0:
            count = len(self.get_scores())

        if count > len(self.get_scores()):
            count = len(self.get_scores())

        return list(sorted(self.get_scores().items(), key=lambda x: x[1], reverse=True)[0:count])

    
    def calc_confidence(self) -> float:
        """Calculate the current camera confidence percentage"""

        sorted_scores = self.get_top_scores()

        confidence = 0.0
        
        # use the full calculation
        if len(sorted_scores) >= 2:
            s1 = sorted_scores[0][1]
            s2 = sorted_scores[1][1]

            sum_score = s1 + s2
            score_factor = sum_score / s1
            base_cnfd = 100 / score_factor

            cnfd_adj = 100 * (1 - (s2 / s1))

            confidence = base_cnfd + cnfd_adj

        # use the simplified calculation when there is only one camera in the scorecard
        elif len(sorted_scores) == 1:
            s1 = sorted_scores[0][1]

            confidence = (s1 / static.max_score) * 100.0


        return confidence


    def process_section_hints(self, cam_id: str, type: str, tracks, all_hints: list[Hint]) -> None:
        """Search for any hints for the given section and add to the camera score if any match"""

        logging.debug(f'[{cam_id}] {type} >> enter')

        if cam_id not in list(self._scores.keys()):
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
                    self._scores[cam_id] += static.hint_weights[hint.weight]
                    # print(f'{hint_key} - MEOW YAY!')
            else:
                logging.debug(f'[{cam_id}] {type}/{hint_key}:  NOT FOUND')

    
    def process_file_extension(self, cam_id: str, file_extension: str, hints: list[Hint]) -> None:
        """Search for the 'file_extension' hint and to the camera score if it matches"""

        logging.debug(f'[{cam_id}] process_file_extension >> enter')

        if cam_id not in list(self._scores.keys()):
            raise Exception(f'{cam_id} does not exist in scorecard')

        extension_hint = next(filter(lambda x: x.type == 'file_extension', hints), None)

        if extension_hint:
            if file_extension == extension_hint.value:
                self._scores[cam_id] += static.hint_weights[extension_hint.weight]


    def process_file_pattern(self, cam_id: str, file_basename: str, hints: list[Hint]) -> None:
        """Search for the "file_basename" hint and to the camera score if the pattern matches"""

        logging.debug(f'[{cam_id}] process_file_pattern >> enter')

        if cam_id not in list(self._scores.keys()):
            raise Exception(f'{cam_id} does not exist in scorecard')

        pattern_hint = next(filter(lambda x: x.type == 'file_pattern', hints), None)

        if pattern_hint:
            if re.search(pattern_hint.value, file_basename):
                self._scores[cam_id] += static.hint_weights[pattern_hint.weight]
            