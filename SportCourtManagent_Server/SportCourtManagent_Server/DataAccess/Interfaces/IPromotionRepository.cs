using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IPromotionRepository
    {
        Task<IEnumerable<Promotion>> GetAllAsync();
        Task<Promotion?> GetByIdAsync(int id);
        Task<Promotion?> GetByCodeAsync(string promoCode);
        Task AddAsync(Promotion entity);
        Task UpdateAsync(Promotion entity);
        Task DeleteAsync(int id);

        IEnumerable<Promotion> GetAll();
        Promotion? GetById(int id);
        Promotion? GetByCode(string code);
        void Add(Promotion entity);
        void Update(Promotion entity);
        void Delete(int id);
    }
}
