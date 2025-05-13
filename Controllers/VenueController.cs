using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventEase.Models;
using EventEasePOE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public VenueController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Venue
        public async Task<IActionResult> Index()
        {
            var venueList = await _context.Venue.ToListAsync();
            return View(venueList);
        }

        // GET: Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null)
                    {
                        var blobUrl = await UploadImageToBlobAzure(venue.ImageFile);
                        venue.ImageUrl = blobUrl;
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Venue created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving venue: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "There was a problem saving the venue.");
                }
            }
            return View(venue);
        }

        // GET: Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(v => v.Id == id);
            if (venue == null)
                return NotFound();

            return View(venue);
        }

        // POST: Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVenue = await _context.Venue.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);

                    if (existingVenue == null)
                        return NotFound();

                    if (venue.ImageFile != null)
                    {
                        var blobUrl = await UploadImageToBlobAzure(venue.ImageFile);
                        venue.ImageUrl = blobUrl;
                    }
                    else
                    {
                        venue.ImageUrl = existingVenue.ImageUrl;
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Venue updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Venue.Any(e => e.Id == venue.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(venue);
        }

        // GET: Venue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(v => v.Id == id);
            if (venue == null)
                return NotFound();

            return View(venue);
        }

        // POST: Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue
                .Include(v => v.Booking)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null)
                return NotFound();

            if (venue.Booking != null && venue.Booking.Any())
            {
                TempData["Error"] = "Cannot delete this venue because it has associated bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Venue deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Venue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var venue = await _context.Venue
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null)
                return NotFound();

            return View(venue);
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
