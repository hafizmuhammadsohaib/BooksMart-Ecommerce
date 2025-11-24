using System.ComponentModel.DataAnnotations;

namespace BooksMart.Models.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        [Display(Name ="Category Name")]
        public string Name { get; set; }
        [Display(Name="Display Order")]
        [Range(1,100)]
        public int DisplayOrder { get; set; }
    }
}
