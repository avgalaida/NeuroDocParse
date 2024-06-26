import asyncio
import json
import os
from dependency_injector.wiring import inject, Provide
from src.broker.kafka_broker import KafkaBroker
from src.storage.minio_client import IStorageClient
from src.detect.yolo_model import IModel
from src.dependency_injection.containers import Container
from src.proccessing.crop import CropImage
from src.proccessing.rotate import rotate_image

@inject
async def process_detection_request(
    message,
    broker: KafkaBroker = Provide[Container.broker],
    storage: IStorageClient = Provide[Container.storage],
    model: IModel = Provide[Container.model]
):
    image_info = json.loads(message.decode('utf-8'))
    bucket_name = image_info['BucketName']
    object_name = image_info['ObjectName']
    model_name = image_info['Model']
    file_path = f"/tmp/{object_name}"
    rotated_file_path = f"/tmp/rotated_{object_name}"

    await storage.download_file(bucket_name, object_name, file_path)

    # Поворот изображения в отдельном потоке
    loop = asyncio.get_event_loop()
    await loop.run_in_executor(None, rotate_image, file_path, rotated_file_path)

    if not os.path.exists(rotated_file_path):
        rotated_file_path = file_path

    model.set_model(f"src/models/{model_name}.pt")

    # Предсказание модели в отдельном потоке
    detection_result = await loop.run_in_executor(None, model.predict, rotated_file_path)

    cropped_img_path = await loop.run_in_executor(None, CropImage, rotated_file_path, detection_result['xyxy'])

    await storage.upload_file(cropped_img_path, "croped", "croped_"+object_name)

    result_message = {
        'ClientId': image_info['ClientId'],
        'BucketName': "croped",
        'ObjectName': "croped_"+object_name,
        'DocumentName': detection_result['document_name'],
    }

    serialized_message = json.dumps(result_message)

    await broker.send_message('documentDetection.result', serialized_message)
    await broker.commit()

    print(f"Serialized message: {serialized_message}")

    if os.path.exists(file_path):
        os.remove(file_path)
    if os.path.exists(rotated_file_path):
        os.remove(rotated_file_path)
    if os.path.exists(cropped_img_path):
        os.remove(cropped_img_path)

@inject
async def main(broker: KafkaBroker = Provide[Container.broker]):
    await broker.start()
    try:
        while True:
            msg = await broker.receive_message()
            if msg:
                await process_detection_request(msg.value)
    finally:
        await broker.stop()

if __name__ == "__main__":
    container = Container()
    container.init_resources()
    container.wire(modules=[__name__])

    asyncio.run(main())