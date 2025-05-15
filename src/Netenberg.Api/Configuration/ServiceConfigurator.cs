using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Netenberg.Api.Auth;
using Netenberg.Application.Jobs;
using Netenberg.Application.Services;
using Netenberg.Application.Validators;
using Netenberg.Database.DatabaseContext;
using Netenberg.Database.Repositories;
using Netenberg.Model.Options;
using Quartz;
using Quartz.Impl;
using System.Threading.RateLimiting;

namespace Netenberg.Api.Configuration;

public static class ServiceConfigurator
{
    public async static Task<WebApplication> ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IReadOnlyBookRepository, BookRepository>();
        builder.Services.AddScoped<IBookRepository, BookRepository>();
        builder.Services.AddScoped<IBooksService, BooksService>();
        builder.Services.AddScoped<IValidator<GetBooksOptions>, GetBooksOptionsValidator>();
        builder.Services.AddLogging(x => x.AddConsole());
        builder.Services.AddScoped<DataUpdaterJob>();
        builder.Services.AddOutputCache();

        builder.Services.ConfigureCors();
        builder.Services.ConfigureOutputCache();
        builder.Services.ConfigureRateLimiter(builder.Configuration["PRIVATE_API_KEY"]!);
        builder.Services.ConfigureDbContext(builder.Configuration["COSMOS_DB_CONNECTION_STRING"]!);
        await ConfigureDataUpdaterJob();

        return builder.Build();
    }

    private static IServiceCollection ConfigureCors(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddCors(options =>
        {
            options.AddPolicy("WithApiKey", policy =>
            {
                policy.AllowAnyOrigin()
                      .WithMethods("GET")
                      .WithHeaders(AuthConstants.ApiKeyHeaderName)
                      .SetPreflightMaxAge(TimeSpan.FromHours(1));
            });
        });

        return serviceCollection;
    }

    private static IServiceCollection ConfigureRateLimiter(this IServiceCollection serviceCollection, string apiKeyFromConfiguration)
    {
        serviceCollection.AddRateLimiter(options =>
        {
            options.AddPolicy("ApiKeyRateLimit", context =>
            {
                if (!context.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var apiKey))
                    return RateLimitPartition.GetNoLimiter("missing-key");

                if (apiKey == apiKeyFromConfiguration)
                {
                    return RateLimitPartition.GetNoLimiter<string>(apiKey!);
                }

                return RateLimitPartition.GetSlidingWindowLimiter<string>(
                    partitionKey: apiKey!,
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromHours(1),
                        SegmentsPerWindow = 5
                    });
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    Error = "Too many requests",
                }, token);
            };
        });

        return serviceCollection;
    }

    private static IServiceCollection ConfigureOutputCache(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOutputCache(options =>
        {
            options.AddPolicy("ShortTerm", policy =>
                policy.Expire(TimeSpan.FromSeconds(30)));
        });

        return serviceCollection;
    }

    private static IServiceCollection ConfigureDbContext(this IServiceCollection serviceCollection, string connectionString)
    {
        serviceCollection.AddDbContextPool<NetenbergContext>(options =>
        {
            var client = new MongoClient(connectionString);
            options.UseMongoDB(client, "netenberg");
        });

        return serviceCollection;
    }

    private static async Task ConfigureDataUpdaterJob()
    {
        StdSchedulerFactory factory = new();

        IScheduler scheduler = await factory.GetScheduler();

        await scheduler.Start();

        var job = JobBuilder.Create<DataUpdaterJob>().Build();
        var trigger = TriggerBuilder.Create()
            .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Sunday, 9, 0))
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}
