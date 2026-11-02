using AutoMapper;
using LibraryApi.DTOs.Authors;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Categories;
using LibraryApi.Models;

namespace LibraryApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        MapCategories();
        MapAuthors();
        MapBooks();
    }

    private void MapCategories()
    {
        CreateMap<Category, CategoryResponse>();

        CreateMap<CreateCategoryRequest, Category>()
            .ForMember(category => category.Id, options => options.Ignore())
            .ForMember(category => category.Books, options => options.Ignore());

        CreateMap<UpdateCategoryRequest, Category>()
            .ForMember(category => category.Id, options => options.Ignore())
            .ForMember(category => category.Books, options => options.Ignore());
    }

    private void MapAuthors()
    {
        CreateMap<Author, AuthorResponse>();

        CreateMap<CreateAuthorRequest, Author>()
            .ForMember(author => author.Id, options => options.Ignore())
            .ForMember(author => author.BookAuthors, options => options.Ignore());

        CreateMap<UpdateAuthorRequest, Author>()
            .ForMember(author => author.Id, options => options.Ignore())
            .ForMember(author => author.BookAuthors, options => options.Ignore());
    }

    // BookAuthors is not mapped from the requests: the service builds the pivot rows from AuthorIds.
    private void MapBooks()
    {
        CreateMap<BookAuthor, BookAuthorResponse>();

        // The responses are positional records, so their values go through constructor parameters.
        CreateMap<Book, BookResponse>()
            .ForCtorParam(nameof(BookResponse.Authors), options => options.MapFrom(book => OrderAuthors(book)));

        CreateMap<Book, BookListItemResponse>()
            .ForCtorParam(
                nameof(BookListItemResponse.AuthorNames),
                options => options.MapFrom(book => OrderAuthors(book).Select(bookAuthor => bookAuthor.Author.Name)));

        CreateMap<CreateBookRequest, Book>()
            .ForMember(book => book.Id, options => options.Ignore())
            .ForMember(book => book.Category, options => options.Ignore())
            .ForMember(book => book.BookAuthors, options => options.Ignore());

        CreateMap<UpdateBookRequest, Book>()
            .ForMember(book => book.Id, options => options.Ignore())
            .ForMember(book => book.Category, options => options.Ignore())
            .ForMember(book => book.BookAuthors, options => options.Ignore());
    }

    // Primary author first, so clients can rely on the order.
    private static IEnumerable<BookAuthor> OrderAuthors(Book book)
    {
        return book.BookAuthors
            .OrderByDescending(bookAuthor => bookAuthor.IsPrimary)
            .ThenBy(bookAuthor => bookAuthor.Author.Name);
    }
}
