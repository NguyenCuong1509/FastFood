using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FastFoodOnline.Models;
using static Azure.Core.HttpHeader;

namespace FastFoodOnline.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MonAnGioHang>()
                .HasOne(mg => mg.GioHang)
                .WithMany(g => g.MonAnGioHangs)
                .HasForeignKey(mg => mg.GioHangId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MonAnGioHang>()
                .HasOne(mg => mg.MonAn)
                .WithMany()
                .HasForeignKey(mg => mg.MonAnId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<MonAn>()
                .HasOne(ma => ma.LoaiMonAn)
                .WithMany(l => l.MonAns)
                .HasForeignKey(ma => ma.LoaiMonAnId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<LoaiMonAn> LoaiMonAns { get; set; }
        public DbSet<MonAn> MonAns { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboGioHang> ComboGioHangs { get; set; }
        public DbSet<MonAnCombo> MonAnCombos { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<MonAnGioHang> MonAnGioHangs { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<HoaDonChiTiet> HoaDonChiTiets { get; set; }
    }
}
