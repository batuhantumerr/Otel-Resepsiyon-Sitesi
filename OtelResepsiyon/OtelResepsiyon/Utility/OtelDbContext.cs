using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OtelResepsiyon.Models;

namespace OtelResepsiyon.Utility
{
    public class OtelDbContext:IdentityDbContext
    {
        public OtelDbContext(DbContextOptions<OtelDbContext> options): base(options){}       
        public DbSet<Oda> Odalar { get; set; }
        public DbSet<Rezervasyon> Rezervasyonlar { get; set; }
        public DbSet<MisafirDetay> Misafirler { get; set; }
        
        
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder
        //        .Entity<Oda>()
        //        .Property(e => e.Durum)
        //        .HasConversion(
        //            v => v.ToString(),
        //            v => (OdaDurum)Enum.Parse(typeof(OdaDurum), v));
            
        //    modelBuilder
        //        .Entity<Rezervasyon>()
        //        .Property(e => e.Durum)
        //        .HasConversion(
        //            v => v.ToString(),
        //            v => (RezervasyonDurum)Enum.Parse(typeof(RezervasyonDurum), v));

        //    //modelBuilder.Entity<IdentityUserLogin<string>>().HasNoKey();
        //}

    }

}
