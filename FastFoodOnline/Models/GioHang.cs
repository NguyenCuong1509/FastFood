using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models
{
    public class GioHang
    {
        [Key]
        public int GioHangId { get; set; }

        [Required]
        [Display(Name = "Mã Người Dùng")]
        public string UserId { get; set; }

        [Display(Name = "Người Dùng")]
        public IdentityUser User { get; set; }

        [Display(Name = "Danh Sách Món Ăn Trong Giỏ Hàng")]
        public ICollection<MonAnGioHang> MonAnGioHangs { get; set; } = new List<MonAnGioHang>();

        [Display(Name = "Danh Sách Combo Trong Giỏ Hàng")]
        public ICollection<ComboGioHang> ComboGioHangs { get; set; } = new List<ComboGioHang>();
    }
}
