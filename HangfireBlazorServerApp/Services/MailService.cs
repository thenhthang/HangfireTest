using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HangfireBlazorServerApp.Services
{
    public class MailService
    {
        private SmtpClient BuildClient()
        {
            var smtpClient = new SmtpClient("host", 0);
            try
            {
                smtpClient.Credentials = new NetworkCredential("username", "password");
                smtpClient.EnableSsl = true;
            }
            catch (Exception ex)
            {
                smtpClient.Dispose();
                throw ex;
            }
            return smtpClient;
        }
        public async Task Send(int i,string to,string subject)
        {
            try
            {
                await Task.Delay(1000);
                if (i % 2 == 0)
                {
                    int a = i / 0;
                }
                Console.WriteLine($"To {to}. Subject {subject}");
            }catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
