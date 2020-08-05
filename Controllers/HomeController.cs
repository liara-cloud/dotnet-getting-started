using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using file_upload.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace file_upload.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFileProvider _fileProvider;
        public HomeController(ILogger<HomeController> logger, IFileProvider fileProvider)
        {
            _logger = logger;
            _fileProvider = fileProvider;
        }

        public async Task<IActionResult> UploadFile(IFormFile file) 
        {
            if(file == null || file.Length == 0)
                return Content("Please select file");
            
            var path = Path.Combine(Directory.GetCurrentDirectory(), "file_uploads", file.FileName);

            using(var stream = new FileStream(path, FileMode.Create)){
                await file.CopyToAsync(stream);
            }

            return RedirectToAction("Files");
        }

        public IActionResult Files() {
            var model = new FileViewModel();

            foreach(var item in this._fileProvider.GetDirectoryContents(""))
            {
                model.Files.Add(new FileDetails{
                    Name= item.Name, Path = item.PhysicalPath
                });
            }

            return View(model);
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
