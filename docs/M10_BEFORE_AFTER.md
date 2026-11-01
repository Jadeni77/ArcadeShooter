# M10 Unity before and after

The **before** images were rendered in Unity 6000.6.5f1 from the pre-M10 `develop` commit `d03d873` (the M9 merge). The **after** images were rendered by the same Unity version from the integrated M10 project. Both are Edit-mode captures of actual Unity UI and scenes, not design mockups. The results use the same representative four-player values in both builders; they do not claim a live match.

| Screen | Before M10 | After M10 |
| --- | --- | --- |
| Main menu | [Pre-M10 menu](m10-before-after/main-menu-before-1600x900.png) | [Cinematic menu](m10-review/main-menu-1600x900.png) |
| Mission results | [Pre-M10 results](m10-before-after/results-before-1600x900.png) | [Cinematic report](m10-stage-e-review/results-1600x900.png) |

The menu changed from a centered host/join form to a full-screen operation view with separate multiplayer, profile, and settings pages. The report adds the mission backdrop, stronger outcome and rank hierarchy, XP progress, and restrained count-up motion while preserving the authoritative score and squad table. The temporary baseline capture helper is saved alongside the images so the exact earlier revision can be rendered again; it is documentation, not a shipped Unity script.

These comparisons establish the visual change. Functional multiplayer, focus/cursor, and frame-time checks remain listed in [Stage F review](M10_STAGE_F_REVIEW.md).
