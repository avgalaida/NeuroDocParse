using System.Text.Json;
using HotChocolate;
using HotChocolate.Types;

namespace gateway.GraphQL
{
    public class Subscription
    {
        [Subscribe]
        [Topic]
        public async Task<JsonElement> DataExtracted([EventMessage] JsonElement extractedData, string requestId)
        {
            return extractedData;
        }
    }
}