namespace ProniaMVC.Models
{
    public class Tag:BaseEntity
    {
        public string Name { get; set; }
        public ICollection<ProductTag> productTags { get; set; }
    }
}
