using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public HomeController(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
        _bucketName = "bucketoo"; // Replace with your bucket name
    }

    public async Task<IActionResult> Index()
    {
        var files = await ListFilesAsync();
        return View(files); // Pass the list of files as the Model
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            await UploadFileAsync(file.FileName, stream);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFile(string fileName)
    {
        await DeleteFileAsync(fileName);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> GeneratePresignedUrl(string fileName)
    {
        var url = await GeneratePresignedUrlAsync(fileName);
        ViewBag.PresignedUrl = url;
        return View("Index", await ListFilesAsync());
    }

    public async Task<IActionResult> GetPermanentUrl(string fileName)
    {
        var url = $"https://{_bucketName}.{new Uri(_s3Client.Config.ServiceURL).Host}/{fileName}";
        ViewBag.PermanentUrl = url;
        return View("Index", await ListFilesAsync());
    }

    private async Task<List<string>> ListFilesAsync()
    {
        var listRequest = new ListObjectsV2Request
        {
            BucketName = _bucketName
        };

        var response = await _s3Client.ListObjectsV2Async(listRequest);
        var files = new List<string>();

        foreach (var entry in response.S3Objects)
        {
            files.Add(entry.Key);
        }

        return files;
    }

    private async Task UploadFileAsync(string fileName, Stream fileStream)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            InputStream = fileStream
        };

        await _s3Client.PutObjectAsync(putRequest);
    }

    private async Task DeleteFileAsync(string fileName)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName
        };

        await _s3Client.DeleteObjectAsync(deleteRequest);
    }

    private Task<string> GeneratePresignedUrlAsync(string fileName)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            Expires = System.DateTime.UtcNow.AddHours(1) // URL valid for 1 hour
        };

        return Task.FromResult(_s3Client.GetPreSignedURL(request));
    }
}