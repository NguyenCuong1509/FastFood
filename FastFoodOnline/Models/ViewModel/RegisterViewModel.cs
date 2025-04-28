using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ComfirmPassword { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 10)]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [StringLength(100)]
        [Display(Name = "Thành phố")]
        public string ThanhPho { get; set; }

        [StringLength(20)]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }
    }
}
