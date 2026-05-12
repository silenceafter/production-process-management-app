using Microsoft.EntityFrameworkCore;
using PpmBackend.Data;

namespace PpmBackend.Services
{
    public class PertCalculator
    {
        private readonly ApplicationDbContext _db;

        public PertCalculator(ApplicationDbContext db) => _db = db;

      
    }
}
