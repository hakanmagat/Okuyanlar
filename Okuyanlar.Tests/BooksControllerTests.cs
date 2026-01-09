using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Service.Services;
using Okuyanlar.Web.Controllers;

namespace Okuyanlar.Tests
{
    public class BooksControllerTests
    {
        private readonly Mock<IBookRepository> _bookRepo = new();
        private readonly Mock<IBookRatingRepository> _ratingRepo = new();
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly BookService _bookSvc;
        private readonly BooksController _controller;

        public BooksControllerTests()
        {
            _bookSvc = new BookService(_bookRepo.Object, _ratingRepo.Object, _userRepo.Object);
            // For tests in this file, we only exercise BookService-dependent actions.
            // Create minimal stubs for other services that are not used in these tests.
            var resSvc = (BookReservationService)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(BookReservationService));
            var borrowSvc = (BookBorrowService)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(BookBorrowService));
            _controller = new BooksController(_bookSvc, resSvc, borrowSvc);
        }

        [Fact]
        public void Index_ReturnsView_WithBooks()
        {
            var books = new List<Book> { new Book { Id = 1 }, new Book { Id = 2 } };
            _bookRepo.Setup(x => x.GetAll()).Returns(books);

            var result = _controller.Index() as ViewResult;
            Assert.NotNull(result);
            Assert.Same(books, result!.Model);

            _bookRepo.Verify(x => x.GetAll(), Times.Once);
        }

        [Fact]
        public void Details_ReturnsNotFound_WhenBookMissing()
        {
            _bookRepo.Setup(x => x.GetById(99)).Returns((Book?)null);

            var result = _controller.Details(99);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Details_SetsUserRating_WhenAuthenticated()
        {
            var book = new Book { Id = 1 };
            _bookRepo.Setup(x => x.GetById(1)).Returns(book);
            _userRepo.Setup(x => x.GetByEmail("user@okuyanlar.oku")).Returns(new User { Id = 5 });
            _ratingRepo.Setup(x => x.GetByBookAndUser(1, 5)).Returns(new BookRating { BookId = 1, UserId = 5, Rating = 4.5m });

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "user@okuyanlar.oku")
            }, authenticationType: "Test"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = _controller.Details(1) as ViewResult;
            Assert.NotNull(result);
            Assert.Same(book, result!.Model);
            Assert.Equal(4.5m, _controller.ViewBag.UserRating);
        }

        [Fact]
        public void Rate_ReturnsUnauthorized_When_NoEmail()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = _controller.Rate(1, 4m) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, result!.StatusCode);
        }

        [Fact]
        public void Rate_ReturnsOk_OnSuccess()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "user@okuyanlar.oku")
            }, authenticationType: "Test"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _bookRepo.Setup(x => x.GetById(1)).Returns(new Book { Id = 1 });
            _userRepo.Setup(x => x.GetByEmail("user@okuyanlar.oku")).Returns(new User { Id = 5, Email = "user@okuyanlar.oku", Role = Okuyanlar.Core.Enums.UserRole.EndUser });
            _ratingRepo.Setup(x => x.GetByBookAndUser(1, 5)).Returns((BookRating?)null);
            _ratingRepo.Setup(x => x.Add(It.IsAny<BookRating>()));
            _ratingRepo.Setup(x => x.GetRatingsForBook(1)).Returns(new List<BookRating> { new BookRating { BookId = 1, UserId = 5, Rating = 4m } });
            _bookRepo.Setup(x => x.Update(It.IsAny<Book>()));

            var result = _controller.Rate(1, 4m) as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result!.StatusCode);
        }
    }
}
