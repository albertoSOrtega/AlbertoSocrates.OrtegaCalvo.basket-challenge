# System Overview (by subsystem)

## MainMenu Scene

### MEN (Menus)

This system embraces all the menus of the Menus scene, not the basketball game one.

#### MenuNavigationController.cs
- Central menu/navigation router for the whole UI flow (Initial Screen -> Main Menu -> Mode Select -> Lootbox/Results, plus popups). 
- Uses a navigation stack to support Back behavior (PC Escape + Android back).
- Handles panel transitions (slide + fade) with DOTween, including per-panel customizable duration/distance.
- Manages popups, overlay dimming and blocking and popup slide transitions.

#### InitialScreenController.cs
- Plays the intro animation (logo + text).
- Accepts first tap/click and routes to Main Menu via MenuNavigationController.

#### MainMenuController.cs
- Reads session state (SessionState.I.currency and .daily) and updates money/gold and daily progress UI.
- Holds a SelectedSlot reference used by the lootbox/bag flow. 

#### GameModeSelectorController.cs
- Sets the selected difficulty (SelectedDifficultySO.config) and loads the game scene (BasketballGame).
- Clears match results before starting a new match.

#### LootboxOpenerController.cs
- Implements the 'open bag/lootbox' interaction.
- Switches bag visuals (closed or open depending on the game state), opens the rewards popup, and animates reward cards in sequence.
- Integrates with MainMenuController.SelectedSlot to deactivate the used bag slot.

#### CardController.cs
- Implements the reward card 'flip' animation.
- On reveal, increases money/gold in SessionState.I.currency and updates the UI text.

#### DailyMissionController.cs
- Handles a single daily mission UI row: completion state, claim button enable/disable, reward claiming.
- On claim. updates currency, marks reward claimed, updates daily progress bars/text.
- If all missions are claimed, activates a bag slot reward.

#### MatchResultsPanelController.cs
- Displays the match result using MatchResultSO (scores + win/lose/draw visuals).
- If the player wins, grants rewards based on difficulty config (SelectedDifficultySO.config) including optional bag rewards.
- Advances daily mission progress (marks missions as done).
- Plays appropriate menu/result sounds and activates and animates the winner image.

---

## Basketball Game Scene

### IGUI (In Game User Interface)

#### PauseController.cs (MEN + IGUI, the only menu in this scene, placed here because it's a 'menu overlay' system)
- Toggles pause state: Time.timeScale, DOTween pause/resume, overlay fade, and pause menu visibility.
- Disables/enables gameplay input (ThrowBallInputHandler) while paused.
- Supports 'back' input: Escape on PC and Android back mapped via input device check in code.
- Can return to the main menu or quit the game.

#### InGameUIController.cs
- Chooses Landscape vs Portrait UI references at runtime and drives all in-game UI.
- Subscribes to:
  - ThrowBallInputHandler power updates and input. Shows the shooting power through the input slider in the IGUI.
  - ScoreController.OnScoreUpdated: Score texts.
  - GameTimerController.OnTimerTick: Timer fill UI and red warning color near end.
  - FireballController events: slider bar value and x2 visuals and animations.
  - GameController.OnPlayerScored: Floating shot feedback text (Perfect / 2pt / Bonus).
  - Positions and sizes the perfect/backboard zone overlays on the power slider based on computed zone boundaries.

---

### CPU (CPU Controller)

#### CPUController.cs
- Contains CPU match loop:
- Moves to each generated shooting position.
- Spawns a ball from the pool and shoots after a random wait interval.
- Shot selection process:
  - Weighted random among multiple ShotTypes.
  - When backboard bonus is active, can force PerfectBackboard by an incremented probability.
- Responds to game events:
  - Starts new rounds via ShootingPositionController.
  - Listens to GameController bonus activation/reset events.
- Accepts difficulty tuning via ApplyConfig(GameDifficultyConfigSO) (timings + weights + bonus probability).

---

### CAM (Camera Movement)

#### CameraController.cs
- Smooth follow camera:
  - Follows either the player or the ball depending on shot state.
  - Always looks at the rim.
- Integrates with BallShooterController:
  - OnShotStarted -> follow ball.
  - OnShotCompleted -> stop following ball; shake on Perfect shots; then snap back behind player.
  - Emits OnCameraBehindPlayer when snapped back, used to re-enable input safely.

---

### GTS (Gameplay Orchestration, Time and Score)

