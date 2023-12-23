using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Shop
{
    public class Book
    {
        public string name { get; set; }
        public string author { get; set; }
        public string publisher { get; set; }
        public string genre { get; set; }
        public DateTime releaseDate { get; set; }
        public int pagesCount { get; set; }
        public double totalPrice { get; set; }
        public double costPrice { get; set; }
        public bool isSequel { get; set; }
    }
}
