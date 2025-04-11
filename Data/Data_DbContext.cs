using Book_Managment_SYS.Models;
using Microsoft.EntityFrameworkCore;

namespace Book_Managment_SYS.Data
{
    public class Data_DbContext:DbContext
    {

        public Data_DbContext(DbContextOptions<Data_DbContext> options):base(options)
        {}
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
    }
}
