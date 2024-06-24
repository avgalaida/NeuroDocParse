import asyncio
from dependency_injector.wiring import inject, Provide
from src.broker.message_broker import IMessageBroker
from src.storage.minio_client import IStorageClient
from src.detect.yolo_model import IModel
from src.dependency_injection.containers import Container
import json
import os

@inject
async def process_detection_request(
    message,
    broker: IMessageBroker = Provide[Container.broker],
    storage: IStorageClient = Provide[Container.storage],
    model: IModel = Provide[Container.model]
):
    image_info = json.loads(message.value().decode('utf-8'))
    bucket_name = image_info['BucketName']
    object_name = image_info['ObjectName']
    document_name = image_info['DocumentName']
    file_path = f"/tmp/{object_name}"

    await storage.download_file(bucket_name, object_name, file_path)

    model.set_model(f"src/models/{document_name}.pt")

    detection_result = model.predict(file_path)

    result_message = {
        'ClientId': image_info['ClientId'],
        'BucketName': image_info['BucketName'],
        'ObjectName': image_info['ObjectName'],
        'DocumentName': image_info['DocumentName'],
        'Fields': detection_result['Fields'],
    }

    serialized_message = json.dumps(result_message)
    print(f"Serialized message: {serialized_message}")

    broker.send_message('fieldsDetection.result', serialized_message)
    broker.commit()

    if os.path.exists(file_path):
        os.remove(file_path)

@inject
async def main(broker: IMessageBroker = Provide[Container.broker]):
    while True:
        msg = broker.receive_message('fieldsDetection.request', 'fieldDetectGroup')
        if msg:
            await process_detection_request(msg)

if __name__ == "__main__":
    container = Container()
    container.init_resources()
    container.wire(modules=[__name__])

    asyncio.run(main())