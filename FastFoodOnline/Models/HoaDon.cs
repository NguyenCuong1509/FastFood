using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models
{
    public class HoaDon
    {
        [Key]
        public int HoaDonId { get; set; }

        [Required]
        [Display(Name = "Mã Người Dùng")]
        public string UserId { get; set; }

        [Display(Name = "Người Dùng")]
        public IdentityUser User { get; set; }

        [Required]
        [Display(Name = "Ngày Tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Tổng Tiền")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền không hợp lệ")]
        public decimal TongTien { get; set; }

        [Required]
        [Display(Name = "Trạng Thái Hóa Đơn")]
        public TrangThaiHoaDon TrangThai { get; set; } = TrangThaiHoaDon.ChoXacNhan;

        [Display(Name = "Chi Tiết Hóa Đơn")]
        public ICollection<HoaDonChiTiet> HoaDonChiTiets { get; set; } = new List<HoaDonChiTiet>();
    }
}
