﻿using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Company company)
        {
            if (company is null)
            {
                throw new ArgumentNullException(nameof(company));
            }

            if (_db.Companies is null)
            {
                throw new ArgumentNullException(nameof(_db.Companies));
            }

            _db.Companies.Update(company);
        }
    }
}
