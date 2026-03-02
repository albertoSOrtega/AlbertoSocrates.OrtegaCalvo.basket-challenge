# Changelog

All notable changes to this Project/technical assesment will be documented in this file.

------------------------------------------------------------------------

## \[Unreleased\]

### Planned

-   The rest of mandatory features (UI, Menus, Rewards page)
-   Visual Effects
-   Sound Effects
-   Difficulty level against the CPU

------------------------------------------------------------------------

## \[0.2.0\] - 27/02/2026

### Full basket gameplay release

Second playable version including all mandatory gameplay systems plus CPU, Fireball system and mobile support.

------------------------------------------------------------------------

### Added

#### Gameplay

-   Playing against the CPU mode added.
-   CPU moves arount the court in the same was as player and throws the ball in a range of time.
-   CPU can perform every kind of shot.
-   CPU has its own Score section.
-   Has a higher perfect backboard shooting rate when the bonus is active.

-   Add mobile layout to the game scene.
-   Supports most common screen ratios

-   Improved input system with better quality and DPI support if available in mobile phones.

-   Implements the fireball multiplier system. Everytime the player scores a shot a bar fills. When this bar is completely filled, the player gains a x2 in points.
-   Fireball bar will fill up if scoring a shot (more if a perfect one) and drains with time.
-   Fireball bar empties if player misses a shot while the bonus is active.
-   Fileball slider changes color if bonus is active.

------------------------------------------------------------------------

### Testing

-   Internal gameplay testing sessions with several beta testers
-   Tested aspect ratios are are: PC/Windows -> landscape FHD // Android/IOS -> Portrait FHD, portrait HD and 20x9.
-   Builds fully tested in Android and Windows.

------------------------------------------------------------------------

### Known Issues (Reported by Beta Testers)

#### High

-   No replay option
-   Sometimes the ball will cross the backboard with no collision.
-   Cancel shot needs improvement. It tries to release the shot.

#### Medium

-   No quit menu

#### Low

-   No tutorial/controls

------------------------------------------------------------------------

### Limitations (Some of them expected to be fixed in the next release)

-   No VFX or SFX.
-   No menu navigation.
-   No tablet or uncommon aspect ratios supported.
-   Placeholder assets.
-   Usage of Unity basic UI elements like basic sliders
-   No basket net

------------------------------------------------------------------------

## \[0.1.0\] - 27/02/2026

### Mandatory features with mouse and no UI Release

First playable version including all mandatory gameplay systems.

------------------------------------------------------------------------

### Added

#### Gameplay

-   Core gameplay loop implemented
-   Ball throwing mechanic with 5 different shots, including rebounds
-   Detailed power shooting bar
-   Score calculation system
-   Backboard bonus system. Appears every 5 seconds and provides a point bonus when hitting a backboard shot.
-   Polished Score system 
-   In-game Timer

#### Camera

-   Follow camera system
-   Basic smoothing behaviour

------------------------------------------------------------------------

### Testing

-   Internal gameplay testing sessions with several beta testers
-   Fullscreen testing

------------------------------------------------------------------------

### Known Issues (Reported by Beta Testers)

#### High

-   No replay option


#### Medium

-   No quit menu

#### Low

-   No tutorial/controls

------------------------------------------------------------------------

### Limitations (Expected in this first release)

-   No CPU opponent
-   Mobile controls not verified
-   Placeholder assets
-   Usage of Unity basic UI elements like basic sliders
-   No basket net

------------------------------------------------------------------------

