using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElRawda.Core.Models
{
    public class Cows:BaseModel
    {
        public string CowsId { get; set; }
        public double Weight { get; set; }
        public DateTime Date { get; set; }
        public int MachId { get; set; }
    }
}
