from datetime import datetime
from classes.hint import Hint


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
