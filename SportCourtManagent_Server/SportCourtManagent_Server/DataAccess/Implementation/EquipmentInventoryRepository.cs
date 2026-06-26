using System;
using System.Collections.Generic;
using System.Linq;
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
            return _context.EquipmentInventories.ToList();
        }

        public EquipmentInventory? GetById(int id)
        {
            return _context.EquipmentInventories.Find(id);
        }

        public void Add(EquipmentInventory entity)
        {
            _context.EquipmentInventories.Add(entity);
            _context.SaveChanges();
        }

        public void Update(EquipmentInventory entity)
        {
            _context.EquipmentInventories.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.EquipmentInventories.Find(id);
            if (entity != null)
            {
                _context.EquipmentInventories.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
