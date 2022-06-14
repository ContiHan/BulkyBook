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
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private ApplicationDbContext _db;

        public CoverTypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(CoverType coverType)
        {
            if (coverType is null)
            {
                throw new ArgumentNullException(nameof(coverType));
            }

            if (_db.CoverTypes is null)
            {
                throw new ArgumentNullException(nameof(_db.CoverTypes));
            }

            _db.CoverTypes.Update(coverType);
        }
    }
}
