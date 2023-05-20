using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Library.Data;
using Library.Models;
using CsvHelper;

namespace Library.Controllers
{
    [Authorize(Policy = "IsLibrarian")]
    public class GroupManagement : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public GroupManagement(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
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
        //GET: GroupManagement/DeleteWithUsers/5
        public async Task<IActionResult> DeleteWithUsers(int? id)
        {
            if(id == null || _context.Groups == null)
            {
                return NotFound();
            }
            var groupModel = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == id);
            if(groupModel == null)
            {
                return NotFound();
            }
            return View(groupModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //POST: GroupManagement/DeleteWithUsers/5
        public async Task<IActionResult> DeleteWithUsersConfirmed(int? Id)
        {
            if(Id == null || _context.Groups == null)
            {
                return NotFound();
            }
            var groupModel = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == Id);
            if(groupModel == null)
            {
                return NotFound();
            }

            foreach(var user in groupModel.Members)
            {
                _context.Users.Remove(user);
            }
            _context.Groups.Remove(groupModel);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        //GET: GroupManagement/GenerateLoginDetails/5
        public async Task<IActionResult> GenerateLoginDetails(int? id)
        {
            if (id == null || _context.Groups == null || _context.Users == null)
            {
                return NotFound();
            }
            var groupModel = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == id);
            if (groupModel == null)
            {
                return NotFound();
            }
            return View(groupModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        //POST: /GroupManagement/GenerateLoginDetails/5
        public async Task<IActionResult> GenerateLoginDetailsConfirmed(int? Id)
        {
            if (Id == null || _context.Groups == null || _context.Users == null)
            {
                return NotFound();
            }
            var groupModel = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == Id);
            if (groupModel == null)
            {
                return NotFound();
            }
            var csvContent = new StringBuilder();

            csvContent.AppendLine("Žmogus, Vartotojo Vardas, Slaptažodis");

            foreach(var user in groupModel.Members)
            {
                if(user.TempPassword != null)
                {
                    csvContent.AppendLine($"{user.FullName},{user.UserName},{user.TempPassword}");
                }
            }
            string fileName = $"Login_{groupModel.Name}.csv";

            var csvBytes = Encoding.UTF8.GetBytes(csvContent.ToString());
            return File(csvBytes,"text/csv", fileName);


        }

        private bool GroupModelExists(int id)
        {
          return (_context.Groups?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
