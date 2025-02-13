using ElRawda.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ElRawda.Core.Specifications
{
    public interface ISpecifications<T> where T : BaseModel
    {
        public Expression<Func<T, bool>> Criteria { get; set; } 
        public List<Expression<Func<T, Object>>> Includes { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; } 
        public bool IsPagingationEnabled { get; set; }
    }
}
