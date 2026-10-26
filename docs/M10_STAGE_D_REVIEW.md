# M10 Stage D review — armory and local profile

Stage D builds on the cinematic MainMenu and lobby in Stage B. The [Stage B profile entry capture](m10-review/profile-entry-1600x900.png) shows the earlier profile presentation for comparison. Its scope is presentation of the existing profile and loadout; it does not change AR-01 firing, stats, player control, or networking authority.

## Screens

| Screen | 1600×900 | 2560×1600 |
| --- | --- | --- |
| Armory | [Screenshot](m10-stage-d-review/armory-1600x900.png) | [Screenshot](m10-stage-d-review/armory-2560x1600.png) |
| Operator profile | [Screenshot](m10-stage-d-review/profile-1600x900.png) | [Screenshot](m10-stage-d-review/profile-2560x1600.png) |

The profile shows live local callsign, rank, XP progress, mission record, and unlock state. The armory reads weapon stats from the existing `AutomaticRifle` component and preserves the existing finish, title, difficulty, and close controls. The selected finish chooses a matching rifle preview image.

The rifle reference uses only `Gun_Rifle.fbx`, base color, and normal texture from Quaternius's free Standard Sci-Fi Essentials Kit. It is a rendered menu preview, not a change to the in-game view model. Its license text and source hashes are under `Assets/Weapons/Art/QuaterniusSciFiEssentials/`; the source and approval are logged in [ASSET_LICENSES.md](ASSET_LICENSES.md).

## Verification

- Unity 6000.6.5f1 imported the rifle and saved the `MainMenu` scene without a new C# error.
- Unity rendered the four linked Edit-mode captures from the saved scene. These cover visual composition at 16:9 and 16:10.
- GUI Play mode previously showed the profile page updating from local saved progress. The finish-aware image switch and online lobby interactions still need a hands-on Play-mode pass.
- Final EditMode suite: 44 passed, 1 failed. The failure is the previously recorded `M8BAudioIntegrationTests.MixerContainsRoutedBusesWithCallableVolumeParameters` batch-mode mixer value mismatch (expected −12.04 dB, read 0 dB).

The static captures use representative profile text and armory selection. Relay host/join and two-client readiness were not verified in this isolated checkout because it has no Unity Services project link.
