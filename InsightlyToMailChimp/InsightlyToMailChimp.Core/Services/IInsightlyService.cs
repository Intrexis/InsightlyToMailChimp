using System.Collections.Generic;
using System.Threading.Tasks;
using InsightlyToMailChimp.Core.Models.InsightlyModels;

namespace InsightlyToMailChimp.Core.Services
{
    public interface IInsightlyService
    {
        Task<List<InsightlyContact>> GetInsightlyContacts(List<string> existedEmails);
    }
}