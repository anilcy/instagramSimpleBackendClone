using System;
using System.IO;
using System.Threading.Tasks;
using SocialMediaPlatform.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SocialMediaPlatform.Business.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _uploadsFolder;

    public FileStorageService(IConfiguration configuration)
    {
        // UploadsFolder is read from appsettings.json. If there is no such key, "uploads" is used as default
        _uploadsFolder = configuration["UploadsFolder"] ?? "uploads";
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        // a unique file name is generated
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        // the path where the file will be uploaded is determined
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), _uploadsFolder, fileName);

        // a new directory is generated if there is no any
        var directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // data is written to the file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // url is returned
        return $"/{_uploadsFolder}/{fileName}";
    }
    
    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            // path of the file to be deleted is determined
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileUrl.TrimStart('/'));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true); // deleted successfully
            }
            
            return Task.FromResult(false); // no file
        }
        catch
        {
            return Task.FromResult(false); // delete unsuccessful
        }
    }
}