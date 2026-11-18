# DECISIONS.md

## Week 11: Data and Saves

2026-11-17  Moved player and boss balance numbers (health, damage, blink time, shoot interval, hit flash) out of PlayerHealth.cs and Boss.cs into PlayerStats.asset and BossStats.asset. Current HP stays in the script, not in the asset, because the asset is shared.
2026-11-17  After moving HP to one PlayerStats asset, Level 2 got 3 lives instead of 1. Made a separate PlayerStats asset for that level and dragged it into the player's Stats field.
2026-11-17  Boss tried to load "Level4" after death and Unity printed a build settings error. The old scene name was saved inside the scene (Inspector value beats the code value). Replaced the public string with const NextScene = "WinScene" and checked the scene name in Build Profiles.
2026-11-18  Added volume to save.json (musicVol) and made Reset progress restore it. Load() clamps musicVol and removes level numbers outside 1-3, so a hand-edited file cannot break the game. на русском
