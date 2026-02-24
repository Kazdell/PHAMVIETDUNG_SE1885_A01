using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PHAMVIETDUNG_SE1885_A02_FE.Infrastructure.Filters
{
    public class OfflineExceptionHandlerAttribute : ExceptionFilterAttribute
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public OfflineExceptionHandlerAttribute(IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is HttpRequestException || context.Exception is TaskCanceledException)
            {
                var result = new ViewResult
                {
                    ViewName = "Offline",
                    StatusCode = 503
                };
                
                var viewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState)
                {
                    { "IsOffline", true }
                };
                
                result.ViewData = viewData;
                context.Result = result;
                context.ExceptionHandled = true;
            }
        }
    }
}
