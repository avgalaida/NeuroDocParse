using System.Text.Json;
using gateway.Service;
using Microsoft.Extensions.Logging;
using HotChocolate.Subscriptions;

namespace gateway.GraphQL
{
    public class Mutation
    {
        private readonly ILogger<Mutation> _logger;
        private readonly GatewayService _gatewayService;
        private readonly EventPublisher _eventPublisher;

        public Mutation(ILogger<Mutation> logger, GatewayService gatewayService, EventPublisher eventPublisher)
        {
            _logger = logger;
            _gatewayService = gatewayService;
            _eventPublisher = eventPublisher;
        }

        public async Task<JsonElement> UploadImage(string b64Img, string userId, string requestId, string requestType, string model)
        {
            _logger.LogInformation("UploadImage called with userId: {UserId}, requestId: {RequestId}, requestType: {RequestType}, model: {Model}", userId, requestId, requestType, model);
            
            var data = await _gatewayService.ExtractData(b64Img, userId, requestId, requestType, model);
            _logger.LogInformation("Data extracted");

            var imageData = JsonDocument.Parse(data).RootElement;

            // Публикация события
            await _eventPublisher.PublishDataExtracted(imageData, requestId);

            return imageData;
        }

        public async Task<JsonElement> UploadImageWithFields(string b64Img, string userId, string requestId, string model, JsonElement fields)
        {
            _logger.LogInformation("UploadImageWithFields called with userId: {UserId}, requestId: {RequestId}, model: {Model}", userId, requestId, model);

            var data = await _gatewayService.ExtractDataWithFields(b64Img, userId, requestId, model, fields);
            _logger.LogInformation("Data extracted with fields");

            var imageData = JsonDocument.Parse(data).RootElement;

            await _eventPublisher.PublishDataExtracted(imageData, requestId);

            return imageData;
        }
    }

    [ExtendObjectType(Name = "Mutation")]
    public class UploadImageMutation
    {
        private readonly Mutation _mutation;

        public UploadImageMutation(Mutation mutation)
        {
            _mutation = mutation;
        }

        [GraphQLType(typeof(JsonType))]
        public Task<JsonElement> UploadImage(string b64Img, string userId, string requestId, string requestType, string model) =>
            _mutation.UploadImage(b64Img, userId, requestId, requestType, model);

        [GraphQLType(typeof(JsonType))]
        public Task<JsonElement> UploadImageWithFields(string b64Img, string userId, string requestId, string model, JsonElement fields) =>
            _mutation.UploadImageWithFields(b64Img, userId, requestId, model, fields);
    }
}