using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using InsightlyToMailChimp.Core;
using InsightlyToMailChimp.Core.Infrastructure;
using InsightlyToMailChimp.Core.Models.InsightlyModels;
using InsightlyToMailChimp.Core.Services;
using Newtonsoft.Json;

namespace InsightlyToMailChimp.Services
{
    public class InsightlyService : IInsightlyService
    {
        #region Fields

        private const int MaxInsightlyContactsPerRequest = 500;

        private const string GetContactsAction = "Contacts";

        private readonly string _insightlyApi = ConfigurationManager.AppSettings["InsightlyApi"];

        private readonly Dictionary<string, string> _requestHeaders = new Dictionary<string, string>
        {
            { "Authorization", "Basic ZmFiZTllNWMtMmU1Yi00NmQzLTliN2ItMTNkMmFmMjQ4OTFk" }
        };

        #endregion

        #region Public methods
        public async Task<List<InsightlyContact>> GetInsightlyContacts(List<string> existedEmails)
        {
            LogHelper.DebugHeader("GET INSIGHTLY CONTACTS");

            var contacts = await GetContacts(existedEmails);

            var totalCount = contacts.Count;

            LogHelper.Debug($"{Environment.NewLine}Insightly contacts to add:");
            LogHelper.Debug($"Total count: {totalCount}");

            for (var i = 0; i < contacts.Count; i++)
            {
                Console.WriteLine();

                var contact = contacts[i];

                LogHelper.Debug($"Insightly {i + 1} of {totalCount}. {contact.FirstName} {contact.LastName} ({contact.ContactId}).");
                LogHelper.Debug($" Email: '{contact.EmailAddress}'");
                LogHelper.Debug($" Branch: '{contact.Branch}'");

                contact.EmailAddress = AdjustEmail(contact.EmailAddress);
            }

            return contacts;
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Get all Contacts except existed emails and which contains appropriate CustomFieldId
        /// </summary>
        /// <param name="existedEmails"></param>
        /// <returns></returns>
        private async Task<List<InsightlyContact>> GetContacts(List<string> existedEmails)
        {
            var allContacts = new List<InsightlyContact>();

            List<InsightlyContact> contacts = new List<InsightlyContact>();

            do
            {
                contacts.Clear();
                contacts = await InsightlyGetRequest<List<InsightlyContact>>(GetContactsAction, $"?brief=false&top={MaxInsightlyContactsPerRequest}&skip={allContacts.Count}");
                allContacts.AddRange(contacts);
            }
            while(contacts.Any());

            // filter contacts by existed email and branch
            allContacts = allContacts
                .Where(c => !string.IsNullOrWhiteSpace(c.EmailAddress)
                            && !string.IsNullOrWhiteSpace(c.FirstName)
                            && !string.IsNullOrWhiteSpace(c.LastName)
                            && !existedEmails.Contains(AdjustEmail(c.EmailAddress))
                            && !string.IsNullOrEmpty(c.Branch))
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToList();

            return allContacts;
        }

        private async Task<T> InsightlyGetRequest<T>(string action, string parameters)
        {
            var contactResponse = await WebClient.GetRequest($"{_insightlyApi}/{action}{parameters}", _requestHeaders);

            if (!contactResponse.IsSuccessStatusCode)
            {
                LogHelper.Debug($" {contactResponse.ReasonPhrase}.", newLine: false);
                return default(T);
            }

            var responseString = await contactResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(responseString);
        }

        private static string AdjustEmail(string email)
        {
            email = email.Replace("Work Email:", string.Empty);

            var splitted = email.Split(';');

            email = splitted.First().ToLower();

            return email;
        }
        #endregion
    }
}