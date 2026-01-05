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

    // --- GET ALL BOOKS TESTS ---

    [Fact]
    public void GetAllBooks_Should_ReturnAllBooks()
    {
      // Arrange
      var books = new List<Book>
      {
        new Book { Id = 1, Title = "Book 1", ISBN = "978-111" },
        new Book { Id = 2, Title = "Book 2", ISBN = "978-222" }
      };

      _mockBookRepository.Setup(x => x.GetAll()).Returns(books);

      // Act
      var result = _bookService.GetAllBooks();

      // Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count());
      _mockBookRepository.Verify(x => x.GetAll(), Times.Once);
    }

    // --- GET BOOK BY ID TESTS ---

    [Fact]
    public void GetBookById_Should_ReturnBook_When_BookExists()
    {
      // Arrange
      var bookId = 1;
      var book = new Book { Id = bookId, Title = "Test Book", ISBN = "978-123" };
      _mockBookRepository.Setup(x => x.GetById(bookId)).Returns(book);

      // Act
      var result = _bookService.GetBookById(bookId);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(bookId, result.Id);
      Assert.Equal("Test Book", result.Title);
    }

    [Fact]
    public void GetBookById_Should_ReturnNull_When_BookDoesNotExist()
    {
      // Arrange
      var bookId = 999;
      _mockBookRepository.Setup(x => x.GetById(bookId)).Returns((Book?)null);

      // Act
      var result = _bookService.GetBookById(bookId);

      // Assert
      Assert.Null(result);
    }

    // --- SEARCH BOOKS TESTS ---

    [Fact]
    public void SearchBooks_Should_ReturnMatchingBooks()
    {
      // Arrange
      var searchTerm = "Clean";
      var matchingBooks = new List<Book>
      {
        new Book { Id = 1, Title = "Clean Code", ISBN = "978-111" },
        new Book { Id = 2, Title = "Clean Architecture", ISBN = "978-222" }
      };

      _mockBookRepository.Setup(x => x.SearchBooks(searchTerm)).Returns(matchingBooks);

      // Act
      var result = _bookService.SearchBooks(searchTerm);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count());
      _mockBookRepository.Verify(x => x.SearchBooks(searchTerm), Times.Once);
    }

    [Fact]
    public void SearchBooks_Should_ReturnEmptyList_When_NoMatchesFound()
    {
      // Arrange
      var searchTerm = "NonExistent";
      _mockBookRepository.Setup(x => x.SearchBooks(searchTerm)).Returns(new List<Book>());

      // Act
      var result = _bookService.SearchBooks(searchTerm);

      // Assert
      Assert.NotNull(result);
      Assert.Empty(result);
    }

    [Fact]
    public void SearchBooks_Should_ReturnAllBooks_When_SearchTermIsEmpty()
    {
      // Arrange
      var allBooks = new List<Book>
      {
        new Book { Id = 1, Title = "Book 1", ISBN = "978-111" },
        new Book { Id = 2, Title = "Book 2", ISBN = "978-222" }
      };

      _mockBookRepository.Setup(x => x.SearchBooks(string.Empty)).Returns(allBooks);

      // Act
      var result = _bookService.SearchBooks(string.Empty);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count());
    }

    [Fact]
    public void DeleteBook_Should_DoNothing_When_BookDoesNotExist()
    {
      // Arrange
      var bookId = 999;
      _mockBookRepository.Setup(x => x.GetById(bookId)).Returns((Book?)null);

      // Act
      _bookService.DeleteBook(bookId);

      // Assert - should not throw exception
      _mockBookRepository.Verify(x => x.Delete(bookId), Times.Never);
    }
  }
}