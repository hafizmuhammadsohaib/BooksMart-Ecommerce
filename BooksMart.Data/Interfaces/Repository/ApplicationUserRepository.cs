using BooksMart.Data.Data;
using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Models.Models;

namespace BooksMart.Data.Interfaces.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>,IApplicationUserRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ApplicationUserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
