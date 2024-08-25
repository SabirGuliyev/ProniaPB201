namespace ProniaMVC.Models
{
    public class Setting
    {
        public int Id { get; set; }
        public string Key { get; set; }

        public string Value { get; set; }
    }


    // setting key= logo, value=".png"
    //setting key=Phone, value="050 333"
    //setting key=FbLink, value=".com"
}
