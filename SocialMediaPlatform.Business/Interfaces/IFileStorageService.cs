using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SocialMediaPlatform.Business.Interfaces;

public interface IFileStorageService
{
    // The file is uploaded to the specified folder and returns the access URL.
    Task<string> UploadFileAsync(IFormFile file);
    // deletes the file 
    // fileUrl is the url of the file which is deleted
    // returns if successfully deleted or not
    Task<bool> DeleteFileAsync(string fileUrl);
}
