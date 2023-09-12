using System;
using System.Collections.Generic;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using DotNetEnv;

class Program
{
    static async Task Main(string[] args)
    {
        // loading env variables
        Env.Load();

        // setting env variables
        string accessKey = Env.GetString("LIARA_ACCESS_KEY");
        string secretKey = Env.GetString("LIARA_SECRET_KEY");
        string bucketName = Env.GetString("LIARA_BUCKET_NAME");
        string endpoint = Env.GetString("LIARA_ENDPOINT");

        // making s3 connections
        var config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            SignatureVersion = "4"
        };
        var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
        using var client = new AmazonS3Client(credentials, config);

        if (args.Length > 0)
        {
            string command = args[0];

            if (command == "list")
            {
                // listing items from bucket   
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = bucketName
                };
                ListObjectsV2Response response = client.ListObjectsV2Async(request).Result;

                foreach (S3Object entry in response.S3Objects)
                {
                    Console.WriteLine($"File: {entry.Key} (Size: {entry.Size} bytes)");
                }
            }
            
            else if (command == "download")
            {
                string objectKey = "example.txt"; 
                string filePath = "downloaded_file.txt";
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

            else if (command == "upload")
            {
                string filePath = "liara-poster.png"; 
                string objectKey = "liara-poster.png";

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

            else if (command == "geturls")
            {
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = bucketName
                };
                ListObjectsV2Response response = client.ListObjectsV2Async(request).Result;

                foreach (S3Object entry in response.S3Objects)
                {
                    GetPreSignedUrlRequest urlRequest = new GetPreSignedUrlRequest
                    {
                        BucketName = bucketName,
                        Key = entry.Key,
                        Expires = DateTime.Now.AddHours(1) // expires after 1 hour
                    };
                    string url = client.GetPreSignedURL(urlRequest);
                    Console.WriteLine($"File: {entry.Key}, URL: {url}");
                }
            }

            else if (command == "listbuckets")
            {
                ListBucketsResponse response = client.ListBucketsAsync().Result;

                Console.WriteLine("Available Buckets:");
                foreach (S3Bucket bucket in response.Buckets)
                {
                    Console.WriteLine(bucket.BucketName);
                }
            }
        }
    }
}