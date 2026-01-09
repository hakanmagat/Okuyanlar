using Moq;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Service.Services;

namespace Okuyanlar.Tests
{
    public class BookReservationServiceTests
    {
        private readonly Mock<IBookReservationRepository> _resRepo = new();
        private readonly Mock<IBookRepository> _bookRepo = new();
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly BookReservationService _svc;

        public BookReservationServiceTests()
        {
            _svc = new BookReservationService(_resRepo.Object, _bookRepo.Object, _userRepo.Object);
        }

        [Fact]
        public void ReserveBook_Should_Throw_When_User_NotFound()
        {
            var ex = Assert.Throws<Exception>(() => _svc.ReserveBook(1, "u@e"));
            Assert.Equal("User not found.", ex.Message);
        }

        [Fact]
        public void ReserveBook_Should_Throw_When_Book_NotFound()
        {
            _userRepo.Setup(x => x.GetByEmail("u@e")).Returns(new User { Id = 7 });
            var ex = Assert.Throws<Exception>(() => _svc.ReserveBook(1, "u@e"));
            Assert.Equal("Book not found.", ex.Message);
        }

        [Fact]
        public void ReserveBook_Should_Throw_When_Book_NotAvailable()
        {
            _userRepo.Setup(x => x.GetByEmail("u@e")).Returns(new User { Id = 7 });
            _bookRepo.Setup(x => x.GetById(1)).Returns(new Book { Id = 1, Stock = 0, IsActive = true });
            var ex = Assert.Throws<Exception>(() => _svc.ReserveBook(1, "u@e"));
            Assert.Equal("Book is not available for reservation.", ex.Message);
        }

        [Fact]
        public void ReserveBook_Should_Throw_When_Book_Inactive()
        {
            _userRepo.Setup(x => x.GetByEmail("u@e")).Returns(new User { Id = 7 });
            _bookRepo.Setup(x => x.GetById(1)).Returns(new Book { Id = 1, Stock = 1, IsActive = false });
            var ex = Assert.Throws<Exception>(() => _svc.ReserveBook(1, "u@e"));
            Assert.Equal("Book is not available for reservation.", ex.Message);
        }

        [Fact]
        public void ReserveBook_Should_Throw_When_AlreadyReserved()
        {
            _userRepo.Setup(x => x.GetByEmail("u@e")).Returns(new User { Id = 7 });
            _bookRepo.Setup(x => x.GetById(1)).Returns(new Book { Id = 1, Stock = 1, IsActive = true });
            _resRepo.Setup(x => x.HasActiveReservation(1)).Returns(true);
            var ex = Assert.Throws<Exception>(() => _svc.ReserveBook(1, "u@e"));
            Assert.Equal("This book is already reserved by another user.", ex.Message);
        }

        [Fact]
        public void ReserveBook_Should_Throw_When_UserHas3Reservations()
        {
            _userRepo.Setup(x => x.GetByEmail("u@e")).Returns(new User { Id = 7 });
            _bookRepo.Setup(x => x.GetById(1)).Returns(new Book { Id = 1, Stock = 1, IsActive = true });
            _resRepo.Setup(x => x.HasActiveReservation(1)).Returns(false);
            _resRepo.Setup(x => x.GetActiveReservationCountByUserId(7)).Returns(3);
            var ex = Assert.Throws<Exception>(() => _svc.ReserveBook(1, "u@e"));
            Assert.Equal("You can only have a maximum of 3 active reservations.", ex.Message);
        }

