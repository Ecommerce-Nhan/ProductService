using Amazon.S3;
using Amazon.S3.Model;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ProductService.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class FileController : Controller
{
    private readonly IAmazonS3 _s3Client;
    public FileController(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllFilesAsync(string bucketName, string? prefix)
    {
        var request = new ListObjectsV2Request()
        {
            BucketName = bucketName,
            Prefix = prefix
        };
        var result = await _s3Client.ListObjectsV2Async(request);
        var s3Objects = result.S3Objects.Select(s =>
        {
            var urlRequest = new GetPreSignedUrlRequest()
            {
                BucketName = bucketName,
                Key = s.Key,
                Expires = DateTime.UtcNow.AddMinutes(1)
            };
            return new
            {
                Name = s.Key.ToString(),
                PresignedUrl = _s3Client.GetPreSignedURL(urlRequest),
            };
        });
        return Ok(s3Objects);
    }
    [HttpGet("GetByKey")]
    public async Task<IActionResult> GetFileByKeyAsync(string bucketName, string key)
    {
        var s3Object = await _s3Client.GetObjectAsync(bucketName, key);
        return File(s3Object.ResponseStream, s3Object.Headers.ContentType);
    }
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadFileAsync(IFormFile file, string bucketName, string? prefix)
    {
        var request = new PutObjectRequest()
        {
            BucketName = bucketName,
            Key = string.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
            InputStream = file.OpenReadStream()
        };
        request.Metadata.Add("Content-Type", file.ContentType);
        await _s3Client.PutObjectAsync(request);
        return Ok($"File {prefix}/{file.FileName} uploaded to S3 successfully!");
    }
    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteFileAsync(string bucketName, string key)
    {
        await _s3Client.DeleteObjectAsync(bucketName, key);
        return NoContent();
    }
}
