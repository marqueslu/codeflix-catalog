using System;
using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Application.UseCases.Category.CreateCategory;
using Codeflix.Catalog.Domain.Repository;
using Codeflix.Catalog.UnitTests.Common;
using Moq;
using Xunit;

namespace Codeflix.Catalog.UnitTests.Application.CreateCategory;

[CollectionDefinition(nameof(CreateCategoryTestFixture))]
public class CreateCategoryTestFixtureCollection : ICollectionFixture<CreateCategoryTestFixture>
{
}

public class CreateCategoryTestFixture : BaseFixture
{
    public string GetValidCategoryName()
    {
        var categoryName = "";
        while (categoryName.Length < 3)
            categoryName = Faker.Commerce.Categories(1)[0];

        if (categoryName.Length > 255)
            categoryName = categoryName[..255];

        return categoryName;
    }

    public string GetValidCategoryDescription()
    {
        var categoryDescription = Faker.Commerce.ProductDescription();
        if (categoryDescription.Length > 10_000)
            categoryDescription = categoryDescription[..10_000];

        return categoryDescription;
    }

    public bool GetRandomBoolean()
        => (new Random()).NextDouble() < 0.5;

    public CreateCategoryInput GetInput()
        => new(
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean()
        );

    public CreateCategoryInput GetInvalidInputShortName()
    {
        var invalidInputShortName = GetInput();
        invalidInputShortName.Name = invalidInputShortName.Name[..2];
        return invalidInputShortName;
    }

    public CreateCategoryInput GetInvalidInputToLongName()
    {
        var invalidInputLongName = GetInput();
        invalidInputLongName.Name = Faker.Lorem.Letter(256);
        return invalidInputLongName;
    }

    public CreateCategoryInput GetInvalidInputNullDescription()
    {
        var invalidInputDescriptionNull = GetInput();
        invalidInputDescriptionNull.Description = null;
        return invalidInputDescriptionNull;
    }

    public CreateCategoryInput GetInvalidInputToLongDescription()
    {
        var invalidInputLongDescription = GetInput();
        invalidInputLongDescription.Description = Faker.Lorem.Letter(10001);
        return invalidInputLongDescription;
    }
    
    public Mock<ICategoryRepository> GetRepositoryMock()
        => new();

    public Mock<IUnitOfWork> GetUnitOfWorkMock()
        => new();
}