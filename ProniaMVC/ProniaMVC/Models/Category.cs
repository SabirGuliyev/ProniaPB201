using System.ComponentModel.DataAnnotations;

namespace ProniaMVC.Models
{
    public class Category:BaseEntity
    {
        [Required(ErrorMessage ="Ad mutleqdir")]
        [MaxLength(25,ErrorMessage ="Uzunlugu 25den cox ola bilmez")]
        public string Name { get; set; }


        //relational
        public ICollection<Product>? Products { get; set; }
    }
}
