import cv2
import mediapipe as mp
import socket
import json

# 初始化MediaPipe手部检测
mp_drawing = mp.solutions.drawing_utils
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(
    max_num_hands=2,
    min_detection_confidence=0.7,
    min_tracking_confidence=0.5)

# 设置Socket通信
host = "127.0.0.1"
port = 5052
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# 打开摄像头
cap = cv2.VideoCapture(0)

while cap.isOpened():
    success, image = cap.read()
    if not success:
        continue

    # 转换颜色空间并处理
    image = cv2.cvtColor(cv2.flip(image, 1), cv2.COLOR_BGR2RGB)
    image.flags.writeable = False
    results = hands.process(image)
    image.flags.writeable = True

    hand_data = {
        "handCount": 0,
        "hands": []
    }

    if results.multi_hand_landmarks:
        hand_data["handCount"] = len(results.multi_hand_landmarks)

        # 获取左右手信息
        for hand_idx, hand_landmarks in enumerate(results.multi_hand_landmarks):
            # 绘制关键点和连接线
            mp_drawing.draw_landmarks(
                image,
                hand_landmarks,
                mp_hands.HAND_CONNECTIONS,
                mp_drawing.DrawingSpec(color=(121, 22, 76)))  # 连接线颜色 (BGR)

            # 关键：获取手部类型（左/右）
            handedness = results.multi_handedness[hand_idx]
            hand_type = handedness.classification[0].label  # "Left" or "Right"

            hand = {
                "type": hand_type,  # 新增手部类型字段
                "landmarks": []
            }

            for idx, landmark in enumerate(hand_landmarks.landmark):
                hand["landmarks"].append({
                    "x": float(landmark.x),
                    "y": float(landmark.y),
                    "z": float(landmark.z),
                    "id": idx
                })

            hand_data["hands"].append(hand)

    # 发送JSON格式数据到Unity
    print("Sending data:", json.dumps(hand_data, indent=2))
    sock.sendto(json.dumps(hand_data).encode(), (host, port))

    # 显示图像（可选）
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
    cv2.imshow('Hand Tracking', image)
    if cv2.waitKey(5) & 0xFF == 27:
        break

cap.release()
cv2.destroyAllWindows()
hands.close()