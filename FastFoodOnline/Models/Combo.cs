using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFoodOnline.Models
{
    public class Combo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ComboId { get; set; }

        [Required(ErrorMessage = "Tên combo không được để trống")]
        [StringLength(100, ErrorMessage = "Tên combo không được quá 100 ký tự")]
        [Display(Name = "Tên Combo")]
        public string TenCombo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá khuyến mãi không được để trống")]
        [Range(1000, 1000000, ErrorMessage = "Giá khuyến mãi phải từ 1,000 đến 1,000,000")]
        [Display(Name = "Giá Khuyến Mãi")]
        public decimal GiaKhuyenMai { get; set; }

        [Required(ErrorMessage = "Giá gốc không được để trống")]
        [Range(1000, 1000000, ErrorMessage = "Giá gốc phải từ 1,000 đến 1,000,000")]
        [Display(Name = "Giá Gốc")]
        public decimal GiaGoc { get; set; }

        [Display(Name = "Danh Sách Món Ăn Trong Combo")]
        public ICollection<MonAnCombo> MonAnCombos { get; set; } = new List<MonAnCombo>();
    }
}
