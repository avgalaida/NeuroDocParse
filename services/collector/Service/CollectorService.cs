using System.Text.Json;
using collector.Broker;
using collector.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace collector.Service
{
    public class CollectorService : IHostedService
    {
        private readonly ILogger<CollectorService> _logger;
        private readonly IMessageBroker _messageBroker;

        public CollectorService(ILogger<CollectorService> logger, IMessageBroker messageBroker)
        {
            _logger = logger;
            _messageBroker = messageBroker;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Collector Service started.");
            Task.Run(() => ProcessMessagesAsync(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Collector Service stopped.");
            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _messageBroker.ReceiveMessageAsync("extractData.request", "collectorGroup", cancellationToken);
                if (message != null)
                {
                    _logger.LogInformation($"Received message: {message}");

                    // Парсинг входящего сообщения
                    var incomingMessage = JsonSerializer.Deserialize<IncomingMessage>(message);

                    if (incomingMessage.RequestType == "triple" || incomingMessage.RequestType == "edited")
                    {
                        await ProcessTripleRequestAsync(incomingMessage, cancellationToken);
                    }
                    else
                    {
                        await _messageBroker.SendMessageAsync("extractData.result", "Unknown Request Type");
                    }

                    // Логгирование успешной обработки сообщения
                    _logger.LogInformation("Message successfully processed and committed.");
                }
            }
        }

        private async Task ProcessTripleRequestAsync(IncomingMessage incomingMessage, CancellationToken cancellationToken)
        {
            // Шаг 1: Обработка documentDetection.request
            var docDetectRes = await ProcessDocDetectRequestAsync(incomingMessage, cancellationToken);

            // Шаг 2: Обработка fieldsDetection.request
            var fieldsDetectRes = await ProcessFieldsDetectRequestAsync(docDetectRes, cancellationToken);

            // Шаг 3: Обработка textRecognition.request
            var textRecognitionRes = await ProcessTextRecognitionRequestAsync(fieldsDetectRes, cancellationToken);

            // Финальный шаг: отправка результата
            await _messageBroker.SendMessageAsync("extractData.result", textRecognitionRes);
        }

        private async Task<DocumentDetectionResult> ProcessDocDetectRequestAsync(IncomingMessage incomingMessage, CancellationToken cancellationToken)
        {
            var docDetectRequest = new DocumentDetectionRequest
            {
                ClientId = incomingMessage.ClientId,
                BucketName = incomingMessage.BucketName,
                ObjectName = incomingMessage.ObjectName,
                Model = incomingMessage.Model
            };

            var docDetectRequestMessage = JsonSerializer.Serialize(docDetectRequest);
            await _messageBroker.SendMessageAsync("documentDetection.request", docDetectRequestMessage);

            _logger.LogInformation("Sent message to documentDetection.request");

            var documentResult = await _messageBroker.ReceiveMessageAsync("documentDetection.result", "collectorDocDetectGroup", cancellationToken);
            _logger.LogInformation($"Document detection result: {documentResult}");

            return JsonSerializer.Deserialize<DocumentDetectionResult>(documentResult);
        }

        private async Task<string> ProcessFieldsDetectRequestAsync(DocumentDetectionResult docDetectRes, CancellationToken cancellationToken)
        {
            var fieldsDetectRequestMessage = JsonSerializer.Serialize(docDetectRes);
            await _messageBroker.SendMessageAsync("fieldsDetection.request", fieldsDetectRequestMessage);

            _logger.LogInformation("Sent message to fieldsDetection.request");

            var fieldsResult = await _messageBroker.ReceiveMessageAsync("fieldsDetection.result", "collectorFieldsDetectGroup", cancellationToken);
            _logger.LogInformation($"Fields detection result: {fieldsResult}");

            return fieldsResult;
        }

        private async Task<string> ProcessTextRecognitionRequestAsync(string fieldsDetectRes, CancellationToken cancellationToken)
        {
            var textRecognitionRequestMessage = fieldsDetectRes;
            await _messageBroker.SendMessageAsync("textRecognition.request", textRecognitionRequestMessage);

            _logger.LogInformation("Sent message to textRecognition.request");

            var textRecognitionResult = await _messageBroker.ReceiveMessageAsync("textRecognition.result", "collectorTextRecognitionGroup", cancellationToken);
            _logger.LogInformation($"Text recognition result: {textRecognitionResult}");

            return textRecognitionResult;
        }
    }
}