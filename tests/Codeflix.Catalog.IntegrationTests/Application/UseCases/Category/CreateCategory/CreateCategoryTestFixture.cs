using Codeflix.Catalog.Application.UseCases.Category.CreateCategory;
using Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.Common;
using Xunit;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.CreateCategory;

[CollectionDefinition(nameof(CreateCategoryTestFixture))]
public class CreateCategoryTestFixtureCollection : ICollectionFixture<CreateCategoryTestFixture>
{
}

public class CreateCategoryTestFixture : CategoryUseCasesBaseFixture
{
    public CreateCategoryInput GetInput()
    {
        var exampleCategory = GetExampleCategory();
        return new(
            exampleCategory.Name,
            exampleCategory.Description,
            exampleCategory.IsActive
        );
    }

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
}