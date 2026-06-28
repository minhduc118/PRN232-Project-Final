using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SportCourtManagerment.Services;

public class CloudinaryService
{
  private readonly Cloudinary _cloudinary;

  public CloudinaryService(IConfiguration config)
  {
    var cloudName = config["Cloudinary:CloudName"];
    var apiKey = config["Cloudinary:ApiKey"];
    var apiSecret = config["Cloudinary:ApiSecret"];

    var account = new Account(cloudName, apiKey, apiSecret);
    _cloudinary = new Cloudinary(account);
  }

  public async Task<string> UploadImageAsync(IFormFile file)
  {
    if (file == null || file.Length == 0)
      throw new ArgumentException("File is empty");

    using var stream = file.OpenReadStream();
    var uploadParams = new ImageUploadParams
    {
      File = new FileDescription(file.FileName, stream),
      Transformation = new Transformation().Quality("auto").FetchFormat("auto")
    };

    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

    if (uploadResult.Error != null)
      throw new Exception(uploadResult.Error.Message);

    return uploadResult.SecureUrl.ToString();
  }
}
