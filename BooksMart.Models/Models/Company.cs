using System.ComponentModel.DataAnnotations;

namespace BooksMart.Models.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Province { get; set; }
        public string? PostalCode { get; set; }
        [Required]
        public string PhoneNumber{ get; set; }
    }
}
