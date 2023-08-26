using Codeflix.Catalog.Application.Exceptions;
using FluentAssertions;
using Xunit;
using Repository = Codeflix.Catalog.Infra.Data.EF.Repositories;

namespace Codeflix.Catalog.IntegrationTests.Repositories.CategoryRepository;

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
        var exampleCategoryList = _fixture.GetExampleCategoryList(15);
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
            .AddRangeAsync(_fixture.GetExampleCategoryList(15), CancellationToken.None);
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
        var exampleCategoryList = _fixture.GetExampleCategoryList(15);
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
        var exampleCategoryList = _fixture.GetExampleCategoryList(15);
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
}