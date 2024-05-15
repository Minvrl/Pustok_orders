namespace MVC_Pustok.ViewModels
{
    public class BasketItemViewModel
    {
        public int BookId { get; set; }
        public decimal BookPrice { get; set; }
        public string BookName { get; set; }
        public string BookImage { get; set; }
        public int Count { get; set; }
    }
}
