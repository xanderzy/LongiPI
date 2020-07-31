using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositorys
{
    public class MarkInfoRepository : Repository<MarkInfo>, IMarkInfoRepository
    {
        private readonly DataContext _dbContext;
        public MarkInfoRepository(DataContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
