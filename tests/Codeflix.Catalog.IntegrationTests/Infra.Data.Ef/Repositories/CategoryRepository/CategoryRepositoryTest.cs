using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Xunit;
using Repository = Codeflix.Catalog.Infra.Data.EF.Repositories;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.Ef.Repositories.CategoryRepository;

[Collection(nameof(CategoryRepositoryTestFixture))]
public class CategoryRepositoryTest
{
    private readonly CategoryRepositoryTestFixture _fixture;

    public CategoryRepositoryTest(CategoryRepositoryTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Insert()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        var categoryRepository = new Repository.CategoryRepository(dbContext);

        await categoryRepository.InsertAsync(exampleCategory, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var dbCategory = await (_fixture.CreateDbContext(true)).Categories.FindAsync(exampleCategory.Id);
        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(exampleCategory.Name);
        dbCategory.Description.Should().Be(exampleCategory.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);
    }

    [Fact(DisplayName = nameof(Get))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Get()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        var exampleCategoryList = _fixture.GetExampleCategoriesList(15);
        exampleCategoryList.Add(exampleCategory);
        await dbContext.AddRangeAsync(exampleCategoryList, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var categoryRepository = new Repository.CategoryRepository(_fixture.CreateDbContext(true));
        var dbCategory = await categoryRepository.GetAsync(exampleCategory.Id, CancellationToken.None);

        dbCategory.Should().NotBeNull();
        dbCategory.Id.Should().Be(exampleCategory.Id);
        dbCategory!.Name.Should().Be(exampleCategory.Name);
        dbCategory.Description.Should().Be(exampleCategory.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);
    }

    [Fact(DisplayName = nameof(GetThrowIfNotFound))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task GetThrowIfNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleId = Guid.NewGuid();
        await dbContext
            .AddRangeAsync(_fixture.GetExampleCategoriesList(15), CancellationToken.None);
        await dbContext
            .SaveChangesAsync(CancellationToken.None);

        var categoryRepository = new Repository.CategoryRepository(dbContext);
        var task = async () => await categoryRepository.GetAsync(exampleId, CancellationToken.None);
        await task
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Category '{exampleId}' not found.");
    }

    [Fact(DisplayName = nameof(Update))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Update()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        var newCategoriesValues = _fixture.GetExampleCategory();
        var exampleCategoryList = _fixture.GetExampleCategoriesList(15);
        exampleCategoryList.Add(exampleCategory);
        await dbContext.AddRangeAsync(exampleCategoryList, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        exampleCategory.Update(newCategoriesValues.Name, newCategoriesValues.Description);

        var categoryRepository = new Repository.CategoryRepository(dbContext);
        await categoryRepository.UpdateAsync(exampleCategory, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var dbCategory = await (_fixture.CreateDbContext(true)).Categories.FindAsync(exampleCategory.Id);

        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(exampleCategory.Name);
        dbCategory.Id.Should().Be(exampleCategory.Id);
        dbCategory.Description.Should().Be(exampleCategory.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);
    }

    [Fact(DisplayName = nameof(Delete))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Delete()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        var newCategoriesValues = _fixture.GetExampleCategory();
        var exampleCategoryList = _fixture.GetExampleCategoriesList(15);
        exampleCategoryList.Add(exampleCategory);
        await dbContext.AddRangeAsync(exampleCategoryList, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        exampleCategory.Update(newCategoriesValues.Name, newCategoriesValues.Description);

        var categoryRepository = new Repository.CategoryRepository(dbContext);
        await categoryRepository.DeleteAsync(exampleCategory, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var dbCategory = await (_fixture.CreateDbContext()).Categories.FindAsync(exampleCategory.Id);

        dbCategory.Should().BeNull();
    }

    [Fact(DisplayName = nameof(SearchReturnsListAndTotal))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task SearchReturnsListAndTotal()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategoryList = _fixture.GetExampleCategoriesList(15);
        await dbContext.AddRangeAsync(exampleCategoryList, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);

        var categoryRepository = new Repository.CategoryRepository(dbContext);
        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
        output.Total.Should().Be(exampleCategoryList.Count);
        output.Items.Should().HaveCount(exampleCategoryList.Count);
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

    [Fact(DisplayName = nameof(SearchReturnsEmptyWhenPersistenceIsEmpty))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task SearchReturnsEmptyWhenPersistenceIsEmpty()
    {
        var dbContext = _fixture.CreateDbContext();
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);

        var categoryRepository = new Repository.CategoryRepository(dbContext);
        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().HaveCount(0);
    }

    [Theory(DisplayName = nameof(SearchReturnsPaginated))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
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
        var searchInput = new SearchInput(page, perPage, "", "", SearchOrder.Asc);

        var categoryRepository = new Repository.CategoryRepository(dbContext);
        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
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
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
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
        var searchInput = new SearchInput(page, perPage, search, "", SearchOrder.Asc);

        var categoryRepository = new Repository.CategoryRepository(dbContext);
        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
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
    
    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
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
        var searchInput = new SearchInput(1, 20, "", orderBy, searchOrder);

        var categoryRepository = new Repository.CategoryRepository(dbContext);
        var output = await categoryRepository.Search(searchInput, CancellationToken.None);
        var expectedOrderedList = _fixture.CloneCategoriesListOrdered(exampleCategoryList, orderBy, searchOrder);
        
        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
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