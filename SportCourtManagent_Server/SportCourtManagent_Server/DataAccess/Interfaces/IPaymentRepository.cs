using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IPaymentRepository
    {
        IEnumerable<Payment> GetAll();
        Payment? GetById(int id);
        void Add(Payment entity);
        void Update(Payment entity);
        void Delete(int id);
    }
}
