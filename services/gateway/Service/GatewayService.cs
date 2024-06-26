using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using gateway.Broker;
using gateway.Storage;

namespace gateway.Service
{
    public class GatewayService
    {
        private readonly IStorage storage;
        private readonly IMessageBroker messageBroker;

        public GatewayService(IStorage storage, IMessageBroker messageBroker)
        {
            this.storage = storage;
            this.messageBroker = messageBroker;
        }

        public async Task<string> ExtractData(string b64Img, string userId, string request, string model)
        {
            var imageBytes = Convert.FromBase64String(b64Img);
            var imgData = await storage.UploadImage(imageBytes);

            var message = new
            {
                ClientId = userId,
                RequestType = request,
                BucketName = imgData.BucketName,
                ObjectName = imgData.ObjectName,
                Model = model
            };

            var messageJson = JsonSerializer.Serialize(message);
            await messageBroker.SendMessageAsync("extractData.request", messageJson);

            // Ожидание результата из топика `extractData.result`
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5)); // тайм-аут
            var result = await messageBroker.ReceiveMessageAsync("extractData.result", "gatewayGroup", cancellationTokenSource.Token);

            // Парсинг результата
            using (var jsonDoc = JsonDocument.Parse(result))
            {
                var root = jsonDoc.RootElement;
                string bucketName = root.GetProperty("BucketName").GetString();
                string objectName = root.GetProperty("ObjectName").GetString();

                // Получение изображения из Minio и конвертация в base64
                string base64Image = await GetImageAsBase64(bucketName, objectName);

                // Включение base64 изображения в результат
                var jsonObject = new Dictionary<string, JsonElement>();
                foreach (var property in root.EnumerateObject())
                {
                    jsonObject[property.Name] = property.Value;
                }
                jsonObject["Base64Image"] = JsonDocument.Parse($"\"{base64Image}\"").RootElement;

                var updatedJson = JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });

                return updatedJson;
            }
        }

        private async Task<string> GetImageAsBase64(string bucketName, string objectName)
        {
            try
            {
                var ms = new MemoryStream();
                await storage.DownloadImage(bucketName, objectName, ms);
                ms.Position = 0; // Reset stream position after copy
                var imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching image from Minio: {ex.Message}");
                throw;
            }
        }
    }
}