namespace Infrastructure.Repository.Azure;

public interface IAzureStorageQuery
{
    Task<string> DownloadTextBlobAsync(string blobName, string containerName);
    Task<bool> BlobExists(string blobName, string containerName);
    Task<bool> ContainerExists(string containerName);
    Task<List<string>> ListBlobsAsync(string containerName, string prefix = "");
}
