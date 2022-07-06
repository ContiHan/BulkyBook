using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _db;

        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCart shoppingCart)
        {
            if (shoppingCart is null)
            {
                throw new ArgumentNullException(nameof(shoppingCart));
            }

            if (_db.ShoppingCarts is null)
            {
                throw new ArgumentNullException(nameof(_db.ShoppingCarts));
            }

            _db.ShoppingCarts.Update(shoppingCart);
        }
    }
}