        [Fact]
        public void ReserveBook_Should_AddReservation_DecrementStock_And_Save()
        {
            var user = new User { Id = 7 };
            var book = new Book { Id = 1, Stock = 2, IsActive = true };
            _userRepo.Setup(x => x.GetByEmail("u@e")).Returns(user);
            _bookRepo.Setup(x => x.GetById(1)).Returns(book);
            _resRepo.Setup(x => x.HasActiveReservation(1)).Returns(false);
            _resRepo.Setup(x => x.GetActiveReservationCountByUserId(7)).Returns(0);

            _svc.ReserveBook(1, "u@e", reservationHours: 24);

            Assert.Equal(1, book.Stock);
            _bookRepo.Verify(x => x.Update(It.Is<Book>(b => b.Stock == 1)), Times.Once);
            _resRepo.Verify(x => x.Add(It.Is<BookReservation>(r => r.BookId == 1 && r.UserId == 7 && r.Status == "Active")), Times.Once);
            _resRepo.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void RequestCheckIn_Should_Throw_When_Expired()
        {
            var user = new User { Id = 7 };
            var reservation = new BookReservation { Id = 5, UserId = 7, Status = "Active", ExpiresAt = DateTime.UtcNow.AddHours(-1) };
            _userRepo.Setup(x => x.GetByEmail("u@e")).Returns(user);
            _resRepo.Setup(x => x.GetById(5)).Returns(reservation);

            var ex = Assert.Throws<Exception>(() => _svc.RequestCheckIn(5, "u@e"));
            Assert.Equal("This reservation has expired.", ex.Message);
            Assert.Equal("Expired", reservation.Status);
            _resRepo.Verify(x => x.Update(It.Is<BookReservation>(r => r.Status == "Expired")), Times.Once);
            _resRepo.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void RequestCheckIn_Should_SetFlags_And_Save()
        {
            var user = new User { Id = 7 };
            var reservation = new BookReservation { Id = 5, UserId = 7, Status = "Active", ExpiresAt = DateTime.UtcNow.AddHours(1) };
            _userRepo.Setup(x => x.GetByEmail("u@e")).Returns(user);
            _resRepo.Setup(x => x.GetById(5)).Returns(reservation);

            _svc.RequestCheckIn(5, "u@e");

            Assert.True(reservation.CheckInRequested);
            Assert.NotNull(reservation.CheckInRequestedAt);
            _resRepo.Verify(x => x.Update(It.Is<BookReservation>(r => r.CheckInRequested)), Times.Once);
            _resRepo.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void AcceptCheckIn_Should_ReturnBorrow_And_UpdateReservation()
        {
            var reservation = new BookReservation { Id = 5, BookId = 10, UserId = 7, Status = "Active", CheckInRequested = true };
            _resRepo.Setup(x => x.GetById(5)).Returns(reservation);

            var borrow = _svc.AcceptCheckIn(5, librarianId: 100, borrowDays: 7);

            Assert.NotNull(borrow);
            Assert.Equal(10, borrow.BookId);
            Assert.Equal(7, borrow.UserId);
            Assert.Equal("Active", borrow.Status);
            Assert.True((borrow.DueDate - borrow.BorrowedAt).TotalDays >= 6.9);
            Assert.Equal("CheckedIn", reservation.Status);
            Assert.NotNull(reservation.CheckedInAt);
            _resRepo.Verify(x => x.Update(It.Is<BookReservation>(r => r.Status == "CheckedIn")), Times.Once);
            _resRepo.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void CancelReservation_Should_RestoreStock_UpdateStatus_And_Save()
        {
            var user = new User { Id = 7 };
            var book = new Book { Id = 10, Stock = 0 };
            var reservation = new BookReservation { Id = 5, BookId = 10, UserId = 7, Status = "Active" };
            _userRepo.Setup(x => x.GetByEmail("u@e")).Returns(user);
            _resRepo.Setup(x => x.GetById(5)).Returns(reservation);
            _bookRepo.Setup(x => x.GetById(10)).Returns(book);

            _svc.CancelReservation(5, "u@e");

            Assert.Equal(1, book.Stock);
            Assert.Equal("Cancelled", reservation.Status);
            _bookRepo.Verify(x => x.Update(It.Is<Book>(b => b.Stock == 1)), Times.Once);
            _resRepo.Verify(x => x.Update(It.Is<BookReservation>(r => r.Status == "Cancelled")), Times.Once);
            _resRepo.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void ExpireOldReservations_Should_ExpireAndRestoreStock()
        {
            var nowMinus = DateTime.UtcNow.AddHours(-2);
            var nowPlus = DateTime.UtcNow.AddHours(2);
            var r1 = new BookReservation { Id = 1, BookId = 10, ExpiresAt = nowMinus, Status = "Active" };
            var r2 = new BookReservation { Id = 2, BookId = 11, ExpiresAt = nowPlus, Status = "Active" };
            var b1 = new Book { Id = 10, Stock = 0 };

            _resRepo.Setup(x => x.GetAllActiveReservations()).Returns(new[] { r1, r2 });
            _bookRepo.Setup(x => x.GetById(10)).Returns(b1);
            _bookRepo.Setup(x => x.GetById(11)).Returns((Book?)null);

            _svc.ExpireOldReservations();

            Assert.Equal("Expired", r1.Status);
            Assert.Equal("Active", r2.Status);
            Assert.Equal(1, b1.Stock);
            _resRepo.Verify(x => x.Update(It.Is<BookReservation>(r => r.Id == 1 && r.Status == "Expired")), Times.Once);
            _resRepo.Verify(x => x.Save(), Times.Once);
        }
    }
}
