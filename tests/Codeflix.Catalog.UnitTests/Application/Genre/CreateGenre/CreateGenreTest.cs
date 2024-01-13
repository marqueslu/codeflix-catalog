using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;
using DomainEntity = Codeflix.Catalog.Domain.Entity;
using UseCase = Codeflix.Catalog.Application.UseCases.Genre.CreateGenre;

namespace Codeflix.Catalog.UnitTests.Application.Genre.CreateGenre;

[Collection(nameof(CreateGenreTestFixture))]
public class CreateGenreTest
{
    private readonly CreateGenreTestFixture _fixture;

    public CreateGenreTest(CreateGenreTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(Create))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task Create()
    {
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var useCase = new UseCase.CreateGenre(
            genreRepositoryMock.Object,
            unitOfWorkMock.Object,
            categoryRepositoryMock.Object
        );
        var input = _fixture.GetExampleInput();

        var dateTimeBefore = DateTime.Now;
        var output = await useCase.Handle(input, CancellationToken.None);
        var dateTimeAfter = DateTime.Now.AddSeconds(1);

        genreRepositoryMock
            .Verify(x => x.InsertAsync(
                It.IsAny<DomainEntity.Genre>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        unitOfWorkMock
            .Verify(x => x.Commit(
                It.IsAny<CancellationToken>()
            ), Times.Once);

        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.Categories.Should().HaveCount(0);
        output.CreatedAt.Should().NotBeSameDateAs(default);
        (output.CreatedAt >= dateTimeBefore).Should().BeTrue();
        (output.CreatedAt <= dateTimeAfter).Should().BeTrue();
    }

    [Fact(DisplayName = nameof(CreateWithRelatedCategories))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task CreateWithRelatedCategories()
    {
        var input = _fixture.GetExampleInputWithCategories();
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var useCase = new UseCase.CreateGenre(
            genreRepositoryMock.Object,
            unitOfWorkMock.Object,
            categoryRepositoryMock.Object
        );

        categoryRepositoryMock.Setup(
            x => x.GetIdsListByIdsAsync(
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync(
            input.CategoriesIds!
        );


        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepositoryMock
            .Verify(x => x.InsertAsync(
                It.IsAny<DomainEntity.Genre>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        unitOfWorkMock
            .Verify(x => x.Commit(
                It.IsAny<CancellationToken>()
            ), Times.Once);
        categoryRepositoryMock.Verify(
            x => x.GetIdsListByIdsAsync(
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        
        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.Categories.Should().HaveCount(input.CategoriesIds?.Count ?? 0);
        input.CategoriesIds?
            .ForEach(id => output.Categories.Should().Contain(id));
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(CreateThrowWhenRelatedCategoryNotFound))]
    [Trait("Application", "CreateGenre - Use Cases")]
    public async Task CreateThrowWhenRelatedCategoryNotFound()
    {
        var input = _fixture.GetExampleInputWithCategories();
        var exampleGuid = input.CategoriesIds![^1];

        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();

        categoryRepositoryMock.Setup(
            x => x.GetIdsListByIdsAsync(
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync(
            input.CategoriesIds
                .FindAll(x => x != exampleGuid)
        );
        var useCase = new UseCase.CreateGenre(
            genreRepositoryMock.Object,
            unitOfWorkMock.Object,
            categoryRepositoryMock.Object
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);
        await action
            .Should()
            .ThrowAsync<RelatedAggregateException>()
            .WithMessage($"Related category id (or ids) not found: '{exampleGuid}'");
        categoryRepositoryMock.Verify(
            x => x.GetIdsListByIdsAsync(
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
    
    [Theory(DisplayName = nameof(ThrowWhenNameIsInvalid))]
    [Trait("Application", "CreateGenre - Use Cases")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task ThrowWhenNameIsInvalid(string name)
    {
        var input = _fixture.GetExampleInput(name);
        var genreRepositoryMock = _fixture.GetGenreRepositoryMock();
        var categoryRepositoryMock = _fixture.GetCategoryRepositoryMock();
        var unitOfWorkMock = _fixture.GetUnitOfWorkMock();
        
        var useCase = new UseCase.CreateGenre(
            genreRepositoryMock.Object,
            unitOfWorkMock.Object,
            categoryRepositoryMock.Object
        );

        var action = async () => await useCase.Handle(input, CancellationToken.None);
        await action
            .Should()
            .ThrowAsync<EntityValidationException>()
            .WithMessage($"Name should not be empty or null");
    }
}