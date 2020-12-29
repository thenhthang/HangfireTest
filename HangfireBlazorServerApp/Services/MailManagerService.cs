using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Components;

namespace HangfireBlazorServerApp.Services
{
    public class MailManagerService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IJobCancellationToken _jobCancellationToken;
        public MailManagerService(IBackgroundJobClient backgroundJobClient,
                                IRecurringJobManager recurringJobManager,
                                IJobCancellationToken jobCancellationToken
                                )
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
            _jobCancellationToken = jobCancellationToken;
        }
        public async Task SendAll()
        {
            _jobCancellationToken.ThrowIfCancellationRequested();
            
            
            for(int i = 0; i < 100; i++)
            {
                 _backgroundJobClient.Enqueue<MailService>(
                    mail =>mail.Send(i,"To "+i.ToString(),"Hello Mr "+i.ToString()));
                 await Task.Delay(1000);
            }
        }
    }
}
