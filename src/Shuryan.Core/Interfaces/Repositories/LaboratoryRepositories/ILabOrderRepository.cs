using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories
{
    public interface ILabOrderRepository : IGenericRepository<LabOrder>
    {
        Task<LabOrder?> GetOrderWithDetailsAsync(Guid orderId);
        Task<IEnumerable<LabOrder>> GetPagedOrdersForPatientAsync(Guid patientId, int pageNumber, int pageSize);
        Task<IEnumerable<LabOrder>> GetPagedOrdersForLaboratoryAsync(Guid laboratoryId, LabOrderStatus? status, int pageNumber, int pageSize);
        Task<IEnumerable<LabOrder>> GetOrdersByStatusAsync(LabOrderStatus status);
        Task<IEnumerable<LabOrder>> GetPendingOrdersForLaboratoryAsync(Guid laboratoryId);
        Task<IEnumerable<LabOrder>> GetActiveOrdersForPatientAsync(Guid patientId);
    }
}

