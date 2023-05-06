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
    [Authorize(Policy="IsLibrarian")]
    public class UserManagement : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserManagement(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index()
        {
              return _context.Users != null ? 
                          View(await _context.Users.Include(u => u.Group).ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Users'  is null.");
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            int? currentGroupId = applicationUser.Group?.Id;

            ViewBag.Groups = await _context.Groups.Where(g => g.Id != currentGroupId).ToListAsync();

            return View(applicationUser);
        }

        // POST: UserManagement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("FirstName,LastName,Id,Email,PhoneNumber,Group")] ApplicationUser applicationUser)
        {
            if (id != applicationUser.Id)
            {
                return NotFound();
            }
            //we load the real user and then copy the values
            var applicationUserReal = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (applicationUserReal == null)
            {
                return NotFound();
            }

            applicationUserReal.FirstName = applicationUser.FirstName;
            applicationUserReal.LastName = applicationUser.LastName;
            applicationUserReal.Email = applicationUser.Email;
            applicationUserReal.PhoneNumber = applicationUser.PhoneNumber;

            applicationUserReal.Group = await _context.Groups.FindAsync(applicationUser.Group.Id);
            ModelState.Clear();
            if (TryValidateModel(nameof(applicationUserReal)))
            {
                try
                {
                    //_context.Entry(applicationUserReal).Reference(x => x.Group).IsModified = true;
                    _context.Users.Update(applicationUserReal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationUserExists(applicationUserReal.Id))
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
            return await Edit(id);
        }

        // GET: UserManagement/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // POST: UserManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users'  is null.");
            }
            var applicationUser = await _context.Users.FindAsync(id);
            if (applicationUser != null)
            {
                _context.Users.Remove(applicationUser);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //GET: UserManagement/ControlPanel
        public async Task<IActionResult> ControlPanel()
        {
            return View();
        }

        private bool ApplicationUserExists(string id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
