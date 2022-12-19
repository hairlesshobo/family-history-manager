from __future__ import annotations

from datetime import datetime
from typing import Any

import click
import json
import logging
import os
import pathlib
import re


allowed_hint_types: list[str] = ['file_extension', 'file_pattern', 'mediainfo']
hint_weights = {
    'ulow': 0.5,
    'low': 1.0,
    'medium': 2.0,
    'high': 3.0
}
max_score = 20.0
required_confidence = 70.0


# TODO: Add flag to force re-scan of known cam dirs
@click.command('identify-camera')
@click.argument('profiles_dir', required=True, type=str)
@click.argument('media_file', required=True, type=str)
def command(profiles_dir: str, media_file: str) -> None:
    """Run a scan to identify camera using mediainfo metadata"""

    profiles_dir_path = os.path.abspath(profiles_dir)
    media_file_path = os.path.abspath(media_file)
    mediainfo_file_path = f"{media_file_path}.mediainfo"

    if not os.path.exists(media_file_path):
        raise Exception(f"Media file does not exist: {media_file_path}")

    if not os.path.exists(mediainfo_file_path):
        raise Exception(f"Mediainfo file does not exist: {mediainfo_file_path}")

    logging.debug(f"Using profiles dir: {profiles_dir_path}")
    logging.debug(F"Using media file path: {media_file}")

    cam_profiles = _load_cam_profiles(profiles_dir_path)
    cam_name_mappings = _load_cam_name_mappings(cam_profiles)

    path_info = pathlib.Path(media_file_path)
    name = path_info.name
    extension = path_info.suffix[1:]
    base_name = name[:-len(extension) - 1]

    # Read file mediainfo
    with open(mediainfo_file_path) as handle:
        minfo = json.load(handle)


    ## Generate a blank scorecard
    scorecard = Scorecard(list(cam_profiles.keys()))

    tracks = minfo['media']['track']

    for cam_id in cam_profiles:
        cam = cam_profiles[cam_id]

        # TODO: use provided (de)commission dates as binary qualifiers
        
        scorecard.process_section_hints(cam_id, 'General', tracks, cam.hints)
        scorecard.process_section_hints(cam_id, 'Video', tracks, cam.hints)
        scorecard.process_section_hints(cam_id, 'Audio', tracks, cam.hints)
        scorecard.process_file_extension(cam_id, extension, cam.hints)
        scorecard.process_file_pattern(cam_id, base_name, cam.hints)

    scores = scorecard.get_top_scores(0)
    confidence = scorecard.calc_confidence()
    confidence_pass = confidence > required_confidence

    c1 = scores[0]
    c1_score = round(100 * (c1[1] / max_score), 0)
    m1 = f'{c1[0]} / C: {round(confidence, 1)}% / S: {c1_score}%'
    m2 = ''

    if len(scores) > 1:
        c2 = scores[1]
        c2_score = round(100 * (c2[1] / max_score), 0)
        m2 = f'[dim][2nd: {c2[0]} / S: {c2_score}%][/]'

    identified_cam_name = cam_name_mappings[scores[0][0]]

    result = {
        'file_path': media_file_path,
        'mediainfo_path': mediainfo_file_path,
        'identified_cam_name': identified_cam_name,
        'confidence': confidence,
        'confidence_pass': confidence_pass,
        'scores': dict(scores)
    }

    print(json.dumps(result, indent=True))


def _load_cam_profiles(profile_dir_path: str) -> dict[str, CamProfile]:

    cams: dict[str, CamProfile] = dict()

    for entry in sorted(os.listdir(profile_dir_path)):
        # TODO: convert to reading from json
        if entry.endswith('.json'):
            with open(os.path.join(profile_dir_path, entry), "r") as file:
                cjson = json.load(file)

                cam_profile = CamProfile(cjson)
                cams[cjson['id']] = cam_profile

    return cams


def _load_cam_name_mappings(profiles: dict[str, CamProfile]) -> dict[str, str]:
    mappings: dict[str, str] = dict()

    for cam_id in profiles:
        mappings[cam_id] = profiles[cam_id].name

    return mappings


class Scorecard:
    def __init__(self, cam_ids: list[str]):
        self._scores: dict[str, float] = dict()
        self._cam_ids = cam_ids

        for cam_id in cam_ids:
            self._scores[cam_id] = 0.0

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

            confidence = (s1 / max_score) * 100.0

        if confidence > 100.0:
            confidence = 100


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

            if track_entry_value is not None:
                # TODO: handle startswith, endswith, contains, or similar
                is_match = track_entry_value == hint_value
                
                logging.debug(f'[{cam_id}] {type}/{hint_key}:  {track_entry_value} is {hint_value}?  {is_match}')

                if is_match:
                    self._scores[cam_id] += hint_weights[hint.weight]
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
                self._scores[cam_id] += hint_weights[extension_hint.weight]


    def process_file_pattern(self, cam_id: str, file_basename: str, hints: list[Hint]) -> None:
        """Search for the "file_basename" hint and to the camera score if the pattern matches"""

        logging.debug(f'[{cam_id}] process_file_pattern >> enter')

        if cam_id not in list(self._scores.keys()):
            raise Exception(f'{cam_id} does not exist in scorecard')

        pattern_hint = next(filter(lambda x: x.type == 'file_pattern', hints), None)

        if pattern_hint:
            if re.search(pattern_hint.value, file_basename):
                self._scores[cam_id] += hint_weights[pattern_hint.weight]


class CamProfile:
    def __init__(self, input: dict):
        self.id: str = input['id']
        self.commission_date: datetime = input['commission_date']
        self.decommission_date: datetime = input['decommission_date']
        self.name: str = input['name']
        self.description: str = input.get('description', ' ')
        self.hints: list[Hint] = []

        in_hints = input.get('hints')

        if in_hints:
            self.hints = list(map(lambda x: Hint(x), in_hints))

        # TODO: Add __repr__ method


class Hint:
    """Class used to define a hint, which is used to help score a video file"""
    
    def __init__(self, in_hint: Any):
        self.type: str = in_hint.get('type', '')
        self.value: str = str(in_hint.get('value', ''))
        self.weight: str = in_hint.get('weight', 'low')
        self.section: str = in_hint.get('section', None)
        self.key: str = in_hint.get('key', None)

        if self.value == '':
            raise Exception(f'No value provided for hint {in_hint}')

        if self.type not in allowed_hint_types:
            raise Exception(f'{self.type} is not a valid hint type')

        if self.weight not in list(hint_weights.keys()):
            raise Exception(f'{self.weight} is not a valid weight for hint {in_hint}')

    def __repr__(self) -> str:
        return 'Hint(' + str({
            'type': self.type,
            'value': self.value,
            'weight': self.weight,
            'section': self.section,
            'key': self.key,
        }) + ')'
        