using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class OrderVM
    {
        OrderHeader OrderHeader { get; set; }

        IEnumerable<OrderDetail> OrderDetails { get; set; }
    }
}
