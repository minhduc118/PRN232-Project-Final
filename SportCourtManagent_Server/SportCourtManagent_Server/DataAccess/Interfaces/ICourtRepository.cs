using System;
using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ICourtRepository
    {
        IEnumerable<Court> GetAll();
        Court? GetById(int id);
        void Add(Court entity);
        void Update(Court entity);
        void Delete(int id);
        decimal GetCourtPrice(int courtId, int slotId, DateTime date);
    }
}
