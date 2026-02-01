# HapticFit

A motion-tracked fitness game built in Unity that combines real-time pose detection with haptic feedback to create an interactive, guided workout experience — inspired by Just Dance.

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

## Why I Built This

This project started from my time volunteering at the GiGi's Playhouse fitness program, where I saw how movement-based activities can make a meaningful difference for individuals with Down syndrome. The Lenient mode was designed specifically with GiGi's participants in mind — it provides a wider tolerance for movement so the experience stays encouraging and accessible for those with motor difficulties.

Developed under the guidance of Dr. Tony Liao at the Cougar Lab, University of Houston.

## Tech Stack

- **Unity 6** (6000.3.8f1) — game engine
- **C#** — game logic, UI, haptic control, pose comparison
- **Python 3** — webcam capture, pose processing, vest communication
- **MediaPipe Pose Landmarker** — real-time body tracking (33 landmarks)
- **bHaptics SDK** — haptic vest pattern design and playback
- **UDP sockets** — real-time communication between Unity and Python

## Architecture

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

## How to Run

### Prerequisites
- Unity 6 (LTS)
- Python 3.x with `mediapipe` and `opencv-python`
- bHaptics Player app (for vest connectivity)
- A webcam

### Setup
1. Clone this repository
2. Open the project in Unity
3. Install Python dependencies:
   ```
   pip3 install mediapipe opencv-python
   ```
4. Start the pose server:
   ```
   python3 pose_server.py
   ```
5. Hit Play in Unity

### With bHaptics Vest
- Connect the vest via the bHaptics Player app
- Ensure your device and vest are on the same network
- The vest will respond automatically when the game runs

## Current Status

This is an MVP prototype proving the core concept. Current exercise support is limited to squats, with the architecture designed to scale to additional exercises.

## Future Plans

- Expanded exercise library (lunges, arm raises, jumping jacks, and more)
- Full fitness evaluation mode for structured assessments
- Session history and progress tracking across multiple workouts
- Additional bHaptics patterns for different exercise types
- Music and audio integration

## Acknowledgments

- **Dr. Tony Liao** and the **Cougar Lab** at the University of Houston
- **GiGi's Playhouse** for inspiring this project and providing guidance on accessibility
- **bHaptics** for the TactSuit platform and developer tools
- **Mixamo** by Adobe for character animations