# Project status

Last reviewed: 2026-10-11 (America/New_York). Inspect `git status` and the project before each task.

## Current state

- M0 documentation was merged into `develop` by [bootstrap PR #1](https://github.com/Jadeni77/ArcadeShooter/pull/1). `main` remains at the historical initial scaffold commit.
- M1 first-person exploration is implemented on `feature/first-person-controller` for review through a PR to `develop`.
- `Assets/Scenes/FirstPersonTest.unity` is the enabled build scene. It contains three connected graybox rooms, collision walls, obstacles, a spawn marker, a player prefab instance, and directional lighting. `SampleScene.unity` is preserved and disabled in build settings.
- `Assets/Prefabs/FirstPersonPlayer.prefab` uses a `CharacterController`, the existing Input System actions asset, a first-person camera, and separate movement and look scripts. Inspector fields tune movement, sprint, acceleration, jump, gravity, sensitivity, and pitch limit.
- No networking, combat, enemy, UI, or paid asset/service was added. The scene uses Unity primitives and URP materials only.

## M1 verification

- Unity 6000.6.5f1 imported the new scripts and reported successful recompilation (`failed: false`, `compilationFailed: false`). The Editor console reported zero errors and zero warnings after testing.
- Play mode opened the saved scene with the player camera at the spawn. Simulated W moved the player from x = -10.0 through both passages to the east room; movement stopped at the outer wall at x = 14.42. Left Shift was held during the second traversal.
- A queued mouse delta changed player yaw from 90° to 102° and camera pitch from 0° to 6°. A large mouse delta stopped at the 85° pitch limit. Escape unlocked the cursor.
- A queued Space press raised the player from y = 0.98 to y = 2.06. A second press while airborne did not reset the upward velocity.
- The Unity Editor was left out of Play mode. Movement feel, all keyboard directions, and the full exploration loop still need a human play test in the Game view before merge.

## Human play test

Open `Assets/Scenes/FirstPersonTest.unity` and press Play. Click the Game view if it does not have focus. Move with WASD, hold Left Shift to sprint, press Space to jump, and move the mouse to look. Walk through both doorways into room 3, then backtrack; try walking into walls and obstacles and jumping again before landing. Press Escape to release the cursor, then click the Game view to capture it again.

## Existing Git change to preserve

`ProjectSettings/ProjectSettings.asset` still has an uncommitted cloud project, organization, and project-name edit predating M1. Its cloud association needs owner confirmation before it is committed. Keep it out of the M1 PR and do not discard it.

## Next milestone

Review and approve M1 after the human play test. Then begin M2, the two-player online foundation, only after separately validating package compatibility, service setup, ownership, and any costs. An eligible human reviewer is still needed for ordinary protected-branch PR approval.
