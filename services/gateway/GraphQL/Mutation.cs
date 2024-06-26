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
        private readonly ITopicEventSender _eventSender;

        public Mutation(ILogger<Mutation> logger, GatewayService gatewayService, ITopicEventSender eventSender)
        {
            _logger = logger;
            _gatewayService = gatewayService;
            _eventSender = eventSender;
        }

        public async Task<JsonElement> UploadImage(string b64Img, string userId)
        {
            _logger.LogInformation("UploadImage called with userId: {UserId}", userId);
            
            var data = await _gatewayService.ExtractData(b64Img, userId, "triple", "default");
            _logger.LogInformation("Data extracted");

            var imageData = JsonDocument.Parse(data).RootElement;

            // Публикация события
            await _eventSender.SendAsync(nameof(Subscription.DataExtracted), imageData);

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
        public Task<JsonElement> UploadImage(string b64Img, string userId) =>
            _mutation.UploadImage(b64Img, userId);
    }
}