using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Library.Data;
using Library.Models;
using ExcelDataReader;
using EFCore.BulkExtensions;


namespace Library.Controllers
{
    public class LibraryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LibraryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Library
        public async Task<IActionResult> Index(string? sortOrder, string? currentFilter, string? searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSort"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["AuthorSort"] = sortOrder == "Author" ? "Author_desc" : "Author";
            ViewData["YearSort"] = sortOrder == "Year" ? "Year_desc" : "Year";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            
            ViewData["CurrentFilter"] = searchString;

            var bookModels = _context.Books;
            var groupedBookModels = bookModels
                .GroupBy(b => new {
                        b.Title,
                        b.BookAuthor,
                        b.ReleaseDate,
                    })
                .Select(g => new BookGrouping
                        {
                            Title = g.Key.Title,
                            BookAuthor = g.Key.BookAuthor,
                            ReleaseDate = g.Key.ReleaseDate,
                            Models = g.ToList(),
                        });
            if(!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToUpper();
                groupedBookModels = groupedBookModels.Where(g => g.Title.ToUpper().Contains(searchString) || g.BookAuthor.ToUpper().Contains(searchString) || g.ReleaseDate.ToUpper().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "title_desc":
                    groupedBookModels = groupedBookModels.OrderByDescending(b => b.Title);
                    break;
                case "Author":
                    groupedBookModels = groupedBookModels.OrderBy(b => b.BookAuthor);
                    break;
                case "author_desc":
                    groupedBookModels = groupedBookModels.OrderByDescending(b => b.BookAuthor);
                    break;
                case "Year":
                    groupedBookModels = groupedBookModels.OrderBy(b => b.ReleaseDate);
                    break;
                case "Year_desc":
                    groupedBookModels = groupedBookModels.OrderByDescending(b => b.ReleaseDate);
                    break;
                default:
                    groupedBookModels = groupedBookModels.OrderBy(b => b.Title);
                    break;
            }
            int pageSize = 20;
            return _context.Books != null ? 
                        View(await PaginatedList<BookGrouping>.CreateAsync(groupedBookModels, pageNumber ?? 1, pageSize)) :
                        Problem("Entity set 'ApplicationDbContext.Books'  is null.");
        }

        // GET: Library/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var bookModel = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookModel == null)
            {
                return NotFound();
            }

            return View(bookModel);
        }

