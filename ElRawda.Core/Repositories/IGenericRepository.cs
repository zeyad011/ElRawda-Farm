using ElRawda.Core.Models;
using ElRawda.Core.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElRawda.Core.Repositories
{
    public interface IGenericRepository<T> where T : BaseModel
    {
        Task<IReadOnlyList<T>> GetAllAysnc();
        Task<T> GetByIdAysnc(int id);
        Task<IReadOnlyList<T>> GetAllAysncWithspec(ISpecifications<T> spec);
        Task<T> GetByIDAysncWithspec(ISpecifications<T> spec);

        Task<T> AddAsync(T entity);
        T Update(T entity);
         T Delete(T entity);

    }
}
