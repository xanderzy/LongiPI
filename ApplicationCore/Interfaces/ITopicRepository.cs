﻿using ApplicationCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ApplicationCore.Interfaces
{
    public interface ITopicRepository : IRepository<Topic>
    {
        Page<Topic> PageList(int pagesize, int pageindex);
        Page<Topic> PageList(Expression<Func<Topic, bool>> predicate, int pagesize, int pageindex);
    }
}
