using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Repositorys
{
        public class MyFileRepository : Repository<MyFile>, IMyFileRepository
        {
            private readonly DataContext _dbContext;
            public MyFileRepository(DataContext dbContext) : base(dbContext)
            {
                _dbContext = dbContext;
            }
    }
}
