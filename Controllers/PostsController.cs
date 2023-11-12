using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using aspnet_blog_application.Models;
using aspnet_blog_application.Models.ViewModels;
using Microsoft.Data.Sqlite;
using Amazon.S3;
using Amazon.S3.Model;
using DotNetEnv;

namespace aspnet_blog_application.Controllers;

public class PostsController : Controller
{
    private readonly ILogger<PostsController> _logger;

    private readonly IConfiguration _configuration;

    public PostsController(ILogger<PostsController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        using (var connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS post (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title TEXT, Body TEXT, CreatedAt TEXT, UpdatedAt TEXT, ImagePath TEXT)";
                command.ExecuteNonQuery();
            }
        }
    }

    public IActionResult Index()
    {
        var postListViewModel = GetAllPosts();
        return View(postListViewModel);
    }

    public IActionResult NewPost()
    {
        return View();
    }

    public IActionResult EditPost(int id) 
    {
        var post = GetPostById(id);
        var postViewModel = new PostViewModel();
        postViewModel.Post = post;
        return View(postViewModel);
    }

    public IActionResult ViewPost(int id) 
    {
        var post = GetPostById(id);
        var postViewModel = new PostViewModel();
        postViewModel.Post = post;
        return View(postViewModel);
    }

    internal PostModel GetPostById(int id)
    {
        PostModel post = new();

        using (var connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM post Where Id = '{id}'";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        post.Id = reader.GetInt32(0);
                        post.Title = reader.GetString(1);
                        post.Body = reader.GetString(2);
                        post.CreatedAt = reader.GetDateTime(3);
                        post.UpdatedAt = reader.GetDateTime(4);
                        post.ImagePath = reader.GetString(5);
                    }
                    else
                    {
                        return post;
                    }
                };
            }
        }

        return post;
    }

    internal PostViewModel GetAllPosts()
    {
        List<PostModel> postList = new();

        using (SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "SELECT * FROM post";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            postList.Add(
                                new PostModel
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Body = reader.GetString(2),
                                    CreatedAt = reader.GetDateTime(3),
                                    UpdatedAt = reader.GetDateTime(4)
                                });
                        }
                    }
                    else
                    {
                        return new PostViewModel
                        {
                            PostList = postList
                        };
                    }
                };
            }
        }

        return new PostViewModel
        {
            PostList = postList
        };
    }

    public async Task<ActionResult> Insert(PostModel post, IFormFile image)
    {
        if (post.Image != null && post.Image.Length > 0)
        {
            // // uploading images using disks
            // var filePath = Path.Combine("wwwroot/images", post.Image.FileName);
            // using (var stream = new FileStream(filePath, FileMode.Create))
            // {
            //     post.Image.CopyTo(stream);
            // }
            // post.ImagePath = "/images/" + post.Image.FileName;
            // post.ImagePath = post.ImagePath;
            // Console.WriteLine(post.ImagePath);

            // // uploading images using S3 Access
            Env.Load();
            var config = new AmazonS3Config
            {
                ServiceURL = Env.GetString("LIARA_ENDPOINT"),
                ForcePathStyle = true,
                SignatureVersion = "4"
            };

            var credentials  = new Amazon.Runtime.BasicAWSCredentials(Env.GetString("LIARA_ACCESS_KEY"), Env.GetString("LIARA_SECRET_KEY"));
            using var client = new AmazonS3Client(credentials, config);
            
            string objectKey = Guid.NewGuid().ToString() + post.Image.FileName;
            try
                {
                
                    using var memoryStream = new MemoryStream();
                    await post.Image.CopyToAsync(memoryStream).ConfigureAwait(false);

                    PutObjectRequest request = new PutObjectRequest
                    {
                        BucketName = Env.GetString("LIARA_BUCKET_NAME"),
                        Key = objectKey,
                        InputStream = memoryStream,
                    };

                    await client.PutObjectAsync(request);
                    Console.WriteLine($"File '{objectKey}' uploaded successfully.");
                }

            catch (AmazonS3Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            string fileUrl = $"{Env.GetString("LIARA_ENDPOINT")}/{Env.GetString("LIARA_BUCKET_NAME")}/{objectKey}";    
            post.ImagePath = fileUrl;
        }
        post.CreatedAt = DateTime.Now;
        post.UpdatedAt = DateTime.Now;
        
        using (SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"INSERT INTO post (Title, Body, CreatedAt, UpdatedAt, ImagePath) VALUES ('{post.Title}', '{post.Body}', '{post.CreatedAt}', '{post.UpdatedAt}', '{post.ImagePath}')";
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    public ActionResult Update(PostModel post)
    {
        post.UpdatedAt = DateTime.Now;

        using (SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE post SET Title = '{post.Title}', Body = '{post.Body}', UpdatedAt = '{post.UpdatedAt}', ImagePath = '{post.ImagePath}' WHERE Id = '{post.Id}'";
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public JsonResult Delete(int id)
    {
        using (SqliteConnection connection =
                new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"DELETE from post WHERE Id = '{id}'";
                command.ExecuteNonQuery();
            }
        }

        return Json(new Object{});
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
