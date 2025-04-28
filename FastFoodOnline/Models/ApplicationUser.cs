using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }  // Họ và Tên

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; } // Địa chỉ

        [StringLength(100)]
        [Display(Name = "Thành phố")]
        public string ThanhPho { get; set; } // Thành phố

        [StringLength(20)]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; } // Giới tính

        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; } // Ngày sinh

    }
}
