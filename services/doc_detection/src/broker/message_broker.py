from abc import ABC, abstractmethod

class IMessageBroker(ABC):
    
    @abstractmethod
    def send_message(self, topic, message):
        pass

    @abstractmethod
    def receive_message(self, topic, group_id):
        pass

    @abstractmethod
    def commit(self):
        pass