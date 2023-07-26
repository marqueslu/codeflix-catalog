using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Application.UseCases.Category.Common;
using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Category.UpdateCategory;

public class UpdateCategory : IUpdateCategory
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategory(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryModelOutput> Handle(UpdateCategoryInput request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetAsync(request.Id, cancellationToken);

        category.Update(request.Name, request.Description);
        if (request.IsActive is not null && request.IsActive != category.IsActive)
            if ((bool)request.IsActive!) category.Activate();
            else category.Deactivate();

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        return CategoryModelOutput.FromCategory(category);
    }
}