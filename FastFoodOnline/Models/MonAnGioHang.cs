using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models
{
    public class MonAnGioHang
    {
        [Key]
        public int MonAnGioHangId { get; set; }

        [Display(Name = "Mã Giỏ Hàng")]
        public int GioHangId { get; set; }

        [Display(Name = "Giỏ Hàng")]
        public GioHang GioHang { get; set; }

        [Display(Name = "Mã Món Ăn")]
        public int MonAnId { get; set; }

        [Display(Name = "Món Ăn")]
        public MonAn MonAn { get; set; }

        [Display(Name = "Số Lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }
    }
}
