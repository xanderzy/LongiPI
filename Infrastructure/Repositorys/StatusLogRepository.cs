using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositorys
{
    public class StatusLogRepository : Repository<StatusLog>, IStatusLogRepository
    {
        private readonly DataContext _dbContext;
        public StatusLogRepository(DataContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
