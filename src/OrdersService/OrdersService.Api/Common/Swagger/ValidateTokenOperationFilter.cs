using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using OrdersService.Api.Common.Attributes;

namespace OrdersService.Api.Common.Swagger;

public class ValidateTokenOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            return;

        var hasValidateTokenAttribute = actionDescriptor.MethodInfo.GetCustomAttributes(typeof(ValidateTokenAttribute), true).Any() ||
                                      actionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(ValidateTokenAttribute), true).Any();

        if (hasValidateTokenAttribute)
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Token"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };
        }
    }
} 