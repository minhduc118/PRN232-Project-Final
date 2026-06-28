using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IInvoiceRepository
    {
        IEnumerable<Invoice> GetAll();
        Invoice? GetById(int id);
        void Add(Invoice entity);
        void Update(Invoice entity);
        void Delete(int id);
    }
}
