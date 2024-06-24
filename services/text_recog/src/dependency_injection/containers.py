from dependency_injector import containers, providers
from src.recognition.easy_ocr import EasyOCRModel
from src.broker.kafka_broker import KafkaBroker
from src.storage.minio_client import MinioClient

class Container(containers.DeclarativeContainer):
    config = providers.Configuration()

    broker = providers.Singleton(
        KafkaBroker,
        bootstrap_servers=config.kafka.bootstrap_servers
    )

    storage = providers.Singleton(
        MinioClient,
        endpoint=config.minio.endpoint,
        access_key=config.minio.access_key,
        secret_key=config.minio.secret_key
    )

    model = providers.Singleton(EasyOCRModel)