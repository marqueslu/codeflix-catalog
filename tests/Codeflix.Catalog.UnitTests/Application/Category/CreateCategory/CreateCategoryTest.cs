using System;
using System.Threading;
using System.Threading.Tasks;
using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;
using UseCase = Codeflix.Catalog.Application.UseCases.Category.CreateCategory;

namespace Codeflix.Catalog.UnitTests.Application.Category.CreateCategory;

[Collection(nameof(CreateCategoryTestFixture))]
public class CreateCategoryTest
{
    private readonly CreateCategoryTestFixture _fixture;

    public CreateCategoryTest(CreateCategoryTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(CreateCategory))]
    [Trait("Application", "CreateCategory - Use Cases")]
    public async Task CreateCategory()
    {
        // Arrange
        var repositoryMock = _fixture.GetRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();

        var useCase = new UseCase.CreateCategory(
            repositoryMock.Object,
            unitOfWorkMock.Object
        );
        var input = _fixture.GetInput();

        // Act 
        var output = await useCase.Handle(input, CancellationToken.None);

        // Assert
        repositoryMock.Verify(
            repository => repository.InsertAsync(
                It.IsAny<Catalog.Domain.Entity.Category>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(
            uow => uow.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(input.IsActive);
        (output.Id != Guid.Empty).Should().BeTrue();
        (output.CreatedAt != default(DateTime)).Should().BeTrue();
    }

    [Fact(DisplayName = nameof(CreateCategoryWithOnlyName))]
    [Trait("Application", "CreateCategory - Use Cases")]
    public async Task CreateCategoryWithOnlyName()
    {
        // Arrange
        var repositoryMock = _fixture.GetRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();

        var useCase = new UseCase.CreateCategory(
            repositoryMock.Object,
            unitOfWorkMock.Object
        );
        var input = new UseCase.CreateCategoryInput(_fixture.GetValidCategoryName());

        // Act 
        var output = await useCase.Handle(input, CancellationToken.None);

        // Assert
        repositoryMock.Verify(
            repository => repository.InsertAsync(
                It.IsAny<Catalog.Domain.Entity.Category>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(
            uow => uow.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be("");
        output.IsActive.Should().BeTrue();
        (output.Id != Guid.Empty).Should().BeTrue();
        (output.CreatedAt != default(DateTime)).Should().BeTrue();
    }

    [Fact(DisplayName = nameof(CreateCategoryWithOnlyNameAndDescription))]
    [Trait("Application", "CreateCategory - Use Cases")]
    public async Task CreateCategoryWithOnlyNameAndDescription()
    {
        // Arrange
        var repositoryMock = _fixture.GetRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();

        var useCase = new UseCase.CreateCategory(
            repositoryMock.Object,
            unitOfWorkMock.Object
        );
        var input = new UseCase.CreateCategoryInput(
            _fixture.GetValidCategoryName(),
            _fixture.GetValidCategoryDescription()
        );

        // Act 
        var output = await useCase.Handle(input, CancellationToken.None);

        // Assert
        repositoryMock.Verify(
            repository => repository.InsertAsync(
                It.IsAny<Catalog.Domain.Entity.Category>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(
            uow => uow.Commit(It.IsAny<CancellationToken>()),
            Times.Once);

        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().BeTrue();
        (output.Id != Guid.Empty).Should().BeTrue();
        (output.CreatedAt != default(DateTime)).Should().BeTrue();
    }

    [Theory(DisplayName = nameof(ThrowWhenCantInstantiateCategory))]
    [Trait("Application", "CreateCategory - Use Cases")]
    [MemberData(
        nameof(CreateCategoryTestDataGenerator.GetInvalidInputs),
        parameters: 12,
        MemberType = typeof(CreateCategoryTestDataGenerator)
    )]
    public async Task ThrowWhenCantInstantiateCategory(UseCase.CreateCategoryInput input, string exceptionMessage)
    {
        // Arrange
        var useCase = new UseCase.CreateCategory(
            _fixture.GetRepositoryMock().Object,
            _fixture.GetUnitOfWorkMock().Object
        );

        // Act / Assert
        Func<Task> task = async () => await useCase.Handle(input, CancellationToken.None);
        await task
            .Should()
            .ThrowAsync<EntityValidationException>()
            .WithMessage(exceptionMessage);
    }
}