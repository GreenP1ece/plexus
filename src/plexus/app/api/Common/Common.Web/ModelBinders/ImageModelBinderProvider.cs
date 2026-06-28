using Common.Application.Images;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Common.Web.ModelBinders;
public class ImageModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
        => context.Metadata.ModelType == typeof(ImageRequestModel)
            ? new ImageModelBinder()
            : default;
}