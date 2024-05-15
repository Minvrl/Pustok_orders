using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Pustok.Data;
using MVC_Pustok.Models;
using MVC_Pustok.ViewModels;
using System.Security.Claims;
using System.Text.Json;

namespace MVC_Pustok.Controllers
{
    public class OrderController : Controller
    {
        private AppDbContext _context;
        private UserManager<AppUser> _userManager;

        public OrderController(AppDbContext context, UserManager<AppUser> userManager)
        {
         
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Checkout()
        {

            CheckoutViewModel checkvm = new CheckoutViewModel() { BasketVM = getBasket() };

            return View(checkvm);
        }

        [Authorize(Roles = "member")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Checkout(OrderCreateViewModel ordervm)
        {
            if (!ModelState.IsValid)
            {
                CheckoutViewModel checkvm = new CheckoutViewModel { BasketVM = getBasket(),Order=ordervm };
                return View(checkvm);
            }


            AppUser user = await _userManager.GetUserAsync(User);
            Order order = new Order
            {
                AppUserId = user.Id,
                Address = ordervm.Adress,
                CreatedAt = DateTime.Now,
                Phone = ordervm.Phone,
                FullName = user.FullName,
                Email = user.Email,
                Note = ordervm.Note,
                Status = Models.Enum.OrderStatus.Pending

            };

            var basketItems = _context.BasketItems.Include(x => x.Book).Where(x => x.AppUserId == user.Id).ToList();

            order.OrderItems = basketItems.Select(x => new OrderItem
            {
                BookId = x.BookId,
                Count = x.Count,
                SalePrice = x.Book.SalePrice,
                DiscountPercent = x.Book.DiscountPerc,
                CostPrice = x.Book.CostPrice,
            }).ToList();


            _context.Orders.Add(order);
            _context.BasketItems.RemoveRange(basketItems);

            _context.SaveChanges();

            return RedirectToAction("profile", "account", new { tab = "orders" });
        }

        public IActionResult Delete(int id)
        {
            BasketItemViewModel bitem = getBasket().Items.FirstOrDefault(x=> x.BookId == id);

            getBasket().Items.Remove(bitem);

            return RedirectToAction("index", "home");

        }
        private BasketViewModel getBasket()
        {
            BasketViewModel vm = new BasketViewModel();

            if (User.Identity.IsAuthenticated && User.IsInRole("member"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                var basketItems = _context.BasketItems
               .Include(x => x.Book)
               .Where(x => x.AppUserId == userId)
               .ToList();

                vm.Items = basketItems.Select(x => new BasketItemViewModel
                {
                    BookId = x.BookId,
                    BookName = x.Book.Name,
                    BookPrice = x.Book.DiscountPerc > 0 ? (x.Book.SalePrice * (100 - x.Book.DiscountPerc) / 100) : x.Book.SalePrice,
                    Count = x.Count
                }).ToList();

                vm.TotalPrice = vm.Items.Sum(x => x.Count * x.BookPrice);
            }
            else
            {
                var cookieBasket = Request.Cookies["basket"];

                if (cookieBasket != null)
                {
                    List<BasketCookieItemViewModel> cookieItemsVM = JsonSerializer.Deserialize<List<BasketCookieItemViewModel>>(cookieBasket);
                    ;
                    foreach (var cookieItem in cookieItemsVM)
                    {
                        Book? book = _context.Books.Include(x => x.BookImages.Where(bi => bi.PosterStatus == true)).FirstOrDefault(x => x.Id == cookieItem.BookId);

                        if (book != null)
                        {
                            BasketItemViewModel itemVM = new BasketItemViewModel
                            {
                                BookId = cookieItem.BookId,
                                Count = cookieItem.Count,
                                BookName = book.Name,
                                BookPrice = book.DiscountPerc > 0 ? (book.SalePrice * (100 - book.DiscountPerc) / 100) : book.SalePrice,
                            };
                            vm.Items.Add(itemVM);
                        }

                    }

                    vm.TotalPrice = vm.Items.Sum(x => x.Count * x.BookPrice);
                }
            }

            return vm;

        }



        private List<OrderBasketItemViewModel> getOrderBasket(string userId)
        {

            List<OrderBasketItemViewModel> items = new List<OrderBasketItemViewModel>();

            var basketItems = _context.BasketItems
                                .Include(x => x.Book)
                                .Where(x => x.AppUserId == userId)
                                .ToList();

            items = basketItems.Select(x => new OrderBasketItemViewModel
            {
                BookId = x.BookId,
                Count = x.Count
            }).ToList();


            return items;
        }

		[Authorize(Roles = "member")]
		public IActionResult GetOrderItems(int orderId)
		{
			AppUser user = _userManager.GetUserAsync(User).Result;

			Order order = _context.Orders.Include(x => x.OrderItems).ThenInclude(oi => oi.Book).FirstOrDefault(x => x.Id == orderId && x.AppUserId == user.Id);
			if (order == null) return RedirectToAction("notfound", "error");


			return PartialView("_OrderDetailPartial", order.OrderItems);
		}

		[Authorize(Roles = "member")]
		public IActionResult Cancel(int id)
		{
			AppUser user = _userManager.GetUserAsync(User).Result;

			Order order = _context.Orders.FirstOrDefault(x => x.Id == id && x.AppUserId == user.Id && x.Status == Models.Enum.OrderStatus.Pending);

			if (order == null) return RedirectToAction("notfound", "error");

			order.Status = Models.Enum.OrderStatus.Canceled;

			_context.SaveChanges();

			return RedirectToAction("profile", "account", new { tab = "orders" });
		}
	}
}
