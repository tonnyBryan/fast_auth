using fast_auth.annotation;
using fast_auth.model.dto;
using fast_authenticator.context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace fast_auth.util
{
    public class TokenValidationFilter : ActionFilterAttribute
    {
        private readonly MyDbContext _dbContext;

        public TokenValidationFilter(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var isTokenRequired = context.ActionDescriptor.EndpointMetadata
                .OfType<TokenRequired>()
                .Any();

            if (isTokenRequired)
            {
                var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token) || !ValidateToken(token))
                {
                    var error = new Error(500); 
                    var apiResponse = new ApiResponse<string>(401, "Token manquant ou invalide", error);

                    context.Result = new JsonResult(apiResponse)
                    {
                        StatusCode = 401
                    };

                    return;  
                }
            }

            base.OnActionExecuting(context);
        }


        private bool ValidateToken(string token)
        {
            var tokenExists = _dbContext.Tokens
                 .Where(t => t.Key == token)
                 .Any();

            return tokenExists;
        }
    }
}



