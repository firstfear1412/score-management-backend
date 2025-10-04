using ScoreManagement.Common;
using ScoreManagement.Entity;
using System.Net.Mail;

namespace ScoreManagement.Services
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;
        private string _mail_host;
        private string _mail_port;
        private string _mail_from;
        private string _mail_display;
        private string _mail_pass;
        private string _mail_timeout;
        public MailService(IConfiguration configuration) {
            _configuration = configuration;
            _mail_host = _configuration["MAIL:HOST"]!.ToString();
            _mail_port = _configuration["MAIL:PORT"]!.ToString();
            _mail_from = _configuration["MAIL:FROM"]!.ToString();
            _mail_display = _configuration["MAIL:DISPLAY"]!.ToString();
            _mail_pass = _configuration["MAIL:PASS"]!.ToString();
            _mail_timeout = _configuration["MAIL:TIMEOUT"]!.ToString();
        }

        public bool SendMail(string topic, string body, string mailTo)
        {
            return SendMail(topic, body, mailTo, false);
        }
        public bool SendMail(string topic, string body, string mailTo, bool isBodyHTML)
        {
            bool isSuccess = false;
            WebEvent _webEvent = new WebEvent();
            MailMessage Mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(_mail_host);
            Mail.From = new MailAddress(_mail_from, _mail_display);
            mailTo.Split(',').ToList().ForEach(mailTo => { Mail.To.Add(mailTo.Trim()); });
            Mail.Subject = topic;
            Mail.Body = body;
            Mail.IsBodyHtml = isBodyHTML;

            // For test send mail
            //string ccAddresses = "pamornpon.t@ku.th"; 
            string ccAddresses = _mail_from;
            ccAddresses.Split(',').ToList().ForEach(cc => { Mail.CC.Add(cc.Trim()); });

            SmtpServer.Port = int.Parse(_mail_port);
            SmtpServer.Timeout = int.Parse(_mail_timeout);
            SmtpServer.EnableSsl = true;
            SmtpServer.Credentials = new System.Net.NetworkCredential(_mail_from, _mail_pass);
            //SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            //SmtpServer.UseDefaultCredentials = true;

            try
            {
                SmtpServer.Send(Mail);
                isSuccess = true;
            }
            catch (Exception ex)
            {
                // Log or handle exception
                throw new Exception($"Error sending email: {ex.Message}", ex);
            }
            return isSuccess;
        }
    }
}
