using ProniaMVC.Models;

namespace ProniaMVC.Areas.ProniaAdmin.ViewModels
{
    public class GetAdminProductVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }

        public string Image { get; set; }

    }
}
