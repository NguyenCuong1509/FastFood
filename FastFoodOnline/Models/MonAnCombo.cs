using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFoodOnline.Models
{
    public class MonAnCombo
    {
        [Key]
        public int MonAnComboId { get; set; }

        [Required]
        [Display(Name = "Mã Combo")]
        public int ComboId { get; set; }

        [ForeignKey("ComboId")]
        [Display(Name = "Combo")]
        public Combo Combo { get; set; }

        [Required]
        [Display(Name = "Mã Món Ăn")]
        public int MonAnId { get; set; }

        [ForeignKey("MonAnId")]
        [Display(Name = "Món Ăn")]
        public MonAn MonAn { get; set; }

        [Display(Name = "Số Lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }
    }
}