#### GameController.cs
- Main gameplay orchestrator.
- Initializes match, starts timer, generates positions for player and CPU.
- Handles scoring events from BasketballDetectorController, updates ScoreController.
- Runs the backboard bonus state machine ('ready' -> activate on next basket; reset on PerfectBackboard score).
- Notifies CPU about bonus activation/reset.
- On match end: disables player input + CPU, stores results in MatchResultSO, loads Main Menu.

#### GameTimerController.cs
- Match timer (frame-accurate Update-based timer, not using coroutines).
- OnTimerTick provides normalized remaining time for UI (normalized because it is used to fill the IGUI bar associated with the shooting power input).
- Plays 'tictac' SFX when crossing 10 seconds remaining.
- Ends match when time hits 0 (OnMatchEnded).
- Contains the backboard bonus interval timer. Triggers the bonus interval, then pauses until the bonus is collected/reset.
- Supports changing match duration.

#### ScoreController.cs
- Calculates points by shot type and applies Fireball multiplier for the player only.
- Tracks player/cpu scores and passes information to the InGameUIController through events.
- Applies backboard bonus points for PerfectBackboard shot.
- Plays scoring SFX (perfect shot, normal shot, 'nice shot' on bonus).

#### GameInitializer.cs
- Pre-match configuration distributor.
- Pulls difficulty config from SelectedDifficultySO and applies it to CPU, zones, timer, and fireball.
- Starts in-game music.

---

### TBM (Throwing Ball - Mouse/Mobile)

Supports touchpad and mouse control. Used by the player and CPU to throw the ball and score points. Controls the player input and the trajectory that the ball should do.

#### BallShooterController.cs (Throwing Ball 'Shot Execution Engine')
- Executes the selected ShotType by driving the ball through either scripted trajectories (DOTween Bezier) or real physics projectile motion, handling ball detaching, spin, temporary physics settings, and shot lifecycle events.
- Core flow / lifecycle:
  - SetBall(...): stores ballTransform + Rigidbody.
  - Before shooting: detaches ball from player (SetParent(null)) so player jump tweens don't affect it.
  - Emits OnShotStarted when a shot begins and OnShotCompleted(ShotType) when it ends.
  - Switches between kinematic (tween-driven) and non-kinematic + gravity (physics-driven) modes depending on shot type.
- Shot types supported (6) and their trajectory model:
  - Perfect (StartPerfectShot)
    - DOTween cubic Bezier from ball -> rim using tuned control-point offsets.
    - Uses BallSpinController inducing backspin to the ball.
    - At the end, re-enables physics and adds a small finishing impulse (to make the drop feel good).
    - Always scores.
  - Short (StartShortShot)
    - Same DOTween Bezier system as Perfect, but target is not the rim. It's a point before reaching the rim computed from shootPower.
    - Computes a 'short target distance' by mapping power into the Short zone range (ties into ShootingBarZoneController + input min swipe).
    - Also uses BallSpinController.
    - Always misses.
  - Imperfect (StartImperfectShot)
    - DOTween Bezier to a random rim-edge point (not center).
    - On completion, re-enables physics for a realistic bounce.
    - Always scores.
  - LowerBackboard (StartLowerBackboardShot)
    - Pure physics projectile. Computes initial velocity with projectile-motion math and sets rb.velocity.
    - Target is a randomized point on the lower board (biased to the side where the shooter is).
    - Always misses.
  - UpperBackboard (StartUpperBackboardShot)
    - Same physics projectile approach, but enables a security barrier to prevent accidental scoring.
    - Temporarily changes the ball physics material bounceCombine = Maximum for bouncier behavior.
    - Uses a time multiplier (faster flight) but the shot still completes via delayed coroutine.
    - Always misses.
  - PerfectBackboard (StartPerfectBackboardShot)
    - Physics projectile to a computed board contact point (uses reflection laws for the calculation: reflect rim across backboard plane to find where the ball should hit).
    - Before launching, configures BackboardCollisionController on the ball to mark it as a perfect backboard shot, temporarily ignore the backboard collider and reset the rebound state.
    - The actual guided rebound toward the rim is then handled by BackboardCollisionController on impact.
    - Always scores.

#### ThrowBallInputHandler.cs 
- Handles mouse/touch swipe-to-shoot using New Input System  (press/hold/release) and emits swipe/shoot events.
- Uses screen-height percentages for swipe distance normalization on PC/editor for predictability.
- Prevents ghost swipes with a 'wait until release' guard.
- Computes swipe distances using physical cm via DPI when available. Falls back to screen % if DPI is missing.
- Enforces a maximum swipe time and emits the same event set used by gameplay/UI.

