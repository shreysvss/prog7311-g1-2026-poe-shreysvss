//Code attribution
//Title: File Upload in ASP.NET Core MVC
//Author: Code Maze
//Date: 18 April 2026
//Version: 1
//Availability: https://code-maze.com/file-upload-aspnetcore-mvc

//Code attribution
//Anthropic. 2026. Claude (Version 4.5) [Large language model].
//Used to help clean up and refine code, not to generate it.
//Available at: https://claude.ai
//[Accessed: 20 April 2026].


namespace shrey_st10438635_PROG7311.Services
{
    // The interface that defines what a file service must be able to do
    public interface IFileService
    {
        Task<(string storedPath, string fileName)> SaveContractFileAsync(IFormFile file);
        void DeleteFile(string filePath);
        bool IsValidPdf(IFormFile file);
        bool IsWithinSizeLimit(IFormFile file);
    }

    // Handles saving, deleting, and validating uploaded PDF files for contracts
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        // The only file extensions and MIME types the app accepts
        private static readonly string[] AllowedExtensions = { ".pdf" };
        private static readonly string[] AllowedMimeTypes = { "application/pdf" };

        // The largest file we allow — set to 5 MB
        public const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        // Checks if the file is a valid PDF by looking at both the extension AND the MIME type
        // This way someone can't rename a .exe to .pdf and sneak it past the check
        public bool IsValidPdf(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension)) return false;

            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant())) return false;

            return true;
        }

        // Checks that the uploaded file isn't larger than the 5 MB limit
        public bool IsWithinSizeLimit(IFormFile file)
        {
            if (file == null) return false;
            return file.Length <= MaxFileSizeBytes;
        }

        // Saves the uploaded PDF to disk with a unique name and returns the path
        public async Task<(string storedPath, string fileName)> SaveContractFileAsync(IFormFile file)
        {
            // Block the save if the file isn't a valid PDF
            if (!IsValidPdf(file))
                throw new InvalidOperationException("Only PDF files are permitted.");

            // Block the save if the file is too big
            if (!IsWithinSizeLimit(file))
                throw new InvalidOperationException($"File exceeds the maximum size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

            // Make sure the upload folder exists before saving
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
            Directory.CreateDirectory(uploadsFolder);

            // Add a unique ID to the start of the filename so two files with the same name don't clash
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Write the file bytes to disk
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation("File saved: {FilePath}", filePath);

            // Return the web-friendly path and the original file name for display
            return ($"/uploads/contracts/{uniqueFileName}", file.FileName);
        }

        // Deletes a file from disk if it exists
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