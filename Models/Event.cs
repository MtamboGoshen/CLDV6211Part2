using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace EventEasePOE.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        // Navigation property to the Venue model
        public Venue? Venue { get; set; }

        public List<Booking> Booking { get; set; } = new List<Booking>();

        // Property for uploading image (not stored in the DB)
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
