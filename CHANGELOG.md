# Changelog

All notable changes to this Project/technical assesment will be documented in this file.

------------------------------------------------------------------------

## \[Unreleased\]

### Planned

- Add some 3d models for the basketball scene.

------------------------------------------------------------------------

## \[0.3.0\] - 05/03/2026

### All mandatory and optional features release

Third playable version including all mandatory and optional tasks. With a few extras like the Lootbox System or de daily missions system.

------------------------------------------------------------------------

### Added

#### Gameplay

- all money and gold are stores in user's unique session.

Here is a summary of all the UI pages that gave been included to the game. All these popups and panels are connected with animations.
- Added Menu initial screen page
- Added main menu page
- Added Lootbox opening page - interactive with animations
- added match results page
- added daily missions system and popup
- added quit menu

- Add Pause menu in the basketball game.
- Improve input system and fix some bugs related to the basketball game.
- Add Swipetrail - finger swipe feedback

#### In game UI
- Improved game UI with pictures, new sliders and new time clock based in a bar that runs out.
- Improved font in all UI elements
- Add text upon scoring a shot


#### VFX
- Import Cartoon FX Remaster Free package - all 3d vfx have been done with this optimized package
- 3 different types of effects upon scoring
- fireball efect when the bonus is up

#### SFX
Implemented sounds:
- backSound - UI back button
- confirmSound - All buttons
- cancelSound - close popups buttons
- firstTapSound - Initial screen tap
- popupSlideInSound - When a popup slides in
- popupSlideOutSound - When a popup slides out
- redeemDailyMissionSound - After redeeming a daily mission
- winSound - After winning a game
- loseDrawSound - After losing a game or getting a draw
- openLootboxSound - Upon opening a lootbox
- cardFlipSound - Upon flipping the lootbox cards
- netNormalSound - When scoring a normal/backboard with no bonus basket
- netPerfectSound - When scoring a perfect shot
- niceShotSound - Whn getting abonus
- rimBounceSound - Hitting the rim
- backboardBounceSound - hitting the backboard
- floorBounceSound - hitting the floor
- playerMoanSound - before shooting the ball
- tictacSound - 10 seconds before the time runs out
- fireSound - when the fire bonus is active
- fireOverSound - when missing a shot/fire bonus time is over
- backgroundMusic - menu background music
- basketGameBackgroundSound - basketball course ambience sound

#### Compatibility
- LS/PT mode in both scenes

------------------------------------------------------------------------

### Testing

-   Internal gameplay testing sessions with several beta testers
-   Tested aspect ratios are are: PC/Windows -> landscape FHD // Android/IOS -> Portrait FHD, portrait HD and 20x9.
-   Builds fully tested in Android and Windows.

------------------------------------------------------------------------

### Known Issues (Reported by Beta Testers)

#### High

#### Medium

#### Low 

------------------------------------------------------------------------

### Limitations (Some of them expected to be fixed in the next release)

-   No detailed 3d models in the basket scene.
-   No tablet or uncommon aspect ratios supported.
-   No basket net.


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

