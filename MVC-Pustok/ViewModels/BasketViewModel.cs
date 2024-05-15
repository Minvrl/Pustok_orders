namespace MVC_Pustok.ViewModels
{
    public class BasketViewModel
    {
        public decimal TotalPrice { get; set; }
        public List<BasketItemViewModel> Items { get; set; } = new List<BasketItemViewModel>();
    }
}
