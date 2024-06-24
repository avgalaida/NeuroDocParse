from abc import ABC, abstractmethod

class ITextRecognitionModel(ABC):
    
    @abstractmethod
    def recognize_text(self, image, bbox):
        pass