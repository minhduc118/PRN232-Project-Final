using System;
using System.Collections.Generic;
using System.Linq;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly AppDbContext _context;

        public PromotionRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Promotion> GetAll()
        {
            return _context.Promotions.ToList();
        }

        public Promotion? GetById(int id)
        {
            return _context.Promotions.Find(id);
        }

        public void Add(Promotion entity)
        {
            _context.Promotions.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Promotion entity)
        {
            _context.Promotions.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Promotions.Find(id);
            if (entity != null)
            {
                _context.Promotions.Remove(entity);
                _context.SaveChanges();
            }
        }

        public Promotion? GetByCode(string code)
        {
            try
            {
                var promotion = _context.Promotions.FirstOrDefault(p => p.PromoCode.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (promotion == null)
                {
                    return null;
                }
                return promotion;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException("An error occurred while retrieving the promotion by code.", ex);
            }
        }
    }
}
