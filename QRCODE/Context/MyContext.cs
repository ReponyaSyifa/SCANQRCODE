using Microsoft.EntityFrameworkCore;
using QRCODE.Models;

namespace QRCODE.Context
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options) : base(options)
        {

        }

        public DbSet<qrstored> qrstores { get; set; }
    }
}
