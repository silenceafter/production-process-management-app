using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PpmBackend.Models;

namespace PpmBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    }
}