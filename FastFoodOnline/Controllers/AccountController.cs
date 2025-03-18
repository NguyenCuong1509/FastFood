using FastFoodOnline.Models.ViewModel;
using FastFoodOnline.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FastFoodOnline.Data;
using System.Data;

namespace FastFoodOnline.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           ApplicationDbContext context) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context; // Gán context vào biến _context
        }

        public IActionResult Index()
        {
            return View();
        }
     
        [AllowAnonymous]
        public IActionResult LoginUser(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/"); // Nếu returnUrl là null, đặt về trang chủ
            var model = new Login { ReturnURL = returnUrl };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginUser(Login login)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser appUser = await _userManager.FindByEmailAsync(login.Email);
                if (appUser == null)
                {
                    ModelState.AddModelError(nameof(login.Email), "Email không tồn tại.");
                    return View(login);
                }

                await _signInManager.SignOutAsync();
                var result = await _signInManager.PasswordSignInAsync(appUser, login.Password, false, false);
                if (result.Succeeded)
                {
                    // 🔹 Kiểm tra nếu _context không bị null trước khi truy cập database
                    if (_context != null && _context.GioHangs != null)
                    {
                        var existingCart = await _context.GioHangs.FirstOrDefaultAsync(g => g.UserId == appUser.Id);
                        if (existingCart == null)
                        {
                            var newCart = new GioHang
                            {
                                UserId = appUser.Id
                            };
                            _context.GioHangs.Add(newCart);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // 🔹 Luôn chuyển về Home/Index sau khi đăng nhập thành công
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(nameof(login.Email), "Login Failed: Email hoặc mật khẩu không đúng.");
            }
            return View(login);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    HoTen = model.HoTen,
                    DiaChi = model.DiaChi,
                    ThanhPho = model.ThanhPho,
                    GioiTinh = model.GioiTinh,
                    NgaySinh = model.NgaySinh,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                    return RedirectToAction("LoginUser");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // Đăng xuất khỏi hệ thống
            HttpContext.Session.Clear(); // Xóa toàn bộ session

            return RedirectToAction("LoginUser", "Account");
        }
        ////////////////////////////////////////////////////////////////////////////
        [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền truy cập
        public async Task<IActionResult> QuanLyTaiKhoan()
        {
            var users = await _userManager.Users
        .Where(u => u.Email != "admin@example.com") // Ẩn tài khoản admin
        .ToListAsync();
            var userRoles = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.ToList();
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> TaoTaiKhoanAdmin(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ email và mật khẩu!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            // Kiểm tra xem email đã tồn tại chưa
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["Error"] = "Email đã tồn tại!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin"); // Gán quyền Admin
                TempData["Success"] = "Tạo tài khoản Admin thành công!";
            }
            else
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("QuanLyTaiKhoan");
        }
        [HttpPost]
        public async Task<IActionResult> TaoTaiKhoanKhachHang(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ email và mật khẩu!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            // Kiểm tra xem email đã tồn tại chưa
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["Error"] = "Email đã tồn tại!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer"); // Gán quyền Khách hàng
                TempData["Success"] = "Tạo tài khoản khách hàng thành công!";
            }
            else
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("QuanLyTaiKhoan");
        }
        [HttpPost]
        public async Task<IActionResult> DisableUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.EmailConfirmed = false; // Vô hiệu hóa tài khoản
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Đã ngừng hoạt động tài khoản!";
            return RedirectToAction("QuanLyTaiKhoan");
        }
        [HttpPost]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.EmailConfirmed = true; // Kích hoạt tài khoản
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Tài khoản đã được kích hoạt!";
            return RedirectToAction("QuanLyTaiKhoan");
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                HoTen = user.HoTen,
                DiaChi = user.DiaChi,
                ThanhPho = user.ThanhPho,
                GioiTinh = user.GioiTinh,
                NgaySinh = user.NgaySinh
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Cập nhật tài khoản thành công!";

                // Kiểm tra nếu user KHÔNG phải Admin => về trang Profile
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin)
                {
                    return RedirectToAction("Profile");
                }

                return RedirectToAction("QuanLyTaiKhoan");
            }

            TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = "Cập nhật quyền thành công!";
            return RedirectToAction("QuanLyTaiKhoan");
        }
        [Authorize] // Yêu cầu đăng nhập mới xem được Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                UserName = user.UserName,
                HoTen = user.HoTen,
                DiaChi = user.DiaChi,
                ThanhPho = user.ThanhPho,
                GioiTinh = user.GioiTinh,
                NgaySinh = user.NgaySinh
            };

            return View(model);
        }

    }
}
