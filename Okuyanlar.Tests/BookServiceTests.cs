using Moq;
using Xunit;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Service.Services;

namespace Okuyanlar.Tests
{
  /// <summary>
  /// Unit tests for <see cref="BookService"/>.
  /// Verifies business rules for adding, updating, and deleting books.
  /// </summary>
  public class BookServiceTests
  {
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
      _mockBookRepository = new Mock<IBookRepository>();
      // Servisi henüz yazmadık ama test, bu imzaya sahip olacağını varsayar.
      _bookService = new BookService(_mockBookRepository.Object);
    }

    // --- ADD BOOK TESTS ---

    [Fact]
    public void AddBook_Should_AddBook_When_DetailsAreValid()
    {
      // Arrange
      var book = new Book { Title = "Clean Code", ISBN = "978-123", Stock = 5 };

      // Mock: No existing book with this ISBN
      _mockBookRepository.Setup(x => x.GetByISBN(book.ISBN)).Returns((Book?)null);

      // Act
      _bookService.AddBook(book);

      // Assert
      _mockBookRepository.Verify(x => x.Add(book), Times.Once);
    }

    [Fact]
    public void AddBook_Should_ThrowException_When_ISBNAlreadyExists()
    {
      // Arrange
      var book = new Book { Title = "Duplicate Book", ISBN = "978-SAME", Stock = 1 };

      // Mock: Simulate that a book with this ISBN already exists
      _mockBookRepository.Setup(x => x.GetByISBN(book.ISBN)).Returns(new Book());

      // Act & Assert
      var exception = Assert.Throws<InvalidOperationException>(() => _bookService.AddBook(book));
      Assert.Equal("A book with this ISBN already exists.", exception.Message);

      _mockBookRepository.Verify(x => x.Add(It.IsAny<Book>()), Times.Never);
    }

    [Fact]
    public void AddBook_Should_ThrowException_When_StockIsNegative()
    {
      // Arrange
      var book = new Book { Title = "Negative Stock", ISBN = "978-555", Stock = -1 };

      // Act & Assert
      var exception = Assert.Throws<ArgumentException>(() => _bookService.AddBook(book));
      Assert.Equal("Stock cannot be negative.", exception.Message);
    }

    // --- UPDATE BOOK TESTS ---

    [Fact]
    public void UpdateBook_Should_Update_When_BookExists()
    {
      // Arrange
      var existingBook = new Book { Id = 1, Title = "Old Title", Stock = 5 };
      var updatedInfo = new Book { Id = 1, Title = "New Title", Stock = 10 };

      _mockBookRepository.Setup(x => x.GetById(1)).Returns(existingBook);

      // Act
      _bookService.UpdateBook(updatedInfo);

      // Assert
      _mockBookRepository.Verify(x => x.Update(It.Is<Book>(b => b.Title == "New Title" && b.Stock == 10)), Times.Once);
    }

    // --- DELETE BOOK TESTS ---

    [Fact]
    public void DeleteBook_Should_Delete_When_BookExists()
    {
      // Arrange
      var bookId = 1;
      var existingBook = new Book { Id = bookId };
      _mockBookRepository.Setup(x => x.GetById(bookId)).Returns(existingBook);

      // Act
      _bookService.DeleteBook(bookId);

      // Assert
      _mockBookRepository.Verify(x => x.Delete(bookId), Times.Once);
    }

    [Fact]
    public void UpdateBook_Should_ThrowException_When_BookDoesNotExist()
    {
      // Arrange
      var nonExistentBookId = 999;
      var updateModel = new Book { Id = nonExistentBookId, Title = "Ghost Book" };

      // Mock: No record with this ID exists in the database.      
      _mockBookRepository.Setup(x => x.GetById(nonExistentBookId)).Returns((Book?)null);

      // Act & Assert
      // Note: Use this test if your service is throwing errors.
      // If it passes silently, you should use Verify
      var exception = Assert.Throws<Exception>(() => _bookService.UpdateBook(updateModel));
      Assert.Equal("Book not found.", exception.Message);
    }
  }
}