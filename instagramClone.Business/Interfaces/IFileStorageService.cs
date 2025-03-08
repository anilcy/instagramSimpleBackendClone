using Microsoft.AspNetCore.Http;

namespace instagramClone.Business.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Verilen dosyayı belirlenen klasöre yükler ve erişim URL'sini döndürür.
    /// </summary>
    /// <param name="file">Yüklenecek dosya</param>
    /// <returns>Dosyanın erişim URL'si</returns>
    Task<string> UploadFileAsync(IFormFile file);
}
