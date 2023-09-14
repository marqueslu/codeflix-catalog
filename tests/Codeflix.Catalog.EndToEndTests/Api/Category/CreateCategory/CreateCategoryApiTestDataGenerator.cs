namespace Codeflix.Catalog.EndToEndTests.Api.Category.CreateCategory;

public class CreateCategoryApiTestDataGenerator
{
    public static IEnumerable<object[]> GetInvalidInputs()
    {
        var fixture = new CreateCategoryApiTestFixture();
        var invalidInputsList = new List<object[]>();
        const int totalInvalidCases = 3;

        for (var index = 0; index < totalInvalidCases; index++)
        {
            switch (index % totalInvalidCases)
            {
                case 0:
                    var input0 = fixture.GetExampleInput();
                    input0.Name = fixture.GetInvalidNameToShort();
                    invalidInputsList.Add(new object[]
                    {
                        input0,
                        "Name should be at least 3 characters long"
                    });
                    break;
                case 1:
                    var input1 = fixture.GetExampleInput();
                    input1.Name = fixture.GetInvalidNameToLong();
                    invalidInputsList.Add(new object[]
                    {
                        input1,
                        "Name should be less or equal 255 characters long"
                    });
                    break;
                case 2:
                    var input2 = fixture.GetExampleInput();
                    input2.Description = fixture.GetInvalidDescriptionToLong();
                    invalidInputsList.Add(new object[]
                    {
                        input2,
                        "Description should be less or equal 10000 characters long"
                    });
                    break;
            }
        }

        return invalidInputsList;
    }
}