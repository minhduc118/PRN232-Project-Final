using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IEquipmentInventoryRepository
    {
        IEnumerable<EquipmentInventory> GetAll();
        EquipmentInventory? GetById(int id);
        void Add(EquipmentInventory entity);
        void Update(EquipmentInventory entity);
        void Delete(int id);
    }
}
