# 🎡 Personalized CountryFair VR

A VR serious game for stroke rehabilitation, featuring adaptive mini-games in an immersive country fair environment built for Meta Quest 3 with hand-tracking support.

This repository contains the source code developed as part of a Master's Thesis titled *"Exploring the Effect of a Personalized Virtual Reality Serious Game for Stroke Rehabilitation"*, focused on stroke survivor rehabilitation using Virtual Reality Serious Games.

---

# 📺 Demo

### [CountryFair VR Demo](https://www.youtube.com/watch?v=bDXMD0OPzRI)

---

# 🔍 Overview

CountryFair VR is a VR serious game designed to enhance post-stroke rehabilitation through immersive, gamified exercises targeting upper-limb motor recovery. The project targets **Meta Quest 3** with hand-tracking support and was developed in **Unity 6000.2.5f1**. It features two rehabilitation mini-games set within an interactive country fair hub (`CountryFair.unity`) with 3D spatial audio, and incorporates a rule-based Dynamic Difficulty Adjustment (DDA) system **CarnyWise** (`CarnyWise.cs`) that continuously adapts challenge parameters to the player's performance, maintaining an optimal flow state during rehabilitation sessions.

The system was developed using a Human-Centered Design (HCD) methodology in collaboration with the **Rovisco Pais Rehabilitation Center** and validated through user testing with **38 participants**.

---

# 🎯 The Problem

Traditional post-stroke rehabilitation often suffers from low patient adherence due to the monotony of repetitive exercises and a lack of personalization in the therapeutic process.

---

# 💡 Our Approach

We propose a solution set in a VR Country Fair environment, designed to enhance stroke survivor motivation and engagement. The system utilizes:

- **Immersive Virtual Reality** and serious mini-games to combat demotivation and increase engagement among stroke survivors.
- A **rule-based Dynamic Difficulty Adjustment (DDA)** system `CarnyWise` that adapts game parameters in real time based on stroke survivor performance (precision and time), promoting an optimal flow state via consecutive-attempt counters and configurable thresholds.
- **3D spatial audio** (via FMOD) to reinforce environmental immersion and provide directional feedback.
- **Hand-tracking interaction** on Meta Quest 3 — pinch gestures detected in `BowHandTracking.cs` with controller fallback — enabling natural gesture-based gameplay without controllers.
- **Personalized hub layout** — healthcare professionals can grab and rearrange mini-game tents (`MiniGameTent.cs`, `TentsPlaceHolderManager.cs`) before each session to set the desired activity order for the stroke survivor, using the Meta XR Distance Grab interaction.
- **Hybrid difficulty personalization via cheat codes** — beyond automated DDA, healthcare professionals can manually adjust the difficulty level at runtime using a keyboard cheat code system (`CheatCodes.cs`, `MiniGameCheatCodes.cs`), creating a **hybrid personalization model** that combines automatic adaptation with direct clinician control. The same system also exposes additional session management capabilities (skipping tutorial phases, triggering score/miss events, controlling the emotion display), though these fall outside the scope of personalization itself.
- **Emotion Recognition Module (complementary work)** — a proof-of-concept integration of a colleague's master's research that relays a user's recognized emotional state into the VR mini-games via TCP (`ServerListener.cs`) and Unity Netcode (`ExpressionDisplay.cs`, `SliderDisplay.cs`), using one of three randomly-selected representations per session: emoji, facial expression, or a cumulative mood slider. See [🧠 Emotion Recognition Module](#-emotion-recognition-module-complementary-work) below.
- **Score and streak system** (`ScoreAndStreakSystem.cs`) with DOTween-animated UI feedback and session goal tracking.
- **JSON-driven dialogue system** (`UIDialog.cs`) for onboarding dialogues and tutorial flows, loading from `StreamingAssets/DialogFiles/`.

## Implemented Mini-games

- 🏹🎈 **Archery (`ArcheryGame.unity`):** A balloon-target shooting game where the player uses a virtual bow (hand-tracking via `BowHandTracking.cs`, trajectory preview via `TrajectoryLine.cs`). `ArcheryGameManager.cs` spawns colored balloons inside a defined volume and uses AnimationCurves to scale balloon count, movement ratio, transparency ratio, and movement speed with difficulty.
- 🥏🐶 **Frisbee (`FrisbeeGame.unity`):** A throwing game where the player tosses a frisbee for a dog to catch. `FrisbeeGameManager.cs` spawns score areas around the player at adaptive distances and uses AnimationCurves to control movement and visibility ratios. Dog AI is driven by a five-state FSM (`DogIdle`, `GoToTarget`, `Jump`, `CatchFrisbee`, `GiveFrisbeeToPlayer`).

