using ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ApplicationCore.Entities;
using System.Threading;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using System.Diagnostics;
using System.Linq;
using MimeKit;

namespace Infrastructure.Repositorys
{
   public class MailQueueManager : IMailQueueManager
    {
        //private readonly SmtpClient _client;
        private readonly IMailQueueProvider _provider;
        //private readonly ILogger<MailQueueManager> _logger;
        //private readonly EmailOptions _options;
        private bool _isRunning = false;
        private bool _tryStop = false;
        private Thread _thread;

        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// IOptions<EmailOptions> options
        /// , ILogger<MailQueueManager> logger
        public MailQueueManager(IMailQueueProvider provider)
        {
            //_options = options.Value;
            /*
            _client = new SmtpClient
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                ServerCertificateValidationCallback = (s, c, h, e) => true
            };

            // Note: since we don't have an OAuth2 token, disable
            // the XOAUTH2 authentication mechanism.
            _client.AuthenticationMechanisms.Remove("XOAUTH2");
            /*if (_options.DisableOAuth)
            {
                _client.AuthenticationMechanisms.Remove("XOAUTH2");
            }*/

            _provider = provider;
            //_logger = logger;
        }

        /// <summary>
        /// 正在运行
        /// </summary>
        public bool IsRunning => IsRunning1;

        /// <summary>
        /// 计数
        /// </summary>
        public int Count => _provider.Count;

        public bool IsRunning1 { get => _isRunning; set => _isRunning = value; }

        /// <summary>
        /// 启动队列
        /// </summary>
        public void Run()
        {
            if (IsRunning1 || (_thread != null && _thread.IsAlive))
            {
                //_logger.LogWarning("已经运行，又被启动了,新线程启动已经取消");
                return;
            }
            IsRunning1 = true;
            _thread = new Thread(StartSendMail)
            {
                Name = "PmpEmailQueue",
                IsBackground = true,
            };
            //_logger.LogInformation("线程即将启动");
            _thread.Start();
           // _logger.LogInformation("线程已经启动，线程Id是：{0}", _thread.ManagedThreadId);
        }

        /// <summary>
        /// 停止队列
        /// </summary>
        public void Stop()
        {
            if (_tryStop)
            {
                return;
            }
            _tryStop = true;
        }

        private void StartSendMail()
        {
            var sw = new Stopwatch();
            try
            {
                while (true)
                {
                    if (_tryStop)
                    {
                        break;
                    }

                    if (_provider.IsEmpty)
                    {
                        //_logger.LogTrace("队列是空，开始睡眠");
                        //Thread.Sleep(_options.SleepInterval);
                        Thread.Sleep(1200000);
                        continue;
                    }
                    if (_provider.TryDequeue(out MailBox box))
                    {
                        //_logger.LogInformation("开始发送邮件 标题：{0},收件人 {1}", box.Subject, box.To.First());
                        sw.Restart();
                        SendMail(box);
                        sw.Stop();
                       // _logger.LogInformation("发送邮件结束标题：{0},收件人 {1},耗时{2}", box.Subject, box.To.First(), sw.Elapsed.TotalSeconds);
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "循环中出错,线程即将结束");
                IsRunning1 = false;
            }

            //_logger.LogInformation("邮件发送线程即将停止，人为跳出循环，没有异常发生");
            _tryStop = false;
            IsRunning1 = false;
        }

        private void SendMail(MailBox box)
        {
            if (box == null)
            {
                throw new ArgumentNullException(nameof(box));
            }

            try
            {
                MimeMessage message = ConvertToMimeMessage(box);
                SendMail(message);
            }
            catch (Exception exception)
            {
                //_logger.LogError(exception, "发送邮件发生异常主题：{0},收件人：{1}", box.Subject, box.To.First());
            }
            /*finally
            {
                if (box.Attachments != null && box.Attachments.Any())
                {
                    foreach (var item in box.Attachments)
                    {
                        item.Dispose();
                    }
                }
            }*/
        }

        private MimeMessage ConvertToMimeMessage(MailBox box)
        {
            var message = new MimeMessage();

            var from = InternetAddress.Parse("zjservices@longi-silicon.com");
            from.Name = "zjservices@longi-silicon.com";
            message.From.Add(from);
            if (!box.To.Any())
            {
                throw new ArgumentNullException("to必须含有值");
            }
            foreach (var item in box.To) { 
            message.To.Add(new MailboxAddress(item));
            }
            /*
            if (box.Cc != null && box.Cc.Any())
            {
                message.Cc.AddRange(box.Cc.Convert());
            }*/

            message.Subject = box.Subject;

            var builder = new BodyBuilder();

            if (box.IsHtml)
            {
                builder.HtmlBody = box.Body;
            }
            else
            {
                builder.TextBody = box.Body;
            }

           /* if (box.Attachments != null && box.Attachments.Any())
            {
                foreach (var item in GetAttechments(box.Attachments))
                {
                    builder.Attachments.Add(item);
                }
            }*/

            message.Body = builder.ToMessageBody();
            return message;
        }

        private void SendMail(MimeMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Timeout = 10 * 1000;   //设置超时时间
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect("mail.longigroup.com", 465, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate("zjservices@longi-silicon.com", "Longi@123");
                    client.Send(message);
                    client.Disconnect(true);
                }
                /*_client.Connect(_options.Host, _options.Port, false);
                // Note: only needed if the SMTP server requires authentication
                if (!_client.IsAuthenticated)
                {
                    _client.Authenticate(_options.UserName, _options.Password);
                }
                _client.Send(message);*/
            }
            catch(Exception ex)
            {
                string exinfo = ex.ToString();
                int aa=1111;
            }
            finally
            {
                //client.Disconnect(false);
            }
        }

        /*
        private AttachmentCollection GetAttechments(IEnumerable<IAttachment> attachments)
        {
            if (attachments == null)
            {
                throw new ArgumentNullException(nameof(attachments));
            }

            AttachmentCollection collection = new AttachmentCollection();
            List<Stream> list = new List<Stream>(attachments.Count());

            foreach (var item in attachments)
            {
                var fileName = item.GetName();
                var fileType = MimeTypes.GetMimeType(fileName);
                var contentTypeArr = fileType.Split('/');
                var contentType = new ContentType(contentTypeArr[0], contentTypeArr[1]);
                MimePart attachment = null;
                Stream fs = null;
                try
                {
                    fs = item.GetFileStream();
                    list.Add(fs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "读取文件流发生异常");
                    fs?.Dispose();
                    continue;
                }

                attachment = new MimePart(contentType)
                {
                    Content = new MimeContent(fs),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };

                var charset = "UTF-8";
                attachment.ContentType.Parameters.Add(charset, "name", fileName);
                attachment.ContentDisposition.Parameters.Add(charset, "filename", fileName);

                foreach (var param in attachment.ContentDisposition.Parameters)
                {
                    param.EncodingMethod = ParameterEncodingMethod.Rfc2047;
                }

                foreach (var param in attachment.ContentType.Parameters)
                {
                    param.EncodingMethod = ParameterEncodingMethod.Rfc2047;
                }

                collection.Add(attachment);
            }
            return collection;
        }
        */
    }
}
