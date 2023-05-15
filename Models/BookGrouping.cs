namespace Library.Models;

public class BookGrouping
{
    public string Title {get;set;}
    public string BookAuthor {get;set;}
    public string ReleaseDate {get;set;}
    public List<BookModel> Models {get;set;}
}
