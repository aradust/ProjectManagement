namespace RadustovTestTask.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using RadustovTestTask.BLL.DTO;
    using RadustovTestTask.BLL.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetProjectDocuments(long projectId)
        {
            try
            {
                List<DocumentDto> documents = await _documentService.GetProjectDocumentsAsync(projectId);
                return Ok(documents);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost("upload/{projectId}")]
        public async Task<IActionResult> UploadDocument(long projectId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            try
            {
                using var stream = file.OpenReadStream();
                DocumentDto document = await _documentService.AddDocumentAsync(
                    projectId,
                    file.FileName,
                    stream
                );

                return Ok(document);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> DeleteDocument(long documentId)
        {
            try
            {
                bool result = await _documentService.DeleteDocumentAsync(documentId);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("download/{documentId}")]
        public async Task<IActionResult> DownloadDocument(long documentId)
        {
            try
            {
                var (fileContent, contentType, fileName) = await _documentService.DownloadDocumentAsync(documentId);
                return File(fileContent, contentType, fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}