using ExpenseReconciliation.DataContext;
using ExpenseReconciliation.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseReconciliation.Repository
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> ListAllAsync();
        Task<Category> FindByIdAsync(int id);
        Task<Category> AddAsync(Category category);
        Task DeleteAsync(Category category);

    }

    public class CategoryRepository : RepositoryBase, ICategoryRepository
    {
        private readonly ILogger<ICategoryRepository> _logger;
        
        public CategoryRepository(AppDbContext appDbContext,
            ILogger<ICategoryRepository> logger) : base(appDbContext)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Category>> ListAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }
        
        public async Task<Category> FindByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteAsync(Category category)
        {
            if (category != null)
            {
                if (await IsCategoryParent(category.Id))
                {
                    _logger.LogInformation("Couldn't delete category with id {Id} because it has child categories", category.Id);
                    throw new Exception("Can't delete category with children");
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<bool> IsCategoryParent(int id)
        {
            return await _context.Categories.Where(c => c.ParentId == id).AnyAsync();
        }
    }
}