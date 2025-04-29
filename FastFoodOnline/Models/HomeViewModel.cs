namespace FastFoodOnline.Models
{
    public class HomeViewModel
    {
        public List<MonAn> MonAns { get; set; }
        public List<Combo> Combos { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalPagesCombos { get; set; }
        public string SearchQuery { get; set; } // Thêm trường tìm kiếm
        public int? LoaiMonAnId { get; set; } // Lọc theo loại món ăn
        public List<LoaiMonAn> LoaiMonAns { get; set; } // Danh sách loại món ăn
    }
}