using System.Collections.Generic;
namespace file_upload.Models
{
public class FileDetails {
    public string Name { get; set; }
    public string Path { get; set; }
}

public class FileViewModel {
    public List<FileDetails> Files { get; set; } = new List<FileDetails>();

}
}