using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Application.UseCases.Category.UpdateCategory;
using Codeflix.Catalog.Domain.Exceptions;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using UseCase = Codeflix.Catalog.Application.UseCases.Category.UpdateCategory;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.UpdateCategory;

[Collection(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTest
{
    private readonly UpdateCategoryTestFixture _fixture;

    public UpdateCategoryTest(UpdateCategoryTestFixture _fixture)
    {
        this._fixture = _fixture;
    }

    [Theory(DisplayName = nameof(UpdateCategory))]
    [Trait("Integration/Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 5,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateCategory(Domain.Entity.Category exampleCategory, UpdateCategoryInput input)
    {
        var dbContext = await GenerateDbData(exampleCategory);

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.UpdateCategory(
            repository,
            unitOfWork
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        var dbCategory = await (_fixture.CreateDbContext(true))
            .Categories
            .FindAsync(output.Id);

        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(input.Name);
        dbCategory.Description.Should().Be(input.Description);
        dbCategory.IsActive.Should().Be((bool)input.IsActive!);
        dbCategory.CreatedAt.Should().Be(output.CreatedAt);
        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be((bool)input.IsActive!);
    }

    [Theory(DisplayName = nameof(UpdateCategoryWithoutIsActive))]
    [Trait("Integration/Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 5,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateCategoryWithoutIsActive(Domain.Entity.Category exampleCategory, UpdateCategoryInput exampleInput)
    {
        var input = new UpdateCategoryInput(
            exampleInput.Id,
            exampleInput.Name,
            exampleInput.Description
        );
        var dbContext = await GenerateDbData(exampleCategory);

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.UpdateCategory(
            repository,
            unitOfWork
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        var dbCategory = await (_fixture.CreateDbContext(true))
            .Categories
            .FindAsync(output.Id);

        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(input.Name);
        dbCategory.Description.Should().Be(input.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive!);
        dbCategory.CreatedAt.Should().Be(output.CreatedAt);
        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(exampleCategory.IsActive);
    }

    [Theory(DisplayName = nameof(UpdateCategoryOnlyName))]
    [Trait("Integration/Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 5,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateCategoryOnlyName(Domain.Entity.Category exampleCategory, UpdateCategoryInput exampleInput)
    {
        var input = new UpdateCategoryInput(
            exampleInput.Id,
            exampleInput.Name
        );

        var dbContext = await GenerateDbData(exampleCategory);

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.UpdateCategory(
            repository,
            unitOfWork
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        var dbCategory = await (_fixture.CreateDbContext(true))
            .Categories
            .FindAsync(output.Id);

        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(input.Name);
        dbCategory.Description.Should().Be(exampleCategory.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(output.CreatedAt);
        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(exampleCategory.Description);
        output.IsActive.Should().Be(exampleCategory.IsActive);
    }

    [Fact(DisplayName = nameof(UpdateThrowsWhenNotFoundCategory))]
    [Trait("Integration/Application", "UpdateCategory - Use Cases")]
    public async Task UpdateThrowsWhenNotFoundCategory()
    {
        var input = _fixture.GetValidInput();
        var dbContext = _fixture.CreateDbContext();
        await dbContext.AddRangeAsync(_fixture.GetExampleCategoriesList());
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.UpdateCategory(
            repository,
            unitOfWork
        );

        var task = async () =>
            await useCase.Handle(input, CancellationToken.None);

        await task
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Category '{input.Id}' not found.");
    }

    [Theory(DisplayName = nameof(UpdateThrowsWhenCantInstantiateCategory))]
    [Trait("Integration/Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetInvalidInputs),
        parameters: 5,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateThrowsWhenCantInstantiateCategory(UpdateCategoryInput input, string expectedExceptionMessage)
    {
        var exampleCategories = _fixture.GetExampleCategoriesList();
        input.Id = exampleCategories.First().Id;
        var dbContext = _fixture.CreateDbContext();
        await dbContext.AddRangeAsync(exampleCategories);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.UpdateCategory(
            repository,
            unitOfWork
        );

        var task = async () =>
            await useCase.Handle(input, CancellationToken.None);

        await task
            .Should()
            .ThrowAsync<EntityValidationException>()
            .WithMessage(expectedExceptionMessage);
    }

    private async Task<CodeflixCatalogDbContext> GenerateDbData(DomainEntity.Category exampleCategory)
    {
        var dbContext = _fixture.CreateDbContext();
        await dbContext.AddRangeAsync(_fixture.GetExampleCategoriesList());
        var trackingInfo = await dbContext.AddAsync(exampleCategory);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        trackingInfo.State = EntityState.Detached;
        return dbContext;
    }
}