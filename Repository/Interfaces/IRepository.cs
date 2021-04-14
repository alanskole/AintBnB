using System.Collections.Generic;
using System.Threading.Tasks;

namespace AintBnB.Repository.Interfaces
{
    public interface IRepository<T>
    {
        Task CreateAsync(T t);

        Task<T> ReadAsync(int id);

        Task<List<T>> GetAllAsync();

        Task UpdateAsync(int id, T t);

        Task DeleteAsync(int id);
    }
}
