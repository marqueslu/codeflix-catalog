using System.Collections.Generic;
using Codeflix.Catalog.Application.UseCases.Category.ListCategories;

namespace Codeflix.Catalog.UnitTests.Application.Category.ListCategories;

public class ListCategoriesTestDataGenerator
{
    public static IEnumerable<object[]> GetInputsWithoutAllParameters(int times = 12)
    {
        var fixture = new ListCategoriesTestFixture();
        var inputExample = fixture.GetExampleInput();

        for (var i = 0; i < times; i++)
        {
            yield return (i % 6) switch
            {
                0 => new object[] { new ListCategoriesInput() },
                1 => new object[] { new ListCategoriesInput(inputExample.Page) },
                2 => new object[] { new ListCategoriesInput(inputExample.Page, inputExample.PerPage) },
                3 => new object[] { new ListCategoriesInput(inputExample.Page, inputExample.PerPage, inputExample.Search) },
                4 => new object[] { new ListCategoriesInput(inputExample.Page, inputExample.PerPage, inputExample.Search, inputExample.Sort) },
                5 => new object[] { inputExample },
                _ => new object[] { new ListCategoriesInput() }
            };
        }
    }
}