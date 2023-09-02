using Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.Common;
using Xunit;
namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.ListCategories;

[CollectionDefinition(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTextFixtureCollection : ICollectionFixture<ListCategoriesTestFixture>
{
}

public class ListCategoriesTestFixture : CategoryUseCasesBaseFixture
{
}