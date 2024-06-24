import easyocr
import cv2
from src.preprocessing.otsu_binarization import apply_otsu_binarization
from src.recognition.model_interface import ITextRecognitionModel

class EasyOCRModel(ITextRecognitionModel):
    def __init__(self):
        self.reader = easyocr.Reader(['ru'])
    
    def recognize_text(self, image, bbox):
        x1, y1, x2, y2 = map(int, bbox)
        crop = image[y1:y2, x1:x2]

        otsu_binary = apply_otsu_binarization(crop)

        result = self.reader.readtext(otsu_binary, detail=0, paragraph=True)
        recognized_text = " ".join(result)
        return recognized_text