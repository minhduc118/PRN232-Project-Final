using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class EquipmentInventoryRepository : IEquipmentInventoryRepository
    {
        private readonly AppDbContext _context;

        public EquipmentInventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<EquipmentInventory> GetAll()
        {
            throw new NotImplementedException();
        }

        public EquipmentInventory? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(EquipmentInventory entity)
        {
            throw new NotImplementedException();
        }

        public void Update(EquipmentInventory entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
