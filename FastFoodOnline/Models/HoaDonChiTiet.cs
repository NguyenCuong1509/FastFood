using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFoodOnline.Models
{
    public class HoaDonChiTiet
    {
        [Key]
        public int HoaDonChiTietId { get; set; }

        [Display(Name = "Mã Hóa Đơn")]
        public int HoaDonId { get; set; }

        [ForeignKey("HoaDonId")]
        [Display(Name = "Hóa Đơn")]
        public HoaDon HoaDon { get; set; }

        [Display(Name = "Mã Món Ăn")]
        public int? MonAnId { get; set; } // Cho phép null vì có thể là combo

        [ForeignKey("MonAnId")]
        [Display(Name = "Món Ăn")]
        public MonAn MonAn { get; set; }

        [Display(Name = "Mã Combo")]
        public int? ComboId { get; set; } // Cho phép null vì có thể là món ăn

        [ForeignKey("ComboId")]
        [Display(Name = "Combo")]
        public Combo Combo { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống!")]
        [Display(Name = "Số Lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Giá không được để trống!")]
        [Display(Name = "Giá")]
        [Range(1000, 1000000, ErrorMessage = "Giá phải từ 1.000 đến 1.000.000")]
        public decimal Gia { get; set; }
    }
}
