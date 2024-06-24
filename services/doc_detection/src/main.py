import asyncio
from dependency_injector.wiring import inject, Provide
from src.broker.message_broker import IMessageBroker
from src.storage.minio_client import IStorageClient
from src.detect.yolo_model import IModel
from src.dependency_injection.containers import Container
from src.postprocces.crop import CropImage
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
    model_name = image_info['Model']
    file_path = f"/tmp/{object_name}"

    await storage.download_file(bucket_name, object_name, file_path)

    model.set_model(f"src/models/{model_name}.pt")
    detection_result = model.predict(file_path)

    cropped_img_path = CropImage(file_path, detection_result['xyxy'])

    await storage.upload_file(cropped_img_path, "croped", "croped_"+object_name)

    result_message = {
        'ClientId': image_info['ClientId'],
        'BucketName': "croped",
        'ObjectName': "croped_"+object_name,
        'DocumentName': detection_result['document_name'],
    }

    serialized_message = json.dumps(result_message)
    print(f"Serialized message: {serialized_message}")

    broker.send_message('documentDetection.result', serialized_message)
    broker.commit()

    if os.path.exists(file_path):
        os.remove(file_path)

    if os.path.exists(cropped_img_path):
        os.remove(cropped_img_path)

@inject
async def main(broker: IMessageBroker = Provide[Container.broker]):
    while True:
        msg = broker.receive_message('documentDetection.request', 'docDetectGroup')
        if msg:
            await process_detection_request(msg)

if __name__ == "__main__":
    container = Container()
    container.init_resources()
    container.wire(modules=[__name__])

    asyncio.run(main())