namespace FastFoodOnline.Models
{
    internal class HomeViewModel
    {
        public List<MonAn> MonAns { get; set; }
        public List<Combo> Combos { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalPagesCombos { get; set; }
    }
}