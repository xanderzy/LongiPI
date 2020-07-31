using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositorys
{
    //实现邮件队列提供其
   public class MailQueueProvider:IMailQueueProvider
    {
        private static readonly ConcurrentQueue<MailBox> _mailQueue = new ConcurrentQueue<MailBox>();
        public int Count => _mailQueue.Count;
        public bool IsEmpty => _mailQueue.IsEmpty;
        public void Enqueue(MailBox mailBox)
        {
            _mailQueue.Enqueue(mailBox);
        }
        public bool TryDequeue(out MailBox mailBox)
        {
            return _mailQueue.TryDequeue(out mailBox);
        }
    }
}
