using LongTermCareMatching.Data;
using LongTermCareMatching.Models;
using Microsoft.AspNetCore.Http; // 引用 HttpContext 用於 Session
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LongTermCareMatching.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context; // 請確認與你的 DbContext 名稱一致

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // [GET] 待審核清單
        public IActionResult AuditList()
        {
            // 🔒 安全防護：檢查目前 Session 裡的身分是不是 Admin
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                // 如果不是管理員，直接跳轉踢回登入頁！
                return RedirectToAction("Login", "Account");
            }

            var pendingCaregivers = _context.Users
                .Where(u => u.Role == "Caregiver" && !u.IsApproved)
                .ToList();

            return View(pendingCaregivers);
        }

        // [GET] 會員總覽與停權
        public IActionResult UserList()
        {
            // 🔒 安全防護：檢查身分
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var users = _context.Users
                .Where(u => u.Role != "Admin")
                .ToList();

            return View(users);
        }

        // [POST] 一鍵審核通過
        [HttpPost]
        public IActionResult Approve(int userId)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin") return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(userId);
            if (user != null)
            {
                user.IsApproved = true;
                _context.SaveChanges();
            }

            return RedirectToAction("AuditList");
        }

        // [POST] 切換停權狀態
        [HttpPost]
        public IActionResult ToggleBan(int userId)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin") return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(userId);
            if (user != null)
            {
                user.IsBanned = !user.IsBanned;
                _context.SaveChanges();
            }

            return RedirectToAction("UserList");
        }
    }

}