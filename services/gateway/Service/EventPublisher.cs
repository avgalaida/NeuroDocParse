using System.Text.Json;
using HotChocolate.Subscriptions;

namespace gateway.Service
{
    public class EventPublisher
    {
        private readonly ITopicEventSender _sender;

        public EventPublisher(ITopicEventSender sender)
        {
            _sender = sender;
        }

        public async Task PublishDataExtracted(JsonElement extractedData, string requestId)
        {
            await _sender.SendAsync("DataExtracted", extractedData);
        }
    }
}