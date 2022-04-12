#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LinkedHUCENGv2.Data;
using LinkedHUCENGv2.Models;

namespace LinkedHUCENGv2.Controllers
{
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FollowController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Follow
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Follows.Include(f => f.Account1).Include(f => f.Account2);
            Console.WriteLine("sadfasdfasdfasdf");
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Follow/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var follow = await _context.Follows
                .Include(f => f.Account1)
                .Include(f => f.Account2)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (follow == null)
            {
                return NotFound();
            }

            return View(follow);
        }

        // GET: Follow/Create
        public IActionResult Create()
        {
            ViewData["Account1Id"] = new SelectList(_context.Accounts, "Id", "Id");
            ViewData["Account2Id"] = new SelectList(_context.Accounts, "Id", "Id");
            return View();
        }

        // POST: Follow/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DateCreated,Account1Id,Account2Id")] Follow follow)
        {
            if (ModelState.IsValid)
            {
                _context.Add(follow);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Account1Id"] = new SelectList(_context.Accounts, "Id", "Id", follow.Account1Id);
            ViewData["Account2Id"] = new SelectList(_context.Accounts, "Id", "Id", follow.Account2Id);
            return View(follow);
        }

        // GET: Follow/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var follow = await _context.Follows.FindAsync(id);
            if (follow == null)
            {
                return NotFound();
            }
            ViewData["Account1Id"] = new SelectList(_context.Accounts, "Id", "Id", follow.Account1Id);
            ViewData["Account2Id"] = new SelectList(_context.Accounts, "Id", "Id", follow.Account2Id);
            return View(follow);
        }

        // POST: Follow/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DateCreated,Account1Id,Account2Id")] Follow follow)
        {
            if (id != follow.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(follow);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FollowExists(follow.Id))
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
            ViewData["Account1Id"] = new SelectList(_context.Accounts, "Id", "Id", follow.Account1Id);
            ViewData["Account2Id"] = new SelectList(_context.Accounts, "Id", "Id", follow.Account2Id);
            return View(follow);
        }

        // GET: Follow/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var follow = await _context.Follows
                .Include(f => f.Account1)
                .Include(f => f.Account2)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (follow == null)
            {
                return NotFound();
            }

            return View(follow);
        }

        // POST: Follow/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var follow = await _context.Follows.FindAsync(id);
            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FollowExists(int id)
        {
            return _context.Follows.Any(e => e.Id == id);
        }
    }
}
