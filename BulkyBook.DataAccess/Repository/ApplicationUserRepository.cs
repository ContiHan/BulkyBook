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
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ApplicationUser applicationUser)
        {
            if (applicationUser is null)
            {
                throw new ArgumentNullException(nameof(applicationUser));
            }

            if (_db.ApplicationUsers is null)
            {
                throw new ArgumentNullException(nameof(_db.ApplicationUsers));
            }

            _db.ApplicationUsers.Update(applicationUser);
        }
    }
}
