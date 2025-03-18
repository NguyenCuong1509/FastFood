using System.ComponentModel.DataAnnotations;

namespace FastFoodOnline.Models.ViewModel
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string HoTen { get; set; }
        public string DiaChi { get; set; }
        public string ThanhPho { get; set; }
        public string GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
    }
}