---

# 🧪 User Testing

The prototype was evaluated in two testing waves. In both, participants played seated, wearing a Meta Quest 3.

## First Wave: Informal Usability Testing

- **Context:** demo sessions in the Virtual and Augmented Reality course at the **VAR Lab, IEETA** (University of Aveiro), with about **6–8 students and teachers**. Participants wore Sony WH-CH500 headphones so they could hear the 3D spatial audio.
- **Prototype state:** the fair scene, tents and both mini-games were working, but there was **no personalization module yet**, no session goal, a HUD fixed in world space, and no emotion module.
- **Findings:** participants were engaged, needed little help, and showed no cybersickness. The audio and the fair atmosphere were praised. The main problems were:
  - no in-game tutorial;
  - the frisbee curved too much toward the throwing hand, and the dog's way of bringing it back felt unnatural;
  - the arrow sometimes fired while the string was still being drawn;
  - scoring balloons were told apart by color alone.
- **What changed afterwards:** a tutorial and practice phase was added before each mini-game, and the early-arrow bug was fixed. The color-only scoring was replaced by a **target-color rule**: the player must hit the requested color, which turns scoring into a selective-attention exercise.

## Second Wave: Formal Usability & Personalization Testing

A formal study with **38 participants** (ages 21–57; 6 female, 32 male; 17 had used VR before). Most were students, mainly in computer science. The others included two nurses, a personal trainer, an XR university professor and an AI engineer. Most sessions ran at the VAR Lab, IEETA, and some in private homes.

- **Prototype state:** the **personalization module was fully implemented** (DDA via `CarnyWise`), except that its feedback text was still fixed in world space. The intro by Zeca Bigodes, the tutorials, the session goals and the final scoring were all in place. Only the emoji form of the emotion module existed, and it was not used in this wave.
- **Setup:** a SteelSeries Nova 3 headset (optional this time) and a **Bluetooth keyboard connected to the HMD**, so the observer could type cheat codes. The observer used them to skip tutorials and to **force difficulty changes** (`increase` / `decrease`). Forcing changes was needed because the DDA could match a player's performance so well that they never saw a noticeable change in difficulty.
- **Tasks:** pick a mini-game, complete the Frisbee mini-game, complete the Archery mini-game.
- **Measurements:** one or two observers noted difficulties and spoken comments. Afterwards participants filled in a 7-point Likert questionnaire with these sections: Demographics, About Personalization, Engagement & Motivation, Comfort & Trust, Feedback & Understanding, **SUS**, optional Acceptance and Effectiveness & Perceived Value sections, and open-ended suggestions.

### Personalization Results

| Statement | Agree | Neutral | Disagree |
|---|---|---|---|
| Noticed the system adapting the mini-games during the session | **31 (82%)** | 2 | 5 (13%) |
| "The system adapted the mini-games to my specific abilities and needs" | **33 (87%)** | 4 | 1 |

These results show the DDA worked and that most players **noticed it**. They measure what players *perceived*, not the adaptation's measured effect on performance. Measuring that effect would need a controlled comparison against a version without adaptation.

### Other Results

| Metric | Result |
|---|---|
| **SUS score** | **82 / 100** ("Excellent", grade A- to A per Bangor et al.) |
| Wanted to keep playing after the test (observed) | 26 (68%) |
| Instructions and objectives were clear | 31 (82%) |
| Did not feel overly tired or overwhelmed | 37 (98%) |
| Felt immersed in a country fair | 34 (90%), no disagreements |
| Fair environment was fun, motivating and engaging | 37 (98%) |
| Spatial sound helped guide actions | 25 (65%), 11 neutral, 2 negative |
| Personalized VR could help stroke rehabilitation *(optional, n = 37)* | 32 (86%), no disagreements |
| Could complement traditional rehabilitation by reducing monotony *(optional, n = 37)* | 36 (97%) |
| Personalized XR could support my rehabilitation progress *(optional, n = 27)* | 22 (81%), no disagreements |

