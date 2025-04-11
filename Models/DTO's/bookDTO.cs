using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book_Managment_SYS.Models.DTO_s
{
    public class bookDTO
    {
        [Required] public string book_title { get; set; }
        [Required] public string book_author { get; set; }
        [Required] public int book_pages { get; set; }
        public string? book_desc { get; set; }
        [Required] public double book_price { get; set; }
        [Required]public IFormFile book_img { get; set; }
        public string oldimage { get; set; }
        public int book_category { get; set; }
        [ForeignKey("book_category")]
        public Category? Category { get; set; }


    }
}
