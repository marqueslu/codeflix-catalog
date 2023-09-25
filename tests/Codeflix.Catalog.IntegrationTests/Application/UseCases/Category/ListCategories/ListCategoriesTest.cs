using Codeflix.Catalog.Application.UseCases.Category.ListCategories;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Xunit;
using UseCase = Codeflix.Catalog.Application.UseCases.Category.ListCategories;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.ListCategories;

[Collection(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTest
{
    private readonly ListCategoriesTestFixture _fixture;

    public ListCategoriesTest(ListCategoriesTestFixture fixture)
    {
        this._fixture = fixture;
    }

    [Fact(DisplayName = nameof(SearchReturnsListAndTotal))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    public async Task SearchReturnsListAndTotal()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategoryList = _fixture.GetExampleCategoriesList(15);
        await dbContext.AddRangeAsync(exampleCategoryList, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var input = new ListCategoriesInput(1, 20, "", "", SearchOrder.Asc);

        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListCategories(categoryRepository);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleCategoryList.Count);
        output.Items.Should().HaveCount(exampleCategoryList.Count);
        foreach (var outputItem in output.Items)
        {
            var exampleItem = exampleCategoryList.Find(
                category => category.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem!.Name.Should().Be(exampleItem!.Name);
            outputItem.Id.Should().Be(exampleItem.Id);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }

    [Fact(DisplayName = nameof(SearchReturnsEmptyWhenPersistenceIsEmpty))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    public async Task SearchReturnsEmptyWhenPersistenceIsEmpty()
    {
        var dbContext = _fixture.CreateDbContext();
        var input = new ListCategoriesInput(1, 20, "", "", SearchOrder.Asc);

        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListCategories(categoryRepository);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().HaveCount(0);
    }

    [Theory(DisplayName = nameof(SearchReturnsPaginated))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task SearchReturnsPaginated(
        int quantityCategoryToGenerate,
        int page,
        int perPage,
        int expectedQuantityItems
    )
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategoryList = _fixture.GetExampleCategoriesList(quantityCategoryToGenerate);
        await dbContext.AddRangeAsync(exampleCategoryList, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var input = new ListCategoriesInput(page, perPage, "", "", SearchOrder.Asc);

        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListCategories(categoryRepository);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(quantityCategoryToGenerate);
        output.Items.Should().HaveCount(expectedQuantityItems);
        foreach (var outputItem in output.Items)
        {
            var exampleItem = exampleCategoryList.Find(
                category => category.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem!.Name.Should().Be(exampleItem.Name);
            outputItem.Id.Should().Be(exampleItem.Id);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData("Action", 1, 5, 1, 1)]
    [InlineData("Horror", 1, 5, 3, 3)]
    [InlineData("Horror", 2, 5, 0, 3)]
    [InlineData("Sci-fi", 1, 5, 4, 4)]
    [InlineData("Sci-fi", 1, 2, 2, 4)]
    [InlineData("Sci-fi", 2, 3, 1, 4)]
    [InlineData("Sci-fi Other", 1, 3, 0, 0)]
    [InlineData("IA", 1, 5, 2, 2)]
    public async Task SearchByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityReturned,
        int expectedQuantityTotalItems
    )
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategoryList = _fixture.GetExampleCategoriesListWithNames(new List<string>()
        {
            "Action",
            "Horror",
            "Horror - IA",
            "Horror - Based on Real Facts",
            "Drama",
            "Comedy",
            "Sci-fi - IA",
            "Sci-fi - Space",
            "Sci-fi - Robots",
            "Sci-fi - Future",
        });
        await dbContext.AddRangeAsync(exampleCategoryList, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var input = new ListCategoriesInput(page, perPage, search);

        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListCategories(categoryRepository);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(expectedQuantityTotalItems);
        output.Items.Should().HaveCount(expectedQuantityReturned);
        foreach (var outputItem in output.Items)
        {
            var exampleItem = exampleCategoryList.Find(
                category => category.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem!.Name.Should().Be(exampleItem.Name);
            outputItem.Id.Should().Be(exampleItem.Id);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    public async Task SearchOrdered(
        string orderBy,
        string order
    )
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategoryList = _fixture.GetExampleCategoriesList();
        await dbContext.AddRangeAsync(exampleCategoryList, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var searchOrder = order.ToLower() == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new ListCategoriesInput(1, 20, "", orderBy, searchOrder);
        var expectedOrderedList = _fixture.CloneCategoriesListOrdered(exampleCategoryList, orderBy, searchOrder);

        var categoryRepository = new CategoryRepository(dbContext);
        var useCase = new UseCase.ListCategories(categoryRepository);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleCategoryList.Count);
        output.Items.Should().HaveCount(exampleCategoryList.Count);
        for (var index = 0; index < expectedOrderedList.Count; index++)
        {
            var expectedItem = expectedOrderedList[index];
            var outputItem = output.Items[index];
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem!.Name.Should().Be(expectedItem.Name);
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Description.Should().Be(expectedItem.Description);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.Should().Be(expectedItem.CreatedAt);
        }
    }
}