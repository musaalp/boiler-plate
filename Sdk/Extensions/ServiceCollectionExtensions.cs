﻿using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sdk.Api.Authorization.Permission;
using Sdk.Api.Mediatr.PipelinesBehaviours;
using Sdk.Api.Settings;
using Sdk.Swagger;
using System.Text;

namespace Sdk.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSdk(this IServiceCollection services, IConfiguration configuration)
        {
            AddSwagger(services, configuration);
            AddPipelineBehaviors(services);
            AddSdkAuthentication(services, configuration);
            AddSdkAuthorization(services);
        }

        public static void AddSwagger(IServiceCollection services, IConfiguration configuration)
        {
            string apiName = configuration.GetSection("ApiCoreSettings").Get<ApiCoreSettings>().ApiName ?? string.Empty;

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = apiName, Version = "v1" });

                const string bearerSecurityDefinitionId = "Bearer";
                options.AddSecurityDefinition(bearerSecurityDefinitionId, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    In = ParameterLocation.Header,
                    Scheme = "bearer",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = bearerSecurityDefinitionId
                            }
                        },
                        new List<string>()
                    }
                });

                options.OperationFilter<RequiredHeadersOperationFilter>();
            });

            services.AddSwaggerGenNewtonsoftSupport();
        }

        public static void AddPipelineBehaviors(IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationPipelineBehavior<,>));
        }

        public static void AddSdkAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            string jwtSecretKey = configuration.GetSection("ApiCoreSettings").Get<ApiCoreSettings>().JwtSettings.JwtSecretKey;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey))
                };
            });
        }

        public static void AddSdkAuthorization(IServiceCollection services)
        {
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("Permission",
                    b => { b.Requirements.Add(new PermissionAuthorizationRequirement()); });
            });

            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }
    }
}
