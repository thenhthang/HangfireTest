using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HangfireBlazorServerApp.Data;
using Hangfire;
using Hangfire.MemoryStorage;
using HangfireBlazorServerApp.Services;
using Hangfire.Storage;

namespace HangfireBlazorServerApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            //services.AddHangfire(c => c.UseMemoryStorage());
            services.AddHangfire(c => c.UseSqlServerStorage(Configuration.GetConnectionString("DBConnection"),
                new Hangfire.SqlServer.SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true // Migration to Schema 7 is required
                }));
            services.AddHangfireServer();
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<MailService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseHangfireDashboard("/jobs");
            
            //using (var connection = JobStorage.Current.GetConnection())
            //{
            //    foreach (var recurringJob in connection.GetRecurringJobs())
            //    {
            //        RecurringJob.RemoveIfExists(recurringJob.Id);
            //    }
            //}
            RemoveAllHangfireJobs();
            //RecurringJob.RemoveIfExists("RecurringJobId");
            RecurringJobManager recurringJobManager = new RecurringJobManager();
            
            
            recurringJobManager.RemoveIfExists("recurringJobid1");
            recurringJobManager.AddOrUpdate("recurringJobid1", () => Console.WriteLine("Haha"),"* * * * * *");
            //BackgroundJob.Enqueue(() => Console.WriteLine("Hello, {0}!", "world"));
            //RecurringJob.AddOrUpdate(() =>
            //Console.WriteLine("Chay moi 1 phut "+DateTime.Now.ToLongTimeString()),Cron.Minutely,TimeZoneInfo.Local);
            //BackgroundJob.Schedule(() => Console.WriteLine("Schedule"), TimeSpan.FromMinutes(1));
            //RecurringJob.AddOrUpdate<MailManagerService>(mamager => mamager.SendAll(), Cron.Minutely, TimeZoneInfo.Local);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
        private void RemoveAllHangfireJobs()
        {
            var hangfireMonitor = JobStorage.Current.GetMonitoringApi();
            
            //RecurringJobs
            JobStorage.Current.GetConnection().GetRecurringJobs().ForEach(xx => RecurringJob.RemoveIfExists(xx.Id)); // this line changed!

            //ProcessingJobs
            hangfireMonitor.ProcessingJobs(0, int.MaxValue).ForEach(xx => BackgroundJob.Delete(xx.Key));

            //ScheduledJobs
            hangfireMonitor.ScheduledJobs(0, int.MaxValue).ForEach(xx => BackgroundJob.Delete(xx.Key));

            //EnqueuedJobs
            hangfireMonitor.Queues().ToList().ForEach(xx => hangfireMonitor.EnqueuedJobs(xx.Name, 0, int.MaxValue).ForEach(x => BackgroundJob.Delete(x.Key)));
        }
    }
}
