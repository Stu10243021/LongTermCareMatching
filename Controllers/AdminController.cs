using LongTermCareMatching.Data;
using LongTermCareMatching.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LongTermCareMatching.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AuditList()
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var pendingCaregivers = _context.Users
                .Where(u => u.Role == "Caregiver" && !u.IsApproved)
                .ToList();

            return View(pendingCaregivers);
        }

        public IActionResult UserList()
        {
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

        // 停權/解除停權會員
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

        // 平台公告管理頁面
        [HttpGet]
        public IActionResult Announcements()
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var list = _context.Announcements
                               .OrderByDescending(a => a.IsPinned)
                               .ThenByDescending(a => a.CreatedAt)
                               .ToList();

            return View(list);
        }

        // 發布新公告
        [HttpPost]
        public IActionResult CreateAnnouncement(Announcement announcement)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin") return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                announcement.CreatedAt = DateTime.Now;
                _context.Announcements.Add(announcement);
                _context.SaveChanges();

                TempData["Success"] = "📢 公告發布成功！";
                return RedirectToAction("Announcements");
            }

            var list = _context.Announcements
                               .OrderByDescending(a => a.IsPinned)
                               .ThenByDescending(a => a.CreatedAt)
                               .ToList();

            return View("Announcements", list);
        }

        //  刪除公告
        [HttpPost]
        public IActionResult DeleteAnnouncement(int id)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin") return RedirectToAction("Login", "Account");

            var item = _context.Announcements.Find(id);
            if (item != null)
            {
                _context.Announcements.Remove(item);
                _context.SaveChanges();
                TempData["Success"] = "🗑️ 公告已成功刪除！";
            }

            return RedirectToAction("Announcements");
        }
    }
}