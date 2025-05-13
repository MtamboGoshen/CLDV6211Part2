using EventEase.Models;
using EventEasePOE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEasePOE.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Booking/Index with search
        public async Task<IActionResult> Index(string searchTerm)
        {
            var bookings = _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                bookings = bookings.Where(b =>
                    b.Event.EventName.Contains(searchTerm) ||
                    b.Venue.Name.Contains(searchTerm) ||
                    b.BookingDate.ToString().Contains(searchTerm));
            }

            return View(await bookings.ToListAsync());
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName");
            ViewBag.VenueId = new SelectList(_context.Venue, "Id", "Name");
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.Booking.AnyAsync(b =>
                    b.VenueId == booking.VenueId &&
                    b.BookingDate == booking.BookingDate);

                if (exists)
                {
                    ModelState.AddModelError("", "This venue is already booked on the selected date.");
                    ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
                    ViewBag.VenueId = new SelectList(_context.Venue, "Id", "Name", booking.VenueId);
                    return View(booking);
                }

                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(_context.Venue, "Id", "Name", booking.VenueId);
            return View(booking);
        }

        // GET: Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(_context.Venue, "Id", "Name", booking.VenueId);
            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(_context.Venue, "Id", "Name", booking.VenueId);
            return View(booking);
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Booking/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }
    }
}