**Issues found:**
- **Frisbee:** 13 participants (34%) struggled with the frisbee mechanics (grabbing, releasing, throwing). The throw velocity comes from the Meta XR SDK and air resistance from Unity physics, so the game has limited control over them.
- **Archery:** 4 participants (11%) had trouble nocking the arrow or pulling the string, and 9 (24%) didn't notice the target-color rule.
- **Text:** in the written feedback, 7 participants (23%) found the on-screen text hard to read or too long.
- **Buttons:** 5 participants (16%) had trouble pressing in-game buttons.
- **Stability:** 7 participants (18%) ran into bugs or crashes.

**Suggestions about personalization:**
- Clearer audio cues when difficulty changes (e.g. a level-up sound).
- Bonus points for keeping a streak.
- A frisbee trajectory line that fades out at higher difficulties.
- A summary of difficulty progression at the end of the session.

Later iterations acted on several of these points:
- the HUD and the DDA feedback text now follow the player's head;
- the target-color text is drawn in the balloon's color;
- narration was added to the dialogue (see 🎙️ Character Voices);
- the arrow trajectory now changes color.

### Limitations

- **No clinical participants:** both waves were run with healthy adults, not stroke survivors. The planned clinical validation at the **Rovisco Pais Rehabilitation Center** could not happen within the dissertation's timeline, so therapeutic efficacy, accessibility for motor-impaired users and clinical safety have **not been validated yet**.
- **Bow hand:** in the tested prototype the bow was attached to the left arm. A later change added the `left` / `right` / `head` positioning modes.

---

# 🧠 Emotion Recognition Module (Complementary Work)

The Emotion Recognition Module is **not a primary objective of this dissertation** — the personalization module (DDA + tent ordering + cheat codes) is. It is a complementary integration of the practical work of a colleague from the same research group, **Bianca Guerreiro**, whose master's dissertation, *"Exploring Emotion Recognition for Stroke Rehabilitation: Towards Personalized Virtual Reality Serious Game"*, studies how detecting a user's emotional state can foster a more adaptive and engaging therapeutic experience in personalized VR for post-stroke rehabilitation.

The integration served two purposes:

1. **Proof of concept for shared emotional awareness** — showing that a stroke survivor's emotional state can be captured, sent to a remote server, and relayed into *another* survivor's mini-game environment, extending the system beyond motor and cognitive training toward mitigating social isolation.
2. **Validation of the architecture's modularity** — Country Fair VR consumes the emotional state as a representation-agnostic output, so an independently developed module was plugged in **without changes to the game's core**.

### How it works

```
Webcam (laptop) → colleague's emotion-recognition pipeline → TCP server
  → ServerListener (Unity, "EXPRESSION;CATEGORY" messages)
  → EmotionDisplay (Netcode NetworkBehaviour) → one of three in-game representations
```

- **Capture:** the original concept was to read facial expressions through the HMD's internal sensors, but no headset with internal face-tracking cameras was available, so a **laptop webcam** captures the participant's face instead.
- **Transport:** `ServerListener.cs` is a raw TCP client that keeps only the latest message (older unprocessed ones are discarded) and flushes it on the main thread every `pollIntervalSeconds`. A failed connection is non-fatal — the game keeps running without emotion data.
- **Sync:** each display inherits `EmotionDisplay : NetworkBehaviour` and stores its state in a server-authoritative `NetworkVariable<T>`, so the therapist's device (`ConnectionManager.StartHost()`) and the headset show the same state.

### Representations

One representation is **chosen at random when entering a mini-game** (`ServerListener.SetCurrentDisplayMode()`) and can be switched at runtime with the `e…`, `f…`, `positive` / `negative` cheat codes:

| Mode | Component | What it shows |
|---|---|---|
| **Emoji** | `ExpressionDisplay` | A simplified symbolic cue for the recognized emotion — quick and intuitive to interpret |
| **Facial expression** | `ExpressionDisplay` (second instance) | A reference face image for the detected expression — a more concrete, recognizable cue |
| **Slider** | `SliderDisplay` | A positive ↔ negative bar initialized at neutral; positive emotions push it one way, negative the other, neutral leaves it unchanged |

