using Azure.Core;
using Azure;
using Microsoft.EntityFrameworkCore;
using MVC_Pustok.Data;
using MVC_Pustok.Models;
using MVC_Pustok.ViewModels;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace MVC_Pustok.Services
{
    public class LayoutService
    {
        private AppDbContext _context;
        private IHttpContextAccessor _httpContextAccessor;

        public LayoutService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;

        }
        public List<Genre> GetGenres()
        {
            return _context.Genres.ToList();
        }

        public Dictionary<String, String> GetSettings()
        {
            return _context.Settings.ToDictionary(x => x.Key, x => x.Value);
        }

        public BasketViewModel GetBasket()
        {
            BasketViewModel vm = new BasketViewModel();

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated && _httpContextAccessor.HttpContext.User.IsInRole("member"))
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                var basketItems = _context.BasketItems
               .Include(x => x.Book)
               .ThenInclude(b => b.BookImages.Where(bi => bi.PosterStatus == true))
               .Where(x => x.AppUserId == userId)
               .ToList();

                vm.Items = basketItems.Select(x => new BasketItemViewModel
                {
                    BookId = x.BookId,
                    BookName = x.Book.Name,
                    BookPrice = x.Book.DiscountPerc > 0 ? (x.Book.SalePrice * (100 - x.Book.DiscountPerc) / 100) : x.Book.SalePrice,
                    BookImage = x.Book.BookImages.FirstOrDefault(x => x.PosterStatus == true)?.Name,
                    Count = x.Count
                }).ToList();

                vm.TotalPrice = vm.Items.Sum(x => x.Count * x.BookPrice);
            }
            else
            {
                var cookieBasket = _httpContextAccessor.HttpContext.Request.Cookies["basket"];

                if (cookieBasket != null)
                {
                    List<BasketCookieItemViewModel> cookieItemsVM = JsonSerializer.Deserialize<List<BasketCookieItemViewModel>>(cookieBasket);
                    ;
                    foreach (var cookieItem in cookieItemsVM)
                    {
                        Book? book = _context.Books.Include(x => x.BookImages.Where(bi => bi.PosterStatus == true)).FirstOrDefault(x => x.Id == cookieItem.BookId );

                        if (book != null)
                        {
                            BasketItemViewModel itemVM = new BasketItemViewModel
                            {
                                BookId = cookieItem.BookId,
                                Count = cookieItem.Count,
                                BookName = book.Name,
                                BookPrice = book.DiscountPerc > 0 ? (book.SalePrice * (100 - book.DiscountPerc) / 100) : book.SalePrice,
                                BookImage = book.BookImages.FirstOrDefault(x => x.PosterStatus == true)?.Name
                            };
                            vm.Items.Add(itemVM);
                        }

                    }

                    vm.TotalPrice = vm.Items.Sum(x => x.Count * x.BookPrice);
                }
            }

            return vm;
        }
    }
}
