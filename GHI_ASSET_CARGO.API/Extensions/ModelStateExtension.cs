using GHI_ASSET_CARGO.Core.Dtos;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GHI_ASSET_CARGO.API.Extensions
{
    public static class ModelStateExtension
    {
        public static IEnumerable<Error> GetErrors(this ModelStateDictionary modelState)
        {
            var errors = modelState
                .Where(e => e.Value!.ValidationState == ModelValidationState.Invalid)
                .SelectMany(e => e.Value!.Errors, (key, error) => new Error(key.Key, error.ErrorMessage));

            return errors;
        }
    }
}