| Emotion | Category |
|---|---|
| Happy | Positive |
| Surprise | Positive |
| Sad | Negative |
| Angry | Negative |
| Fear | Negative |
| Disgust | Negative |
| Neutral | Neutral |

### Parallel evaluation

The three representations were compared in a dedicated study run by Bianca Guerreiro at the **VAR Lab, IEETA** (University of Aveiro) as part of her dissertation. This project's author integrated Country Fair VR as a client of the emotion-recognition pipeline, adapted the in-game UI to support the three formats, and attended the sessions as an observer to keep the integrated system working.

- **17 participants**, within-subjects design, 7-point Likert scale, 14 items in four dimensions (usability, ease of interpretation, social-emotional connection, perceived accuracy).
- All three formats scored above the neutral midpoint in every dimension.
- Emoji and slider performed similarly on usability and ease of interpretation; the **facial-expression** format scored notably higher on **social-emotional connection** (empathy, perceived connectedness).
- Preferences were split almost evenly: slider 6, facial expression 6, emoji 5.
- Qualitative feedback pointed to richer future representations, such as avatars or hybrid formats.

**Related publication:** B. Guerreiro, D. Palricas, I. Figueiredo, S. Oliveira, B. Marques, S. Brás, P. Amorim and B. S. Santos (2026). *"Visualization Framework for Shared Emotional Awareness in Personalized VR for Stroke Recovery"*. Proceedings of the International Conference on Information Visualisation (IV), 1–6.

---

# 📁 Project Structure

```
CountryFair/
├── Assets/
│   ├── Scenes/
│   │   ├── CountryFair.unity          # Hub world scene
│   │   └── MiniGames/
│   │       ├── ArcheryGame.unity      # Archery mini-game scene
│   │       └── FrisbeeGame.unity      # Frisbee mini-game scene
│   ├── Scripts/
│   │   ├── General/
│   │   │   ├── GameManagment/         # GameManager (singleton, also configures foveated rendering), AudioManager, CheatCodes
│   │   │   ├── Animals/               # Needs-based animal AI (AnimalUtility, AnimalState, AnimalWalk, AnimalEat, AnimalIdle)
│   │   │   ├── Balloons/              # BalloonsSpawner, PopBalloon (hub-world decorative balloons)
│   │   │   ├── Others/                # WanderingPerson, AnimatableState, TextAnim, ButtonPressed
│   │   │   └── UIDialog/              # JSON-driven dialogue base class (UIDialog, JSONData)
│   │   ├── CountryFair/
│   │   │   ├── Management/            # CountryFairAudioManager, CountryFairCheatCodes
│   │   │   ├── Dialogue/              # CountryFairDialogue + JSON data types (IntroData, SessionCompletedData)
│   │   │   ├── Tents/                 # MiniGameTent, TentPlaceHolder, TentsPlaceHolderManager
│   │   │   └── Other/                 # CabinScript, RotateWheel, SheepAnim, CountryFairBalloonSpawner
│   │   ├── MiniGames/
│   │   │   ├── CommonElements/
│   │   │   │   ├── MiniGamesManagment/  # MiniGameManager (abstract), MiniGameAudioManager, MiniGameCheatCodes
│   │   │   │   ├── DDASystem/           # CarnyWise (DDA engine), CarnyWiseDiffFeedback (UI feedback)
│   │   │   │   ├── Tutorial/            # Tutorial, TutorialData
│   │   │   │   ├── UI/                  # ScoreAndStreakSystem, Emotions/ (EmotionDisplay, ExpressionDisplay, SliderDisplay, ServerListener, ConnectionManager)
│   │   │   │   └── ReturnToFair.cs      # Scene transition back to hub
│   │   │   ├── ArcheryGame/
│   │   │   │   ├── GameManagment/       # ArcheryGameManager, ArcheryAudioManager, ArcheryCheatCodes
│   │   │   │   ├── ArcheryStuff/        # BowHandTracking, Arrow, TrajectoryLine
│   │   │   │   ├── Balloons/            # BalloonArcheryGame (target logic: movement, transparency)
│   │   │   │   └── Persons/             # Crowd, IdlePerson (crowd ambient behavior)
│   │   │   ├── FrisbeeGame/
│   │   │   │   ├── GameManagment/       # FrisbeeGameManager, FrisbeeAudioManager, FrisbeeCheatCodes
│   │   │   │   ├── Dog/                 # DogFootSteps + FSM states (DogState, DogIdle, GoToTarget, Jump, CatchFrisbee, GiveFrisbeeToPlayer)
│   │   │   │   ├── Frisbee/             # FrisbeeTrajectory, FollowPlayerHead + FSM states (FrisbeeState, Landed, OnMovement, OnPlayerFront)
│   │   │   │   └── ScoreArea/           # ScoreAreaProperties, ScoreAreaAnimations
│   │   │   └── DuckGame/
│   │   │       └── DuckSwim.cs          # NavMesh-based duck wandering with bobbing and wobble animation
│   │   └── Utils/
│   │       ├── FSM/                   # FSM, State, Transition (generic finite state machine framework)
│   │       └── Utils.cs               # RandomValueInRange, GetChildren helpers
│   └── Plugins/                       # FMOD, DOTween, Meta XR SDK, Demigiant
├── StreamingAssets/
│   └── DialogFiles/                   # JSON files for all dialogue and tutorial content
└── Packages/
    └── manifest.json                  # Unity Package Manager dependencies
```

