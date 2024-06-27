# src/preprocessing/transformations.py

import cv2

def apply_otsu_binarization(image):
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    _, otsu_binary = cv2.threshold(gray, 0, 100, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    return otsu_binary

def apply_rotation(image, angle):
    if angle == 90:
        return cv2.rotate(image, cv2.ROTATE_90_CLOCKWISE)
    elif angle == -90:
        return cv2.rotate(image, cv2.ROTATE_90_COUNTERCLOCKWISE)
    elif angle == 180:
        return cv2.rotate(image, cv2.ROTATE_180)
    elif angle == 270:
        return cv2.rotate(image, cv2.ROTATE_90_COUNTERCLOCKWISE)
    else:
        return image