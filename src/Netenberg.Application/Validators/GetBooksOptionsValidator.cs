using FluentValidation;
using Netenberg.Model.Options;

namespace Netenberg.Application.Validators;

public sealed class GetBooksOptionsValidator : AbstractValidator<GetBooksOptions>
{
    private static readonly string[] ValidSortingFields = ["id", "title", "downloads", "publicationDate"];

    public GetBooksOptionsValidator()
    {
        RuleFor(c => c.SortBy)
            .Must(x =>
        {
            return string.IsNullOrEmpty(x) || ValidSortingFields.Contains(x, StringComparer.OrdinalIgnoreCase);
        });

        RuleFor(c => c.PageSize)
            .InclusiveBetween(1, 40);
    }
}