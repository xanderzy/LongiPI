using ApplicationCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Interfaces
{
    //邮件队列存储器
    public interface IMailQueueProvider
    {
        void Enqueue(MailBox mailBox);
        bool TryDequeue(out MailBox mailBox);
        int Count { get; }
        bool IsEmpty { get; }
    }
}
