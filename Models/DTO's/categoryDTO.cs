using System.ComponentModel.DataAnnotations;

namespace Book_Managment_SYS.Models.DTO_s
{
    public class categoryDTO
    {
        [Required] public string category_name { get; set; }
        [Required] public IFormFile? category_image { get; set; }
        public string? category_oldImg { get; set; }
        public DateTime created_at { get; set; }
    }
}
