using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsightlyToMailChimp.Core;
using InsightlyToMailChimp.Core.Models.InsightlyModels;
using InsightlyToMailChimp.Core.Services;
using InsightlyToMailChimp.IoC;
using Task = System.Threading.Tasks.Task;

namespace InsightlyToMailChimp.Scheduler
{
    public class SynchronizeEmailsJob
    {
        private static IInsightlyService _insightlyService;

        private static IMailChimpService _mailChimpService;

        private static IActiveDirectoryService _activeDirectoryService;

        public async Task Execute()
        {
            _insightlyService = DependencyResolver.Get<IInsightlyService>();
            _mailChimpService = DependencyResolver.Get<IMailChimpService>();
            _activeDirectoryService = DependencyResolver.Get<IActiveDirectoryService>();

            // get all MailChimp emails
            var mailChimpEmails = await GetMailChimpEmails();

            // get all Insightly contacts except existed MailChimp emails
            var insightlyContactsToAdd = await GetInsightlyContacts(mailChimpEmails);

            // migrate Insightly contacts to MailChimp for appropriate Branch/List
            await AddContactsToMailChimp(insightlyContactsToAdd);

            // synchronize ActiveDirectory emails with MailChimp 'TMT Employees' List
            LogHelper.DebugHeader("SYNCHRONIZE MAILCHIMP LIST WITH ACTIVE DIRECTORY");

            var activeDirectoryUsers = await _activeDirectoryService.GetAllActiveADUsers();

            await _mailChimpService.SyncWithActiveDirectory(activeDirectoryUsers);

            LogHelper.Debug("THAT'S IT FOR CURRENT RUN!");
        }

        /// <summary>
        /// Get new Insightly users to add
        /// </summary>
        /// <param name="existedEmails"></param>
        /// <returns></returns>
        private static async Task<List<InsightlyContact>> GetInsightlyContacts(List<string> existedEmails)
        {
            return await _insightlyService.GetInsightlyContacts(existedEmails);
        }

        /// <summary>
        /// Get all MailChimp user emails
        /// </summary>
        /// <returns></returns>
        private static async Task<List<string>> GetMailChimpEmails()
        {
            return await _mailChimpService.GetAllMemberEmails();
        }

        /// <summary>
        /// Add new user to MailChimp
        /// </summary>
        /// <param name="contacts"></param>
        /// <returns></returns>
        private static async Task AddContactsToMailChimp(List<InsightlyContact> contacts)
        {
            if (!contacts.Any())
            {
                Console.WriteLine("No any Contacts to add.");

                return;
            }

            var totalCount = contacts.Count;

            for (var i = 0; i < totalCount; i++)
            {
                var contact = contacts[i];

                await _mailChimpService.AddMember(contact.EmailAddress, contact.FirstName, contact.LastName,
                    contact.Branch, i + 1, totalCount);
            }
        }
    }
}