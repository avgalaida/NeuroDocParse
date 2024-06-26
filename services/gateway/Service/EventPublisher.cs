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

        public async Task PublishDataExtracted(JsonElement extractedData)
        {
            await _sender.SendAsync("DataExtracted", extractedData);
        }
    }
}