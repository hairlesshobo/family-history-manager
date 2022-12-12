
allowed_hint_types: list[str] = ['file_extension', 'file_pattern', 'mediainfo']
hint_weights = {
    'ulow': 0.5,
    'low': 1.0,
    'medium': 2.0,
    'high': 3.0
}
max_score = 20.0
required_confidence = 70.0