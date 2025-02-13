using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElRawda.Core.Models
{
    public class SlaughteredCow: BaseModel
    {
        public string CowsId { get; set; }
        public double WeightAtSlaughter { get; set; }
        public double ?Waste {  get; set; }
        public DateTime DateOfSlaughter { get; set; }
        public int MachId { get; set; }
    }
}
