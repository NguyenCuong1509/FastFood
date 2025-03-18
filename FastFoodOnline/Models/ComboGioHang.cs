using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models
{
    public class ComboGioHang
    {
        [Key]
        public int ComboGioHangId { get; set; }

        [Display(Name = "Mã Giỏ Hàng")]
        public int GioHangId { get; set; }

        [Display(Name = "Giỏ Hàng")]
        public GioHang GioHang { get; set; }

        [Display(Name = "Mã Combo")]
        public int ComboId { get; set; }

        [Display(Name = "Combo")]
        public Combo Combo { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống!")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số Lượng")]
        public int SoLuong { get; set; }
    }
}
