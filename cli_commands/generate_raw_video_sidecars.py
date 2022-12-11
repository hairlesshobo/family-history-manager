import click

from operations.generate_raw_video_sidecars import GenerateRawVideoSidecars

@click.command("generate-raw-video-sidecars")
def command() -> None:
    generator = GenerateRawVideoSidecars()
    generator.process_source_tree()