        // GET: Library/Create
        [Authorize(Policy="IsLibrarian")]
        public IActionResult Create()
        {
            return View();
        }
        // POST: Library/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy="IsLibrarian")]
        public async Task<IActionResult> Create([Bind("InventoryID,Title,BookAuthor,ReleaseDate,Skyrius")] BookModel bookModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bookModel);
        }

        // GET: Library/Edit/5
        [Authorize(Policy = "IsLibrarian")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var bookModel = await _context.Books.FindAsync(id);
            if (bookModel == null)
            {
                return NotFound();
            }
            return View(bookModel);
        }

        // POST: Library/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "IsLibrarian")]
        public async Task<IActionResult> Edit(int id, [Bind("Id","InventoryID,Title,BookAuthor,ReleaseDate,Skyrius,IsAvailable")] BookModel bookModel)
        {
            if (id != bookModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookModelExists(bookModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bookModel);
        }

        // GET: Library/Delete/5
        [Authorize(Policy = "IsLibrarian")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var bookModel = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookModel == null)
            {
                return NotFound();
            }

            return View(bookModel);
        }

        // POST: Library/Delete/5
        [Authorize(Policy = "IsLibrarian")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Books == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Books'  is null.");
            }
            var bookModel = await _context.Books.FindAsync(id);
            if (bookModel != null)
            {
                _context.Books.Remove(bookModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //GET: Library/BookBorrowingList
        [Authorize(Policy = "IsLibrarian")]
        public async Task<IActionResult> BookBorrowingList(int? id)
        {
            if(id != null)
            {
            ViewData["id"] = id;
            return _context.BookBorrowings != null ? 
                View(await _context.BookBorrowings
                        .Include(f => f.Book)
                        .Include(f => f.User)
                        .Where(f => f.Book.Id == id)
                        .OrderByDescending(f => f.WhenBorrowed)
                        .ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.BookBorrowings'  is null.");
            }
            else
            {
                return _context.BookBorrowings != null ? 
                    View(await _context.BookBorrowings
                        .Include(f => f.Book)
                        .Include(f => f.User)
                        .OrderByDescending(f => f.WhenBorrowed)
                        .ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.BookBorrowings'  is null.");
            }
        }
        //GET: Library/BorrowBook/5
        [Route("Library/BorrowBook/{originalId:int}")]
        [Authorize(Policy="IsLibrarian")]
        public async Task<IActionResult> BorrowBook(int? originalId)
        {
            if (originalId == null || _context.Books == null)
            {
                return NotFound();
            }

            var bookModel = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == originalId);
            if (bookModel == null)
            {
                return NotFound();
            }

            var borrowModel = new BorrowingEntryModel();

            borrowModel.Book = bookModel;

            var users = _context.Users.AsQueryable();

            ViewBag.Users = users.ToList();
            return View(borrowModel);
        }
        //POST Library/BorrowBook/5
        [HttpPost]
        [Authorize(Policy = "IsLibrarian")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBorrowBook([Bind("Book, User")] BorrowingEntryModel borrowingEntryModel)
        {
            //clear the model state, find the book and user objects via their ID in the form then set the values.
            ModelState.Clear();
            ApplicationUser modelUser = await _context.Users.FindAsync(borrowingEntryModel.User.Id);
            BookModel modelBook = await _context.Books.FindAsync(borrowingEntryModel.Book.Id);
            
            borrowingEntryModel.User = modelUser;
            borrowingEntryModel.Book = modelBook;

            if (ModelState.IsValid)
            {
                _context.BookBorrowings.Add(borrowingEntryModel);
                modelBook.IsAvailable = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details","Library", new {id = borrowingEntryModel.Book.Id} );

        }
        //GET Library/ReturnBook/5
        [Route("Library/ReturnBook/{originalId:int}")]
        [Authorize(Policy = "IsLibrarian")]
        //the ID that is passed is the book ID, not the entry ID
        public async Task<IActionResult> ReturnBook(int? originalId)
        {
            if (originalId == null || _context.BookBorrowings == null)
            {
                return NotFound();
            }

            var bookBorrowing = await _context.BookBorrowings
                .Where(b => b.Book.Id == originalId)
                .Include(b => b.Book)
                .Include(b => b.User)
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();
            if (bookBorrowing == null)
            {
                return NotFound();
            }

            return View(bookBorrowing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "IsLibrarian")]
        //POST ReturnBook
        public async Task<IActionResult> ConfirmReturnBook(BorrowingEntryModel borrowingEntryModel)
        {
            //hopefully this is a temporary implementation, as this is very inelegant.
            
            var originalBook = await _context.Books
                .FindAsync(borrowingEntryModel.Book.Id);

            ModelState.Clear();

            borrowingEntryModel.Book = originalBook;
            borrowingEntryModel.WhenReturned = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(borrowingEntryModel);
                    borrowingEntryModel.Book.IsAvailable = true;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowingEntryModelExists(borrowingEntryModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            return RedirectToAction("Details","Library", new {id = borrowingEntryModel.Book.Id} );
        }
        //GET: Library/BorrowedUserBooks
        //This controller is to get the current users borrowed but unreturned books.
        //Also, it can show history if viewToDirectTo = History
        [Authorize]
        public async Task<IActionResult> BorrowedUserBooks(string? viewToDirectTo)
        {
            if(viewToDirectTo == null && viewToDirectTo != "Debts" && viewToDirectTo != "History")
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var currentUserId = await _userManager.GetUserIdAsync(user);
            Console.WriteLine(currentUserId);
            var borrowedBooks = await _context.BookBorrowings
                .Where(b => b.User.Id == currentUserId)
                .Include(b => b.Book)
                .OrderByDescending(b => b.WhenBorrowed)
                .ToListAsync();
            if (viewToDirectTo == "Debts")
            {
                return View(borrowedBooks);
            }
            else 
            {
                return View("UserHistory", borrowedBooks);
            }
        }
        //GET: Library/ImportFromExcel
        [Authorize(Policy="IsLibrarian")]
        public async Task<IActionResult> ImportFromExcel()
        {
            return View();
        }
        //GET: Library/ImportFromExcelUploadConfirm
        [Authorize(Policy="IsLibrarian")]
        public async Task<IActionResult> ImportFromExcelConfirm(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Prašome įkelti tinkamą, ne tuščią excel lapą.";
                return View("ImportFromExcel");
            }
            var fileExtension = Path.GetExtension(file.FileName);
            if (fileExtension != ".xlsx" && fileExtension != ".xls")
            {
                ViewBag.Error = "Prašome įkelti tinkamo formato (.xlsx ir .xls) failus.";
                return View("ImportFromExcel");
            }

            List<BookModel> bookModels = new List<BookModel>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true,
                        }
                    });

                    var dataTable = result.Tables[0];
                    int temp = 0;
                    foreach(var c in dataTable.Columns){
                        dataTable.Columns[temp].ColumnName = dataTable.Columns[temp].ColumnName.Trim();
                        temp++;
                    }

                    char[] delimiterChars = {',', '.'};

                    for (int row = 0; row < dataTable.Rows.Count; row++)
                    {
                        try{
                        string[] bookInventoryID = dataTable.Rows[row]["Invent. Nr."].ToString().Split(delimiterChars);
                        foreach(var inventoryID in bookInventoryID)
                        {
                            var bookModel = new BookModel();
                            bookModel.InventoryID = inventoryID;
                            bookModel.Title = dataTable.Rows[row]["Pavadinimas"].ToString().Trim();
                            bookModel.BookAuthor = dataTable.Rows[row]["Autorius"].ToString().Trim();
                            bookModel.ReleaseDate = dataTable.Rows[row]["Metai"].ToString().Trim();
                            bookModel.Skyrius = dataTable.Rows[row]["Skyrius"].ToString().Trim();
                            bookModel.Price = dataTable.Rows[row]["Kaina"].ToString().Trim();

                            bookModels.Add(bookModel);
                        }
                        }
                        catch (System.ArgumentException)
                        {
                            ViewBag.Error = "Nerasti tinkami stulpeliai. Prašome atidžiai peržiūrėti jūsų keliamą failą ar tinkamai yra užvadinti stulpeliai, ar nėra nereikalingų tarpų prieš ar po pavadinimo.";
                            return View("ImportFromExcel");
           
                        }

                    }
                }
            }
            var existingBooks = await _context.Books
                .Select(b => new BookModel 
                        {
                        InventoryID = b.InventoryID,
                        Title = b.Title})
                .ToListAsync();
            //get all books with unique inventory ID and title combinations
            var newBooks = bookModels.Where(n => !existingBooks.Any(e => e.InventoryID == n.InventoryID && e.Title == n.Title)).ToList();



            if(newBooks.Count < 1 && newBooks != null)
            {
                ViewBag.Error = "Nerasta naujų knygų.";
                return View("ImportFromExcel");
            }

            return View(newBooks);
        }

        //POST: Library/ImportFromExcelConfirmPost
        [HttpPost]
        [Authorize(Policy="IsLibrarian")]
        public async Task<IActionResult> ImportFromExcelConfirmPost(List<BookModel> bookModels)
        {
            if(bookModels == null)
            {
                return NotFound();
            }
            await _context.BulkInsertAsync(bookModels);
            await _context.BulkSaveChangesAsync();
            return RedirectToAction("Index");

        }
        [Authorize(Policy = "IsLibrarian")]
        public async Task<IActionResult> BookBorrowingListFilter()
        {
            return View();
        }
        [Authorize(Policy = "IsLibrarian")]
        [HttpPost]
        public async Task<IActionResult>BookBorrowingListFiltered(string? IsGroupExpired, string? IsUnreturned)
        {
            Console.WriteLine($"{IsGroupExpired}, {IsUnreturned}");
            if(IsGroupExpired == null && IsUnreturned == null)
            {
                return RedirectToAction("BookBorrowingList");
            }
            List<BorrowingEntryModel> modelList;
            //Check what librarian wants to look for, then get the appropriate models.
            if(IsGroupExpired != null)
            {
                modelList = await _context.BookBorrowings.Include(b => b.Book).Include(b => b.User.Group).Where(b => b.User.Group.EndDate < DateTime.Now && b.User.Group != null).ToListAsync();
            }
            else
            {
                modelList = await _context.BookBorrowings.Include(b => b.Book).ToListAsync();
            }

            if(IsUnreturned != null)
            {
                modelList = modelList.Where(b => b.WhenReturned == null).ToList();
            }
            return View("BookBorrowingList", modelList);

        }
        
        //GET: Library/SearchByInventoryId
        [Authorize(Policy = "IsLibrarian")]
        public async Task<IActionResult> SearchByInventoryId(string searchQuery)
        {
            List<BookModel> bookModels = new List<BookModel>();

            if(!String.IsNullOrEmpty(searchQuery))
            {
                bookModels = await _context.Books.Where(b => b.InventoryID == searchQuery).ToListAsync();
                ViewBag.CurrentQuery = searchQuery;

            }
            return View(bookModels);
        }
        
        private bool BookModelExists(int id)
        {
            return (_context.Books?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private bool BorrowingEntryModelExists(int id)
        {
            return (_context.BookBorrowings?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
