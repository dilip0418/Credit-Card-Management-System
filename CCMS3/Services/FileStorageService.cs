using CCMS3.Models;
using Microsoft.Extensions.Options;

namespace CCMS3.Services
{

    public class FileStorageService
    {
        private readonly string _fileSavePath;

        public FileStorageService(IOptions<AppSettings> settings)
        {
            _fileSavePath = settings.Value.FileStoragePath;
        }


        public async Task SaveFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                if (!Directory.Exists(_fileSavePath))
                {
                    Directory.CreateDirectory(_fileSavePath);
                }

                var filePath = Path.Combine(_fileSavePath, fileName);

                fileStream.Position = 0; // Ensure stream position is 0
                if (fileStream.Length == 0)
                {
                    throw new InvalidOperationException("File stream is empty");
                }

                using (var destinationStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(destinationStream);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                Console.WriteLine($"Error saving file: {ex.Message}");
                throw;
            }
        }

    }

}
