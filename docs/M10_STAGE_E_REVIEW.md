# M10 Stage E review — results, pause, and settings

Stage E builds on the Stage B menu and Stage D armory. It uses the existing replicated mission score, extraction, and XP receipt data. It does not change mission authority, player travel, or network topology.

## Unity captures

The captures are rendered by Unity 6000.6.5f1 from the real runtime UI builders over the existing mission scene. Result values are representative review data; they are not a claimed multiplayer run. The review capture method is `ArcadeShooter/M10/Capture Stage E Review`.

| Screen | 1280×720 | 1600×900 | 2560×1600 |
| --- | --- | --- | --- |
| Mission results | [Screenshot](m10-stage-e-review/results-1280x720.png) | [Screenshot](m10-stage-e-review/results-1600x900.png) | [Screenshot](m10-stage-e-review/results-2560x1600.png) |
| In-match menu | [Screenshot](m10-stage-e-review/pause-1280x720.png) | [Screenshot](m10-stage-e-review/pause-1600x900.png) | — |
| In-match settings | — | [Screenshot](m10-stage-e-review/settings-1600x900.png) | — |
| Main-menu settings | — | [Screenshot](m10-stage-e-review/menu-settings-1600x900.png) | — |

The results screen now uses a scene-backed cinematic report with a strong outcome heading, animated team score, rank color, XP progress, team breakdown, and four-player statistics. Replay and Return to Lobby retain the existing host-only session actions. The local pause overlay offers Resume, Settings, and a confirmed Leave Match action. It disables only the local player's movement, look, and weapon input; it never changes `Time.timeScale` or pauses the online mission. The cursor unlocks for interaction and relocks on a living player's resume.

Master, Music, and SFX use the existing mixer and local persistence. Mouse sensitivity, display mode, and supported resolution choices are local preferences shared by the main menu and in-match settings. The existing extraction roster and countdown still read host-replicated state in `MissionHud`; Stage C's combat HUD is a separate branch awaiting integration.

## Verification and remaining checks

- Unity compiled the changed source and rendered every linked screenshot without a C# error. The 1280×720 and 2560×1600 results captures were inspected for clipping; the first settings render exposed overlapping slider fills, which were corrected and recaptured.
- The full EditMode suite passed 44 of 45 tests. The one failure is the previously documented M8B batch-mode `MixerContainsRoutedBusesWithCallableVolumeParameters` mismatch (Music volume reads 0 dB rather than −12.04 dB).
- The captures verify presentation and layout only. A live two-client run still needs to verify Escape/resume input, death while the menu is open, leave confirmation, results values and count-up timing, host-only buttons, XP receipt timing, settings persistence, display changes in a macOS Player build, and UI behavior at 1920×1080 and 2560×1440. The isolated worktree has no Unity Services project link, so Relay interaction was not claimed.
- Stage F still needs integrated Stage C HUD review, multiplayer focus/cursor testing, and a performance measurement. These gaps remain open for M10 acceptance.
