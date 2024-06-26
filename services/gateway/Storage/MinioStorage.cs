using System;
using System.IO;
using System.Threading.Tasks;
using Minio;
using Minio.Exceptions;
using Minio.DataModel.Args;
using gateway.Domain;

namespace gateway.Storage
{
    public class MinioStorage : IStorage
    {
        private static string endpoint = "minio:9000";
        private static string accessKey = "admin";
        private static string secretKey = "Secure123$";
        
        private static IMinioClient minio = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .Build();

        public async Task CreateBucketAsync(string bucketName)
        {
            bool found = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!found)
            {
                await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
                Console.WriteLine($"Bucket {bucketName} создан успешно");
            }
            else
            {
                Console.WriteLine($"Bucket {bucketName} уже существует");
            }
        }

        public async Task<ImageData> UploadImage(byte[] bytesImg)
        {
            string bucketName = "images";
            string objectName = $"image_{Guid.NewGuid()}.png";
            
            using (var ms = new MemoryStream(bytesImg))
            {
                // await CreateBucketAsync(bucketName);
                
                try
                {
                    await minio.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(ms)
                        .WithObjectSize(ms.Length)
                        .WithContentType("image/png"));
                    
                    Console.WriteLine($"Успешно загружен {objectName} в {bucketName}");
                    return new ImageData { ObjectName = objectName, BucketName = bucketName };
                }
                catch (MinioException e)
                {
                    Console.WriteLine($"Произошла ошибка: {e}");
                    throw;
                }
            }
        }

        public async Task DownloadImage(string bucketName, string objectName, Stream destinationStream)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await minio.GetObjectAsync(new GetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithCallbackStream(async (stream) =>
                        {
                            await stream.CopyToAsync(memoryStream);
                        }));
                    
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(destinationStream);
                    await destinationStream.FlushAsync(); // Обеспечиваем запись всех данных
                }
                destinationStream.Seek(0, SeekOrigin.Begin); // Сброс позиции потока
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении изображения из Minio: {ex.Message}");
                throw;
            }
        }
    }
}