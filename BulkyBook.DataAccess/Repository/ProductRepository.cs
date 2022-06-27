using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            if (product is null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            if (_db.Products is null)
            {
                throw new ArgumentNullException("Product DB is null", nameof(_db.Products));
            }

            var productFromDb = _db.Products.FirstOrDefault(p => p.Id == product.Id);
            if (productFromDb is null)
            {
                throw new ArgumentNullException(nameof(productFromDb));
            }

            productFromDb.Title = product.Title;
            productFromDb.Description = product.Description;
            productFromDb.ISBN = product.ISBN;
            productFromDb.Author = product.Author;
            productFromDb.ListPrice = product.ListPrice;
            productFromDb.Price = product.Price;
            productFromDb.Price50 = product.Price50;
            productFromDb.Price100 = product.Price100;
            productFromDb.CategoryId = product.CategoryId;
            productFromDb.CoverTypeId = product.CoverTypeId;
            if (product.ImageUrl is not null)
            {
                productFromDb.ImageUrl = product.ImageUrl;
            }
        }
    }
}
