# Git nomenclature and workflow

This repository follows a GitFlow-inspired workflow optimized for **one-person development**, while keeping a **professional PR-based history** that is easy to review.

---

## 1) System Table (codes used in branches and commits)

System codes used in commits and branches:

- MEN (Menus)
- IGUI (In-Game UI)
- CPU (CPU Controller)
- CAM (Camera Movement)
- GTS (Gameplay Time and Score)
- TBM (Throwing Ball - Mouse/Mobile)
- BKB (Backboard)
- FIR (Fireball)
- SCNPC (Scenario, props and characters)
- SND (Sound and music)
- VFX (VFX and Animations)
- GEN (General)

---

## 2) Ticket workflow (Notion)

**Checklist per ticket**

- [ ] `System Code` selected (from section 1)
- [ ] `KEY` correct (auto-generated in Notion)
- [ ] Copied to `Git KEY` (manual, once) to keep a stable identifier even if system code changes
- [ ] Branch created using `Git KEY` (1 branch per ticket)
- [ ] Commits include `[Git KEY]` (1+ commits per branch)
- [ ] PR opened into `develop` with title `[Git KEY] ...` (1 PR per branch)

---

## 3) Branch naming

Branch format:

`<type>/<GitKEY>-<slug>`

Examples:

- `feature/SAV-17-save-system-slot-ui`
- `fix/TBM-203-aim-jitter`
- `refactor/CAM-55-follow-smoothing`

**Slug guidelines:** 3–6 words in kebab-case describing the goal.

### Branch types

These prefixes are used in the branch name:

- **`feature/`**  
  New functionality or a visible expansion of existing behavior.  
  Example: a new menu flow, save system, new gameplay mechanic.

- **`fix/`**  
  Primary goal is to fix a bug (incorrect behavior).  
  Example: score calculation bug, input failing on specific devices.

- **`chore/`**  
  Technical tasks that improve maintenance/setup without adding gameplay features.  
  Example: dependency updates, folder cleanup, project settings, tooling.

- **`refactor/`**  
  Internal restructuring with no intended behavior change.  
  Example: extracting classes, renaming, simplifying architecture, reducing duplication.

- **`test/`**  
  Work centered on testing or testing infrastructure (including perf testing if treated as test work).  
  Example: input test scenes, score validation harnesses.

---

## 4) Commit message convention

Commit format:

`<type>(<SYS>): <summary> [<GitKEY>]`

- `<SYS>`: the system being modified (MEN/CPU/CAM...)
- `[<GitKEY>]`: the stable ticket identifier used in Git
- `<summary>`: imperative, short, no trailing period

Guidelines:
- One commit = one clear intent
- Keep commits small but meaningful (not microscopic)
- Avoid mixing formatting + logic + assets in a single commit when possible

Examples:

- `feat(SAV): add save slot serialization [SAV-17]`
- `fix(TBM): prevent aim jitter on high dpi [TBM-203]`
- `refactor(CAM): simplify follow target logic [CAM-55]`

### Conventional Commit types used

- `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `build`, `ci`, `chore`

---

## 5) Pull Requests

PR title format:

`[<GitKEY>] <ticket title>`

PRs target `develop`.

PR description includes:
- Summary of the change
- How to test
- Notes / risks

---

## 6) GitFlow structure used in this repo

### Branch roles

- `develop`  
  Daily integration branch. All ticket PRs land here.

- `main`  
  Stable/releasable history. Only receives code through release/hotfix PRs.

- `release/*`  
  Stabilization branches used to prepare official builds (QA + final fixes).

- `hotfix/*`  
  Only used if a critical issue is discovered after merging a release into `main`.

---

## 7) Daily development steps (what I followed per ticket)

1. **Create a branch from `develop`**  
   Example: `feature/MEN-17-main-menu-navigation`

2. **Implement the ticket with structured commits**  
   Using: `feat(SYS): ... [GitKEY]` (or `fix/refactor/...`)

3. **Open a PR into `develop`**  
   PR title: `[GitKEY] ...`

4. **Merge into `develop`**
   - Preferred merge method: **Squash and merge**
   - Reason: keeps `develop` clean and easy to audit (1 commit per ticket)

---

## 8) Releases (how builds are stabilized)

When preparing a serious build, features are frozen and the focus becomes stabilization.

### Create release branch

From `develop`:

- `release/0.1.0-mvp` (or similar)

Recommended SemVer plan during development:

- Release 1 (mandatory): `0.1.0`
- Release 2 (CPU + Menu): `0.2.0`
- Release 3 (optional): `0.3.0`
- Release 4 (polish): `1.0.0` or `0.4.0`

### What goes into `release/*`

Only stabilization changes:
- `fix(...)` bug fixes
- `perf(...)` performance optimizations
- `docs(...)` documentation updates
- small build/config adjustments

No major new features should enter during a release branch.

### Merge release -> main (and tag)

When the build is approved:

1. PR: `release/x.y.z-*` -> `main`
2. Merge using merge commit.
3. Tag on `main`: `vX.Y.Z`
4. GitHub Release with:
   - short release notes
   - content checklist

### Back-merge to develop

After merging into `main`:

- PR: `main` -> `develop` (or `release/*` -> `develop` if it still exists)

This ensures all stabilization fixes are synced back into `develop`.

---

## 9) Hotfixes (only if needed)

If a critical bug is found after publishing a tag on `main`:

1. Create branch from `main`  
   - `hotfix/0.1.1-crash-on-startup` (or use a GitKEY if applicable)
2. PR to `main`, merge
3. Tag: `v0.1.1`
4. PR `main` -> `develop` to sync the fix back