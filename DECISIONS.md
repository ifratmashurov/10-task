# DECISIONS.md

## Week 11: Data and Saves

2026-11-17  Moved player and boss balance numbers (health, damage, blink time, shoot interval, hit flash) out of PlayerHealth.cs and Boss.cs into PlayerStats.asset and BossStats.asset. Current HP stays in the script, not in the asset, because the asset is shared.
2026-11-17  After moving HP to one PlayerStats asset, Level 2 got 3 lives instead of 1. Made a separate PlayerStats asset for that level and dragged it into the player's Stats field.
2026-11-17  Boss tried to load "Level4" after death and Unity printed a build settings error. The old scene name was saved inside the scene (Inspector value beats the code value). Replaced the public string with const NextScene = "WinScene" and checked the scene name in Build Profiles.
2026-11-18  Added volume to save.json (musicVol) and made Reset progress restore it. Load() clamps musicVol and removes level numbers outside 1-3, so a hand-edited file cannot break the game.

## Week 12: Sound Pass

2026-11-25  Asked the agent to add sounds to the game. Some sounds were silent because the AudioSource components were not set up: no clip was assigned and the output was not routed to the mixer group. I selected the Player object in Unity, checked its audio components, added the missing clips, routed them to the SFX group and tested in gameplay.
2026-11-25  Repeated sounds felt flat. Added variation by assigning different clips and changing playback settings (pitch) on the Player, then played the level to confirm the difference is audible.
