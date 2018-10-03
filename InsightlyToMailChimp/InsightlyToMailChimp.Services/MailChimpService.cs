using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using InsightlyToMailChimp.Core;
using InsightlyToMailChimp.Core.Models.ActiveDirectory;
using InsightlyToMailChimp.Core.Services;
using MailChimp.Net;
using MailChimp.Net.Interfaces;
using MailChimp.Net.Models;

namespace InsightlyToMailChimp.Services
{
    public class MailChimpService : IMailChimpService
    {
        #region Fields

        private const string FirstNameKey = "FNAME";

        private const string LastNameKey = "LNAME";

        private const string TmtEmployeesList = "TMT Employees";

        private readonly IMailChimpManager _mailChimpManager;

        #endregion

        #region Constructor

        public MailChimpService()
        {
            _mailChimpManager = new MailChimpManager(ConfigurationManager.AppSettings["MailChimpApiKey"]);
        }

        #endregion

        #region Public methods

        public async Task<List<string>> GetAllMemberEmails()
        {
            LogHelper.DebugHeader("GET ALL MAILCHIMP EMAILS");

            var allLists = (await _mailChimpManager.Lists.GetAllAsync()).ToList();

            var emails = new List<string>();

            allLists.ForEach(list =>
            {
                LogHelper.Debug($"{Environment.NewLine}MailChimp List '{list.Name}'");

                LogHelper.Debug(" Emails:");

                var members = this._mailChimpManager.Members.GetAllAsync(list.Id).GetAwaiter().GetResult().ToList();

                var listEmails = members.Select(m => m.EmailAddress).ToList();

                listEmails.ForEach(e => LogHelper.Debug($"   {e}"));

                emails.AddRange(listEmails);
            });

            return emails
                .Distinct()
                .Select(e => e.ToLower())
                .OrderBy(e => e)
                .ToList();
        }

        public async Task AddMember(string email, string firstName, string lastName, string branch, int index, int totalCount)
        {
            LogHelper.DebugHeader("ADD MAILCHIMP MEMBER");

            var member = new Member
            {
                EmailAddress = email,
                StatusIfNew = Status.Subscribed
            };

            member.MergeFields.Add(FirstNameKey, firstName);
            member.MergeFields.Add(LastNameKey, lastName);

            LogHelper.Debug($"MailChimp {index} of {totalCount}: '{firstName}' '{lastName}'.");
            LogHelper.Debug($" Email: {email}.");
            LogHelper.Debug($" Branch: '{branch}'");

            var listId = await GetOrCreateList(branch);

            try
            {
                member = await _mailChimpManager.Members.AddOrUpdateAsync(listId, member);

                LogHelper.Debug($" New member Id = {member.Id}");
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task SyncWithActiveDirectory(List<ActiveADUser> activeDirectoryUsers)
        {
            var tmtEmployeesListId = await GetListId(TmtEmployeesList);

            var tmtEmployeesMembers = await _mailChimpManager.Members.GetAllAsync(tmtEmployeesListId);

            // get not existed in Active Directory members and remove them
            var membersToDelete = tmtEmployeesMembers.Where(m =>
                activeDirectoryUsers.All(u =>
                    !string.Equals(u.Email, m.EmailAddress, StringComparison.CurrentCultureIgnoreCase))).ToList();

            LogHelper.Debug($"Total members to delete = {membersToDelete.Count}...");

            membersToDelete.ForEach(async member =>
            {
                LogHelper.Debug($"Deleting member '{member.EmailAddress}' in List '{TmtEmployeesList}'");

                await DeleteMember(tmtEmployeesListId, member.EmailAddress);
            });

            // add not existed in MailChimp members
            var usersToAdd = activeDirectoryUsers.Where(u =>
                tmtEmployeesMembers.All(m =>
                    !string.Equals(m.EmailAddress, u.Email, StringComparison.CurrentCultureIgnoreCase))).ToList();

            var totalUsersToAdd = usersToAdd.Count;

            LogHelper.Debug($"Total members to add = {totalUsersToAdd}...");

            for (var i = 0; i < totalUsersToAdd; i++)
            {
                var user = usersToAdd[i];

                await AddMember(user.Email, user.FirstName, user.LastName, TmtEmployeesList, i + 1, totalUsersToAdd);
            }
        }

        #endregion

        #region Private methods

        private async Task<string> GetListId(string listName)
        {
            var mailChimpList = (await _mailChimpManager.Lists.GetAllAsync().ConfigureAwait(false))
                .FirstOrDefault(l => l.Name == listName);

            if (mailChimpList == null)
            {
                throw new NullReferenceException($"Can't find MailChimp List by name '{listName}'");
            }

            return mailChimpList.Id;
        }

        private async Task<string> GetOrCreateList(string listName)
        {
            var listMapping = new Dictionary<string, string>
            {
                { "TX - San Antonio/New Braunfels", "San Antonio/New Braunfels Contacts" },
                { "LA - Metairie", "Metairie Contacts" },
                { "VA - Reston", "Reston Contacts" }
            };

            // try to map List name with Insightly branch name
            if (listMapping.ContainsKey(listName))
            {
                listName = listMapping[listName];
            }
            else
            {
                // adjust name
                var splittedName = listName.Split('-');

                if (splittedName.Length > 1)
                {
                    listName = $"{splittedName[1].Trim()} Contacts";
                }
            }

            var mailChimpList = (await _mailChimpManager.Lists.GetAllAsync().ConfigureAwait(false))
                .FirstOrDefault(l => l.Name == listName);

            if (mailChimpList == null)
            {
                mailChimpList = new List
                {
                    Name = listName
                };

                LogHelper.Debug($"Create MailChimp List '{listName}'");

                mailChimpList = await _mailChimpManager.Lists.AddOrUpdateAsync(mailChimpList);
            }

            return mailChimpList.Id;
        }

        private async Task DeleteMember(string listId, string email)
        {
            if (await _mailChimpManager.Members.ExistsAsync(listId, email))
            {
                await _mailChimpManager.Members.DeleteAsync(listId, email);
            }
            else
            {
                LogHelper.Debug($"There is no such member in current list!");
            }
        }

        #endregion
    }
}