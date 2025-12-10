using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BooksMart.Models.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
    }
}
