using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositorys
{
    public class MailQueueService : IMailQueueService
    {
        private readonly IMailQueueProvider _provider;

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="provider"></param>
        public MailQueueService(IMailQueueProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="box"></param>
        public void Enqueue(MailBox box)
        {
            _provider.Enqueue(box);
        }
    }
}