#### PlayerController.cs (character control - placed here as 'player entity orchestration')
- Moves the player to each generated position, orients them toward the rim, spawns a ball in front.
- On swipe release, jumps and executes the shot matching the current slider zone.
- Returns ball to pool after shot completion and advances position.
- Syncs fireball VFX on the currently held ball.

#### ShootingPositionController.cs
- Generates the semicircle shooting positions (near/mid/far, 4 shots each) and alternates left/right 'zone'.
- Emits events when new rounds are generated and when positions advance.
- Provides distances used by the shooting zone system to scale the slider zones (perfect and backboard shot zones).

#### BasketballDetectorController.cs
- Detects when a basket is scored via trigger.
- Only counts if the ball is moving downward.
- Emits OnBasketballScored so all the controllers subscribed to this event can perform their functionality.

#### BallPoolController.cs
- Object pooling to improve performance (avoids runtime instantiation spikes).
- Manages pooled ball objects (spawn/return/reset) used by both player and CPU.
- 6 ball pool. The player and CPU take balls from this pool.

#### BallSpinController.cs
- Handles the ball spin
- Computes a shot-dependent spinAxis (perpendicular to shot direction) and starts backspin.

#### BallController.cs
- Small data component attached to the ball (owner + shot type), used by scoring logic.

#### SwipeTrailController.cs 
- Shows the mouse/touchscreen swipe trail feedback for swipes.

---

### BKB (Backboard)

#### BackboardCollisionController.cs
- For PerfectBackboard shots: captures pre-impact velocity and applies a guided rebound.
- Computes collision normal from closest point and reflects velocity and blends reflection with 'ideal' parabolic velocity toward the rim.
- Temporarily ignores collisions between ball and backboard to avoid double-hit interference.
- Triggers VFX flash + plays backboard bounce sound on trigger hit.

#### BackboardVisualFeedbackController.cs
- Backboard bonus presentation:
  - swaps textures according to bonus value.
  - pulses emission/glow while bonus is active.
  - spikes brightness on hit (flash), then returns to pulse.
- Singleton so gameplay code can trigger it easily.

---

### FIR (Fireball)

#### FireballController.cs
- Implements Fireball bar fill/drain and bonus x2 activation.
- Emits events to drive UI and ball visuals.
- Plays looped fire sound while active and 'fire over' when deactivated.
- Has logic to empty the bar when the player misses while bonus is active.
- Turns fireball VFX objects on/off on the ball and restarts their particle systems.

---

### SCNPC (Scenario, props and characters)

#### CourtLinesGeneratorController.cs
- Procedurally generates court lines (half or full court) with LineRenderer.
- Runs in editor and play mode (ExecuteAlways) and rebuilds safely on validate.

---

## Common Logic (both Main Menu Scene and BasketballGame)

### SND (Sound and music)

#### GameAudioController.cs
- Central audio manager singleton:
  - UI SFX, match result SFX, lootbox SFX, gameplay SFX (net/rim/backboard/floor/player moan), tictac warning, fire loop.
- Menu music vs game music switching.
- Helpers to stop/pause/resume audio channels cleanly.

---

## VFX (VFX and Animations)

Some of the VFX included through the systems that the BasketballGame scene contains:

- BasketParticleController.cs: Plays scoring particle systems depending on shot type and whether backboard bonus points were awarded.
- ParticleRotationFixer.cs: Prevents particles from inheriting undesired rotations (forces identity rotation each frame).
- SwipeTrailController.cs: Visual swipe feedback using TrailRenderer (a VFX/feedback element tied to input).
- BackboardVisualFeedbackController.cs: Emission/glow pulse + hit flash + texture swapping on the backboard during the bonus.
- BallSpinController.cs: Backspin visual effect (and physics handoff) for the ball during/after shot execution.
- BallShooterController.cs: Uses DOTween to animate ball trajectories (Bezier) and player jump timing integration; contains debug gizmos for trajectories.
- CameraController.cs: Camera shake (DOTween punch) on Perfect shots.

The MainMenu scene also contains:

- InitialScreenController.cs, MenuNavigationController.cs, LootboxOpenerController.cs, CardController.cs, MatchResultsPanelController.cs, InGameUIController.cs, PauseController.cs
- All of these also contribute to VFX/animation via DOTween-driven UI transitions, fades, scaling, and looping animations.

---

## General (GEN)

### UILayoutSelector.cs
- Activates the correct UI canvas at startup depending on orientation: Portrait canvas for mobile, landscape for PC/tablet.