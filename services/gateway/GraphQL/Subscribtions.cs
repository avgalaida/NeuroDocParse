using HotChocolate;
using HotChocolate.Subscriptions;

namespace gateway.GraphQL;

public class Subscription
{
    [Subscribe]
    [Topic]
    public string OnImageUploaded([EventMessage] string imageUrl)
    {
        return imageUrl;
    }
}