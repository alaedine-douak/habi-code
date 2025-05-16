using FluentValidation;
using HabiCode.Api.Database;
using HabiCode.Api.DTOs.Habits;
using HabiCode.Api.Entities;
using HabiCode.Api.Middleware;
using HabiCode.Api.Services.Sorting;
using HabiCode.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using HabiCode.Api.Settings;
using System.Text;

namespace HabiCode.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            })
            .AddNewtonsoftJson(options =>
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
            .AddXmlSerializerFormatters();

        builder.Services
            .Configure<MvcOptions>(options =>
            {
                NewtonsoftJsonOutputFormatter formatter = options.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>()
                    .First();

                // Access media type globaly
                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV1);
                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV2);
                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJson);
                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV1);
                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV2);
            });

        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1.0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector = new DefaultApiVersionSelector(options); // Selects the first version in the list
                //options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options); // Selects the latest version

                //options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new MediaTypeApiVersionReader(),
                    new MediaTypeApiVersionReaderBuilder()
                        .Template("application/vnd.habicode.hateoas.v{version}+json")
                        .Build());

            })
            .AddMvc();

        builder.Services
            .AddOpenApi();

        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDbContext<HabiCodeDbContext>(options =>
                options.UseNpgsql(
                 builder.Configuration.GetConnectionString("Database"),
                 npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.HabiCode))
                .UseSnakeCaseNamingConvention());

        builder.Services
            .AddDbContext<HabiCodeIdentityDbContext>(options =>
                options.UseNpgsql(
                 builder.Configuration.GetConnectionString("Database"),
                 npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                .UseSnakeCaseNamingConvention());

        return builder;
    }

    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                };
            });

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


        return builder;
    }

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
                .WithTracing(tracing => tracing
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddNpgsql())
                .WithMetrics(metrics => metrics
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation())
                .UseOtlpExporter();

        builder.Logging
            .AddOpenTelemetry(options =>
            {
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;
            });

        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddTransient<SortMappingProvider>();
        builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<HabitDto, Habit>>(_ =>
            HabitMappings.SortMapping);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<DataShapingService>();

        builder.Services.AddTransient<LinkService>();
        builder.Services.AddTransient<TokenProvider>();

        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<UserContext>();


        return builder;
    }

    public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<HabiCodeIdentityDbContext>();

        builder.Services
            .Configure<JwtAuthOptions>(builder.Configuration.GetSection("Jwt"));

        JwtAuthOptions jwtAuthOptions = builder.Configuration.GetSection("Jwt").Get<JwtAuthOptions>();

        builder.Services
            .AddAuthentication(options =>  
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                TokenValidationParameters validationParameters = new()
                {
                    ValidIssuer = jwtAuthOptions!.Issuer,
                    ValidAudience = jwtAuthOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key)),
                };

                options.TokenValidationParameters = validationParameters;
            });

        builder.Services.AddAuthorization();

        return builder;
    }
}
