using gateway.Domain;

namespace gateway.Storage
{
    public interface IStorage
    {
        Task<ImageData> UploadImage(byte[] bytesImg);
        Task DownloadImage(string bucketName, string objectName, Stream destinationStream);
    }
}