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
                Console.WriteLine($"Bucket {bucketName} created successfully");
            }
            else
            {
                Console.WriteLine($"Bucket {bucketName} already exists");
            }
        }

        public async Task<ImageData> UploadImage(byte[] bytesImg)
        {
            string bucketName = "images";
            string objectName = $"image_{Guid.NewGuid()}.png";
            
            using (var ms = new MemoryStream(bytesImg))
            {
                await CreateBucketAsync(bucketName);
                
                try
                {
                    await minio.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(ms)
                        .WithObjectSize(ms.Length)
                        .WithContentType("image/png"));
                    
                    Console.WriteLine($"Successfully uploaded {objectName} to {bucketName}");
                    return new ImageData { ObjectName = objectName, BucketName = bucketName };
                }
                catch (MinioException e)
                {
                    Console.WriteLine($"Error occurred: {e}");
                    throw;
                }
            }
        }

        public async Task DownloadImage(string bucketName, string objectName, Stream destinationStream)
        {
            try
            {
                await minio.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(async (stream) =>
                    {
                        using (stream) // Ensure stream is disposed after use
                        {
                            await stream.CopyToAsync(destinationStream);
                        }
                    }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching image from Minio: {ex.Message}");
                throw;
            }
        }
    }
}