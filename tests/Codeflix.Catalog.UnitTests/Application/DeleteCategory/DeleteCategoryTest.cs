using System;
using System.Threading;
using System.Threading.Tasks;
using Codeflix.Catalog.Application.Exceptions;
using Xunit;
using Codeflix.Catalog.Application.UseCases.Category.DeleteCategory;
using Codeflix.Catalog.Domain.Entity;
using FluentAssertions;
using Moq;
using useCase = Codeflix.Catalog.Application.UseCases.Category.DeleteCategory;

namespace Codeflix.Catalog.UnitTests.Application.DeleteCategory;

[Collection(nameof(DeleteCategoryTestFixture))]
public class DeleteCategoryTest
{
    private readonly DeleteCategoryTestFixture _fixture;


    public DeleteCategoryTest(DeleteCategoryTestFixture fixture)
        =>
            _fixture = fixture;

    [Fact(DisplayName = nameof(DeleteCategoryTest))]
    [Trait("Application", "DeleteCategory - Use Cases")]
    public async Task DeleteCategory()
    {
        // Arrange
        var repositoryMock = _fixture.GetRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        var categoryExample = _fixture.GetValidCategory();

        repositoryMock
            .Setup(x =>
                x.GetAsync(categoryExample.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryExample);

        repositoryMock
            .Setup(x =>
                x.DeleteAsync(categoryExample, It.IsAny<CancellationToken>()));

        var input = new DeleteCategoryInput(categoryExample.Id);
        var useCase = new useCase.DeleteCategory(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        // Act
        await useCase.Handle(input, CancellationToken.None);

        repositoryMock
            .Verify(x =>
                    x.GetAsync(categoryExample.Id, It.IsAny<CancellationToken>()),
                Times.Once);
        
        repositoryMock
            .Verify(x =>
                    x.DeleteAsync(categoryExample, It.IsAny<CancellationToken>()),
                Times.Once);

        unitOfWorkMock
            .Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowsWhenCategoryNotFound))]
    [Trait("Application", "DeleteCategory - Use Cases")]
    public async Task ThrowsWhenCategoryNotFound()
    {
        // Arrange
        var repositoryMock = _fixture.GetRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        var exampleGuid = Guid.NewGuid();

        repositoryMock
            .Setup(x =>
                x.GetAsync(exampleGuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Category '{exampleGuid}' not found."));

        var input = new DeleteCategoryInput(exampleGuid);
        var useCase = new useCase.DeleteCategory(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        // Act
        var task = async () => await useCase.Handle(input, CancellationToken.None);

        await task
            .Should()
            .ThrowAsync<NotFoundException>();

        repositoryMock
            .Verify(x =>
                    x.GetAsync(exampleGuid, It.IsAny<CancellationToken>()),
                Times.Once);
        
        repositoryMock
            .Verify(x =>
                    x.DeleteAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
                Times.Never);

        unitOfWorkMock
            .Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}