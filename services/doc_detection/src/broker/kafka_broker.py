import asyncio
from aiokafka import AIOKafkaProducer, AIOKafkaConsumer
from aiokafka.errors import KafkaError
from src.broker.message_broker import IAsyncMessageBroker

class KafkaBroker(IAsyncMessageBroker):
    def __init__(self, bootstrap_servers: str):
        self.bootstrap_servers = bootstrap_servers
        self.producer = None
        self.consumer = None

    async def start(self):
        self.producer = AIOKafkaProducer(bootstrap_servers=self.bootstrap_servers)
        await self.producer.start()
        self.consumer = AIOKafkaConsumer(
            'documentDetection.request',
            bootstrap_servers=self.bootstrap_servers,
            group_id='docDetectGroup'
        )
        await self.consumer.start()

    async def stop(self):
        if self.producer:
            await self.producer.stop()
        if self.consumer:
            await self.consumer.stop()

    async def send_message(self, topic: str, message: str):
        await self.producer.send_and_wait(topic, message.encode('utf-8'))

    async def receive_message(self):
        try:
            async for msg in self.consumer:
                return msg
        except KafkaError as e:
            print(f"Error in Kafka consumer: {e}")

    async def commit(self):
        await self.consumer.commit()