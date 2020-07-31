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
    public class TopicReplyRepository : Repository<TopicReply>, ITopicReplyRepository
    {
        private readonly DataContext _dbContext;
        public TopicReplyRepository(DataContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public override TopicReply GetById(int id)
        {
            return _dbContext.TopicReplys.Include(r => r.ReplyUser).FirstOrDefault(r => r.Id == id);
        }

        public override IEnumerable<TopicReply> List(Expression<Func<TopicReply, bool>> predicate)
        {
            return _dbContext.TopicReplys.Include(r => r.ReplyUser).Where(predicate);
        }
    }
}
