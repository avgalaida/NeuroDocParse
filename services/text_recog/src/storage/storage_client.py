from abc import ABC, abstractmethod

class IStorageClient(ABC):
    
    @abstractmethod
    async def upload_file(self, file_path, bucket_name, object_name):
        pass

    @abstractmethod
    async def download_file(self, bucket_name, object_name, file_path):
        pass
