using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElRawda.Core.Models
{
    public class CowsPieces:BaseModel
    {
        public int MachId { get; set; }
        public string? PieceId { get; set; }
        public double? PieceWeight { get; set; }
        public string? PieceTybe { get; set; }
        public string dateOfSupply { get; set; }
        public string dateofExpiere { get; set; }
        public int CowId { get; set; }
        public SlaughteredCow Cow { get; set; }
    }
   
     
}
