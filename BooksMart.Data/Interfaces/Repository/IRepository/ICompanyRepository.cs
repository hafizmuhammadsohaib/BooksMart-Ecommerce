using BooksMart.Models.Models;

namespace BooksMart.Data.Interfaces.Repository.IRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company company);
    }
}
