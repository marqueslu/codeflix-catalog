using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.Common;
using Xunit;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.ListCategories;

[CollectionDefinition(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTextFixtureCollection : ICollectionFixture<ListCategoriesTestFixture>
{
}

public class ListCategoriesTestFixture : CategoryUseCasesBaseFixture
{
    public List<DomainEntity.Category> CloneCategoriesListOrdered(IEnumerable<DomainEntity.Category> categoriesList, string orderBy, SearchOrder order)
    {
        var listClone = new List<DomainEntity.Category>(categoriesList);
        var orderedEnumerable = (orderBy.ToLower(), order) switch
        {
            ("name", SearchOrder.Asc) => listClone.OrderBy(x => x.Name)
                .ThenBy(x => x.Id),
            ("name", SearchOrder.Desc) => listClone.OrderByDescending(x => x.Name)
                .ThenBy(x => x.Id),
            ("id", SearchOrder.Asc) => listClone.OrderBy(x => x.Id),
            ("id", SearchOrder.Desc) => listClone.OrderByDescending(x => x.Id),
            ("createdat", SearchOrder.Asc) => listClone.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.Desc) => listClone.OrderByDescending(x => x.CreatedAt),
            _ => listClone.OrderBy(x => x.Name)
                .ThenBy(x => x.Id),
        };
        return orderedEnumerable.ToList();
    }
}