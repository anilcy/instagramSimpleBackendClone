using instagramClone.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace instagramClone.Business.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _uploadsFolder;

    public FileStorageService(IConfiguration configuration)
    {
        // appsettings.json'dan "UploadsFolder" değeri okunur, yoksa "uploads" klasörü varsayılır.
        _uploadsFolder = configuration["UploadsFolder"] ?? "uploads";
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        // a unique file name is generated
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        // the path where the file will be uploaded is determined
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), _uploadsFolder, fileName);

        // a new directory is generated if there is no any
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        // data is written to the file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // url is returned
        return $"/{_uploadsFolder}/{fileName}";
    }
}