---

# 🏗️ Architecture Overview

![Game Architecture Diagram](GameArch/GameArch.jpg)

### Singletons / Managers

| Class | Pattern | Responsibility |
|---|---|---|
| `GameManager` | Singleton (non-MonoBehaviour) | Cross-scene session state flags (`IntroCompleted`, `FrisbeeTutorialCompleted`, `ArcheryTutorialCompleted`, `FrisbeeSessionCompleted`, `ArcherySessionCompleted`) |
| `AudioManager` | MonoBehaviour | FMOD event management for shared audio |
| `CountryFairAudioManager` | MonoBehaviour | Hub-world FMOD events |
| `MiniGameAudioManager` (abstract) | MonoBehaviour | Base for `FrisbeeAudioManager`, `ArcheryAudioManager` |

### Scene Composition

Three self-contained scenes connected by `SceneManager.LoadScene`:
- **`CountryFair`** → hub world, intro dialogue, tent selection/rearrangement
- **`ArcheryGame`** / **`FrisbeeGame`** → mini-game sessions; return to `CountryFair` on goal completion via `CarnyWise.SessionGoalReached()`

### Communication Patterns

- **`UnityEvent<T>`** primary cross-component wiring done in Inspector (e.g., `CarnyWise.changeDifficulty` → `MiniGameManager.ChangeDifficulty`, `ScoreAndStreakSystem.sessionGoalReached` → `CarnyWise.SessionGoalReached`)
- **`PlayerPrefs`** cross-scene persistence for difficulty levels (`FrisbeeDifficultyLevel`, `ArcheryDifficultyLevel`), dog distance (`DogDistance`), session goal (`SessionGoal`), and scoring color (`BalloonColorToScore`)
- **`NetworkVariable<T>`** (Unity Netcode) server-authoritative sync for `ExpressionDisplay` and `SliderDisplay`

### Finite State Machine (FSM)

`FSM.cs` / `State.cs` / `Transition.cs` a serializable FSM used by both animal ambients and mini-game actors:

```
State (abstract)
└── AnimatableState (adds Animator support)
    ├── AnimalState → AnimalWalk / AnimalEat / AnimalIdle  (needs-based, driven by AnimalUtility)
    ├── DogState    → DogIdle / GoToTarget / Jump / CatchFrisbee / GiveFrisbeeToPlayer
    └── FrisbeeState → OnPlayerFront / OnMovement / Landed
```

### DDA Loop (`CarnyWise.cs`)

```
Player scores / misses → PlayerScored() / PlayerMissed()
  → precision = 1 / attempts_until_score
  → EvaluatePerformance(precision, time)
  → _excelCounter / _struggleCounter updated
  → CheckToChangeDifficulty() (threshold: 3 consecutive)
  → changeDifficulty.Invoke(bool) → MiniGameManager.ChangeDifficulty()
  → ApplyDifficultySettings() → SyncTargets() + UpdateTargetsProperties()
```

Immediate decrease also fires if the player misses `failingConsecutiveThreshold` (default 4) times consecutively.

### Emotion Display System

