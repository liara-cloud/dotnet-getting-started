using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using DotNetEnv;

class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables
        Env.Load();

        // Set environment variables
        string accessKey = Env.GetString("LIARA_ACCESS_KEY");
        string secretKey = Env.GetString("LIARA_SECRET_KEY");
        string bucketName = Env.GetString("LIARA_BUCKET_NAME");
        string endpoint = Env.GetString("LIARA_ENDPOINT");

        // Check for the existence of required variables
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(endpoint))
        {
            Console.WriteLine("Error: Missing required environment variables.");
            return;
        }

        // Create S3 client
        var config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            SignatureVersion = "4"
        };
        var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
        using var client = new AmazonS3Client(credentials, config);

        // Check for command
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a command. Available commands: list, download, upload, geturls, getpermanenturls, listbuckets, delete");
            return;
        }

        string command = args[0];

        switch (command)
        {
            case "list":
                await ListObjectsAsync(client, bucketName);
                break;
            case "download":
                if (args.Length < 2)
                {
                    Console.WriteLine("Please provide the object key to download.");
                    return;
                }
                await DownloadObjectAsync(client, bucketName, args[1], Path.Combine(Directory.GetCurrentDirectory(), args[1]));
                break;
            case "upload":
                if (args.Length < 2)
                {
                    Console.WriteLine("Please provide the object key for upload.");
                    return;
                }
                await UploadObjectAsync(client, bucketName, Path.Combine(Directory.GetCurrentDirectory(), args[1]), args[1]);
                break;
            case "geturls":
                await GetUrlsAsync(client, bucketName, 1);
                break;
            case "getpermanenturls":
                await GetUrlsAsync(client, bucketName, 10);
                break;
            case "listbuckets":
                await ListBucketsAsync(client);
                break;
            case "delete":
                if (args.Length < 2)
                {
                    Console.WriteLine("Please provide the object key to delete.");
                    return;
                }
                await DeleteObjectAsync(client, bucketName, args[1]);
                break;
            default:
                Console.WriteLine("Invalid command. Available commands: list, download, upload, geturls, getpermanenturls, listbuckets, delete");
                break;
        }
    }
    static async Task ListObjectsAsync(IAmazonS3 client, string bucketName)
    {
        ListObjectsV2Request request = new ListObjectsV2Request
        {
            BucketName = bucketName
        };
        ListObjectsV2Response response = await client.ListObjectsV2Async(request);

        foreach (S3Object entry in response.S3Objects)
        {
            Console.WriteLine($"File: {entry.Key} (Size: {entry.Size} bytes)");
        }
    }

    static async Task DownloadObjectAsync(IAmazonS3 client, string bucketName, string objectKey, string filePath)
    {
        try
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            using GetObjectResponse response = await client.GetObjectAsync(request);
            using Stream responseStream = response.ResponseStream;
            using FileStream fileStream = File.Create(filePath);
            await responseStream.CopyToAsync(fileStream);
            Console.WriteLine($"File '{objectKey}' downloaded successfully.");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    static async Task UploadObjectAsync(IAmazonS3 client, string bucketName, string filePath, string objectKey)
    {
        try
        {
            using FileStream fileStream = new FileStream(filePath, FileMode.Open);

            PutObjectRequest request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                InputStream = fileStream
            };

            await client.PutObjectAsync(request);

            Console.WriteLine($"File '{objectKey}' uploaded successfully.");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    static async Task GetUrlsAsync(IAmazonS3 client, string bucketName, int expiresInHours)
    {
        ListObjectsV2Request request = new ListObjectsV2Request
        {
            BucketName = bucketName
        };
        ListObjectsV2Response response = await client.ListObjectsV2Async(request);

        foreach (S3Object entry in response.S3Objects)
        {
            GetPreSignedUrlRequest urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = entry.Key,
                Expires = DateTime.Now.AddHours(expiresInHours)
            };
            string url = client.GetPreSignedURL(urlRequest);
            Console.WriteLine($"File: {entry.Key}, URL: {url}");
        }
    }

    static async Task ListBucketsAsync(IAmazonS3 client)
    {
        ListBucketsResponse response = await client.ListBucketsAsync();

        Console.WriteLine("Available Buckets:");
        foreach (S3Bucket bucket in response.Buckets)
        {
            Console.WriteLine(bucket.BucketName);
        }
    }

    static async Task DeleteObjectAsync(IAmazonS3 client, string bucketName, string objectKey)
    {
        try
        {
            DeleteObjectRequest deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            await client.DeleteObjectAsync(deleteRequest);

            Console.WriteLine($"File '{objectKey}' deleted successfully.");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }
}
