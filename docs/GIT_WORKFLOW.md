# Git branching and pull-request workflow

This is the project owner's required workflow. `develop` has been created and made the GitHub default branch. The bootstrap PR is still pending. GitHub branch protection and rulesets are unavailable for this private repository on its current plan, so the written PR policy remains mandatory even though GitHub cannot yet enforce it.

## Branch roles and PR routes

| Branch | Purpose | Start point | PR destination |
| --- | --- | --- | --- |
| `main` | Approved, stable releases only. The existing initial scaffold is a historical bootstrap commit, not a declared game release. | Existing branch | Receives approved release PRs from `develop` and urgent hotfix PRs. |
| `develop` | Default branch for staging and testing. | Initially at the verified current `main` commit | Receives feature, normal fix, and hotfix back-merge PRs. |
| `feature/<short-description>` | One feature or coherent documentation/configuration change. | `develop` | `develop` |
| `fix/<short-description>` | Non-production bug fix. | `develop` | `develop` |
| `hotfix/<short-description>` | Urgent production fix. | `main` | `main`, followed by a PR that brings the fix into `develop`. |

Examples: `feature/fps-controller`, `fix/player-spawn`, `hotfix/launch-crash`. Use descriptive, scoped commit messages such as `Add first-person movement` or `Fix duplicate enemy spawns`.

## Mandatory practices

1. Put all repository changes, including code, assets, configuration, and documentation, through a PR. After the approved one-time bootstrap, never commit or push directly to `main` or `develop`.
2. A feature or normal fix begins at the latest `develop` and targets `develop`. A release uses a PR from `develop` into `main`. A hotfix begins at `main`, targets `main`, and returns to `develop` through a separate PR or a reviewed backport PR if branches conflict.
3. Preserve existing changes when branching. Never reset, discard, or overwrite uncommitted work as part of a workflow shortcut. Keep Unity assets and their `.meta` files together.
4. Do not force push `main` or `develop`. Codex must not merge a PR without explicit approval from the project owner.
5. Require passing automated checks before merging once reliable CI checks have been configured and observed. Require human code review wherever the repository plan and reviewer access allow GitHub to enforce it. Resolve review comments before merging.
6. Record the change, verification performed, and any untested behavior in the PR. Update `STATUS.md` after a completed milestone and other project documents when decisions change.

Use a merge commit for a `develop` → `main` release PR unless a reviewed change to this workflow selects another strategy; retaining the `develop` ancestry makes later release comparisons easier. A feature PR may use squash merge if the repository's merge settings allow it. Do not enable a linear-history rule that would block the chosen release merge method.

## Recommended GitHub settings

`develop` is now the GitHub default branch. The default branch becomes the default PR base; still verify the base on every PR. [GitHub default-branch instructions](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-branches-in-your-repository/changing-the-default-branch).

Create separate active branch rulesets matching the exact branch names `main` and `develop` after their initial creation. For each branch, recommend:

- Require a PR before merging; block force pushes and branch deletion.
- Require one approving human review when another authorized reviewer is available; dismiss stale approvals on changed diffs and require conversations to be resolved. Do not set a review gate that a solo maintainer cannot satisfy without a deliberate staffing or permission plan.
- Apply the rules to administrators where feasible, and avoid broad bypass permissions. Keep any necessary emergency bypass narrow and auditable.
- Once CI exists, require the actual, stable check names that have reported successfully (for example, a Unity compile/import check and focused tests). Do not require nonexistent checks or enable a check gate before CI can produce it. Consider stricter checks for `main` releases after the pipeline is proven.

GitHub documents these [ruleset controls](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/available-rules-for-rulesets) and [how to create rulesets](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/creating-rulesets-for-a-repository). Live API reads returned HTTP 403 for both rulesets and classic branch protection with the message "Upgrade to GitHub Pro or make this repository public to enable this feature." The repository is private and currently lists only its owner as a collaborator. Do not change visibility or purchase a plan as a workaround without the owner's separate decision. No CI workflow, required checks, or review gate has been configured yet. Until enforcement becomes available, contributors must follow the PR policy manually and state this limitation on PRs.

## One-time bootstrap progress

The live remote was verified before mutation: `main` was the only remote branch and pointed to `0809580` (`Initial check-in`). The initial commit has no demonstrated clean build and includes a one-time helper that later deletes itself. Six earlier Unity changes and the project documents were preserved during branch creation.

1. **Done:** Create and publish `develop` once at `0809580`; create local `feature/repository-bootstrap` at the same commit. All pre-existing working-tree changes carried over without reset or stash.
2. **Done:** Change the GitHub default branch from `main` to `develop`.
3. **Unavailable on current plan:** Enable branch protection/rulesets for `main` and `develop`. Revisit this if the repository becomes public or the plan changes, and if an eligible second human reviewer is added. Required CI checks can be added once a workflow exists and reports stable check names.
4. **In progress:** Commit `80cbda0` recorded the one-time Unity helper deletion, resolved lockfile, and quality serialization update on `feature/repository-bootstrap`. The project documents are included on this branch. `ProjectSettings/ProjectSettings.asset` remains uncommitted while its cloud project and organization fields await confirmation.
5. **Pending:** Open `feature/repository-bootstrap` → `develop`. Review the complete diff and verify Unity import/compilation as practical. Merge only after explicit project-owner approval; Codex must never merge on its own.
6. **Future:** Keep `main` at its historical initial commit until a tested, explicitly approved release is ready. Promote that release through a `develop` → `main` PR. Do not call the initial scaffold a stable production release.

This document is a policy and status record; it does not authorize any later merge, force push, repository visibility change, or paid-plan change.
