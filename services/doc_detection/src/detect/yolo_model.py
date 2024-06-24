from src.detect.model_interface import IModel
from ultralytics import YOLO

class YOLOModel(IModel):
    def __init__(self, model_path='src/models/default.pt'):
        self.model_path = model_path
        self.model = YOLO(model_path)

    def predict(self, image_path):
        results = self.model(image_path)
        if not results:
            return None
        result = results[0]
        document_name = list(result.names.values())[0] if result.names else None
        xywh = result.boxes.xywh[0].tolist() if result.boxes else None
        xyxy = result.boxes.xyxy[0].tolist() if result.boxes else None
        return {
            'document_name': document_name,
            'xywh': xywh,
            'xyxy': xyxy
        }

    def set_model(self, model_path: str):
        self.model_path = model_path
        self.model = YOLO(model_path)