import os
import cv2                              # OpenCV：计算机视觉库，用于图像处理、视频捕获和分析
import mediapipe as mp                  # MediaPipe：Google的跨平台机器学习框架，用于实时人体姿态检测
import numpy as np                      # NumPy：科学计算库，用于高效处理多维数组和矩阵运算
import pandas as pd                     # Pandas：数据处理库，用于结构化数据操作和分析

mpHands = mp.solutions.hands            # MediaPipe提供的手部检测模块
hands = mpHands.Hands(max_num_hands=1)  # 最多检测1只手

"""
从输入帧中检测手部关键点并返回坐标列表

参数:
    frame: 输入的BGR格式图像（OpenCV读取的视频帧）
    imgWidth: 图像宽度（像素）
    imgHeight: 图像高度（像素）

返回:
    output_points: 包含21个关键点的x,y坐标的列表（共42个元素）
                   若未检测到手部，返回42个None的列表
"""
def get_points(frame, imgWidth, imgHeight):
    output_points = []
    #将 BGR（OpenCV 默认格式）转换为 RGB（MediaPipe 要求的格式）
    imgRGB = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    #检测图像中的手部关键点
    result = hands.process(imgRGB)

    # 如果识别到手部
    if result.multi_hand_landmarks:
        # 遍历每只检测到的手
        for handLms in result.multi_hand_landmarks:
            # 遍历21个关键点（i=0~20）
            for i, lm in enumerate(handLms.landmark):
                # MediaPipe 返回的lm.x和lm.y是归一化坐标（范围 0-1）
                # 通过乘以图像宽高（imgWidth, imgHeight）转换为像素坐标
                xPos = int(lm.x * imgWidth)
                yPos = int(lm.y * imgHeight)
                output_points.append(xPos)
                output_points.append(yPos)
    else:
        output_points = [None] * 42

    return output_points

# 图像处理
def process_image(frame):
    frameWidth = frame.shape[1]     # 获取图像宽度（第二维）
    frameHeight = frame.shape[0]    # 获取图像高度（第一维）
    return get_points(frame, frameWidth, frameHeight)

folder = './datasets'   # 数据集路径

array = []
a = 0

# os.listdir(folder)：会返回folder目录下的所有文件和子文件夹的名称列表
for subfolder in os.listdir(folder):
    # 拼接出子文件夹的完整路径
    sub = folder + '/' + subfolder
    # 记录当前子文件夹中文件的数量，用于后续计算处理进度
    subfolder_size = len(os.listdir(sub))
    code = 0

    if subfolder == 'fire':
        code = 0
    elif subfolder == 'fist':
        code = 1
    elif subfolder == 'palm':
        code =2

    i = 0
    for file in os.listdir(sub):
        frame = cv2.imread(sub + '/' + file)                        # 读取图像文件
        points = process_image(frame)                               # 调用函数处理图像并提取特征点
        a = max(a, len(points))                                     # 更新最大特征点数量
        points_gesture = np.append(points, code, axis=None)         # 将特征点与手势标签（子文件夹名称）组合
        array.append(points_gesture)                                # 添加到结果数组
        print("processing: " + subfolder)
        i += 1
        print((i / subfolder_size) * 100, '%')                      # 计算并显示处理进度百分比

# 使用 Pandas 库将处理好的手势数据转换为结构化的表格形式（DataFrame）
processed = pd.DataFrame(array)                                             # 将列表转换为 DataFrame
processed = processed.rename(columns={processed.columns[-1]:"gesture"})     # 重命名最后一列
print(processed)

# 查看是否有none值
result = processed[processed.columns[::2]].isnull().sum()
print(result)

# 按手势类别分组，并对每个特征列的缺失值进行分组均值填充
# 将原始 DataFrame 按gesture列的值分组，每个组包含同一手势的所有样本
for name, group in processed.groupby(["gesture"]):
    # 遍历当前手势组的每一列，计算该列非缺失值的均值，并将均值填充到该组内的缺失位置
    for label, content in group.items():
        av = content.loc[content.notna()].mean()                                    # 计算该列非缺失值的均值
        form = processed['gesture'] == name                                         # 选择原始DataFrame中属于当前手势类别的所有行
        processed.loc[form, label] = processed.loc[form, label].fillna(int(av))     # 用该组均值填充原始DataFrame中的缺失值
print('There are {} missing values'.format(processed.isnull().sum().sum()))

# 将 processed DataFrame 保存为 CSV 格式文件
processed.to_csv('./model/gesture-points-raw.csv', index=None)
# 从 CSV 文件中读取数据并重新赋值给 processed
processed = pd.read_csv('./model/gesture-points-raw.csv')

# 将数据帧分割成点和手势数据帧
gesture_points = processed.drop(['gesture'], axis=1)    # 把gesture列从 DataFrame 中删除，得到仅包含手部关键点坐标的特征矩阵
gesture_meaning = processed['gesture']                        # 单独提取出gesture列，作为分类任务中的标签向量

print(gesture_points)
print(gesture_meaning)