using System.Net;
using Codeflix.Catalog.Application.UseCases.Category.Common;
using Codeflix.Catalog.Application.UseCases.Category.ListCategories;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.EndToEndTests.Extensions;
using Codeflix.Catalog.EndToEndTests.Models;
using FluentAssertions;
using Xunit;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.ListCategories;

[Collection(nameof(ListCategoriesApiTestFixture))]
public class ListCategoriesApiTest : IDisposable
{
    private readonly ListCategoriesApiTestFixture _fixture;

    public ListCategoriesApiTest(ListCategoriesApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(ListCategoriesAndTotalByDefault))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ListCategoriesAndTotalByDefault()
    {
        var defaultPerPage = 15;
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(20);
        await _fixture.Persistence.InsertList(exampleCategoriesList);

        var (response, output) = await _fixture
            .ApiClient
            .Get<TestApiResponseList<CategoryModelOutput>>(
                $"/categories"
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output.Meta.Should().NotBeNull();
        output.Meta.CurrentPage.Should().Be(1);
        output.Meta.PerPage.Should().Be(defaultPerPage);
        output.Data.Should().HaveCount(defaultPerPage);
        output.Meta.Total.Should().Be(exampleCategoriesList.Count);
        foreach (var outputItem in output.Data)
        {
            var exampleItem = exampleCategoriesList
                .FirstOrDefault(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.TrimMilliseconds().Should().Be(exampleItem.CreatedAt.TrimMilliseconds());
        }
    }

    [Fact(DisplayName = nameof(ItemsEmptyWhenPersistenceEmpty))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ItemsEmptyWhenPersistenceEmpty()
    {
        var (response, output) = await _fixture
            .ApiClient
            .Get<TestApiResponseList<CategoryModelOutput>>(
                $"/categories"
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(0);
        output.Meta.Total.Should().Be(0);
    }

    [Fact(DisplayName = nameof(ListCategoriesAndTotal))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ListCategoriesAndTotal()
    {
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(20);
        await _fixture.Persistence.InsertList(exampleCategoriesList);
        var input = new ListCategoriesInput(page: 1, perPage: 5);

        var (response, output) = await _fixture
            .ApiClient
            .Get<TestApiResponseList<CategoryModelOutput>>(
                $"/categories",
                input
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(input.PerPage);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
        output.Meta.Total.Should().Be(exampleCategoriesList.Count);

        foreach (var outputItem in output.Data)
        {
            var exampleItem = exampleCategoriesList
                .FirstOrDefault(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.TrimMilliseconds().Should().Be(exampleItem.CreatedAt.TrimMilliseconds());
        }
    }

    [Theory(DisplayName = nameof(ListPaginated))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListPaginated(
        int quantityCategoryToGenerate,
        int page,
        int perPage,
        int expectedQuantityItems
    )
    {
        var exampleCategoriesList = _fixture.GetExampleCategoriesList(quantityCategoryToGenerate);
        await _fixture.Persistence.InsertList(exampleCategoriesList);
        var input = new ListCategoriesInput(page, perPage);

        var (response, output) = await _fixture
            .ApiClient
            .Get<TestApiResponseList<CategoryModelOutput>>(
                $"/categories",
                input
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(expectedQuantityItems);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
        output.Meta.Total.Should().Be(exampleCategoriesList.Count);

        foreach (var outputItem in output.Data)
        {
            var exampleItem = exampleCategoriesList
                .FirstOrDefault(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.TrimMilliseconds().Should().Be(exampleItem.CreatedAt.TrimMilliseconds());
        }
    }

    [Theory(DisplayName = nameof(ListByText))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    [InlineData("Action", 1, 5, 1, 1)]
    [InlineData("Horror", 1, 5, 3, 3)]
    [InlineData("Horror", 2, 5, 0, 3)]
    [InlineData("Sci-fi", 1, 5, 4, 4)]
    [InlineData("Sci-fi", 1, 2, 2, 4)]
    [InlineData("Sci-fi", 2, 3, 1, 4)]
    [InlineData("Sci-fi Other", 1, 3, 0, 0)]
    [InlineData("IA", 1, 5, 2, 2)]
    public async Task ListByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityReturned,
        int expectedQuantityTotalItems
    )
    {
        var exampleCategoriesList = _fixture.GetExampleCategoriesListWithNames(new List<string>()
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
        await _fixture.Persistence.InsertList(exampleCategoriesList);
        var input = new ListCategoriesInput(page, perPage, search);

        var (response, output) = await _fixture
            .ApiClient
            .Get<TestApiResponseList<CategoryModelOutput>>(
                $"/categories",
                input
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(expectedQuantityReturned);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
        output.Meta.Total.Should().Be(expectedQuantityTotalItems);

        foreach (var outputItem in output.Data)
        {
            var exampleItem = exampleCategoriesList
                .FirstOrDefault(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.TrimMilliseconds().Should().Be(exampleItem.CreatedAt.TrimMilliseconds());
        }
    }

    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    public async Task ListOrdered(
        string orderBy,
        string order
    )
    {
        var exampleCategoriesList = _fixture.GetExampleCategoriesList();
        await _fixture.Persistence.InsertList(exampleCategoriesList);
        var inputOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new ListCategoriesInput(
            page: 1,
            perPage: 20,
            sort: orderBy,
            dir: inputOrder
        );

        var (response, output) = await _fixture
            .ApiClient
            .Get<TestApiResponseList<CategoryModelOutput>>(
                $"/categories",
                input
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(exampleCategoriesList.Count);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
        output.Meta.Total.Should().Be(exampleCategoriesList.Count);
        var expectedOrderedList = _fixture.CloneCategoriesListOrdered(
            exampleCategoriesList,
            input.Sort,
            input.Dir
        );
        for (var index = 0; index < expectedOrderedList.Count; index++)
        {
            var expectedItem = expectedOrderedList[index];
            var outputItem = output.Data[index];
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Description.Should().Be(expectedItem.Description);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.TrimMilliseconds().Should().Be(expectedItem.CreatedAt.TrimMilliseconds());
        }
    }

    [Theory(DisplayName = nameof(ListOrderedDates))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    public async Task ListOrderedDates(
        string orderBy,
        string order
    )
    {
        var exampleCategoriesList = _fixture.GetExampleCategoriesList();
        await _fixture.Persistence.InsertList(exampleCategoriesList);
        var inputOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new ListCategoriesInput(
            page: 1,
            perPage: 20,
            sort: orderBy,
            dir: inputOrder
        );

        var (response, output) = await _fixture
            .ApiClient
            .Get<TestApiResponseList<CategoryModelOutput>>(
                $"/categories",
                input
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(exampleCategoriesList.Count);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
        output.Meta.Total.Should().Be(exampleCategoriesList.Count);
        DateTime? lastItemDate = null;
        foreach (var outputItem in output.Data)
        {
            var exampleItem = exampleCategoriesList
                .FirstOrDefault(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.TrimMilliseconds().Should().Be(exampleItem.CreatedAt.TrimMilliseconds());
            if (lastItemDate is not null)
            {
                if (order is "asc")
                    Assert.True(outputItem.CreatedAt >= lastItemDate);
                else
                    Assert.True(outputItem.CreatedAt <= lastItemDate);
            }

            lastItemDate = outputItem.CreatedAt;
        }
    }

    public void Dispose()
        => _fixture.CleanPersistence();
}