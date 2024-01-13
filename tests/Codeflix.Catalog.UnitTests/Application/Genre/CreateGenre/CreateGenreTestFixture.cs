using System;
using System.Linq;
using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Application.UseCases.Genre.CreateGenre;
using Codeflix.Catalog.Domain.Repository;
using Codeflix.Catalog.UnitTests.Application.Genre.Common;
using Moq;
using Xunit;

namespace Codeflix.Catalog.UnitTests.Application.Genre.CreateGenre;

[CollectionDefinition(nameof(CreateGenreTestFixture))]
public class CreateGenreTestFixtureCollection : ICollectionFixture<CreateGenreTestFixture>
{
}

public class CreateGenreTestFixture : GenreUseCasesBaseFixture
{
    public CreateGenreInput GetExampleInput()
        => new(GetValidGenreName(), GetRandomBoolean());
    
    public CreateGenreInput GetExampleInput(string name)
        => new(name, GetRandomBoolean());

    public CreateGenreInput GetExampleInputWithCategories()
    {
        var numberOfCategoriesIds = new Random().Next(1, 10);

        var categoriesIds = Enumerable
            .Range(1, numberOfCategoriesIds)
            .Select(_ => Guid.NewGuid())
            .ToList();

        return new(GetValidGenreName(), GetRandomBoolean(), categoriesIds);
    }

    public Mock<IGenreRepository> GetGenreRepositoryMock()
        => new();

    public Mock<ICategoryRepository> GetCategoryRepositoryMock()
        => new();

    public Mock<IUnitOfWork> GetUnitOfWorkMock()
        => new();
}