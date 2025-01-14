using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caffetteria.Core.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public int TableNumber { get; set; }
        public List<Coffee> Coffees { get; set; }
    }
}
