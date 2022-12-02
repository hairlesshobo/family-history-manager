from modules.config import Config

from .scorecard import Scorecard


class CamSignature:
    def __init__(self):
        pass

    def new_scorecard(self) -> Scorecard:
        # TODO: exclude camera profiles that have no hints
        # TODO: print a warning if a camera has hints but the total score does not equal the max score
        return Scorecard(list(Config.cam_profiles.keys()))
