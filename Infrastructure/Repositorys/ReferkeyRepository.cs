using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositorys
{
   
    public class ReferkeyRepository : Repository<Referkey>, IReferkeyRepository
    {
        private readonly DataContext _dbContext;
        public ReferkeyRepository(DataContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
