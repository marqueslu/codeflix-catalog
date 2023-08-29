using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.IntegrationTests.Base;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Codeflix.Catalog.IntegrationTests.Repositories.CategoryRepository;

[CollectionDefinition(nameof(CategoryRepositoryTestFixture))]
public class CategoryRepositoryTestFixtureCollection : ICollectionFixture<CategoryRepositoryTestFixture>
{
}

public class CategoryRepositoryTestFixture : BaseFixture
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

    public Category GetExampleCategory()
        => new(
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean()
        );

    public List<Category> GetExampleCategoryList(int length = 10)
        => Enumerable
            .Range(0, length)
            .Select(_ => new Category(
                GetValidCategoryName(),
                GetValidCategoryDescription(),
                GetRandomBoolean()
            ))
            .ToList();

    public List<Category> GetExampleCategoriesListWithNames(IEnumerable<string> names)
        => names.Select(name =>
        {
            var category = GetExampleCategory();
            category.Update(name);
            return category;
        }).ToList();

    public List<Category> CloneCategoriesListOrdered(IEnumerable<Category> categoriesList, string orderBy, SearchOrder order)
    {
        var listClone = new List<Category>(categoriesList);
        var orderedEnumerable = (orderBy.ToLower(), order) switch
        {
            ("name", SearchOrder.Asc) => listClone.OrderBy(x => x.Name),
            ("name", SearchOrder.Desc) => listClone.OrderByDescending(x => x.Name),
            ("id", SearchOrder.Asc) => listClone.OrderBy(x => x.Id),
            ("id", SearchOrder.Desc) => listClone.OrderByDescending(x => x.Id),
            ("createdat", SearchOrder.Asc) => listClone.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.Desc) => listClone.OrderByDescending(x => x.CreatedAt),
            _ => listClone.OrderBy(x => x.Name),
        };
        return orderedEnumerable.ToList();
    }

    public CodeflixCatalogDbContext CreateDbContext(bool preserveData = false)
    {
        var context = new CodeflixCatalogDbContext(
            new DbContextOptionsBuilder<CodeflixCatalogDbContext>()
                .UseInMemoryDatabase("integration-tests-db")
                .Options
        );
        if (!preserveData)
            context.Database.EnsureDeleted();
        return context;
    }
}