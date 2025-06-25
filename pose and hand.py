import cv2
import mediapipe as mp
import socket
import json

# === 平滑参数 ===
SMOOTHING_FACTOR = 0.5  # 越接近1越平稳（但响应越慢）

# 上一帧点缓冲
prev_hand_points = {}  # key: (type, id) → {"x": float, "y": float, "z": float}
prev_pose_points = {}  # key: id → {"x": float, "y": float, "z": float, "v": float}

def smooth_point(prev, current, alpha=SMOOTHING_FACTOR):
    return alpha * prev + (1 - alpha) * current

# === 初始化 MediaPipe ===
mp_drawing = mp.solutions.drawing_utils
mp_hands = mp.solutions.hands
mp_pose = mp.solutions.pose
draw_enable = True

hands = mp_hands.Hands(
    max_num_hands=2,
    min_detection_confidence=0.7,
    min_tracking_confidence=0.5
)

pose = mp_pose.Pose(
    static_image_mode=False,
    model_complexity=1,
    enable_segmentation=False,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

# === 设置 Socket ===
host = "127.0.0.1"
port = 5052
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# === 摄像头设置 ===
cap = cv2.VideoCapture(0)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 320)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 240)

while cap.isOpened():
    success, image = cap.read()
    if not success:
        continue

    image = cv2.cvtColor(cv2.flip(image, 1), cv2.COLOR_BGR2RGB)
    image.flags.writeable = False

    hand_results = hands.process(image)
    pose_results = pose.process(image)

    image.flags.writeable = True

    # === 构造数据结构 ===
    data = {
        "hands": [],
        "handCount": 0,
        "poseLandmarks": []
    }

    # === 处理手部 ===
    if hand_results.multi_hand_landmarks:
        data["handCount"] = len(hand_results.multi_hand_landmarks)
        for hand_idx, hand_landmarks in enumerate(hand_results.multi_hand_landmarks):
            handedness = hand_results.multi_handedness[hand_idx]
            hand_type = handedness.classification[0].label  # "Left" or "Right"

            hand = {
                "type": hand_type,
                "landmarks": []
            }

            for idx, lm in enumerate(hand_landmarks.landmark):
                key = (hand_type, idx)
                if key in prev_hand_points:
                    prev = prev_hand_points[key]
                    x = smooth_point(prev["x"], lm.x)
                    y = smooth_point(prev["y"], lm.y)
                    z = smooth_point(prev["z"], lm.z)
                else:
                    x, y, z = lm.x, lm.y, lm.z

                prev_hand_points[key] = {"x": x, "y": y, "z": z}
                hand["landmarks"].append({
                    "x": x,
                    "y": y,
                    "z": z,
                    "id": idx
                })

            data["hands"].append(hand)

            if draw_enable:
                mp_drawing.draw_landmarks(
                    image,
                    hand_landmarks,
                    mp_hands.HAND_CONNECTIONS,
                    mp_drawing.DrawingSpec(color=(121, 22, 76), thickness=2),
                    mp_drawing.DrawingSpec(color=(250, 44, 250), thickness=2)
                )

    # === 处理姿态点 ===
    if pose_results.pose_landmarks:
        for idx, lm in enumerate(pose_results.pose_landmarks.landmark):
            if idx in prev_pose_points:
                prev = prev_pose_points[idx]
                x = smooth_point(prev["x"], lm.x)
                y = smooth_point(prev["y"], lm.y)
                z = smooth_point(prev["z"], lm.z)
                v = smooth_point(prev["v"], lm.visibility)
            else:
                x, y, z, v = lm.x, lm.y, lm.z, lm.visibility

            prev_pose_points[idx] = {"x": x, "y": y, "z": z, "v": v}

            data["poseLandmarks"].append({
                "x": x,
                "y": y,
                "z": z,
                "visibility": v,
                "id": idx
            })

        if draw_enable:
            mp_drawing.draw_landmarks(
                image,
                pose_results.pose_landmarks,
                mp_pose.POSE_CONNECTIONS,
                mp_drawing.DrawingSpec(color=(0, 255, 0), thickness=2),
                mp_drawing.DrawingSpec(color=(0, 0, 255), thickness=2)
            )

    # === 发送数据 ===
    json_data = json.dumps(data)
    print("Sending data:", json.dumps(data, indent=2))
    sock.sendto(json_data.encode(), (host, port))

    # 显示图像
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
    cv2.imshow('Hand + Pose Tracking (Smoothed)', image)
    if cv2.waitKey(5) & 0xFF == 27:
        break

# 清理资源
cap.release()
cv2.destroyAllWindows()
hands.close()
pose.close()
