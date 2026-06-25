using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IReviewRepository
    {
        IEnumerable<Review> GetAll();
        Review? GetById(int id);
        void Add(Review entity);
        void Update(Review entity);
        void Delete(int id);
    }
}
