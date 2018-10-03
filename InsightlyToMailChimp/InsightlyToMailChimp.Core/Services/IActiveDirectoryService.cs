using System.Collections.Generic;
using System.Threading.Tasks;
using InsightlyToMailChimp.Core.Models.ActiveDirectory;

namespace InsightlyToMailChimp.Core.Services
{
    public interface IActiveDirectoryService
    {
        Task<List<ActiveADUser>> GetAllActiveADUsers();
    }
}