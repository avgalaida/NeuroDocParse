using HotChocolate;
using HotChocolate.Types;
using System.Text.Json;

namespace gateway.GraphQL
{
    public class Subscription
    {
        [Subscribe]
        [Topic]
        public async Task<JsonElement> DataExtracted([EventMessage] JsonElement extractedData)
        {
            return extractedData;
        }
    }
}