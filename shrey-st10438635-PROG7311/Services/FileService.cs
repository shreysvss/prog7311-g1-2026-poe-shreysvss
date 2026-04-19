namespace shrey_st10438635_PROG7311.Services
{
    // Shrey Singh
    // ST10438635
    // References:
    // <Perumal, N., 2026. PROG7311 POE Part Two Workshop. [lecture] The Independent Institute of Education, 15 April 2026.>
    // <Code Maze, 2026. Repository Pattern with ASP.NET Core and Entity Framework. [online] Available at: https://code-maze.com/the-repository-pattern-aspnet-core [Accessed 15 April 2026].>
    // <Refactoring Guru, 2026. Strategy Design Pattern. [online] Available at: https://refactoring.guru/design-patterns/strategy [Accessed 16 April 2026].>
    // <Tutorials Teacher, 2026. Consuming a Web API using HttpClient. [online] Available at: https://www.tutorialsteacher.com/core/consume-web-api-httpclient [Accessed 17 April 2026].>
    // <GeeksforGeeks, 2026. async and await in C#. [online] Available at: https://www.geeksforgeeks.org/async-and-await-in-c-sharp [Accessed 18 April 2026].>

    public interface IFileService
    {
        Task<(string storedPath, string fileName)> SaveContractFileAsync(IFormFile file);
        void DeleteFile(string filePath);
        bool IsValidPdf(IFormFile file);
        bool IsWithinSizeLimit(IFormFile file);
    }
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;
        // Only PDF is allowed
        private static readonly string[] AllowedExtensions = { ".pdf" };
        private static readonly string[] AllowedMimeTypes = { "application/pdf" };
        // Maximum file size: 5 MB (5 * 1024 * 1024 bytes)
        public const long MaxFileSizeBytes = 5 * 1024 * 1024;
        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }
        public bool IsValidPdf(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension)) return false;
            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant())) return false;
            return true;
        }
        public bool IsWithinSizeLimit(IFormFile file)
        {
            if (file == null) return false;
            return file.Length <= MaxFileSizeBytes;
        }
        public async Task<(string storedPath, string fileName)> SaveContractFileAsync(IFormFile file)
        {
            if (!IsValidPdf(file))
                throw new InvalidOperationException("Only PDF files are permitted.");
            if (!IsWithinSizeLimit(file))
                throw new InvalidOperationException($"File exceeds the maximum size of {MaxFileSizeBytes / (1024 * 1024)} MB.");
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
            Directory.CreateDirectory(uploadsFolder);
            // UUID naming prevents overwrites
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            _logger.LogInformation("File saved: {FilePath}", filePath);
            // Return relative web path and original name
            return ($"/uploads/contracts/{uniqueFileName}", file.FileName);
        }
        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            var physicalPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                _logger.LogInformation("File deleted: {FilePath}", physicalPath);
            }
        }
    }
}