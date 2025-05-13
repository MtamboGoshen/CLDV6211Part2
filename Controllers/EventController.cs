using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventEase.Models;
using EventEasePOE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEasePOE.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public EventController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Event
        public async Task<IActionResult> Index()
        {
            var eventList = await _context.Event
                .Include(e => e.Venue)
                .ToListAsync();
            return View(eventList);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            ViewBag.VenueId = new SelectList(_context.Venue, "Id", "Name");
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (@event.ImageFile != null)
                    {
                        var blobUrl = await UploadImageToBlobAzure(@event.ImageFile);
                        // Save blobUrl if needed
                    }

                    _context.Add(@event);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving event: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "There was a problem saving the event.");
                }
            }

            ViewBag.VenueId = new SelectList(_context.Venue, "Id", "Name", @event.VenueId);
            return View(@event);
        }

        // GET: Event/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var @event = await _context.Event.FindAsync(id);
            if (@event == null)
                return NotFound();

            ViewBag.VenueId = new SelectList(_context.Venue, "Id", "Name", @event.VenueId);
            return View(@event);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.EventId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Event.Any(e => e.EventId == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.VenueId = new SelectList(_context.Venue, "Id", "Name", @event.VenueId);
            return View(@event);
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
                return NotFound();

            return View(@event);
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
                return NotFound();

            return View(@event);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event
                .Include(e => e.Booking)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
                return NotFound();

            if (@event.Booking != null && @event.Booking.Any())
            {
                TempData["Error"] = "Cannot delete this event because it has associated booking(s).";
                return RedirectToAction(nameof(Index));
            }

            _context.Event.Remove(@event);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Event deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------
        // Azure Blob Upload Logic
        // -----------------------
        private async Task<string> UploadImageToBlobAzure(IFormFile imageFile)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=cldv6211poe;AccountKey=UKcuBELCmBaWvqckVjNorfg2C1gqEr6GkaWEN+/1WDShgK/vFkT4MtUqNbnw1/4/ganfjMfiv39L+AStXvK3Bg==;EndpointSuffix=core.windows.net";
            var containerName = "cldv6211poe";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = imageFile.ContentType
            };

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }

            return blobClient.Uri.ToString();
        }
    }
}
