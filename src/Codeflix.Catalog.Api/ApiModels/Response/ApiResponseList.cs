using Codeflix.Catalog.Application.Common;

namespace Codeflix.Catalog.Api.ApiModels.Response;

public class ApiResponseList<TITemData> : ApiResponse<IReadOnlyList<TITemData>>
{
    public ApiResponseListMeta Meta { get; private set; }

    public ApiResponseList(
        int currentPage,
        int perPage,
        int total,
        IReadOnlyList<TITemData> data
    ) : base(data)
    {
        Meta = new(currentPage, perPage, total);
    }

    public ApiResponseList(
        PaginatedListOutput<TITemData> paginatedListOutput
    ) : base(paginatedListOutput.Items)
    {
        Meta = new(
            paginatedListOutput.Page,
            paginatedListOutput.PerPage,
            paginatedListOutput.Total
        );
    }
}