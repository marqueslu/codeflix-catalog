using Codeflix.Catalog.Application.UseCases.Category.UpdateCategory;
using Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.Common;
using Xunit;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.UpdateCategory;

[CollectionDefinition(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTestFixtureCollection : ICollectionFixture<UpdateCategoryTestFixture>
{
}

public class UpdateCategoryTestFixture : CategoryUseCasesBaseFixture
{
    public UpdateCategoryInput GetInvalidInputShortName()
    {
        var invalidInputShortName = GetValidInput();
        invalidInputShortName.Name = invalidInputShortName.Name[..2];
        return invalidInputShortName;
    }

    public UpdateCategoryInput GetInvalidInputToLongName()
    {
        var invalidInputLongName = GetValidInput();
        invalidInputLongName.Name = Faker.Lorem.Letter(256);
        return invalidInputLongName;
    }

    public UpdateCategoryInput GetInvalidInputToLongDescription()
    {
        var invalidInputLongDescription = GetValidInput();
        invalidInputLongDescription.Description = Faker.Lorem.Letter(10001);
        return invalidInputLongDescription;
    }

    public UpdateCategoryInput GetValidInput(Guid? id = null)
        => new(
            id ?? Guid.NewGuid(),
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean()
        );
}