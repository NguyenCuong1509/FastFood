namespace FastFoodOnline.Models.ViewModel
{
    public class ProfileViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }

        public string HoTen { get; set; }
        public string DiaChi { get; set; }
        public string ThanhPho { get; set; }
        public string GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}
