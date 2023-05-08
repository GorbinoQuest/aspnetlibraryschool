using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
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
    [Authorize(Policy="IsLibrarian")]
    public class UserManagement : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public UserManagement(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index()
        {
            if (_userManager.Users == null)
            {
                 Problem("Entity set 'UserManager.Users'  is null.");
            }
            //we create a viewmodel that stores both claim value (in this case role) and the user together, so we can print it.
            var users = await _userManager.Users.Include(u => u.Group).ToListAsync();
            List<UserManagementViewModel> userManagementViewModels = new List<UserManagementViewModel>();
            foreach (var user in users)
            {
                var role = await _userManager.GetClaimsAsync(user).ContinueWith(claims => claims.Result.FirstOrDefault(c => c.Type == "Role")?.Value);;
                UserManagementViewModel userManagementViewModel = new UserManagementViewModel 
                {
                    User = user,
                    RoleValue = role,
                };
                userManagementViewModels.Add(userManagementViewModel);
            }
            return View(userManagementViewModels);

        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _userManager.Users == null)
            {
                return NotFound();
            }

            var applicationUser = await _userManager.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            int? currentGroupId = applicationUser.Group?.Id;

            ViewBag.Groups = await _context.Groups.Where(g => g.Id != currentGroupId).ToListAsync();
            
            ViewBag.RoleValue = await _userManager.GetClaimsAsync(applicationUser)
                .ContinueWith(claims => claims.Result.FirstOrDefault(c => c.Type == "Role")?.Value);

            return View(applicationUser);
        }

        // POST: UserManagement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string RoleValue, [Bind("FirstName,LastName,Id,Email,PhoneNumber,Group")] ApplicationUser applicationUser)
        {
            if (id != applicationUser.Id)
            {
                return NotFound();
            }
            //we load the real user and then copy the values
            var applicationUserReal = await _userManager.Users
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
                    if(RoleValue == "User" || RoleValue == "Librarian")
                    {
                        var claim = await _userManager.GetClaimsAsync(applicationUserReal).ContinueWith(claims => claims.Result.FirstOrDefault(c => c.Type == "Role"));
                        if(claim.Value == RoleValue)
                        {
                        }
                        else
                        {
                            await _userManager.RemoveClaimAsync(applicationUser, claim);

                            await _userManager.AddClaimAsync(applicationUser, new Claim("Role", RoleValue));
                        }
                    }

                    var result = await _userManager.UpdateAsync(applicationUserReal);
                    if(result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
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
            return await Task.Run(() => View());
        }
        public async Task<IActionResult> GenerateLoginDetails(string? id)
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateLoginDetailsConfirm(string? Id)
        {
            if(Id == null)
            {
                return NotFound();
            }
            var applicationUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == Id);
            var csvContent = new StringBuilder();

            csvContent.AppendLine("Vardas, Prisijungimo El. Paštas, Slaptažodis");
            csvContent.AppendLine($"{applicationUser.FullName},{applicationUser.Email},{applicationUser.TempPassword}");
            
            string fileName = $"Login_{applicationUser.FirstName}_{applicationUser.LastName}.csv";

            var csvBytes = Encoding.UTF8.GetBytes(csvContent.ToString());
            return File(csvBytes, "text/csv", fileName);

            
            //return RedirectToAction("Index");

        }

        private bool ApplicationUserExists(string id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
