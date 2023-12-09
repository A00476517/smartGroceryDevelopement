using SmartGroceryApp.DataAccess.Data;
using SmartGroceryApp.DataAccess.Repository.IRepository;
using SmartGroceryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartGroceryApp.DataAccess.Repository
{
    public class CardTypeRepository : Repository<CardType> , ICardTypeRepository
	{
        private ApplicationDbContext _db;
        public CardTypeRepository(ApplicationDbContext db) :base(db)
        {
            _db = db;   
        }

        //public void Save()
        //{
        //   _db.SaveChanges();
        //}

        public void Update(CardType obj)
        {
            
            _db.CardTypes.Update(obj);
        }
    }
}
