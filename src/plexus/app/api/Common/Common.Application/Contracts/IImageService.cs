using Common.Application.Images;

namespace Common.Application.Contracts;

public interface IImageService
{
    Task<ImageResponseModel> Process(ImageRequestModel image);
}