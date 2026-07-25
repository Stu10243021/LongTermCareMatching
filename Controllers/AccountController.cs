using Microsoft.AspNetCore.Mvc;
using LongTermCareMatching.Models;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using LongTermCareMatching.Data;
using Microsoft.AspNetCore.Http; // 用於 Session

namespace LongTermCareMatching.Controllers
{
    public class AccountController : Controller
    {
        // 注入資料庫 Context 
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🎯 1. [GET] 登入頁面 (打開網址時顯示畫面，缺了這個就會跳 405 錯誤！)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 🎯 2. [POST] 處理登入驗證 (按下登入按鈕時觸發)
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // 🛑 檢查是否被停權
                if (user.IsBanned)
                {
                    ViewBag.Error = "您的帳號已被停權，請聯繫管理員！";
                    return View();
                }

                // 🔑 將登入資訊寫入 Session
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", user.Role);

                // 🎯 根據身分分流跳轉
                if (user.Role == "Admin")
                {
                    return RedirectToAction("AuditList", "Admin");
                }
                else if (user.Role == "Caregiver")
                {
                    return RedirectToAction("Index", "Case"); // 未來照服員大廳
                }
                else if (user.Role == "Family")
                {
                    return RedirectToAction("Create", "Case"); // 未來家屬發布頁
                }
            }

            ViewBag.Error = "帳號或密碼錯誤！";
            return View();
        }

        // 🚪 3. [GET] 登出功能 (點擊登出時清空 Session)
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // 🎯 4. [GET] 註冊頁面
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // 🎯 5. [POST] 處理註冊資料與檔案上傳存檔
        [HttpPost]
        public async Task<IActionResult> Register(User user, IFormFile? certificateFile)
        {
            // 檢查 Email 是否已被註冊
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "此 Email 已被註冊過！");
                return View(user);
            }

            // 如果是照服員，處理證照檔案上傳
            if (user.Role == "Caregiver" && certificateFile != null && certificateFile.Length > 0)
            {
                // 設定存檔資料夾：wwwroot/uploads
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 產生獨一無二的檔名，避免同名覆蓋 
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + certificateFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 將檔案寫入硬碟
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await certificateFile.CopyToAsync(fileStream);
                }

                // 儲存相對路徑給前端 <img src="..."> 讀取
                user.CertificateUrl = "/uploads/" + uniqueFileName;
                user.IsApproved = false; // 照服員預設為「未審核」
            }
            else
            {
                // 家屬與管理員預設直接審核通過
                user.IsApproved = true;
            }

            // 存入資料庫
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 註冊成功，跳轉回登入頁
            return RedirectToAction("Login");
        }
    }
}