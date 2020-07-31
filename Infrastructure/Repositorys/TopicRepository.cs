using ApplicationCore;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.Repositorys
{
    public class TopicRepository : Repository<Topic>, ITopicRepository
    {
        private readonly DataContext _dbContext;

        public TopicRepository(DataContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public override Topic GetById(int id)
        {
            return _dbContext.Topics.Include(r => r.User).Include(r => r.Node).FirstOrDefault(r => r.Id == id);
        }
        public override IEnumerable<Topic> List(Expression<Func<Topic, bool>> predicate)
        {
            return _dbContext.Topics.Include(r => r.User).Include(r => r.Node).Where(predicate);
        }

        public Page<Topic> PageList(int pagesize = 20, int pageindex = 1)
        {
            return PageList(null, pagesize, pageindex);
        }

        public Page<Topic> PageList(Expression<Func<Topic, bool>> predicate, int pagesize = 20, int pageindex = 1)
        {
            //var topics = _dbContext.Topics.Include(r=>r.Node).AsQueryable().AsNoTracking();
            var topics = _dbContext.Topics.Include(r => r.User).Include(r => r.Node).AsQueryable().AsNoTracking();
            if (predicate != null)
            {
                topics = topics.Where(predicate);
            }
            var count = topics.Count();
            topics = topics.OrderByDescending(r => r.CreateOn).ThenBy(r => r.Type)
                    .Skip((pageindex - 1) * pagesize).Take(pagesize);
            return new Page<Topic>(topics.ToList(), pagesize, count);
        }
    }
}
