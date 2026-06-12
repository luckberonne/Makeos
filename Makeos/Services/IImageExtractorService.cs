using Makeos.Models;

namespace Makeos.Services
{
    public interface IImageExtractorService
    {
        Task<ImageInfo> ExtractTextAsync(IFormFile file);
    }
}
