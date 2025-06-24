import cv2
import mediapipe as mp

# 初始化MediaPipe模块
mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose
mp_hands = mp.solutions.hands

# 打开摄像头
cap = cv2.VideoCapture(0)

# 创建姿势和手部检测实例
with mp_pose.Pose(
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5
) as pose, mp_hands.Hands(
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5,
    max_num_hands=2  # 最多检测2只手
) as hands:
    while cap.isOpened():
        success, image = cap.read()
        if not success:
            break

        # 转换为RGB格式
        image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

        # 姿势检测
        pose_results = pose.process(image_rgb)

        # 手部检测
        hands_results = hands.process(image_rgb)

        # 绘制姿势关键点
        if pose_results.pose_landmarks:
            mp_drawing.draw_landmarks(
                image,
                pose_results.pose_landmarks,
                mp_pose.POSE_CONNECTIONS,
                mp_drawing.DrawingSpec(color=(0, 255, 0), thickness=2),  # 绿色关键点
                mp_drawing.DrawingSpec(color=(255, 0, 0), thickness=2)  # 蓝色连接线
            )

        # 绘制手部关键点
        if hands_results.multi_hand_landmarks:
            for hand_landmarks in hands_results.multi_hand_landmarks:
                mp_drawing.draw_landmarks(
                    image,
                    hand_landmarks,
                    mp_hands.HAND_CONNECTIONS,
                    mp_drawing.DrawingSpec(color=(255, 0, 255), thickness=2),  # 粉色关键点
                    mp_drawing.DrawingSpec(color=(0, 255, 255), thickness=2)  # 黄色连接线
                )

        # 显示结果
        cv2.imshow('Pose + Hand Tracking', image)
        if cv2.waitKey(5) & 0xFF == ord('q'):
            break

cap.release()
cv2.destroyAllWindows()