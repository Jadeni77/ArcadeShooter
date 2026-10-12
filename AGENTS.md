# ArcadeShooter agent instructions

This is a 3D online cooperative first-person shooter for 2–4 players. Players control their own movement and camera in explorable maps. It is not a rail shooter; never add automatic player travel or a camera that drives the player through a level. See [docs/GAME_VISION.md](docs/GAME_VISION.md) for the full design.

## Start every task

1. Read this file and [docs/STATUS.md](docs/STATUS.md).
2. Read [docs/GIT_WORKFLOW.md](docs/GIT_WORKFLOW.md) and the relevant parts of [docs/GAME_VISION.md](docs/GAME_VISION.md), [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), [docs/ROADMAP.md](docs/ROADMAP.md), and [docs/DECISIONS.md](docs/DECISIONS.md). Read [docs/ASSET_LICENSES.md](docs/ASSET_LICENSES.md) before adding assets.
3. Inspect the existing scripts, scenes, prefabs, packages, and `git status` before making changes. Preserve pre-existing work.

## Development rules

- Work in small, testable milestones. Prefer clear, modular C# and the simplest design that meets the current milestone.
- Keep Unity scenes, prefabs, assets, and their `.meta` files together. Use the Unity Editor or its APIs for serialized asset changes when practical. Do not reset or discard uncommitted changes without explicit direction.
- Use the Unity Input System, URP, and the installed project packages where appropriate. Treat multiplayer topology and package additions in `docs/ARCHITECTURE.md` as proposals until verified and recorded in `docs/DECISIONS.md`.
- Do not introduce paid services or paid assets without approval. Record every third-party asset and its license in `docs/ASSET_LICENSES.md` before using it in a deliverable.
- Ask before changing fundamental game design decisions, especially player-controlled movement, cooperative play, first-person perspective, and freely explorable maps.
- Verify compilation and relevant tests when possible. Say exactly what was tested and what could not be tested.
- Update `docs/STATUS.md` after each completed milestone. Update the vision, architecture, roadmap, and decisions when the corresponding design changes.
- Follow [docs/GIT_WORKFLOW.md](docs/GIT_WORKFLOW.md): work from `develop` on a `feature/*` or `fix/*` branch; use `hotfix/*` from `main` only for urgent production fixes. Route all repository changes through PRs. Do not commit or push directly to `main` or `develop` after the approved one-time branch bootstrap.
- Never force push `main` or `develop`. Do not merge a PR, change GitHub settings, or make a Git commit unless requested or explicitly approved. Respect configured checks and human review; report when GitHub cannot enforce them. The current private repository has no enforceable branch protection on its present plan, so the written PR policy remains mandatory.

The initial development and test target is macOS. Keep future Windows support in mind without adding platform complexity before it is needed.
