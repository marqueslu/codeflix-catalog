using System.Net;
using Codeflix.Catalog.Application.UseCases.Category.ListCategories;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
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
            .Get<ListCategoriesOutput>(
                $"/categories"
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Page.Should().Be(1);
        output.PerPage.Should().Be(defaultPerPage);
        output.Items.Should().HaveCount(defaultPerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);
        foreach (var outputItem in output.Items)
        {
            var exampleItem = exampleCategoriesList
                .FirstOrDefault(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }

    [Fact(DisplayName = nameof(ItemsEmptyWhenPersistenceEmpty))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ItemsEmptyWhenPersistenceEmpty()
    {
        var (response, output) = await _fixture
            .ApiClient
            .Get<ListCategoriesOutput>(
                $"/categories"
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(0);
        output.Total.Should().Be(0);
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
            .Get<ListCategoriesOutput>(
                $"/categories",
                input
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(input.PerPage);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);

        foreach (var outputItem in output.Items)
        {
            var exampleItem = exampleCategoriesList
                .FirstOrDefault(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
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
            .Get<ListCategoriesOutput>(
                $"/categories",
                input
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(expectedQuantityItems);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);

        foreach (var outputItem in output.Items)
        {
            var exampleItem = exampleCategoriesList
                .FirstOrDefault(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
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
            .Get<ListCategoriesOutput>(
                $"/categories",
                input
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(expectedQuantityReturned);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(expectedQuantityTotalItems);

        foreach (var outputItem in output.Items)
        {
            var exampleItem = exampleCategoriesList
                .FirstOrDefault(x => x.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem.Id.Should().Be(exampleItem!.Id);
            outputItem.Name.Should().Be(exampleItem.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
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
            .Get<ListCategoriesOutput>(
                $"/categories",
                input
            );

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Items.Should().HaveCount(exampleCategoriesList.Count);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(exampleCategoriesList.Count);
        var expectedOrderedList = _fixture.CloneCategoriesListOrdered(
            exampleCategoriesList,
            input.Sort,
            input.Dir
        );
        for (var index = 0; index < expectedOrderedList.Count; index++)
        {
            var expectedItem = expectedOrderedList[index];
            var outputItem = output.Items[index];
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Description.Should().Be(expectedItem.Description);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.Should().Be(expectedItem.CreatedAt);
        }
    }

    public void Dispose()
        => _fixture.CleanPersistence();
}