using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class PermissionMatrixRepository : IPermissionMatrixRepository
    {
        private readonly AppDbContext _context;

        public PermissionMatrixRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<PermissionMatrixEntry>> GetAllAsync()
        {
            return await _context.PermissionMatrixEntries
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        public async Task UpsertAllAsync(List<PermissionMatrixEntry> entries)
        {
            if (entries == null || entries.Count == 0) return;

            var updatedAt = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                var existing = await _context.PermissionMatrixEntries
                    .FirstOrDefaultAsync(p => p.Feature == entry.Feature);

                if (existing != null)
                {
                    existing.Admin    = entry.Admin;
                    existing.Manager  = entry.Manager;
                    existing.Staff    = entry.Staff;
                    existing.Customer = entry.Customer;
                    existing.UpdatedAt = updatedAt;
                }
                else
                {
                    entry.UpdatedAt = updatedAt;
                    await _context.PermissionMatrixEntries.AddAsync(entry);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
