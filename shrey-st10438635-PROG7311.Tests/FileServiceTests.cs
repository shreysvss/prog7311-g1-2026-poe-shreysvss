using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using shrey_st10438635_PROG7311.Services;
using Xunit;

namespace shrey_st10438635_PROG7311_Tests
{
    /// <summary>
    /// Unit tests for FileService — verifies PDF-only validation and maximum file size rule.
    /// Rubric: "File Validation: Verify that uploading a restricted file type (e.g., .exe) throws an error (only .pdf allowed)."
    /// </summary>
    public class FileServiceTests
    {
        private readonly FileService _fileService;

        public FileServiceTests()
        {
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            var mockLogger = new Mock<ILogger<FileService>>();
            _fileService = new FileService(mockEnv.Object, mockLogger.Object);
        }

        //Helper 
        private static IFormFile MakeFakeFile(string fileName, string contentType, long sizeBytes = 1024)
        {
            var stream = new MemoryStream();
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.ContentType).Returns(contentType);
            mock.Setup(f => f.Length).Returns(sizeBytes);
            mock.Setup(f => f.OpenReadStream()).Returns(stream);
            mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return mock.Object;
        }

        // Happy Path — valid PDF

        [Fact]
        public void IsValidPdf_ValidPdfFile_ReturnsTrue()
        {
            // Arrange
            var file = MakeFakeFile("contract.pdf", "application/pdf");

            // Act
            bool result = _fileService.IsValidPdf(file);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidPdf_ValidPdfUppercaseExtension_ReturnsFalse()
        {
            // Arrange — .PDF uppercase should still be treated correctly (lowered)
            var file = MakeFakeFile("Contract.PDF", "application/pdf");

            // Act
            bool result = _fileService.IsValidPdf(file);

            // Assert — lowercase check covers .pdf, .PDF lowercased = .pdf ✓
            Assert.True(result);
        }

        //  Failure Tests — restricted file types 

        [Fact]
        public void IsValidPdf_ExeFile_ReturnsFalse()
        {
            // Arrange
            var file = MakeFakeFile("malware.exe", "application/octet-stream");

            // Act
            bool result = _fileService.IsValidPdf(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdf_DocxFile_ReturnsFalse()
        {
            // Arrange
            var file = MakeFakeFile("contract.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

            // Act
            bool result = _fileService.IsValidPdf(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdf_JpegFile_ReturnsFalse()
        {
            // Arrange
            var file = MakeFakeFile("photo.jpg", "image/jpeg");

            // Act
            bool result = _fileService.IsValidPdf(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdf_NullFile_ReturnsFalse()
        {
            // Act
            bool result = _fileService.IsValidPdf(null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdf_EmptyFile_ReturnsFalse()
        {
            // Arrange — zero-byte file
            var file = MakeFakeFile("empty.pdf", "application/pdf", sizeBytes: 0);

            // Act
            bool result = _fileService.IsValidPdf(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdf_PdfExtensionButWrongMime_ReturnsFalse()
        {
            // Arrange — correct extension, spoofed MIME
            var file = MakeFakeFile("contract.pdf", "application/octet-stream");

            // Act
            bool result = _fileService.IsValidPdf(file);

            // Assert — both extension AND mime must match
            Assert.False(result);
        }

        [Theory]
        [InlineData("virus.bat", "application/x-msdownload")]
        [InlineData("script.js", "application/javascript")]
        [InlineData("archive.zip", "application/zip")]
        [InlineData("data.csv", "text/csv")]
        [InlineData("page.html", "text/html")]
        public void IsValidPdf_MultipleDisallowedTypes_AllReturnFalse(string fileName, string contentType)
        {
            // Arrange
            var file = MakeFakeFile(fileName, contentType);

            // Act
            bool result = _fileService.IsValidPdf(file);

            // Assert
            Assert.False(result);
        }

        // SaveContractFileAsync — invalid file throws 

        [Fact]
        public async Task SaveContractFileAsync_InvalidFileType_ThrowsInvalidOperationException()
        {
            // Arrange
            var file = MakeFakeFile("bad_file.exe", "application/octet-stream");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _fileService.SaveContractFileAsync(file));
        }

        // Maximum File Size Tests

        [Fact]
        public void IsWithinSizeLimit_FileUnderLimit_ReturnsTrue()
        {
            // Arrange — 1 MB file (well under 5 MB limit)
            var file = MakeFakeFile("contract.pdf", "application/pdf", sizeBytes: 1 * 1024 * 1024);

            // Act
            bool result = _fileService.IsWithinSizeLimit(file);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsWithinSizeLimit_FileAtExactLimit_ReturnsTrue()
        {
            // Arrange — exactly 5 MB (boundary case, should be allowed)
            var file = MakeFakeFile("contract.pdf", "application/pdf", sizeBytes: FileService.MaxFileSizeBytes);

            // Act
            bool result = _fileService.IsWithinSizeLimit(file);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsWithinSizeLimit_FileOverLimit_ReturnsFalse()
        {
            // Arrange — 6 MB file (over 5 MB limit)
            var file = MakeFakeFile("contract.pdf", "application/pdf", sizeBytes: 6 * 1024 * 1024);

            // Act
            bool result = _fileService.IsWithinSizeLimit(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsWithinSizeLimit_NullFile_ReturnsFalse()
        {
            // Act
            bool result = _fileService.IsWithinSizeLimit(null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SaveContractFileAsync_FileOverSizeLimit_ThrowsInvalidOperationException()
        {
            // Arrange — valid PDF but oversized (10 MB)
            var file = MakeFakeFile("huge_contract.pdf", "application/pdf", sizeBytes: 10 * 1024 * 1024);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _fileService.SaveContractFileAsync(file));
            Assert.Contains("maximum size", ex.Message);
        }
    }
}