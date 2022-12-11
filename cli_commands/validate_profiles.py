import click

from modules import static
from classes.cam_signature import CamSignature
from modules.config import Config


@click.command('validate-profiles')
def command() -> None:
    """Validate all known camera profiles"""

    csig = CamSignature()
    cams = Config.cam_profiles

    print(f"{'Camera'.ljust(20)} {'Hints'.ljust(8)} {'Max Score'.ljust(9)} {'Deviation'.ljust(10)}")
    print(f"------------------------------------------------------------------------")
    for cam_id in cams:
        cam = cams[cam_id]

        total_score = 0
        deviation = 0 - static.max_score
        hint_count = 0

        if cam.hints:
            total_score = sum(static.hint_weights[x.weight] for x in cam.hints)
            deviation = total_score - static.max_score
            hint_count = len(cam.hints)

        if deviation > 0:
            deviation = '+' + str(deviation)
        elif deviation == 0.0:
            deviation = str(0)
        else:
            deviation = str(deviation)

        print(f"{cam_id.ljust(20)} {str(hint_count).ljust(8)} {str(total_score).ljust(9)} {deviation.ljust(10)}")
