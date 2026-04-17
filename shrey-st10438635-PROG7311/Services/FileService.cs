namespace shrey_st10438635_PROG7311.Services
{
    public interface IFileService
    {
        Task<(string storedPath, string fileName)> SaveContractFileAsync(IFormFile file);
        void DeleteFile(string filePath);
        bool IsValidPdf(IFormFile file);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        // Only PDF is allowed
        private static readonly string[] AllowedExtensions = { ".pdf" };
        private static readonly string[] AllowedMimeTypes = { "application/pdf" };

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

        public async Task<(string storedPath, string fileName)> SaveContractFileAsync(IFormFile file)
        {
            if (!IsValidPdf(file))
                throw new InvalidOperationException("Only PDF files are permitted.");

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
