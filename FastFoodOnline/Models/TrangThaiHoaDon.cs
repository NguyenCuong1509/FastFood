namespace FastFoodOnline.Models
{
    public enum TrangThaiHoaDon
    {
        ChoXacNhan,   // Đơn hàng mới tạo, chờ xác nhận
        DangXuLy,     // Đơn hàng đang được xử lý
        DangGiao,     // Đang giao hàng
        DaGiao,       // Đã giao thành công
        DaHuy         // Đơn hàng bị hủy
    }
}
