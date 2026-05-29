# FunFit

A motion-tracked fitness game built in Unity that combines real-time pose detection with haptic feedback to create an interactive, guided workout experience for users of all motor abilities — inspired by Just Dance, designed for accessibility, with applications in astronaut training and human spaceflight.

> **Built under the mentorship of [Dr. Tony Liao](https://www.ist.uh.edu/faculty/liao-tony) at the [CougAR Lab](https://www.uhcougarlab.com/), University of Houston.**

## Demo

🎥 (https://youtu.be/N0lM1CcenaU)

## Motivation

This project started from volunteering at [GiGi's Playhouse](https://gigisplayhouse.org/sugarland/) Houston's fitness program, where I saw how movement-based activities can make a meaningful difference for individuals with Down syndrome. Participants were engaged and motivated by music and movement, but existing fitness games (Ring Fit, Just Dance) assume a baseline level of motor control that not everyone has. There was no way to provide real-time, body-level feedback that adapts to different abilities.

I wanted to build something that:
- Gives **immediate, physical feedback** (not just visual) so users know how they're doing without having to watch a screen
- **Adapts to different motor abilities** rather than assuming a single "correct" way to move
- Makes exercise feel like a game, not a therapy session

The Lenient mode was designed specifically with GiGi's participants in mind — it provides a wider angular tolerance for pose matching so the experience stays encouraging and accessible for those with motor difficulties, while Strict mode challenges users who want a more precise workout.

## What It Does

Players follow along with a 3D model performing exercises while a webcam tracks their body movements using MediaPipe pose estimation. A bHaptics TactSuit vest provides real-time haptic feedback that responds to how well the player matches the model:

- **In sync** — the vest buzzes in a smooth trickle pattern guiding the player through each movement
- **Out of sync** — all motors fire at once, signaling the player to adjust their form

An on-screen vest visualization mirrors the haptic feedback in real time, showing exactly what the player would feel.

## Key Features

- **Real-time pose tracking** via webcam using Google's MediaPipe Pose Landmarker
- **bHaptics vest integration** with synced haptic patterns mapped to exercise movements
- **AI Coach** providing live encouragement, rep-by-rep feedback, streak tracking, and post-session performance summaries
- **Two difficulty modes** — Strict and Lenient — designed to accommodate users with varying motor abilities
- **Gamification** including rep counting, accuracy scoring, star ratings, streak bonuses, and milestone celebrations
- **Timed workout sessions** with a detailed results screen at the end

## Architecture & Design Decisions

```
Unity (C#)                          Python
┌─────────────────────┐             ┌─────────────────────┐
│  Game Scene          │            │  pose_server.py      │
│  ├─ Squat Model      │   UDP     │  ├─ MediaPipe Pose   │
│  ├─ Webcam Display ──┼──frames──►│  │  Landmarker       │
│  ├─ Vest Viz         │           │  └─ Pose Data ───────┤
│  ├─ AI Coach         │◄──pose────┤                      │
│  ├─ Haptic Controller│──haptic──►│  ├─ bHaptics Vest    │
│  └─ Game UI          │  commands │  │  Control           │
└─────────────────────┘            └─────────────────────┘
```

### Why two processes (Unity + Python) instead of one?

MediaPipe's Pose Landmarker has the strongest support and most active development in its Python SDK. Unity's C# ecosystem doesn't have a comparable real-time pose estimation library with the same accuracy. Rather than writing a mediocre port or wrapping a C++ library, I split the system: Python handles what Python does best (ML inference via MediaPipe), and Unity handles what Unity does best (real-time 3D rendering, game logic, haptic SDK integration). This also means either side can be swapped independently — e.g., replacing MediaPipe with a different pose model in the future without touching the Unity code.

### Why UDP instead of TCP for pose data?

Pose data is sent at roughly 30 frames per second between the Python server and Unity. At this rate, **low latency matters more than guaranteed delivery**. If a single pose frame is dropped, the next one arrives 33ms later — the user will never notice. TCP's reliability guarantees (retransmission, ordering, flow control) would add latency that the user *would* notice as delayed haptic feedback. In a real-time feedback system, a slightly stale but on-time response is better than a perfect but late one. This is the same reason voice-over-IP and online games use UDP.

### Why MediaPipe over OpenPose or other pose estimators?

- **Runs on-device with no GPU required.** MediaPipe's Pose Landmarker uses a lightweight model optimized for CPU inference, which means the system works on a standard laptop with a webcam — no NVIDIA GPU needed. This matters for accessibility: requiring expensive hardware defeats the purpose of making fitness accessible.
- **33 landmarks vs. OpenPose's 25.** More landmarks = better coverage of hands and feet, which matters for exercise form detection.
- **Actively maintained by Google** with regular model updates. OpenPose development has largely stalled since 2022.
- **Better Python integration.** MediaPipe's Python API is cleaner and better-documented for real-time streaming use cases.

### Why angular comparison for pose matching (not position-based)?

The system compares the **angles** between joints (e.g., elbow angle, knee angle) rather than the absolute positions of body landmarks. This was a deliberate design choice:

- **Body-size invariant.** A 5'0" user and a 6'2" user will have completely different landmark positions on screen, but their elbow angles during a bicep curl should be similar. Angular comparison removes body size as a variable.
- **Camera-distance invariant.** Users standing at different distances from the webcam will have different landmark scales. Angles don't change with distance.
- **More meaningful for exercise form.** In physical therapy and fitness coaching, form is evaluated by joint angles ("keep your knee at 90 degrees"), not by absolute body position in space.

### Why Lenient mode uses wider angular tolerance (not simpler exercises)?

I considered two approaches to accessibility: (1) simpler exercises with fewer movements, or (2) the same exercises with a wider tolerance for how closely the user needs to match. I chose option 2 because:

- **Inclusion, not reduction.** Participants at GiGi's Playhouse wanted to do the same exercises as everyone else — they didn't want a "simplified" version. The goal is to meet users where they are, not to lower the bar.
- **Motor learning research** suggests that practicing the full movement pattern, even imprecisely, builds motor skills more effectively than practicing a simplified version. Wider tolerance encourages the full movement while still providing positive reinforcement.
- **The haptic feedback itself is therapeutic.** Even when the match isn't perfect, the vest's trickle pattern provides proprioceptive input that supports body awareness — which is beneficial for users with motor coordination challenges.

### Haptic feedback design: why trickle vs. full-fire?

The two feedback modes were designed based on how the body processes tactile information:

- **Trickle pattern (in sync):** Motors activate sequentially in a flowing pattern that follows the direction of the exercise movement. This creates a proprioceptive guide — the user can "feel" which direction to move next. It's encouraging and informative.
- **Full-fire (out of sync):** All motors activate simultaneously for a brief pulse. This is an intentionally distinct sensation that signals "something needs to change" without being punitive. The contrast between trickle and full-fire makes the feedback immediately interpretable without any learning curve.

The choice to keep the out-of-sync signal short and non-specific (rather than indicating *which* body part is wrong) was deliberate — pointing out specific errors through haptics would require a much more complex haptic language that users would need to learn. The simple binary (trickle = good, pulse = adjust) keeps the system intuitive for first-time users, including users with cognitive disabilities.

## Beyond Fitness: Applications in Spaceflight & Astronaut Training

While building HapticFit for GiGi's Playhouse, I realized the core problem I was solving — **providing movement guidance through haptic feedback when normal sensory channels are restricted** — shows up in a completely different domain: human spaceflight.

### The shared problem

Individuals with motor difficulties at GiGi's Playhouse have restricted proprioception — reduced awareness of where their body is in space and how it's moving. Astronauts in EVA suits face a strikingly similar constraint. A pressurized spacesuit severely limits tactile feedback, joint mobility, and proprioceptive awareness. Astronauts performing EVA procedures often describe feeling "disconnected" from their own movements. Current training (underwater neutral buoyancy, VR simulations) can prepare astronauts for the visual and cognitive aspects of EVA, but doesn't replicate the proprioceptive loss they'll experience in-suit.

A haptic vest that provides real-time movement guidance could bridge this gap in both contexts:

| | GiGi's Playhouse Users | Astronauts in EVA Suits |
|---|---|---|
| **Core challenge** | Reduced motor control / proprioception | Restricted mobility / proprioception in pressurized suit |
| **Why visual feedback isn't enough** | Users may not be watching the screen; cognitive load of processing visual cues | Helmet restricts field of view; hands/arms often out of sight; high cognitive task load |
| **What haptic feedback adds** | Body-level guidance that doesn't require visual attention | Movement cues that work despite suit restrictions and limited FOV |
| **Adaptive tolerance** | Wider angular threshold for users with motor difficulties | Wider threshold to account for suit-induced movement constraints |

### What would change for a spaceflight application

The core architecture (pose tracking → angular comparison → haptic feedback) stays the same. The adaptations would be:

- **Pose tracking source:** Replace webcam + MediaPipe with IMU sensor data from suit-mounted accelerometers/gyroscopes. In a spacesuit, camera-based tracking is impractical — but inertial measurement units embedded in suit joints can provide the same angular data directly. The angular comparison logic in Unity doesn't need to change; only the input source does.
- **Reference movements:** Replace squat/exercise models with EVA procedure sequences — tool handling, handrail traversal, equipment installation. The reference pose data would come from motion-captured expert astronauts performing nominal procedures.
- **Haptic pattern vocabulary:** Expand beyond trickle/full-fire to include directional cues — "rotate wrist clockwise," "extend elbow further." In the accessibility context, I deliberately kept the haptic language simple (binary good/adjust). For trained astronauts with weeks of familiarization, a richer haptic vocabulary becomes viable and useful.
- **Latency requirements:** EVA operations are safety-critical. The current ~33ms UDP latency is acceptable for a fitness game but would need validation for EVA guidance where incorrect movement cues could be dangerous. A fault-tolerant mode (suppress feedback when confidence is low rather than risk a wrong cue) would be essential.
- **Microgravity considerations:** Angular comparison assumes gravity-oriented movements. In microgravity, the reference frame changes — "up" and "down" lose meaning. The comparison logic would need to shift from absolute angles to relative joint-to-joint angles independent of gravitational orientation.

### Why this matters

NASA's Johnson Space Center (Houston) has active research programs in EVA training, astronaut human factors, and wearable technologies for spaceflight. The Artemis program's return to the lunar surface will require new EVA procedures for surface operations — and new training tools to prepare astronauts for those procedures in environments with reduced proprioceptive feedback (lunar gravity = 1/6 Earth, suit pressurization further restricts it).

The intersection of haptic feedback, adaptive movement guidance, and human factors for constrained-mobility users — whether those constraints come from a disability or a spacesuit — is an underexplored area with real applications at NASA, the commercial space industry (SpaceX, Blue Origin, Axiom Space), and defense (DARPA exoskeleton programs, military EVA training).

## Tech Stack

- **Unity 6** (6000.3.8f1) — game engine, 3D rendering, game logic, UI
- **C#** — game logic, UI management, haptic control, pose comparison algorithms
- **Python 3** — webcam capture, pose processing, server-side communication
- **MediaPipe Pose Landmarker** — real-time body tracking (33 landmarks)
- **bHaptics SDK** — haptic vest pattern design and playback
- **UDP sockets** — real-time communication between Unity and Python

## How to Run

### Prerequisites

- Unity 6 (LTS)
- Python 3.x with `mediapipe` and `opencv-python`
- bHaptics Player app (for vest connectivity)
- A webcam
- A bHaptics TactSuit (x16 or x40)

### Setup

```bash
# 1. Install Python dependencies
pip install mediapipe opencv-python

# 2. Start the pose detection server
python pose_server.py

# 3. Open the Unity project and press Play
# The system will automatically connect via UDP on localhost
```

## Future Directions

### Accessibility Track
- [ ] **ML-based form correction:** Train a classifier on pose landmark sequences to detect specific form errors (e.g., "knees going past toes" vs. "back not straight") and provide targeted haptic feedback to the relevant body area
- [ ] **Adaptive difficulty:** Automatically adjust angular tolerance thresholds based on user performance over time — personalized accessibility that evolves with the user
- [ ] **LLM-powered coaching:** Upgrade the AI Coach from rule-based encouragement to contextual, personalized feedback using a language model that understands the user's history, strengths, and areas for improvement
- [ ] **User study with GiGi's Playhouse participants:** Formal comparison of Lenient vs. Strict mode with 8-12 participants to measure engagement, accuracy improvement over sessions, and subjective experience
- [ ] **Additional exercises beyond squats** — push-ups, lunges, arm raises, dance-style movements
- [ ] **Multiplayer mode** — side-by-side play for social engagement, which is especially motivating for GiGi's participants

### Spaceflight / Aerospace Track
- [ ] **IMU sensor integration:** Replace camera-based pose tracking with accelerometer/gyroscope data from body-mounted sensors, simulating how the system would work inside a spacesuit where cameras can't see the wearer's body
- [ ] **EVA procedure library:** Record and digitize reference movement sequences for common EVA tasks (tool handling, panel manipulation, handrail traversal) as target poses
- [ ] **Directional haptic vocabulary:** Expand from binary (trickle/full-fire) to directional cues — sequential motor activation patterns that indicate which direction to adjust, validated through user testing
- [ ] **Fault-tolerant feedback mode:** Suppress haptic cues when pose confidence drops below a threshold rather than risk sending incorrect guidance during safety-critical procedures
- [ ] **Microgravity angle normalization:** Adapt the angular comparison logic to work without a gravitational reference frame — use relative joint-to-joint angles instead of absolute orientation
- [ ] **Collaboration with NASA JSC Human Systems Integration:** Houston-based — explore whether this framework could integrate with existing EVA training simulators at Johnson Space Center

## Acknowledgments

- **Dr. Tony Liao** and the [CougAR Lab](https://www.ist.uh.edu/faculty/liao-tony) at the University of Houston for research mentorship and lab access
- **GiGi's Playhouse Houston** for the inspiration and the opportunity to work with an incredible community
- **bHaptics** for the TactSuit hardware and SDK

## License

MIT

## Contact

Sahana — [GitHub](https://github.com/sahanasash) · [LinkedIn](https://www.linkedin.com/in/sahana-s-663342325/)
