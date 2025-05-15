using FluentValidation;
using Netenberg.Api.Auth;
using Netenberg.Api.Configuration;
using Netenberg.Api.Mapper;
using Netenberg.Api.Validation;
using Netenberg.Application.Services;
using Netenberg.Model.Options;

var builder = WebApplication.CreateBuilder(args);

var app = await ServiceConfigurator.ConfigureServices(builder);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors();
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
.RequireCors("WithApiKey")
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
.RequireCors("WithApiKey")
.CacheOutput("ShortTerm")
.WithName("GetBook")
.WithOpenApi();

app.Run();
