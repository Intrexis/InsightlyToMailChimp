using System;
using InsightlyToMailChimp.Core;
using InsightlyToMailChimp.Core.Services;
using InsightlyToMailChimp.IoC;
using InsightlyToMailChimp.Scheduler;
using InsightlyToMailChimp.Services;
using SimpleInjector;

namespace InsightlyToMailChimp
{
    internal class Program
    {
        private static void Main()
        {
            var container = new Container();

            container.Register<IInsightlyService, InsightlyService>();
            container.Register<IMailChimpService, MailChimpService>();
            container.Register<IActiveDirectoryService, ActiveDirectoryService>();

            container.Verify();

            DependencyResolver.SetupContainer(container);

            try
            {
                new SynchronizeEmailsJob().Execute().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.InnerException?.Message ?? ex.Message);
            }

            Console.ReadKey();
        }
    }
}