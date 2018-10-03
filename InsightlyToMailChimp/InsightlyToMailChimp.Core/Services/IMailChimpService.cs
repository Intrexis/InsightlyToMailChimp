using System.Collections.Generic;
using System.Threading.Tasks;
using InsightlyToMailChimp.Core.Models.ActiveDirectory;

namespace InsightlyToMailChimp.Core.Services
{
    public interface IMailChimpService
    {
        Task<List<string>> GetAllMemberEmails();

        Task AddMember(string email, string firstName, string lastName, string branch, int index, int totalCount);

        Task SyncWithActiveDirectory(List<ActiveADUser> activeDirectoryUsers);
    }
}