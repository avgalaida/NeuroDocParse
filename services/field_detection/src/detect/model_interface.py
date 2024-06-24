from abc import ABC, abstractmethod

class IModel(ABC):
    @abstractmethod
    def predict(self, image_path):
        pass
    @abstractmethod
    def set_model(self, model_path: str):
        pass