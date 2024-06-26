from abc import ABC, abstractmethod

class IAsyncMessageBroker(ABC):

    @abstractmethod
    async def send_message(self, topic: str, message: str):
        pass

    @abstractmethod
    async def receive_message(self, topic: str, group: str):
        pass

    @abstractmethod
    async def commit(self):
        pass