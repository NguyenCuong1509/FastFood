using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email {  get; set; }
        [Required]
        [StringLength(100,MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="Mat Khau khong khop")]
        public string ComfirmPassword {  get; set; }
        public string PhoneNumber {  get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }


        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Display(Name = "Thành phố")]
        public string ThanhPho { get; set; }

        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

    }
}
