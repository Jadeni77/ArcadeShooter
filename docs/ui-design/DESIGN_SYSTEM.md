# M10 visual direction — review proposal

**Status:** Stage A proposal, awaiting owner visual review before Unity UI implementation. [Editable Figma concepts](https://www.figma.com/design/0stgFPzLkSBN7UbWtdlohf) cover [Main Menu](https://www.figma.com/design/0stgFPzLkSBN7UbWtdlohf?node-id=2-2), [Lobby](https://www.figma.com/design/0stgFPzLkSBN7UbWtdlohf?node-id=2-56), [Combat HUD](https://www.figma.com/design/0stgFPzLkSBN7UbWtdlohf?node-id=3-2), [Results](https://www.figma.com/design/0stgFPzLkSBN7UbWtdlohf?node-id=3-57), [Profile and Loadout](https://www.figma.com/design/0stgFPzLkSBN7UbWtdlohf?node-id=3-101), and [Pause/Settings](https://www.figma.com/design/0stgFPzLkSBN7UbWtdlohf?node-id=3-147). All Figma screens are editable text and vector layers; none are Unity implementation screenshots.

Static review renders: [menu](mockups/main-menu.png), [lobby](mockups/lobby.png), [combat HUD](mockups/combat-hud.png), [results](mockups/mission-results.png), [profile/loadout](mockups/profile-loadout.png), [pause/settings](mockups/pause-settings.png). The two `figma-stage-a*.js` files preserve the editable mockup construction source.

## Identity

**Direction:** research-facility tactical interface. Dark navy surfaces echo the approved abandoned facility, cyan indicates the primary action or live information, amber marks objectives and incoming threats, red marks damage/failure, and green marks completion or readiness. Restrained thin structural lines, strong uppercase labels, spacious cards, and a visible first-person sightline keep the game readable. The menu backdrop is project-original facility geometry in the concept; a production background should use approved scene art or similarly original geometry.

| Token | Value | Use |
| --- | --- | --- |
| `ink` | `#071018` | Full-screen background and dark button text |
| `deep` | `#0B1720` | Secondary background, preview wells |
| `panel` | `#10232D` | Primary cards and overlays |
| `panelRaised` | `#142B36` | List rows and secondary buttons |
| `line` | `#2A4A53` | Hairline dividers and focus-safe panel borders |
| `cyan` | `#57D9DB` | Primary action, selected state, information |
| `cyanDeep` | `#178F9C` | Accent surfaces and reserved highlights |
| `paper` | `#E8F5F4` | Main text |
| `muted` | `#9CB7BC` | Secondary text; avoid on bright surfaces |
| `amber` | `#F5BA61` | Warnings, objectives, rank emphasis |
| `red` | `#F27872` | Critical health, failure, destructive confirmation |
| `green` | `#78D8A8` | Completed, ready, unlocked, success |

The primary and muted text on `panel`, plus the dark text on the cyan action, exceed a 4.5:1 contrast target by palette calculation. During implementation, inspect actual Game-view output over bright and dark parts of the map. Use icon shape and words with status colors: `READY`, `NOT READY`, `LOCKED`, `CLEARED`, and `CRITICAL` must remain legible without color perception.

## Type, spacing and layout

- **Typography proposal:** Inter Black/Bold for title and numerals; Inter Regular/Medium/Semi Bold for body and controls. The game currently ships Liberation Sans through TextMesh Pro. Owner approval and license logging are required before a font is imported; the implementation can begin with existing Liberation Sans while type approval is pending.
- **Type ramp at the 1920 × 1080 reference:** display 72; screen heading 52; section heading 28; card title 22; body 18; control 16; caption 13; micro-label 12. In combat, key HP/ammo numbers are 32–40. Avoid shrinking essential text below 14 effective pixels at 1280 × 720.
- **Spacing scale:** 4, 8, 12, 16, 24, 32, 48, 64. Cards use 24–32 padding; 8px corner radius; 1px low-contrast edge. Primary controls target at least 52px height at the reference resolution.
- **Responsive plan:** uGUI `CanvasScaler` at 1920 × 1080, match width/height 0.5. Use edge/corner anchors and layout groups. At 720p, lobby mission and loadout cards may stack or compress while preserving 44px effective click targets. Reserve a safe margin of 32px at 1080p and 20px at 720p. Verify 16:9 at 1280 × 720, 1920 × 1080, 2560 × 1440 and 16:10 at 2560 × 1600 (or an available common Mac size).

## Reusable implementation pieces proposed for Stages B–E

| Piece | Variants and behavior |
| --- | --- |
| `UiTheme` ScriptableObject | Semantic colors, type sizes, spacing, border and motion durations; one source for menu/HUD/results. |
| Action button prefab | Primary, secondary, danger; default, hover, focused, pressed, disabled and busy. Labels stay explicit; busy blocks repeat action. |
| Panel/card prefab | Solid menu card, translucent HUD card, compact status row, preview well. |
| Status badge | Host, Ready, Not Ready, Connected, Loading, Locked, Cleared, Critical; color + text/shape. |
| Progress meter | Health, XP, extraction time, volume; labeled current/maximum values when meaningful. |
| Notification banner | Warning, objective update, area clear, error; prioritizes one event, then returns to compact HUD. |
| Modal dialog | Confirm Leave Match/Session; explicit consequence and keyboard focus. |
| Navigation focus | Mouse hover and selected keyboard state use a visible cyan outline/fill change; controller navigation may follow later. |

Transitions: 120–180 ms hover/focus, 180–240 ms panel fade/slide, 300–450 ms event notification, then compact state. Health and XP meters animate the display only, never the underlying value. Reduced motion disables positional travel and count-up while keeping readable state changes. Unscaled time allows menus and results to respond while game time continues.

## Screen behaviors represented in the mockups

- **Menu:** Multiplayer is the dominant path, with Profile, Settings and Quit present. The conceptual facility view can animate very slowly only if profiling supports it. The existing host/join form belongs inside the Multiplayer flow.
- **Lobby:** Four slots, join-code copy, host control, mission/difficulty and selected rifle finish form one briefing. Names and ready state in the drawing are proposed features; the current code has numbered players and no ready networking. Start remains host-only and must be disabled until the final rules are satisfied.
- **HUD:** Objectives and squad state hug upper edges. A short danger banner turns into a compact wave tracker. Health/ammo occupy lower corners. Aim space remains open. Extraction replaces zone information when relevant.
- **Results:** Mission outcome, rank and team score lead. The leaderboard and local XP/reward are distinct. Host replay/lobby actions and client waiting text must reflect the real session role.
- **Profile/loadout:** One existing assault rifle, its current stats and two M9 cosmetic choices. The drawing's rifle silhouette is only a design proxy; production preview must use the project model if feasible.
- **Pause/settings:** The online mission continues. Escape releases the cursor and gameplay action map while the menu owns focus. Leaving asks for confirmation; volume, sensitivity and display choices persist locally.

## Implementation sequence after approval

1. Build theme data and shared uGUI prefabs; apply them to Main Menu and Lobby first, preserving existing controller actions. Capture actual before/after Unity screenshots.
2. Integrate lobby membership, names/ready state, code copy, loading/error states and host-only gating with multiplayer validation.
3. Rework player/mission/encounter HUD presenters around existing events, then loadout/profile and finally results/pause/settings.
4. Verify gameplay input, two-client state, four target resolutions, runtime screenshot quality, and UI cost. Record exact measured results and unresolved gaps.

No Unity scene, prefab, gameplay logic, package, or third-party creative asset is changed in Stage A.
