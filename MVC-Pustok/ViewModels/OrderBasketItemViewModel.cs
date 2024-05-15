using Microsoft.Data.SqlClient.DataClassification;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace MVC_Pustok.ViewModels
{
    public class OrderBasketItemViewModel
    {
        public int BookId { get; set; }
        public int Count { get; set; }  
    }
}
