namespace Common.Application.Images;

public class ImageRequestModel(Stream content)
{
    public Stream Content { get; } = content;
}