using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Application.UseCases.Category.DeleteCategory;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using useCase = Codeflix.Catalog.Application.UseCases.Category.DeleteCategory;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.DeleteCategory;

[Collection(nameof(DeleteCategoryTestFixture))]
public class DeleteCategoryTest
{
    private readonly DeleteCategoryTestFixture _fixture;

    public DeleteCategoryTest(DeleteCategoryTestFixture fixture)
    {
        this._fixture = fixture;
    }

    [Fact(DisplayName = nameof(DeleteCategory))]
    [Trait("Integration/Application", "DeleteCategory - Use Cases")]
    public async Task DeleteCategory()
    {
        var dbContext = _fixture.CreateDbContext();
        var categoryExample = _fixture.GetExampleCategory();
        var exampleList = _fixture.GetExampleCategoriesList();
        var tracking = await dbContext.AddAsync(categoryExample);
        await dbContext.AddRangeAsync(exampleList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        tracking.State = EntityState.Detached;

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var input = new DeleteCategoryInput(categoryExample.Id);
        var useCase = new useCase.DeleteCategory(
            repository,
            unitOfWork);

        await useCase.Handle(input, CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);
        var dbCategoryDeleted = await assertDbContext
            .Categories
            .FindAsync(input.Id);
        var dbCategories = await assertDbContext
            .Categories
            .ToListAsync();

        dbCategoryDeleted.Should().BeNull();
        dbCategories.Should().HaveCount(exampleList.Count);
    }

    [Fact(DisplayName = nameof(ThrowsWhenCategoryNotFound))]
    [Trait("Integration/Application", "DeleteCategory - Use Cases")]
    public async Task ThrowsWhenCategoryNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleList = _fixture.GetExampleCategoriesList();
        await dbContext.AddRangeAsync(exampleList);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var input = new DeleteCategoryInput(Guid.NewGuid());
        var useCase = new useCase.DeleteCategory(
            repository,
            unitOfWork);

        var task = async () => await useCase.Handle(input, CancellationToken.None);

        await task
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}