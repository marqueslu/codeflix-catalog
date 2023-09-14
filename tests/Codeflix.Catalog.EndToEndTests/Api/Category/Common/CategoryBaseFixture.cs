using Codeflix.Catalog.Application.UseCases.Category.CreateCategory;
using Codeflix.Catalog.EndToEndTests.Base;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.Common;

public class CategoryBaseFixture : BaseFixture
{
    public CategoryPersistence Persistence;

    public CategoryBaseFixture()
    {
        Persistence = new CategoryPersistence(CreateDbContext());
    }

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
    
    public string GetInvalidNameToShort()
    {
        return Faker.Commerce.ProductName()[..2];
    }

    public string GetInvalidNameToLong()
    {
        return Faker.Lorem.Letter(256);
    }

    public string GetInvalidDescriptionToLong()
    {
        return Faker.Lorem.Letter(10001);
    }

    public CreateCategoryInput GetExampleInput()
        => new(
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean()
        );
}