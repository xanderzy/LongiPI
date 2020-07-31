using ApplicationCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Interfaces
{
    //邮件入队列服务
   public  interface IMailQueueService
    {
        void Enqueue(MailBox box);
    }
}
