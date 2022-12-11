from __future__ import annotations
from datetime import datetime, timedelta
from typing import Any, Dict

import yaml


"""
general: 
  file_name: Tape 1-2022_11_24-16_45_01.m2t
  container_format: MPEG-TS / HDV 1080i
  size: 395517220 # read from filesystem
  capture_dtm: 2022-11-24T16:45:01 # UTC
  duration: 00:02:01.48 # hh:mm:ss.ss
video:
  video_width: 1440
  video_height: 1080
  format: mpeg2
  format_name: HDV 1080i
  bitrate_mode: constant
  frame_rate: 29.970
  frame_count: 3628
  scan_type: interlaced
audio:
  format: mp2
  bitrate_mode: constant
  sampling_rate: 48k
  bitrate: 384k
  channels: 2
  bit_depth: 16 # can this be assumed if no value provided?
hash:
  md5: ee89f300725e6b2e0c0080f41bbb2ab2
  sha1: f5101da5ec1281ab583c5d82c704a3f9878e7b56
  fingerprint: # generate "quick" fingerprint
inferred:
  people:
    - Danielle Cross
    - Edward Sonnenthal
    - Jeff Yost
    - Gracie Yost
"""
class RawMediaFileSidecar:
    def __init__(self):
        self.media_info: Dict[str, Any] = dict()
        self.general: RawMediaFileSidecarGeneral = RawMediaFileSidecarGeneral()

    def dump(self) -> Dict[str, Any]:
        return {
            'general': self.general.dump(),
            # 'media_info': self.media_info
        }

class RawMediaFileSidecarGeneral:
    def __init__(self):
        self.file_name: str = None
        self.container_format: str = None
        self.size: int = 0
        self.capture_dtm: datetime = None
        self.duration: timedelta = None

    def dump(self) -> Dict[str, Any]:
        return {
            'file_name': self.file_name,
            'container_format': self.container_format,
            'size': self.size,
            'capture_dtm': self.capture_dtm.isoformat() if self.capture_dtm is not None else None,
            'duration': str(self.duration) if self.duration is not None else None
        }
