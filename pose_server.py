import cv2
import socket
import json
import numpy as np
import urllib.request
import os

import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision

# Download model if needed
model_path = "pose_landmarker_lite.task"
if not os.path.exists(model_path):
    print("Downloading pose model...")
    url = "https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_lite/float16/latest/pose_landmarker_lite.task"
    urllib.request.urlretrieve(url, model_path)
    print("Download complete.")

base_options = python.BaseOptions(model_asset_path=model_path)
options = vision.PoseLandmarkerOptions(
    base_options=base_options,
    running_mode=vision.RunningMode.VIDEO,
    num_poses=1
)
landmarker = vision.PoseLandmarker.create_from_options(options)

# Receive frames from Unity on port 5066
recv_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
recv_sock.bind(("127.0.0.1", 5066))
recv_sock.settimeout(5.0)

# Send pose data back to Unity on port 5065
send_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

frame_timestamp = 0
print("Pose server running... Waiting for frames from Unity...")

while True:
    try:
        data, addr = recv_sock.recvfrom(65535)

        # Decode JPEG frame from Unity
        frame_array = np.frombuffer(data, dtype=np.uint8)
        frame = cv2.imdecode(frame_array, cv2.IMREAD_COLOR)

        if frame is None:
            continue

        frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=frame_rgb)

        frame_timestamp += 33
        results = landmarker.detect_for_video(mp_image, frame_timestamp)

        if results.pose_landmarks and len(results.pose_landmarks) > 0:
            landmarks = {}
            for idx, landmark in enumerate(results.pose_landmarks[0]):
                landmarks[idx] = {
                    "x": landmark.x,
                    "y": landmark.y,
                    "z": landmark.z,
                    "v": landmark.visibility
                }

            pose_data = json.dumps(landmarks)
            send_sock.sendto(pose_data.encode(), ("127.0.0.1", 5065))
            print(f"Sent pose data with {len(landmarks)} landmarks")
    except socket.timeout:
        continue
    except KeyboardInterrupt:
        break
    except Exception as e:
        print(f"Error: {e}")
        continue

recv_sock.close()
send_sock.close()