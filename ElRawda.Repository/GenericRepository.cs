using ElRawda.Core.Models;
using ElRawda.Core.Repositories;
using ElRawda.Core.Specifications;
using ElRawda.Repository.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElRawda.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseModel
    {
        private readonly ELRawdaContext context;

        public GenericRepository(ELRawdaContext _context) {
            context = _context;
        }

        public async Task<T> AddAsync(T entity)
        {
            await context.AddAsync(entity);
            context.SaveChanges();
            return entity;
        }
        public T Delete(T entity)
        {
            context.Remove(entity);
            context.SaveChanges();
            return entity;
        }
        public async Task<IReadOnlyList<T>> GetAllAysnc()
        {
            return await context.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAysnc(int id)
        {
            return await context.Set<T>().FindAsync(id);
        }

        public T Update(T entity)
        {
            context.Update(entity);
            context.SaveChanges();
            return entity;
        }
        public async Task<IReadOnlyList<T>> GetAllAysncWithspec(ISpecifications<T> spec)
        {
           return await ApplySpecfiication(spec).ToListAsync();
        }
        public async Task<T> GetByIDAysncWithspec(ISpecifications<T> spec)
        {
            return await ApplySpecfiication(spec).FirstOrDefaultAsync();
        }

        private IQueryable<T> ApplySpecfiication(ISpecifications<T> spec)
        {
            return SpecificationEvalutor<T>.GetQuery(context.Set<T>(), spec);
        }
    }
}
