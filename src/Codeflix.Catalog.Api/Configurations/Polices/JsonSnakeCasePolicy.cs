using System.Text.Json;
using Codeflix.Catalog.Api.Extensions.String;

namespace Codeflix.Catalog.Api.Configurations.Polices;

public class JsonSnakeCasePolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
        =>
            name.ToSnakeCase();
}