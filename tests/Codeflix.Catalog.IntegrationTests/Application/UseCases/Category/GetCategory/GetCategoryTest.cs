using Codeflix.Catalog.Application.Exceptions;
using UseCase = Codeflix.Catalog.Application.UseCases.Category.GetCategory;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Xunit;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.GetCategory;

[Collection(nameof(GetCategoryTestFixture))]
public class GetCategoryTest
{
    private readonly GetCategoryTestFixture _fixture;

    public GetCategoryTest(GetCategoryTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(GetCategory))]
    [Trait("Integration/Application", "GetCategory - UseCases")]
    public async Task GetCategory()
    {
        // Arrange
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        dbContext.Categories.Add(exampleCategory);
        await dbContext.SaveChangesAsync();
        var repository = new CategoryRepository(dbContext);

        var input = new UseCase.GetCategoryInput(exampleCategory.Id);
        var useCase = new UseCase.GetCategory(repository);

        // Act
        var output = await useCase.Handle(input, CancellationToken.None);

        // Assert

        output.Should().NotBeNull();
        output.Name.Should().Be(exampleCategory.Name);
        output.Description.Should().Be(exampleCategory.Description);
        output.IsActive.Should().Be(exampleCategory.IsActive);
        output.Id.Should().Be(exampleCategory.Id);
        output.CreatedAt.Should().Be(exampleCategory.CreatedAt);
    }

    [Fact(DisplayName = nameof(NotFoundExceptionWhenCategoryDoesNotExist))]
    [Trait("Integration/Application", "GetCategory - UseCases")]
    public async Task NotFoundExceptionWhenCategoryDoesNotExist()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        dbContext.Categories.Add(exampleCategory);
        await dbContext.SaveChangesAsync();
        var repository = new CategoryRepository(dbContext);

        var input = new UseCase.GetCategoryInput(Guid.NewGuid());
        var useCase = new UseCase.GetCategory(repository);

        var task = async () => await useCase.Handle(input, CancellationToken.None);

        await task
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Category '{input.Id}' not found.");
    }
}