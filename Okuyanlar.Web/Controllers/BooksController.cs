using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Core.Entities;
using Okuyanlar.Service.Services;
using System.Collections.Generic;
using System.Linq;

namespace Okuyanlar.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookService _bookService;
        private readonly BookReservationService _reservationService;
        private readonly BookBorrowService _borrowService;

        public BooksController(BookService bookService, BookReservationService reservationService, BookBorrowService borrowService)
        {
            _bookService = bookService;
            _reservationService = reservationService;
            _borrowService = borrowService;
        }

        // /Books or /Books/Index
        public IActionResult Index()
        {
            var books = _bookService.GetAllBooks();
            return View(books);
        }

        // /Books/Details/9
        public IActionResult Details(int id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null)
                return NotFound();

            // Load user's previous rating if logged in
            decimal? userRating = null;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    userRating = _bookService.GetUserRatingForBook(id, email);
                }
            }

            ViewBag.UserRating = userRating;
            return View(book);
        }

        // /Books/Top10
        public IActionResult Top10()
        {
            var topBooks = _bookService.GetTopRatedBooks(10);
            return View(topBooks);
        }

        // POST /Books/Rate
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "EndUser")]
        public IActionResult Rate(int bookId, decimal rating)
        {
            try
            {
                var email = User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    return Unauthorized(new { success = false, message = "Login required." });
                }

                _bookService.RateBook(bookId, email, rating);
                return Ok(new { success = true, message = "Book rated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST /Books/Reserve
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "EndUser")]
        public IActionResult Reserve(int bookId, int hours = 24)
        {
            try
            {
                var email = User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["ErrorMessage"] = "Login required.";
                    return RedirectToAction("Index", "Catalog");
                }

                _reservationService.ReserveBook(bookId, email, hours);
                TempData["SuccessMessage"] = "Book reserved successfully.";
                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Catalog");
            }
        }

        // POST /Books/RequestCheckIn
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "EndUser")]
        public IActionResult RequestCheckIn(int reservationId)
        {
            try
            {
                var email = User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["ErrorMessage"] = "Login required.";
                    return RedirectToAction("MyReservations");
                }

                _reservationService.RequestCheckIn(reservationId, email);
                TempData["SuccessMessage"] = "Check-in request submitted.";
                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("MyReservations");
            }
        }

        // POST /Books/RequestReturn
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "EndUser")]
        public IActionResult RequestReturn(int borrowId)
        {
            try
            {
                var email = User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["ErrorMessage"] = "Login required.";
                    return RedirectToAction("MyBorrows");
                }

                _borrowService.RequestReturn(borrowId, email);
                TempData["SuccessMessage"] = "Return request submitted.";
                return RedirectToAction("MyBorrows");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("MyBorrows");
            }
        }

        // GET /Books/MyReservations
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "EndUser")]
        public IActionResult MyReservations()
        {
            var email = User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var reservations = _reservationService.GetUserReservations(email);
            return View(reservations);
        }

        // GET /Books/MyBorrows
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "EndUser")]
        public IActionResult MyBorrows()
        {
            var email = User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var borrows = _borrowService.GetUserBorrows(email);
            return View(borrows);
        }
    }
}

