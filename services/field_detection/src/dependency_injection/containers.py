from dependency_injector import containers, providers
from src.broker.kafka_broker import KafkaBroker
from src.broker.message_broker import IMessageBroker
from src.storage.minio_client import MinioClient, IStorageClient
from src.detect.yolo_model import YOLOModel, IModel
import os

class Container(containers.DeclarativeContainer):
    wiring_config = containers.WiringConfiguration(modules=["src.main"])

    broker = providers.Factory(
        KafkaBroker,
        bootstrap_servers=os.getenv('KAFKA_BROKER', 'localhost:9092')
    )

    storage = providers.Singleton(
        MinioClient,
        endpoint=os.getenv('MINIO_ENDPOINT', 'localhost:9000'),
        access_key=os.getenv('MINIO_ACCESS_KEY', 'admin'),
        secret_key=os.getenv('MINIO_SECRET_KEY', 'Secure123$')
    )

    model = providers.Singleton(
        YOLOModel,
        model_path='src/models/passport.pt'
    )