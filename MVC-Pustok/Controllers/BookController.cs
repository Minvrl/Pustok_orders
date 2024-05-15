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
    public class BookController:Controller
    {
        private AppDbContext _context;
		private UserManager<AppUser> _userManager;

		public BookController(AppDbContext context,UserManager<AppUser> userManager)
        {
            _context = context;
			_userManager = userManager;	
        }
        public IActionResult GetBookById(int id)
        {
            Book book = _context.Books.Include(x => x.Genre).Include(x => x.BookImages.Where(x => x.PosterStatus == true)).FirstOrDefault(x => x.Id == id);
            return PartialView("_BookModalPartial", book);
        }

        public IActionResult Details(int id)
        {
			var vm = getBookDetailVM(id);

			if (vm.Book == null) return RedirectToAction("notfound", "error");

			return View(vm);
		}

        [HttpPost]
        public async Task<IActionResult> Review(BookReview review)
        {
            AppUser? user = await _userManager.GetUserAsync(User);


            if (user == null || !await _userManager.IsInRoleAsync(user, "member"))
                return RedirectToAction("login", "account", new { returnUrl = Url.Action("details", "book", new { id = review.BookId }) });

            if (!_context.Books.Any(x => x.Id == review.BookId))
                return RedirectToAction("notfound", "error");

            if (_context.BookReviews.Any(x => x.Id == review.BookId && x.AppUserId == user.Id))
                return RedirectToAction("notfound", "error");

            if (!ModelState.IsValid)
            {
                var vm = getBookDetailVM(review.BookId);
                vm.Review = review;
                return View("details", vm);
            }

            review.AppUserId = user.Id;
            review.CreatedAt = DateTime.Now;

            _context.BookReviews.Add(review);
            _context.SaveChanges();

            return RedirectToAction("details", new { id = review.BookId });
        }


		public IActionResult AddToBasket(int bookId)
		{
			Book book = _context.Books.FirstOrDefault(x => x.Id == bookId);

			if (book == null) return RedirectToAction("notfound", "error");

			if (User.Identity.IsAuthenticated && User.IsInRole("member"))
			{
				string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

				BasketItem? basketItem = _context.BasketItems.FirstOrDefault(x => x.AppUserId == userId && x.BookId == bookId);

				if (basketItem == null)
				{
					basketItem = new BasketItem
					{
						AppUserId = userId,
						BookId = bookId,
						Count = 1
					};
					_context.BasketItems.Add(basketItem);
				}
				else basketItem.Count++;

				_context.SaveChanges();
				return PartialView("_BasketPartial", getBasket());
			}
			else
			{
				List<BasketCookieItemViewModel> basketItems = new List<BasketCookieItemViewModel>();

				var cookieItem = Request.Cookies["basket"];

				if (cookieItem != null)
				{
					basketItems = JsonSerializer.Deserialize<List<BasketCookieItemViewModel>>(cookieItem);
				}

				BasketCookieItemViewModel item = basketItems.FirstOrDefault(x => x.BookId == bookId);

				if (item == null)
				{
					item = new BasketCookieItemViewModel
					{
						BookId = bookId,
						Count = 1
					};
					basketItems.Add(item);
				}
				else
				{
					item.Count++;
				}

				Response.Cookies.Append("basket", JsonSerializer.Serialize(basketItems));
				return PartialView("_BasketPartial", getBasket(basketItems));
			}

		}




		private BookDetailViewModel getBookDetailVM(int bookId)
		{
			Book? book = _context.Books
			  .Include(x => x.Genre)
			  .Include(x => x.Author)
			  .Include(x => x.BookImages)
			  .Include(x => x.BookReviews.Take(2)).ThenInclude(r => r.AppUser)
			  .Include(x => x.BookTags).ThenInclude(bt => bt.Tag)
			  .FirstOrDefault(x => x.Id == bookId);

			BookDetailViewModel vm = new BookDetailViewModel
			{
				Book = book,
				RelatedBooks = _context.Books
					   .Include(x => x.Author)
					   .Include(x => x.BookImages
							   .Where(bi => bi.PosterStatus != null))
					   .Where(x => book != null && x.GenreId == book.GenreId)
					   .Take(5).ToList(),
				Review = new BookReview { BookId = bookId }
			};

			AppUser? user = _userManager.GetUserAsync(User).Result;

			if (_userManager.IsInRoleAsync(user, "member").Result && _context.BookReviews.Any(x => x.BookId == bookId && x.AppUserId == user.Id && x.Status != MVC_Pustok.Models.Enum.ReviewStatus.Rejected))
			{
				vm.HasUserReview = true;
			}

			vm.TotalReviewsCount = _context.BookReviews.Count(x => x.BookId == bookId);
			vm.AvgRate = vm.TotalReviewsCount > 0 ? (int)Math.Ceiling(_context.BookReviews.Where(x => x.BookId == bookId).Average(x => x.Rate)) : 0;

			return vm;
		}

		private BasketViewModel getBasket(List<BasketCookieItemViewModel>? items = null)
		{
			BasketViewModel vm = new BasketViewModel();

			if (User.Identity.IsAuthenticated && User.IsInRole("member"))
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

				var basketItems = _context.BasketItems
			   .Include(x => x.Book).ThenInclude(b => b.BookImages.Where(bi => bi.PosterStatus == true))
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
				if (items != null)
				{
					foreach (var cookieItem in items)
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