Client side of the colleague's Emotion Recognition Module (see [🧠 Emotion Recognition Module](#-emotion-recognition-module-complementary-work)). `ServerListener` connects via TCP to the external emotion-recognition server, polling at `pollIntervalSeconds` (default 0.5 s). One of three display modes is randomly selected per session:
- **Emoji mode** `ExpressionDisplay` shows one of 7 expression GameObjects (SAD, HAPPY, ANGRY, DISGUST, SURPRISE, FEAR, NEUTRAL)
- **Face mode** second `ExpressionDisplay` instance with a different visual set
- **Slider mode** `SliderDisplay` nudges a cumulative mood bar up (POSITIVE) or down (NEGATIVE)

All three inherit `EmotionDisplay : NetworkBehaviour` and sync state via server-authoritative `NetworkVariable<T>`.

> **Note (testing):** For testing purposes, the emotion-recognition server address in `ServerListener` was set to a static IP.

### Web App Connection

`ConnectToWebApp.cs` connects the game to a companion web app (`CountryFairWebApp/ServerSide`, a Colyseus server) that lets a healthcare professional view/reorder the fair state live. The connection is plain `ws://`, no TLS — a deliberate choice, since the server is meant to run on the same PC whose Mobile Hotspot the headset joins, not on an untrusted network. See **`CLAUDE.md` → "Web App Connection & Cleartext Networking"** for the full setup (required Android/Player Settings, common device-build pitfalls) and the deployment caveat.

---

# 🚀 Getting Started

## Prerequisites

| Requirement | Version |
|---|---|
| **Unity** | 6000.2.5f1 (Unity 6) |
| **Meta Quest 3** | Required for deployment |
| **Blender** *(optional)* | 4.3 or above for editing 3D models |
| **FMOD Studio** *(optional)* | 2.02.32 for editing audio events |

## Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/DavidPalricas/CountryFair-VR.git
   ```
2. **Open in Unity Hub:** Add the cloned `CountryFair/` folder and ensure Unity **6000.2.5f1** is installed. Unity Hub will prompt to install it if missing.
3. **Resolve packages:** On first open, Unity imports packages from `Packages/manifest.json` (Meta XR SDK v78.0.0, XR Hands v1.5.1, XR OpenXR v1.15.1, Unity Netcode, DOTween). Allow the import to complete before interacting with the Editor.
4. **FMOD setup:** If FMOD banks are not pre-built, open FMOD Studio 2.02.32, load the project's FMOD project file, and build banks to `Assets/StreamingAssets/`.
5. **Run in Editor:** Open `Assets/Scenes/CountryFair.unity` as the start scene and press **Play**. For on-device testing, configure the Meta Quest 3 build profile under **File → Build Settings** (Android, OpenXR).

---

# 🕹️ Controls

| Action | Input |
|---|---|
| Navigate / select (hub) | Meta Quest ray + pinch |
| Grab / rearrange tent | Meta Quest Distance Grab → release over placeholder |
| Draw bow | Pinch (index + middle + ring > 0.25 strength) or primary index trigger > 0.2 |
| Release arrow | Open hand (release pinch) |
| Throw frisbee | Wrist flick gesture detected by `FrisbeeTrajectory.cs` |
| Cheat codes (keyboard) | Type code in Editor see table below |

### Keyboard Cheat Codes (available to healthcare professionals at runtime)

| Input | Function | Context | Precondition |
|---|---|---|---|
| `intro` | Skips the fair introduction sequence | Fair Scene | Intro not completed |
| `archery` | Loads the Archery mini-game | Fair Scene | Intro completed |
| `frisbee` | Loads the Frisbee mini-game | Fair Scene | Intro completed |
| `return` | Returns to the Fair Scene | Both mini-games | None |
| `tutorial` | Skips the mini-game tutorial sequence | Both mini-games | Tutorial not completed |
| `increase` | Signals *CarnyWise* to raise the difficulty by one level | Both mini-games | Tutorial completed |
| `decrease` | Signals *CarnyWise* to lower the difficulty by one level | Both mini-games | Tutorial completed |
| `reset` | Resets the difficulty to the first level | Both mini-games | Tutorial completed |
| `e(expression_name)` | Displays the specified emotion via the emoji representation | Both mini-games | Tutorial completed |
| `positive` / `negative` | Shifts the emotion slider right or left, triggering the slider representation | Both mini-games | Tutorial completed |
| `f(expression_name)` | Displays the specified emotion via the facial expression representation | Both mini-games | Tutorial completed |
| `score` | Simulates a successful player score | Both mini-games | Tutorial completed |
| `miss` | Simulates a player miss | Both mini-games | Tutorial completed |
| `dog` | Simulates a score by landing the frisbee in the dog's surrounding area | Frisbee only | Tutorial completed |
| `head` | Makes the bow follow the player's head | Archery only | Tutorial completed |
| `left` | Holds the bow in the left hand | Archery only | Tutorial completed |
| `right` | Holds the bow in the right hand | Archery only | Tutorial completed |

---

# 🎙️ Character Voices

The characters' narrated dialogue lines were generated with [ElevenLabs](https://elevenlabs.io/) using **free-tier voice models in European Portuguese (Portugal)**, so every character speaks in the same language and accent as the on-screen text. The generated clips are played in-game through FMOD (see the narration system in `UIDialog`).

| Character | ElevenLabs Voice | Stability | Similarity |
|---|---|---|---|
| **Zeca Bigodes** | Vicente – *Warm and Raspy* | 40% | 75% |
| **Carny Wise** | Diogo – *Warm and Conversational* | 65% | 75% |

---

# 🛠️ Tools Used

| Tool | Purpose |
|---|---|
| **Unity 6** | Game engine and XR runtime |
| **Blender** | 3D modeling and animation |
| **FMOD Studio** | 3D spatial audio design |
| **ElevenLabs** | Text-to-speech for the characters' narrated dialogue (European Portuguese) |
| **Meta XR SDK** | Hand-tracking and Quest 3 integration |

---

# 🙏 Acknowledgments

The following 3D assets were sourced from [Poly Pizza](https://poly.pizza/) and the Unity Asset Store:

| Asset | Author | Link |
|---|---|---|
| Sheep Model | Google | [poly.pizza/m/dXBMV4AY2DL](https://poly.pizza/m/dXBMV4AY2DL) |
| Ferris Wheel Model | Google | [poly.pizza/m/5KiVEnXN5Cw](https://poly.pizza/m/5KiVEnXN5Cw) |
| Tent Model | Google | [poly.pizza/m/9Ob6OO8HMjX](https://poly.pizza/m/9Ob6OO8HMjX) |
| Fair Animals Models and Animations (Bundle) | Quaternius | [poly.pizza/bundle/Animated-Animal-Pack-ILAPXeUYiS](https://poly.pizza/bundle/Animated-Animal-Pack-ILAPXeUYiS) |
| Persons Models and Animations (Bundle) | J-Toastie | [poly.pizza/bundle/CUTES-Part-One-WD91WrT0gx](https://poly.pizza/bundle/CUTES-Part-One-WD91WrT0gx) |
| Carnival Booth Model | sirkitree | [poly.pizza/m/4NizXJZsuO2](https://poly.pizza/m/4NizXJZsuO2) |
| Merry Go-Round Model | sirkitree | [poly.pizza/m/8BliGG-e55g](https://poly.pizza/m/8BliGG-e55g) |
| Ground Model | Adam Tomkins | [poly.pizza/m/achm-Cr9Rr3](https://poly.pizza/m/achm-Cr9Rr3) |
| Big Tent Model | Ian MacGillivray | [poly.pizza/m/0TmXixFN81A](https://poly.pizza/m/0TmXixFN81A) |
| Popcorn Cart Model | Don Carson | [poly.pizza/m/1T9untmohmj](https://poly.pizza/m/1T9untmohmj) |
| Bench Model | Sammy | [poly.pizza/m/3ytbP2GLb0b](https://poly.pizza/m/3ytbP2GLb0b) |
| SkyBox Texture | Borodar | [assetstore.unity.com — Farland Skies: Cloudy Crown](https://assetstore.unity.com/packages/2d/textures-materials/sky/farland-skies-cloudy-crown-60004) |

---

# 📝 Extra Notes

- Part of this project was developed for the [Virtual And Augmented Reality](https://www.ua.pt/en/uc/12023) and [Serious Games](https://www.ua.pt/en/uc/15477) courses at the **University of Aveiro**.
- The Emotion Recognition Module is the work of **Bianca Guerreiro** (same research group), integrated here as a complementary proof of concept.
- Development period: 2025–Present.