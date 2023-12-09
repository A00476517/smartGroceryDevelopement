using SmartGroceryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartGroceryApp.DataAccess.Repository.IRepository
{
    public interface ICardTypeRepository : IRepository<CardType>
    {
        void Update(CardType obj);
       // void Save();
    }
}
