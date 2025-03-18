using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models
{
    public class LoaiMonAn
    {
        [Key]
        public int LoaiMonAnId { get; set; }

        [Required(ErrorMessage = "Tên loại không được để trống")]
        [StringLength(100, ErrorMessage = "Tên loại không được vượt quá 100 ký tự")]
        [Display(Name = "Tên Loại Món Ăn")]
        public string TenLoai { get; set; } = string.Empty;

        // Liên kết với danh sách món ăn
        [Display(Name = "Danh Sách Món Ăn")]
        public ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
    }
}
