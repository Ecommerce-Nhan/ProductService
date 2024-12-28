using Amazon.S3;
using Amazon.S3.Model;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ProductService.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class BucketController : ControllerBase
{
    private readonly IAmazonS3 _s3Client;
    public BucketController(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllBucketAsync()
    {
        return Ok(await _s3Client.ListBucketsAsync());
    }
    [HttpPost("Create")]
    public async Task<bool> CreateBucketAsync(string bucketName)
    {
        try
        {
            var request = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true,
            };

            var response = await _s3Client.PutBucketAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error creating bucket: '{ex.Message}'");
            return false;
        }
    }
    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteBucketAsync(string bucketName)
    {
        await _s3Client.DeleteBucketAsync(bucketName);
        return NoContent();
    }
}
