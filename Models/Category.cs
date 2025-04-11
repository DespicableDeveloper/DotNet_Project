using System.ComponentModel.DataAnnotations;

namespace Book_Managment_SYS.Models
{
    public class Category
    {

        [Key]public int Id {get;set;}
        [Required]public string category_name {get;set;}
        [Required]public string category_image {get;set;}
        public DateTime created_at {get;set;}
    }
}
