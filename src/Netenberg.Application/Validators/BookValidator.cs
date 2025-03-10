using FluentValidation;
using Netenberg.Model.Entities;

namespace Netenberg.Application.Validators;

public class BookValidator : AbstractValidator<Book>
{
    public BookValidator()
    {

    }
}
