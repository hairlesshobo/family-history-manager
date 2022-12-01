from datetime import datetime
from modules.hint import Hint


class CamProfile:
    def __init__(self, input: dict):
        self.id: str = input['id']
        self.commission_date: datetime = input['commission_date']
        self.decommission_date: datetime = input['decommission_date']
        self.name: str = input['name']
        self.description: str = input.get('description', ' ')
        self.hints: list[Hint]
