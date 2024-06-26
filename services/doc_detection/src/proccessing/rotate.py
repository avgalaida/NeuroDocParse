import cv2
import numpy as np

def rotate_image(image_path, output_path):
    # Загрузить изображение
    image = cv2.imread(image_path)

    # Преобразовать в оттенки серого
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # Применить GaussianBlur для уменьшения шума и улучшения обнаружения краев
    blurred = cv2.GaussianBlur(gray, (5, 5), 0)

    # Обнаружение краев
    edged = cv2.Canny(blurred, 50, 150)

    # Найти контуры
    contours, _ = cv2.findContours(edged, cv2.RETR_LIST, cv2.CHAIN_APPROX_SIMPLE)

    # Отфильтровать контуры, которые не являются документом
    contours = sorted(contours, key=cv2.contourArea, reverse=True)[:5]
    document_contour = None

    for contour in contours:
        # Аппроксимация контура
        peri = cv2.arcLength(contour, True)
        approx = cv2.approxPolyDP(contour, 0.02 * peri, True)

        # Ищем контур с четырьмя точками (прямоугольник)
        if len(approx) == 4:
            document_contour = approx
            break

    if document_contour is None:
        print("Контур с четырьмя точками не найден, поиск самой длинной линии...")

        # Используем Hough Line Transform для поиска длинных линий
        lines = cv2.HoughLinesP(edged, 1, np.pi / 180, threshold=100, minLineLength=50, maxLineGap=10)
        if lines is not None:
            longest_line = None
            max_length = 0

            for line in lines:
                x1, y1, x2, y2 = line[0]
                length = np.sqrt((x2 - x1) ** 2 + (y2 - y1) ** 2)
                if length > max_length:
                    max_length = length
                    longest_line = line

            if longest_line is not None:
                x1, y1, x2, y2 = longest_line[0]
                angle = np.degrees(np.arctan2(y2 - y1, x2 - x1))
                print(f"Угол поворота на основе самой длинной линии: {angle}")
                if angle < 100 and angle > 80: 
                    return
            else:
                print("Не удалось найти достаточную длину линии для определения угла.")
                return
        else:
            print("Линии не найдены!")
            return
    else:
        # Вычислить прямоугольник минимальной площади для контура документа
        rect = cv2.minAreaRect(document_contour)
        box = cv2.boxPoints(rect)
        box = np.int0(box)

        # Получить угол поворота
        angle = rect[-1]

        if angle < -45:
            angle = 90 + angle
        elif angle < 100 and angle > 80:
            angle = 0
        else:
            angle = angle

    # Повернуть изображение для коррекции ориентации
    (h, w) = image.shape[:2]
    center = (w // 2, h // 2)
    M = cv2.getRotationMatrix2D(center, angle, 1.0)
    rotated = cv2.warpAffine(image, M, (w, h), flags=cv2.INTER_CUBIC, borderMode=cv2.BORDER_REPLICATE)

    # Сохранить повернутое изображение
    cv2.imwrite(output_path, rotated)
    print(f"Повернутое изображение сохранено в {output_path}")