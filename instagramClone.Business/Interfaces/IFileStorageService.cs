using Microsoft.AspNetCore.Http;

namespace instagramClone.Business.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Verilen dosyayı belirlenen klasöre yükler ve erişim URL'sini döndürür.
    /// </summary>
    /// <param name="file">The file which will be uploaded</param>
    /// <returns>Access URL</returns>
    Task<string> UploadFileAsync(IFormFile file);
    
    
    /// <summary>
    /// Belirtilen dosyayı siler.
    /// </summary>
    /// <param name="fileUrl">Silinecek dosyanın URL'si</param>
    /// <returns>Başarılı olup olmadığını döndürür</returns>
    Task<bool> DeleteFileAsync(string fileUrl);
}
