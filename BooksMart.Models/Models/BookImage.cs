
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BooksMart.Models.Models
{
    public class BookImage
    {
        public int Id { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        public int BookId { get; set; }
        [ForeignKey("BookId")]
        public Book Book { get; set; }
    }
}
