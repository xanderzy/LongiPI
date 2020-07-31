using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Interfaces
{
    //邮件发送服务
   public interface IMailQueueManager
    {
        void Run();
        void Stop();
        bool IsRunning { get; }
        int Count { get; }
    }
}
