# M10 visual direction — cinematic revision for owner review

**Status:** Stage A design proposal. The owner rejected the first pass as web-like and supplied a collage of game UI references. This revision responds with original, environment-led concepts. Approval is still required before Unity UI implementation.

## Review the revision

| Surface | New review render | Design intent |
| --- | --- | --- |
| Main Menu | [Cinematic menu](mockups/main-menu-v2.png) | Facility scene carries the mood; compact operation navigation and an active mission dossier. |
| Multiplayer Lobby | [Four-operator staging bay](mockups/lobby-v2.png) | Character silhouettes lead; each operator has an attached equipment and ready lane. |
| Combat HUD | [First-person combat](mockups/combat-hud-v2.png) | Thin edge-aligned objective, squad, health, ammunition, and encounter cues leave the aiming area open. |
| Mission Results | [Extraction aftermath](mockups/mission-results-v2.png) | Mission rank and atmosphere lead; team statistics and XP remain legible secondary information. |
| Profile / Loadout | [Operator profile](mockups/profile-loadout-v2.png) | Identity and progression sit beside the squad staging scene. |
| Pause / Settings | [In-mission pause](mockups/pause-settings-v2.png) | The world remains visible, darkened beneath the controls. Online play continues. |

The [editable Figma file](https://www.figma.com/design/0stgFPzLkSBN7UbWtdlohf) has a **Stage A / Cinematic revision** page with the [menu](https://www.figma.com/design/0stgFPzLkSBN7UbWtdlohf?node-id=6-3) and [lobby](https://www.figma.com/design/0stgFPzLkSBN7UbWtdlohf?node-id=6-40) in editable text/vector layers over uploaded original concept art. The prior six Figma screens are retained on **Archive / first visual pass** for comparison. The Figma Starter call limit stopped further remote edits during this revision; the six new PNG renders and their [AppKit drawing source](render-concepts.swift) are the complete current review set. These are concept images, not Unity Game-view screenshots or game assets.

## Art direction

- **World first.** Keep the research facility and 2–4 player first-person cooperative premise visible. Use full-bleed environment or character imagery; UI sits on thin rails, local dark washes, and compact panels. The game never moves a player or camera along a rail.
- **Tactical structure.** Condensed uppercase display type, numbered operations, thin dividers, equipment marks, and precise state language replace rounded dashboard cards. Save large typography for mission title, rank, and countdown; routine data stays compact.
- **Color roles.** Amber calls out active operation, launch, danger, and rank. Cyan marks live technical information, focus, and progress. Green marks verified ready/completed states. Red is reserved for actual damage, failure, or destructive confirmation. Pair colors with words and shapes.
- **Depth and clarity.** Confine translucent black to text contrast areas. Avoid filling screens with equal-weight cards. The HUD protects the center sightline; large warning messages collapse to a compact wave tracker.
- **State fidelity.** Names, join code, readiness, score, XP, and counts in the renders are illustrative. Runtime values must come from real session, mission, weapon, and profile state.

The four original backdrops in [concept-art](concept-art/) were generated for this review from text prompts about the project's facility, squad, combat, and extraction. The user's collage was a quality and layout reference only. No screenshot, logo, character, or art from it was copied into the deliverables. The images are **not imported into Unity** and are not approved production art. Overlays were drawn locally with AppKit. They use macOS DIN Condensed and Avenir Next as concept typography; these system fonts are not included in the project. Obtain approval and log a distributable font license before importing a replacement into Unity. The game currently uses logged Liberation Sans SDF.

## Proposed visual tokens

| Token | Value | Use |
| --- | --- | --- |
| `night` | `#02080C` | Text wash and rails, typically at 65–96% opacity |
| `white` | `#EFF4F2` | Primary text and health segments |
| `soft` | `#BFCBC9` | Secondary text |
| `dim` | `#8B9A9C` | Nonessential inactive labels |
| `amber` | `#E3AD55` | Active operation, launch, danger, rank |
| `cyan` | `#7AD7DB` | Technical state, focus edge, progress |
| `green` | `#A8D797` | Ready, complete, unlocked |
| `red` | `#EB776C` | Critical health, failure, destructive action |

Reference compositions are 1600 × 900. The conceptual type ramp is 90–100 px for exceptional titles, 32–42 px for headings/actions, 19–23 px for primary data, and 13–17 px for secondary labels. Judge actual size and contrast in the Unity Game view at 1280 × 720, 1920 × 1080, 2560 × 1440, and a common 16:10 Mac size. Essential data should remain at least 14 effective pixels at 720p. Use a CanvasScaler reference of 1920 × 1080 with anchored safe areas; do not assume this concept scale transfers directly.

## Proposed behaviors

- **Menu:** Play / Multiplayer leads. Profile, Settings, and Quit remain explicit. Existing Host and Join controls remain functional inside Multiplayer; the active operation dossier is informational until mission selection exists.
- **Lobby:** Four positions correspond to real session members. Display names, loadout/cosmetic, ready state, and code copy need real data and operations. Host-only Start requires valid session and readiness conditions. Posed operators are illustrative, not existing 3D prefabs.
- **HUD:** Objective and team health stay left, encounter/wave state at the top, player health and ammo in lower corners. The center aim stays clear. All counts bind to replicated authority.
- **Results:** Frozen mission outcome, rank, score, and time lead. Personal XP is separate from team score. Host actions and client waiting state reflect the real role. Pictured numbers are placeholders.
- **Profile/loadout:** Show the existing assault rifle, M9 level/progression, and two finish choices. A 3D preview must use approved project models or properly licensed new assets.
- **Pause/settings:** Online simulation continues. Opening redirects the Input System to UI and releases aim; Resume restores game input. Leave asks for confirmation. Settings need persistence and real value binding.

## Implementation proposal after approval

Use existing uGUI/TextMesh Pro with theme data and reusable rail, action, status, progress, warning, and confirmation components. Actions need mouse/keyboard focus, pressed, disabled, loading, and error states. Motion targets: 120–180 ms focus, 180–240 ms panel transition, and 300–450 ms for a new encounter notice before it compacts. Reduced motion removes positional travel and count-up. UI time remains unscaled while the online mission runs.

Stage B: menu/lobby; C: HUD; D: profile/loadout; E: results/pause/settings; F: visual, multiplayer, input, and performance validation. Capture actual before/after Unity screenshots, compile, verify target resolutions, and test the lobby/HUD/results with two clients. Stage A changes no Unity scene, prefab, gameplay, networking, package, or production asset.
