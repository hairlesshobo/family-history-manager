from __future__ import annotations
from . import static

from typing import Any

    # - type: file_extension
    #   value: mp4
    #   weight: medium
    # - type: file_pattern
    #   value: '^\d{8}_\d{6}$'
    #   weight: medium
    # - key: Format
    #   section: general
    #   type: mediainfo
    #   value: MPEG-4
    #   weight: low

class Hint:
    def __init__(self, in_hint: Any):
        self.type: str = in_hint.get('type', '')
        self.value: str = str(in_hint.get('value', ''))
        self.weight: str = in_hint.get('weight', 'low')
        self.section: str = in_hint.get('section', None)
        self.key: str = in_hint.get('key', None)

        if self.value == '':
            raise Exception(f'No value provided for hint {in_hint}')

        if self.type not in static.allowed_hint_types:
            raise Exception(f'{self.type} is not a valid hint type')

        if self.weight not in list(static.hint_weights.keys()):
            raise Exception(f'{self.weight} is not a valid weight for hint {in_hint}')

    def __str__(self) -> str:
        return str({
            'type': self.type,
            'value': self.value,
            'weight': self.weight,
            'section': self.section,
            'key': self.key,
        })
        

    @staticmethod
    def to_list(hints: list[Any]) -> list[Hint]:
        new_hints: list[Hint] = list()

        if hints:
            for hint in hints:
                new_hints.append(Hint(hint))

        return new_hints