using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indexer.Documents
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int? Stock { get; set; } = 0;
        public int? Sold { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool Disabled { get; set; }
        public List<Image> Images { get; set; } = new();
        public List<Category> Categories { get; set; } = new();


        public override string ToString()
        {
            return $"{Id}, {Name}, {Description}, {Price}, {Stock}, {Sold}, {CreatedAt}, {Disabled}";
        }
    }
}
