using System.Threading.RateLimiting;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Netenberg.Api.Auth;
using Netenberg.Application.Services;
using Netenberg.Application.Validators;
using Netenberg.Contracts.Responses;
using Netenberg.Database.DatabaseContext;
using Netenberg.Database.Repositories;
using Netenberg.DataUpdater;
using Netenberg.Model.Entities;
using Netenberg.Model.Enums;
using Netenberg.Model.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(config => {
    config.CreateMap<Book, BookResponse>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GutenbergId));
});
builder.Services.AddScoped<IReadOnlyBookRepository, BookRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBooksService, BooksService>();
builder.Services.AddScoped<IValidator<GetBooksOptions>, GetBooksOptionsValidator>();
builder.Services.AddLogging(x => x.AddConsole());
builder.Services.AddScoped<DataUpdaterService>();
builder.Services.AddDbContextPool<NetenbergContext>(options =>
{
    string connectionString = "mongodb+srv://bencebolgovics:qfnWAeMbUhibP5Q@netenberg.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000";
    string databaseName = "netenberg";

    var client = new MongoClient(connectionString);
    options.UseMongoDB(client, databaseName);
});
builder.Services.AddCors(options =>
{
    //for testing purposes
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .WithMethods("GET")
              .AllowAnyHeader();
    });

    options.AddPolicy("PublicApiWithKey", policy =>
    {
      policy.AllowAnyOrigin()
            .WithMethods("GET")
            .WithHeaders(AuthConstants.ApiKeyHeaderName)
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy<string>("ApiKeyRateLimit", context =>
    {
        if (!context.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var apiKey))
            return RateLimitPartition.GetNoLimiter("missing-key");

        return RateLimitPartition.GetSlidingWindowLimiter<string>(
            partitionKey: apiKey!,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("PublicApiWithKey");
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseRateLimiter();

app.MapGet("/books", async (
    [FromQuery] string? ids,
    [FromQuery] string? sortBy,
    [FromQuery] int page,
    [FromQuery] int pageSize,
    IBooksService booksService,
    IMapper mapper,
    IValidator<GetBooksOptions> validator,
    CancellationToken cancellationToken) =>
{
    var options = new GetBooksOptions()
    {
        Ids = ids,
        SortBy = sortBy?.Trim('+', '-'),
        SortingOrder = sortBy is null ? SortingOrder.Unsorted : sortBy.Trim().StartsWith('+') ? SortingOrder.Ascending : SortingOrder.Descending,
        Page = page,
        PageSize = pageSize
    };
    
    var validationResult = validator.Validate(options);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var books = await booksService.GetBooks(options, cancellationToken);
    var count = await booksService.GetCountAsync(cancellationToken);
    
    var response = new BooksResponse()
    {
        Items = books.Select(x => mapper.Map<BookResponse>(x)),
        Page = page,
        PageSize = pageSize,
        Total = count
    };

    return Results.Ok(response);
})
.RequireRateLimiting("ApiKeyRateLimit")
.WithName("GetBooks")
.WithOpenApi();

app.MapGet("/books/{id}", async (int id, IMapper mapper, IBooksService booksService, CancellationToken cancellationToken) =>
{
    var book = await booksService.GetBook(id, cancellationToken);
    
    if (book is null)
        return Results.NotFound($"Book with the id {id} was not found");

    return Results.Ok(mapper.Map<BookResponse>(book));
})
.RequireRateLimiting("ApiKeyRateLimit")
.WithName("GetBook")
.WithOpenApi();

app.Run();
