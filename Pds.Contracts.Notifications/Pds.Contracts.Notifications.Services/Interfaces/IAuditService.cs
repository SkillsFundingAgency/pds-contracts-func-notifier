using Pds.Contracts.Notifications.Services.Models;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces
{
    /// <summary>
    /// Interface to allow notificaiton to sent to the Audit API.
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Creates an entry via the audit API in a fire and forget model.
        /// </summary>
        /// <param name="audit">The audit record to create.</param>
        /// <returns>Task representing async operation.</returns>
        Task CreateAudit(Audit audit);
    }
}
