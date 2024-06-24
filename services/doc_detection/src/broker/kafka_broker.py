from confluent_kafka import Producer, Consumer, KafkaError
import json
from src.broker.message_broker import IMessageBroker

class KafkaBroker(IMessageBroker):
    def __init__(self, bootstrap_servers):
        self.bootstrap_servers = bootstrap_servers
        self.producer = Producer({'bootstrap.servers': self.bootstrap_servers})
        self.consumer = None

    def send_message(self, topic, message):
        self.producer.produce(topic, message)
        self.producer.flush()

    def receive_message(self, topic, group_id):
        if self.consumer is None:
            self.consumer = Consumer({
                'bootstrap.servers': self.bootstrap_servers,
                'group.id': group_id,
                'auto.offset.reset': 'earliest',
                'enable.auto.commit': True  
            })
            self.consumer.subscribe([topic])

        while True:
            msg = self.consumer.poll(1.0)
            if msg is None:
                continue
            if msg.error():
                if msg.error().code() == KafkaError._PARTITION_EOF:
                    continue
                else:
                    print(msg.error())
                    break

            return msg

    def commit(self):
        if self.consumer:
            self.consumer.commit()