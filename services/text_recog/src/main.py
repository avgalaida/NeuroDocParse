import asyncio
import json
import os
import cv2
from dependency_injector.wiring import inject, Provide
from src.broker.message_broker import IMessageBroker
from src.storage.minio_client import IStorageClient
from src.recognition.model_interface import ITextRecognitionModel
from src.dependency_injection.containers import Container
from src.doc_config.fields_conf import DOCUMENT_FIELD_CONFIGS, FieldType, FieldTransformations

@inject
async def process_detection_request(
    message,
    broker: IMessageBroker = Provide[Container.broker],
    storage: IStorageClient = Provide[Container.storage],
    model: ITextRecognitionModel = Provide[Container.model]
):
    image_info = json.loads(message.value().decode('utf-8'))
    bucket_name = image_info['BucketName']
    object_name = image_info['ObjectName']
    document_name = image_info['DocumentName']
    fields = image_info['Fields']
    file_path = f"/tmp/{object_name}"

    await storage.download_file(bucket_name, object_name, file_path)

    # Load the image
    image = cv2.imread(file_path)

    recognized_fields = {}
    field_config = DOCUMENT_FIELD_CONFIGS.get(document_name, {})

    for field_name, bboxes in fields.items():
        recognized_fields[field_name] = []
        transformations = field_config.get(field_name, FieldTransformations(field_type=FieldType.TEXT))
        
        for bbox in bboxes:
            recognized_text = model.recognize_text(image, bbox, transformations)
            recognized_fields[field_name].append(recognized_text)

    result_message = {
        'RequestId': image_info['RequestId'],
        'BucketName': image_info['BucketName'],
        'ObjectName': image_info['ObjectName'],
        'DocumentName': image_info['DocumentName'],
        'Fields': recognized_fields,
    }

    serialized_message = json.dumps(result_message)
    print(f"Serialized message: {serialized_message}")

    broker.send_message('textRecognition.result', serialized_message)
    broker.commit()

    if os.path.exists(file_path):
        os.remove(file_path)

@inject
async def main(broker: IMessageBroker = Provide[Container.broker]):
    while True:
        msg = broker.receive_message('textRecognition.request', 'textRecognitionGroup')
        if msg:
            await process_detection_request(msg)

if __name__ == "__main__":
    container = Container()
    container.init_resources()
    container.config.from_dict({
        'kafka': {
            'bootstrap_servers': os.getenv('KAFKA_BROKER', 'localhost:9092')
        },
        'minio': {
            'endpoint': os.getenv('MINIO_ENDPOINT', 'http://minio:9000'),
            'access_key': os.getenv('MINIO_ACCESS_KEY', 'admin'),
            'secret_key': os.getenv('MINIO_SECRET_KEY', 'Secure123$')
        }
    })
    container.wire(modules=[__name__])

    asyncio.run(main())