using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartGroceryApp.DataAccess.Data;
using SmartGroceryApp.DataAccess.Repository.IRepository;
using SmartGroceryApp.Models;

namespace SmartGroceryApp.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public ICategoryRepository Category { get; private set; }

        public ICompanyRepository Company { get; private set; }

		public ICardTypeRepository CardType { get; private set; }


		public ICountryRepository Country { get; private set; }


        public IProductRepository Product { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IOrderHeaderRepository OrderHeader { get; private set; }

        public IOrderDetailRepository OrderDetail { get; private set; }

        public IProductImageRepository ProductImage { get; private set; }



        public UnitOfWork(ApplicationDbContext db) 
        {
            _db = db;
            Category =  new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);

			CardType = new CardTypeRepository(_db);

			Country = new CountryRepository(_db);

            ShoppingCart = new ShoppingCartRepository(_db);

            ApplicationUser = new ApplicationUserRepository(_db);

            OrderHeader = new OrderHeaderRepository(_db);

            OrderDetail = new OrderDetailRepository(_db);


            ProductImage = new ProductImageRepository(_db);
        }
       

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
