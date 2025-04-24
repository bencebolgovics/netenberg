using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Netenberg.Api.Auth;
using Netenberg.Api.Mapper;
using Netenberg.Api.Validation;
using Netenberg.Application.Services;
using Netenberg.Application.Validators;
using Netenberg.Database.DatabaseContext;
using Netenberg.Database.Repositories;
using Netenberg.DataUpdater;
using Netenberg.Model.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IReadOnlyBookRepository, BookRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBooksService, BooksService>();
builder.Services.AddScoped<IValidator<GetBooksOptions>, GetBooksOptionsValidator>();
builder.Services.AddLogging(x => x.AddConsole());
builder.Services.AddScoped<DataUpdaterService>();
builder.Services.AddOutputCache();

builder.Services.AddDbContextPool<NetenbergContext>(options =>
{
    string connectionString = builder.Configuration["COSMOS_DB_CONNECTION_STRING"]!;
    string databaseName = "netenberg";

    var client = new MongoClient(connectionString);
    options.UseMongoDB(client, databaseName);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("WithApiKey", policy =>
    {
      policy.AllowAnyOrigin()
            .WithMethods("GET")
            .WithHeaders(AuthConstants.ApiKeyHeaderName)
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ApiKeyRateLimit", context =>
    {
        if (!context.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var apiKey))
            return RateLimitPartition.GetNoLimiter("missing-key");
        
        if (apiKey == builder.Configuration["PRIVATE_API_KEY"])
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

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("ShortTerm", policy =>
        policy.Expire(TimeSpan.FromSeconds(30)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("WithApiKey");
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseRateLimiter();
app.UseOutputCache();

app.MapGet("/books", async (
    [AsParameters] GetBooksOptions options,
    IBooksService booksService,
    CancellationToken cancellationToken) =>
{
    var books = await booksService.GetBooks(options, cancellationToken);
    var count = await booksService.GetCountAsync(cancellationToken);

    return Results.Ok(books.ToBooksResponse(options, count));
})
.RequireRateLimiting("ApiKeyRateLimit")
.AddEndpointFilter<ValidationFilter<GetBooksOptions>>()
.CacheOutput("ShortTerm")
.WithName("GetBooks")
.WithOpenApi();

app.MapGet("/books/{id}", async (int id, IBooksService booksService, CancellationToken cancellationToken) =>
{
    var book = await booksService.GetBook(id, cancellationToken);
    
    if (book is null)
        return Results.NotFound($"Book with the id {id} was not found");

    return Results.Ok(book.ToBookResponse());
})
.RequireRateLimiting("ApiKeyRateLimit")
.CacheOutput("ShortTerm")
.WithName("GetBook")
.WithOpenApi();

app.Run();
