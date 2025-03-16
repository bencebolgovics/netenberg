using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Netenberg.Application.Services;
using Netenberg.Application.Validators;
using Netenberg.Contracts.Responses;
using Netenberg.Database.Repositories;
using Netenberg.DataUpdater;
using Netenberg.Model.Entities;
using Netenberg.Model.Enums;
using Netenberg.Model.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Book, BookResponse>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GutenbergId));
});

var mapper = configuration.CreateMapper();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IReadOnlyBookRepository, BookRepository>();
builder.Services.AddScoped<IBooksService, BooksService>();
builder.Services.AddScoped<IValidator<GetBooksOptions>, GetBooksOptionsValidator>();
builder.Services.AddLogging(x => x.AddConsole());
builder.Services.AddScoped<DataUpdaterService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapGet("/books", async (
    [FromQuery] string? ids,
    [FromQuery] string? sortBy,
    [FromQuery] int page,
    [FromQuery] int pageSize,
    IBooksService booksService,
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
#if !DEBUG
.AddEndpointFilter<ApiKeyAuthFilter>()
# endif
.WithName("GetBooks")
.WithOpenApi();

app.MapGet("/books/{id}", async (int id, IBooksService booksService, CancellationToken cancellationToken) =>
{
    var book = await booksService.GetBook(id, cancellationToken);
    
    if (book is null)
        return Results.NotFound($"Book with the id {id} was not found");

    return Results.Ok(mapper.Map<BookResponse>(book));
})
#if !DEBUG
.AddEndpointFilter<ApiKeyAuthFilter>()
# endif
.WithName("GetBook")
.WithOpenApi();

app.MapGet("/update", async (DataUpdaterService dataUpdaterService, CancellationToken cancellationToken) =>
{
    await dataUpdaterService.UpdateDatabase(cancellationToken);

    return Results.Ok();
})
#if !DEBUG
.AddEndpointFilter<ApiKeyAuthFilter>()
# endif
.WithName("Update")
.WithOpenApi();

app.Run();
