using ElRawda.Core.Models;
using Microsoft.EntityFrameworkCore;


namespace ElRawda.Repository.Data
{
    public class ELRawdaContext:DbContext
    {
        public ELRawdaContext(DbContextOptions<ELRawdaContext> options) : base(options)
        {

        }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);


        //    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        //}
        public DbSet<Cows> cows { get; set; }
        public DbSet<SlaughteredCow> slaughteredCows { get; set; }
        public DbSet<CowsPieces> cowsPieces { get; set; }
    }
}
