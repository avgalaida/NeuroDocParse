import easyocr
from src.preprocessing.transformations import apply_otsu_binarization, apply_rotation
from src.recognition.model_interface import ITextRecognitionModel
from src.doc_config.fields_conf import FieldType, DOCUMENT_FIELD_CONFIGS

class EasyOCRModel(ITextRecognitionModel):
    def __init__(self, model_dir='src/models'):
        self.reader = easyocr.Reader(['ru'], model_storage_directory=model_dir, download_enabled=False)
    
    def recognize_text(self, image, bbox, transformations):
        x1, y1, x2, y2 = map(int, bbox)
        crop = image[y1:y2, x1:x2]

        if transformations.rotate:
            crop = apply_rotation(crop, transformations.rotate)
        if transformations.binarize:
            crop = apply_otsu_binarization(crop)

        result = self.reader.readtext(crop, detail=0, paragraph=True)
        recognized_text = " ".join(result)
        return recognized_text