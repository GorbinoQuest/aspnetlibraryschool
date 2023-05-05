using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Library.Data;
using Library.Models;

namespace Library.Controllers
{
    [Authorize(Policy = "IsLibrarian")]
    public class GroupManagement : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupManagement(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GroupManagement
        public async Task<IActionResult> Index()
        {
              return _context.Groups != null ? 
                          View(await _context.Groups.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Groups'  is null.");
        }

        // GET: GroupManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Groups == null)
            {
                return NotFound();
            }

            var groupModel = await _context.Groups
                .Include(m => m.Members)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupModel == null)
            {
                return NotFound();
            }

            return View(groupModel);
        }

        // GET: GroupManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GroupManagement/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,EndDate,GroupTeacherName")] GroupModel groupModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(groupModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(groupModel);
        }

        // GET: GroupManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Groups == null)
            {
                return NotFound();
            }

            var groupModel = await _context.Groups.FindAsync(id);
            if (groupModel == null)
            {
                return NotFound();
            }
            return View(groupModel);
        }

        // POST: GroupManagement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,EndDate,GroupTeacherName")] GroupModel groupModel)
        {
            if (id != groupModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupModelExists(groupModel.Id))
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
            return View(groupModel);
        }

        // GET: GroupManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Groups == null)
            {
                return NotFound();
            }

            var groupModel = await _context.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupModel == null)
            {
                return NotFound();
            }

            return View(groupModel);
        }

        // POST: GroupManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Groups == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Groups'  is null.");
            }
            var groupModel = await _context.Groups.FindAsync(id);
            if (groupModel != null)
            {
                _context.Groups.Remove(groupModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //GET GroupManagement/AddUser
        public async Task<IActionResult> AddUser(int? id)
        {
            if (id == null || _context.Groups == null)
            {
                return NotFound();
            }

            //get the group model
            var groupModel = await _context.Groups
                .Include(m => m.Members)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupModel == null)
            {
                return NotFound();
            }

            //get all users that have librarian role claim, then filter them out. Librarians aren't people (sorry).
            var idsToFilter = await _context.UserClaims
                .Where(c => c.ClaimType=="Role" && c.ClaimValue == "Librarian")
                .Select(c => c.UserId)
                .ToListAsync();

            var userList = await _context.Users
                .Where(u => !idsToFilter.Contains(u.Id))
                .Include(u => u.Group)
                .ToListAsync();

            ViewBag.UserList = userList;
            
            return View(groupModel);
        }
        [HttpPost, ActionName("AddUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGroupUsers(int? id, string[] selectedUsers)
        {
            if (id == null || _context.Groups == null)
            {
                return NotFound();
            }

            var groupModel = await _context.Groups
                .Include(m => m.Members)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (groupModel == null)
            {
                return NotFound();
            }
            //selects all users who have the IDs received from the form
            var users = await _context.Users.Where(u => selectedUsers.Contains(u.Id)).ToListAsync();
            //sets all Members to be the selected users
            groupModel.Members = users;

            if(TryValidateModel(groupModel))
            {
                _context.Groups.Update(groupModel);
                _context.SaveChanges();
            }
            
            return RedirectToAction("Details", "GroupManagement", new {id = id});
        }

        private bool GroupModelExists(int id)
        {
          return (_context.Groups?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
