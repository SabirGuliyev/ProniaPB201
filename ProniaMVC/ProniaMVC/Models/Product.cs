namespace ProniaMVC.Models
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }

        //relational
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<ProductImage> ProductImages { get; set; }
        public ICollection<ProductTag> ProductTags { get; set; }
    }
}
