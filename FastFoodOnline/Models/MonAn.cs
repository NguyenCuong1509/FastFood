using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFoodOnline.Models
{
    public class MonAn
    {
        [Key]
        public int MonAnId { get; set; }

        [Required(ErrorMessage = "Tên món ăn không được để trống!")]
        [Display(Name = "Tên Món Ăn")]
        public string TenMon { get; set; }

        [Required(ErrorMessage = "Giá không được để trống!")]
        [Range(1000, 1000000, ErrorMessage = "Giá phải từ 1.000 đến 1.000.000")]
        [Display(Name = "Giá")]
        public decimal Gia { get; set; }

        [Display(Name = "Mô Tả")]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Hình ảnh không được để trống!")]
        [Display(Name = "Hình Ảnh")]
        public string HinhAnh { get; set; }

        [Required(ErrorMessage = "Phải chọn loại món ăn!")]
        [Display(Name = "Loại Món Ăn")]
        public int LoaiMonAnId { get; set; }

        [ForeignKey("LoaiMonAnId")]
        [Display(Name = "Loại Món Ăn")]
        public LoaiMonAn? LoaiMonAn { get; set; }
    }
}
