using System.Text.Json;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using gateway.Service; // Убедитесь, что этот namespace правильный

namespace gateway.GraphQL
{
    public class Mutation
    {
        private readonly ILogger<Mutation> _logger;
        private readonly GatewayService _gatewayService; // Добавляем сервис

        public Mutation(ILogger<Mutation> logger, GatewayService gatewayService)
        {
            _logger = logger;
            _gatewayService = gatewayService; // Инициализируем сервис
        }

        public async Task<JsonElement> UploadImage(string b64Img, string userId)
        {
            _logger.LogInformation("UploadImage called with userId: {UserId} and b64Img: {B64Img}", userId, b64Img);
            
            // Используем сервис для извлечения данных
            var data = await _gatewayService.ExtractData(b64Img, userId, "triple", "default");
            _logger.LogInformation("Extracted data: {Data}", data);

            return JsonDocument.Parse(data).RootElement;
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