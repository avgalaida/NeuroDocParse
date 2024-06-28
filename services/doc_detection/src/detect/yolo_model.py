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
        
        # Находим самый вероятный класс документа
        if result.names and result.boxes:
            confidences = result.boxes.conf.numpy()
            max_conf_idx = confidences.argmax()
            
            document_name = result.names[result.boxes.cls[max_conf_idx].item()]
            xywh = result.boxes.xywh[max_conf_idx].tolist()
            xyxy = result.boxes.xyxy[max_conf_idx].tolist()
        else:
            document_name = None
            xywh = None
            xyxy = None
        
        return {
            'document_name': document_name,
            'xywh': xywh,
            'xyxy': xyxy
        }

    def set_model(self, model_path: str):
        self.model_path = model_path
        self.model = YOLO(model_path)