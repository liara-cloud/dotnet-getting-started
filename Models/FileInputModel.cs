using Microsoft.AspNetCore.Http;

namespace file_upload.Models
{
    public class FileInputModel {
        public IFormFile FileToUpload {get;set;}
    }

    public class Student {
        public string Name { get; set; }

        public string Address { get; set; }

        public string Mobile { get; set; }
        public IFormFile Photo { get; set; }
    }
}