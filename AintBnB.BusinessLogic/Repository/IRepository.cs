using System.Collections.Generic;

namespace AintBnB.BusinessLogic.Repository
{
    public interface IRepository<T>
    {
        void Create(T t);

        T Read(int id);

        List<T> GetAll();

        void Update(int id, T t);

        void Delete(int id);
    }
}
