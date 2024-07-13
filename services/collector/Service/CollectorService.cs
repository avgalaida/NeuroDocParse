using System.Text.Json;
using collector.Broker;
using collector.Domain;
using collector.Repository;

namespace collector.Service
{
    public class CollectorService : IHostedService
    {
        private readonly ILogger<CollectorService> _logger;
        private readonly IMessageBroker _messageBroker;
        private readonly IDataBase _database;

        public CollectorService(ILogger<CollectorService> logger, IMessageBroker messageBroker, IDataBase database)
        {
            _logger = logger;
            _messageBroker = messageBroker;
            _database = database;
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

                    var incomingMessage = JsonSerializer.Deserialize<IncomingMessage>(message);

                    if (incomingMessage.RequestType == "triple" || incomingMessage.RequestType == "edited")
                    {
                        await ProcessTripleRequestAsync(incomingMessage, cancellationToken);
                    }
                    else if (incomingMessage.RequestType == "duo")
                    {
                        await ProcessDuoRequestAsync(incomingMessage, cancellationToken);
                    }
                    else if (incomingMessage.RequestType == "ocr")
                    {
                        await ProcessOcrRequestAsync(message, cancellationToken);
                    }
                    else
                    {
                        await _messageBroker.SendMessageAsync("extractData.result", "Unknown Request Type");
                    }

                    _logger.LogInformation("Message successfully processed and committed.");
                }
            }
        }

        private async Task ProcessTripleRequestAsync(IncomingMessage incomingMessage, CancellationToken cancellationToken)
        {
            var docDetectRes = await ProcessDocDetectRequestAsync(incomingMessage, cancellationToken);
            var fieldsDetectRes = await ProcessFieldsDetectRequestAsync(docDetectRes, cancellationToken);
            var textRecognitionRes = await ProcessTextRecognitionRequestAsync(fieldsDetectRes, cancellationToken);

            await SaveRequestToDatabase(incomingMessage, textRecognitionRes);

            await _messageBroker.SendMessageAsync("extractData.result", textRecognitionRes);
        }

        private async Task ProcessDuoRequestAsync(IncomingMessage incomingMessage, CancellationToken cancellationToken)
        {
            var fieldsDetectionRequest = new DocumentDetectionResult
            {
                RequestId = incomingMessage.RequestId,
                BucketName = incomingMessage.BucketName,
                ObjectName = incomingMessage.ObjectName,
                DocumentName = incomingMessage.Model
            };

            var fieldsDetectRes = await ProcessFieldsDetectRequestAsync(fieldsDetectionRequest, cancellationToken);
            var textRecognitionRes = await ProcessTextRecognitionRequestAsync(fieldsDetectRes, cancellationToken);

            await SaveRequestToDatabase(incomingMessage, textRecognitionRes);

            await _messageBroker.SendMessageAsync("extractData.result", textRecognitionRes);
        }

        private async Task<DocumentDetectionResult> ProcessDocDetectRequestAsync(IncomingMessage incomingMessage, CancellationToken cancellationToken)
        {
            var docDetectRequest = new DocumentDetectionRequest
            {
                RequestId = incomingMessage.RequestId,
                BucketName = incomingMessage.BucketName,
                ObjectName = incomingMessage.ObjectName,
                Model = incomingMessage.Model,
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

        private async Task ProcessOcrRequestAsync(string incomingMessage, CancellationToken cancellationToken)
        {
            var textRecognitionRes = await ProcessTextRecognitionRequestAsync(incomingMessage,  cancellationToken);

            // await SaveRequestToDatabase(incomingMessage, textRecognitionRes);

            await _messageBroker.SendMessageAsync("extractData.result", textRecognitionRes);
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

        private async Task SaveRequestToDatabase(IncomingMessage incomingMessage, string resultJson)
        {
            var requestHistory = new RequestHistory
            {
                RequestId = incomingMessage.RequestId,
                UserId = incomingMessage.ClientId,
                RequestType = incomingMessage.RequestType,
                BucketName = incomingMessage.BucketName,
                ObjectName = incomingMessage.ObjectName,
                ResultJson = resultJson,
                Timestamp = DateTime.UtcNow
            };

            await _database.SaveRequestAsync(requestHistory);
        }
    }
}