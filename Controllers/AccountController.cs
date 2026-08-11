using Microsoft.AspNetCore.Mvc;
using LongTermCareMatching.Models;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using LongTermCareMatching.Data;
using Microsoft.AspNetCore.Http;

namespace LongTermCareMatching.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                if (user.IsBanned)
                {
                    ViewBag.Error = "您的帳號已被停權，請聯繫管理員！";
                    return View();
                }

                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", user.Role);

                if (user.Role == "Admin")
                {
                    return RedirectToAction("AuditList", "Admin");
                }
                else if (user.Role == "Caregiver")
                {
                    return RedirectToAction("Index", "Case"); 
                }
                else if (user.Role == "Family")
                {
                    return RedirectToAction("Create", "Case"); 
                }
            }

            ViewBag.Error = "帳號或密碼錯誤！";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, IFormFile? certificateFile)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "此 Email 已被註冊過！");
                return View(user);
            }

            if (user.Role == "Caregiver" && certificateFile != null && certificateFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + certificateFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await certificateFile.CopyToAsync(fileStream);
                }

                user.CertificateUrl = "/uploads/" + uniqueFileName;
                user.IsApproved = false; 
            }
            else
            {
                user.IsApproved = true;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login");
            }

            if (int.TryParse(userIdStr, out int userId))
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Profile(User updatedUser)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login");
            }

            if (int.TryParse(userIdStr, out int userId))
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound();
                }

                user.Name = updatedUser.Name;
                user.Phone = updatedUser.Phone;
                user.Area = updatedUser.Area;
                user.Experience = updatedUser.Experience;
                user.LicenseNumber = updatedUser.LicenseNumber;
                user.Bio = updatedUser.Bio;

                _context.SaveChanges();

                HttpContext.Session.SetString("UserName", user.Name);

                TempData["Success"] = "🎉 個人資料與履歷已成功儲存！";
                return RedirectToAction("Profile");
            }

            return RedirectToAction("Login");
        }
    }
}