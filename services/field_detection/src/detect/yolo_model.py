from src.detect.model_interface import IModel
from ultralytics import YOLO

class YOLOModel(IModel):
    def __init__(self, model_path='src/models/default.pt'):
        self.model_path = model_path
        self.model = YOLO(model_path)
        
    def set_model(self, model_path):
        # Initialize your YOLO model here
        self.model = YOLO(model_path)

    def predict(self, image_path):
        results = self.model(image_path)
        if not results:
            return None
        result = results[0]

        # Convert detection results to a dictionary with field names and corresponding boxes
        detection_fields = {}
        for box in result.boxes:
            class_name = result.names[int(box.cls[0])]
            if class_name not in detection_fields:
                detection_fields[class_name] = []
            detection_fields[class_name].append((box.conf[0], box.xyxy.tolist()))

        # Keep only the most probable bbox for each class
        most_probable_fields = {class_name: max(bboxes, key=lambda x: x[0])[1] for class_name, bboxes in detection_fields.items()}
        
        return {
            'Fields': most_probable_fields,
        }