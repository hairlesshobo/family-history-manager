import logging
import os
import yaml

from typing import Any, Dict

from modules.config import Config


def distinct(sequence):
    seen = set()
    for s in sequence:
        if s not in seen:
            seen.add(s)
            yield s


def read_raw_cams() -> Dict[str, Any]:
    """Read the contents of cams_raw.yaml and return as dict"""

    raw_path = os.path.join(Config.project_root_path, 'yaml', 'cams_raw.yaml')

    with open(raw_path) as file:
        try:
            cams_raw = yaml.safe_load(file)
            return cams_raw
        except yaml.YAMLError as exc:
            logging.error(exc)
            return {}

