using System;
using System.Collections.Generic;
using System.Text;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;

namespace Infrastructure.Repositorys
{
    public class BonusRepository : Repository<Bonus>, IBonusRepository
    {
        private readonly DataContext _dbContext;
        public BonusRepository(DataContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
    
}
