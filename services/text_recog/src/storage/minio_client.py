import aiofiles
from aiobotocore.session import get_session
from async_generator import asynccontextmanager
from src.storage.storage_client import IStorageClient

class MinioClient(IStorageClient):
    def __init__(self, endpoint, access_key, secret_key):
        self.endpoint = endpoint
        self.access_key = access_key
        self.secret_key = secret_key

    @asynccontextmanager
    async def get_client(self):
        session = get_session()
        async with session.create_client(
            's3',
            region_name='us-east-1',  # or your specific region
            endpoint_url=self.endpoint,
            aws_secret_access_key=self.secret_key,
            aws_access_key_id=self.access_key,
        ) as client:
            yield client

    async def upload_file(self, file_path, bucket_name, object_name):
        async with self.get_client() as client:
            async with aiofiles.open(file_path, 'rb') as file_data:
                await client.put_object(Bucket=bucket_name, Key=object_name, Body=await file_data.read())

    async def download_file(self, bucket_name, object_name, file_path):
        async with self.get_client() as client:
            response = await client.get_object(Bucket=bucket_name, Key=object_name)
            async with aiofiles.open(file_path, 'wb') as file_data:
                async for chunk in response['Body'].iter_chunks():
                    await file_data.write(chunk)