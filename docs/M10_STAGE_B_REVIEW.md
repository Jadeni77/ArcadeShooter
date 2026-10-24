# M10 Stage B review — main menu and multiplayer lobby

The design direction was revised in [Stage A PR #14](https://github.com/Jadeni77/ArcadeShooter/pull/14) after the owner supplied AAA game UI references. This branch implements the cinematic menu and squad lobby in Unity's existing `MainMenu` scene. It leaves mission HUD, results, and pause work to later M10 stages.

## Implemented visual language

- Full-screen original facility and squad art, with dark legibility rails instead of a web-page card layout.
- Night charcoal, cyan status, amber primary actions and mission accents, green ready state, and explicit `READY`/`NOT READY` text.
- TextMesh Pro type hierarchy, four aligned squad lanes, common hover/pressed/disabled button colors, and a short unscaled page reveal.
- Existing uGUI is retained because the menu, session controller, and Unity scene already use it. A UI Toolkit migration would duplicate the live controls for this stage. The common palette and button construction live in `M10MenuLobbySetup`; later stages should extract shared runtime prefabs or a theme asset when more screens use them.

## Functionality wired

- Main menu: Play/Multiplayer, local profile, audio settings, and Quit.
- Multiplayer: existing Host and Join, sanitized saved operator name, error/status text, and Escape/back navigation.
- Lobby: live session code/copy, player names, host label, selected finish, mission/difficulty, ready status, host-only Start, Leave, and existing cosmetic/difficulty controls in the loadout overlay. The host can start from the menu only when every connected player is ready.
- Audio: Master, Music, and SFX mixer sliders persist in local PlayerPrefs and are loaded by `AudioMixSettings` in later scenes.

## Unity scene captures

These PNGs come directly from Unity rendering the edited scene in Edit mode. They show the menu composition and empty-session layout; runtime player data, button callbacks, transition timing, and Relay sync are not represented in the stills.

| Screen | 16:9 | 16:10 |
| --- | --- | --- |
| Main menu | [1600×900](m10-review/main-menu-1600x900.png), [1280×720](m10-review/main-menu-1280x720.png) | [2560×1600](m10-review/main-menu-2560x1600.png) |
| Empty lobby | [1600×900](m10-review/lobby-empty-1600x900.png), [1280×720](m10-review/lobby-empty-1280x720.png) | [2560×1600](m10-review/lobby-empty-2560x1600.png) |
| Entry pages | [Multiplayer](m10-review/multiplayer-entry-1600x900.png), [Settings](m10-review/settings-entry-1600x900.png), [Profile](m10-review/profile-entry-1600x900.png) | — |

The Art backdrops make the Edit-mode empty lobby look occupied; at runtime, open lanes are darkened and labeled `AWAITING OPERATOR`. The profile capture is blank where saved player data fills it at runtime.

## Verification and open checks

- Unity 6000.6.5f1 imported and compiled the changed scripts with zero new C# errors; batch Editor rendering with a graphics device produced the linked captures. `-nographics` produces blank captures under URP and is not a valid visual check.
- The full EditMode regression run passed 44/45 tests. The only failure is the pre-existing `M8BAudioIntegrationTests.MixerContainsRoutedBusesWithCallableVolumeParameters`, which reads Music volume as 0 dB instead of about −12.04 dB under batch mode; M9 had the same failure. No M10 test failed.
- Inspect the menu, multiplayer entry, settings, and lobby captures at 1280×720, 1600×900, and 2560×1600. 1920×1080 and 2560×1440 share the 16:9 CanvasScaler path but still need a live Game-view check.
- Run two macOS clients with this branch connected to the existing Unity cloud project. Verify Host → code copy → Join, readiness on both clients, Start disabled until all ready, difficulty/finish changes, disconnects, status/errors, Leave, and replay/return. This isolated worktree has no cloud project link; that run is outstanding.
- In Play mode, check keyboard focus, cursor lock and restore, display-name typing without gameplay input, slider persistence after scene load, and the 0.18-second page reveal. Review typography against the approved concept; this implementation uses bundled Liberation Sans until the owner approves an external font import.
- The screenshots and compile pass do not establish a frame-rate target. Measure UI impact during live play in Stage F.
