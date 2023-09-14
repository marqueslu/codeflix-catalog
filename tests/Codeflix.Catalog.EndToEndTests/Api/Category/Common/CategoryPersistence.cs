using Codeflix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.Common;

using DomainEntity = Codeflix.Catalog.Domain.Entity;

public class CategoryPersistence
{
    private readonly CodeflixCatalogDbContext _dbContext;

    public CategoryPersistence(CodeflixCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DomainEntity.Category?> GetById(Guid id)
    {
        return await _dbContext
            .Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}