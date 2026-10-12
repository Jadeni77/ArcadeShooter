# Project status

Last reviewed: 2026-10-11 (America/New_York). This is a snapshot; inspect `git status` and the project again before each task.

## Current state

- M0 documentation is complete. No gameplay has been implemented by this documentation task.
- Unity 6000.6.5f1; URP 17.6.0; Apple M4 Mac with 24 GB RAM is the initial development and test environment.
- The Unity starter project has one enabled scene, `Assets/Scenes/SampleScene.unity`, an Input System actions asset, and template/readme scripts. There is no first-person controller, gun, enemy, mission flow, or online gameplay.
- Input System and AI Navigation are installed. Netcode for GameObjects and Unity Multiplayer Services are proposed but not installed. `com.unity.pipeline@0.8.0-exp.1` supports Unity CLI Editor control.
- The existing Unity Editor log showed successful package resolution and script compilation. This task did not rerun the Editor, a build, or gameplay tests.
- Git workflow is documented in [GIT_WORKFLOW.md](GIT_WORKFLOW.md). `develop` has been published and set as GitHub's default branch; the bootstrap feature branch is active.

## Existing Git changes to preserve

These six changes predate M0; their content was preserved during the branch bootstrap:

1. Deleted `Assets/Editor.meta`.
2. Deleted `Assets/Editor/HubForceResolve.cs`.
3. Deleted `Assets/Editor/HubForceResolve.cs.meta`.
4. Modified `Packages/packages-lock.json` after Unity resolved the already-declared Pipeline package.
5. Modified `ProjectSettings/ProjectSettings.asset` with cloud project, organization, and project-name fields.
6. Modified `ProjectSettings/QualitySettings.asset` with an Editor serialization update.

The deleted script was a one-time package-resolve helper that deleted itself and its empty folder. It is not gameplay code. Feature-branch commit `80cbda0` records the helper deletion, lockfile update, and quality serialization update. The cloud association should be confirmed before `ProjectSettings/ProjectSettings.asset` is committed; that file remains in the working tree.

The active local branch is `feature/repository-bootstrap`. `main`, `develop`, `origin/main`, and `origin/develop` point to the initial commit (`0809580`); `develop` is the verified GitHub default. The project documents are recorded on the feature branch. The initial commit is a project scaffold, not a verified clean release baseline. Nothing has been committed directly to `main` or `develop` after branch creation.

## Known issues and open work

- No blocking package resolution or C# compilation error was found in the existing logs. UPM selected Newtonsoft JSON 3.2.2 over a dependency's requested 3.0.2 and completed resolution.
- Online service prerequisites, package compatibility, movement authority, host-exit behavior, and any service costs have not been validated for this game.
- Combat feel, movement feel, audio mix, performance, and two-player online operation are untested because gameplay does not exist yet.
- The published `develop` branch still contains the one-time helper until the bootstrap PR is reviewed and merged. The feature branch records its intentional removal and the regenerated lockfile.
- GitHub reports HTTP 403 for branch rulesets and classic protection on this private repository: it requires GitHub Pro or public visibility. Only the owner is listed as a collaborator, so a separate human approval is unavailable in the current roster. No CI workflow or required checks exist. Follow the documented PR rules manually; do not claim GitHub is enforcing them.
- Review the cloud project identifiers in the pending settings change before committing that file.

## Next milestone

**Finish the repository bootstrap PR without merging it**, then **M1 — first-person exploration.** See [GIT_WORKFLOW.md](GIT_WORKFLOW.md) for the branch and PR plan and [ROADMAP.md](ROADMAP.md) for the first gameplay acceptance criteria. Multiplayer and combat wait for their own milestones.
