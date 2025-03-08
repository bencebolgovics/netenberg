using Microsoft.AspNetCore.Mvc;
using Netenberg.Application.Services;

namespace Netenberg.Api.Controllers;

[Route("api/books")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksService _booksService;

    public BooksController(IBooksService booksService)
    {
        _booksService = booksService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks(CancellationToken cancellationToken)
    {
        var books = await _booksService.GetBooks(cancellationToken);
        
        return Ok(books);
    }

    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }
}
