using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Library.Data;
using Library.Models;

namespace Library.Controllers
{
    public class LibraryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LibraryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Library
        public async Task<IActionResult> Index()
        {
              return _context.Books != null ? 
                          View(await _context.Books.ToListAsync()) :
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
        public async Task<IActionResult> Create([Bind("Title,BookAuthor,ReleaseDate,Genre")] BookModel bookModel)
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
        public async Task<IActionResult> Edit(int id, [Bind("Title,BookAuthor,ReleaseDate,Genre,IsAvailable")] BookModel bookModel)
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
                        .OrderByDescending(f => f.Id)
                        .ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.BookBorrowings'  is null.");
            }
            else
            {
                return _context.BookBorrowings != null ? 
                    View(await _context.BookBorrowings
                        .Include(f => f.Book)
                        .Include(f => f.User)
                        .OrderByDescending(f => f.Id)
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
            //TODO: Get the latest entry of BorrowingEntryModel and pass it to the view. Set IsAvailable to true if successful
            //BE SURE TO INCLUDE THE USER WITH .INCLUDE AND BOOK TO KNOW WHERE TO GO BACK
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
