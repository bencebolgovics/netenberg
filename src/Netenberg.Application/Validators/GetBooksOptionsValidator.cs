using FluentValidation;
using Netenberg.Model.Models;

namespace Netenberg.Application.Validators;

public sealed class GetBooksOptionsValidator : AbstractValidator<GetBooksOptions>
{
    private static readonly string[] ValidSortingFields = ["id", "title", "author"];

    public GetBooksOptionsValidator()
    {
        RuleFor(c => c.SortBy).Must(x =>
        {
            return x is null || ValidSortingFields.Contains(x, StringComparer.OrdinalIgnoreCase);
        });
    }
}