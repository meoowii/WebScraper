using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper.Models;

public class Product
{
    public string Sku { get; set; }
    public string Title { get; set; }
    public string Price { get; set; }
    public string CategoryUrl { get; set; }
}
