# Decisions log

13.11.2026 Fixed the Patroller bug: the enemy was flying back and forth instead of patrolling normally. To fix it, I removed the Box Collider 2D component and added it again. After that, the bug disappeared.

13.11.2026 Adjusted the Patroller settings: changed Patrol Speed from `2` to `1.7` and Attack Range from `2.3` to `2`.

13.11.2026 Set Obstacle Mask to the Wall layer so walls block the Patroller's line of sight and prevent it from seeing the player through walls.
13.11.2026 Tested Patroller's movement between patrol points and checked that the enemy moves correctly without getting stuck.

13.11.2026 Tested player detection and verified that the Patroller detects the player when they enter its detection range.

13.11.2026 Tested wall collisions and verified that walls block the enemy's movement and line of sight.

13.11.2026 Tested the Patroller's attack behaviour and verified that it stops chasing the player at the correct distance and starts preparing for an attack.
