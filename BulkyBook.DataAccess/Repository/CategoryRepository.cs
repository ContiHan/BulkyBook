using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category category)
        {
            if (category is null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            if (_db.Categories is null)
            {
                throw new ArgumentNullException(nameof(_db.Categories));
            }

            _db.Categories.Update(category);
        }

        public void Wipe()
        {
            _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Categories]");
        }
    }
}
