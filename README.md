# Basket Challenge (Unity) — Technical Assessment Project

Video game developed in one week as part of a mobile videogame recruitment process (Miniclip).  
This repository contains both the **MainMenu** scene (meta/game loop) and the **BasketballGame** scene (core gameplay).

---

## Features (Technical Test Requirements)

The project was built to match the original assessment brief, including **mandatory** and **optional** features.

### Mandatory features (implemented)

- **Single-player gameplay** (no animated character required).
- **Camera movements** (player/ball follow with smooth behavior + shot feedback).
- **Mouse input** (PC/Editor swipe-to-shoot).
- **Mobile version with touch input** (New Input System touch support + DPI fallback).
- **Basic score system**
  - **3 points** for perfect shots.
  - **2 points** for other scoring shots.
- **Backboard bonus**
  - The backboard can become active and **blink/glow**.
  - If you hit the backboard correctly before scoring, it grants a higher score reward.
- **Basic UI pages**
  - Start / initial page.
  - In-game UI.
  - Reward/results page.
  - Daily missions page.
  - Lootbox opening page.

### Optional features (implemented)

- **AI-controlled opponent (CPU)** that competes against the player with the same shooting gameplay loop.
- **Fireball gameplay**
  - Scoring fills a bar.
  - When full, the ball enters a “fire” state and **points are doubled** for a limited time (bar drains).
  - The bonus ends when the bar empties (or when a miss is detected, depending on the shot rules).
- **Special effects (VFX)**
  - Scoring particles and bonus-related visual feedback (backboard glow/flash, fireball visuals, swipe trail, etc.).
- **Sound effects and music (SFX)**
  - UI, scoring, impacts, bonus loops, match results, and scene music switching.

---

## Project highlights

- **Two-scene structure**
  - **MainMenu scene:** menus, difficulty selection, daily missions, lootboxes, results screen.
  - **BasketballGame scene:** swipe-to-shoot gameplay, CPU opponent, scoring, bonuses, pause, VFX/SFX.

- **Cross-platform input**
  - New Input System support for **mouse (PC/Editor)** and **touch (mobile)**.

- **Gameplay systems**
  - Multiple shot types (Bezier/tweened + projectile/physics).
  - Backboard bonus system + guided rebound.
  - Fireball (x2) multiplier system.
  - CPU opponent with weighted shot selection and difficulty tuning.

- **Orientation support**
  - Landscape / Portrait UI variants and runtime selection.

---

## Documentation

- System overview (by subsystem): see `docs/SYSTEMS.md`
- Git nomenclature and workflow: see `docs/GIT_WORKFLOW.md`

---

## Notes

- `CHANGELOG.md` documents releases and feature milestones.
- The project is tuned around two orientations (portrait/landscape) and two primary input modes (mouse/touch).