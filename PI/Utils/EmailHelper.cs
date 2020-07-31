using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace PI.Utils
{
    public class EmailHelper
    {
        public static bool SendEmail(string toemail, string toname, string totitle, string tobody)
        {
            bool isok = true;
            string fromemail = "zjservices@longi-silicon.com";
            string frompassword = "Longi@456";
            try
            {

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("提案改善系统", fromemail));
                message.To.Add(new MailboxAddress(toname, toemail));
                message.Subject = totitle;
                message.Body = new TextPart("html") { Text = tobody };
                using (var client = new SmtpClient())
                {
                    client.Timeout = 10 * 1000;   //设置超时时间
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect("mail.longi-silicon.com", 465, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(fromemail, frompassword);
                    client.Send(message);
                    client.Disconnect(true);
                }
                return isok;
            }
            catch (Exception)
            {
                isok = false;
                return isok;
            }
        }
    }
}
