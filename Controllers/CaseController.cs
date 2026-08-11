using Microsoft.AspNetCore.Mvc;
using LongTermCareMatching.Data;
using LongTermCareMatching.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;

namespace LongTermCareMatching.Controllers
{
    public class CaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var openCases = _context.Cases
                .Where(c => c.Status == "Open")
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            var announcements = _context.Announcements
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.CreatedAt)
                .Take(3)
                .ToList();

            ViewBag.Announcements = announcements;

            return View(openCases);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Create(Case newCase)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }

            if (newCase.ServiceType != "陪診")
            {
                newCase.Hospital = null;
                newCase.Department = null;

                ModelState.Remove(nameof(Case.Hospital));
                ModelState.Remove(nameof(Case.Department));
            }

            if (int.TryParse(userIdStr, out int userId))
            {
                newCase.FamilyUserId = userId;
            }

            newCase.Status = "Draft";
            newCase.PaymentStatus = "Pending";
            newCase.CreatedAt = DateTime.Now;

            ModelState.Remove(nameof(Case.Status));
            ModelState.Remove(nameof(Case.PaymentStatus));

            if (ModelState.IsValid)
            {
                _context.Cases.Add(newCase);
                _context.SaveChanges();

                return RedirectToAction("ConfirmPayment", new { id = newCase.Id });
            }

            return View(newCase);
        }

        [HttpGet]
        public IActionResult ConfirmPayment(int id)
        {
            var caseItem = _context.Cases.FirstOrDefault(c => c.Id == id);
            if (caseItem == null) return NotFound();
            return View(caseItem);
        }

        [HttpPost]
        public IActionResult AuthorizePayment(int id)
        {
            var caseItem = _context.Cases.FirstOrDefault(c => c.Id == id);
            if (caseItem == null) return NotFound();

            caseItem.PaymentStatus = "Authorized";
            caseItem.Status = "Open";
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var caseItem = _context.Cases.FirstOrDefault(c => c.Id == id);
            if (caseItem == null) return NotFound();

            var userIdStr = HttpContext.Session.GetString("UserId");
            int currentUserId = string.IsNullOrEmpty(userIdStr) ? 0 : int.Parse(userIdStr);

            var comments = _context.CaseComments
                                   .Where(c => c.CaseId == id)
                                   .OrderBy(c => c.CreatedAt)
                                   .ToList();

            var applications = _context.CaseApplications
                                       .Where(a => a.CaseId == id)
                                       .OrderByDescending(a => a.AppliedAt)
                                       .ToList();

            bool hasApplied = applications.Any(a => a.CaregiverUserId == currentUserId && a.Status != "Cancelled");

            ViewBag.Comments = comments;
            ViewBag.Applications = applications;
            ViewBag.HasApplied = hasApplied;

            return View(caseItem);
        }

        [HttpPost]
        public IActionResult Apply(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "請先登入會員！";
                return RedirectToAction("Login", "Account");
            }

            if (userRole != "Caregiver")
            {
                TempData["Error"] = "只有『照服員』身分才能進行應徵喔！";
                return RedirectToAction("Details", new { id });
            }

            int caregiverId = int.Parse(userIdStr);
            var caseItem = _context.Cases.FirstOrDefault(c => c.Id == id);
            if (caseItem == null) return NotFound();

            if (caseItem.Status != "Open")
            {
                TempData["Error"] = "該案件目前非開放應徵狀態！";
                return RedirectToAction("Index");
            }

            var existingApp = _context.CaseApplications.FirstOrDefault(a => a.CaseId == id && a.CaregiverUserId == caregiverId && a.Status != "Cancelled");
            if (existingApp != null)
            {
                TempData["Error"] = "您已經應徵過此案件囉！";
                return RedirectToAction("Details", new { id });
            }

            var app = new CaseApplication
            {
                CaseId = id,
                CaregiverUserId = caregiverId,
                CaregiverName = userName ?? "照服員",
                Status = "Pending",
                AppliedAt = DateTime.Now
            };

            _context.CaseApplications.Add(app);

            var notification = new Notification
            {
                UserId = caseItem.FamilyUserId,
                Title = "🙋‍♂️ 新應徵通知",
                Message = $"照服員【{app.CaregiverName}】已應徵您的案件：{caseItem.Title}",
                Url = $"/Case/Details/{id}",
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            _context.SaveChanges();

            TempData["Success"] = "🙋‍♂️ 成功送出應徵！請等待家屬確認媒合。";

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public IActionResult AcceptCaregiver(int caseId, int applicationId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userIdStr) || userRole != "Family")
            {
                return RedirectToAction("Login", "Account");
            }

            int familyId = int.Parse(userIdStr);
            var caseItem = _context.Cases.FirstOrDefault(c => c.Id == caseId && c.FamilyUserId == familyId);
            if (caseItem == null) return NotFound();

            var selectedApp = _context.CaseApplications.FirstOrDefault(a => a.Id == applicationId && a.CaseId == caseId);
            if (selectedApp == null) return NotFound();

            selectedApp.Status = "Accepted";

            var otherApps = _context.CaseApplications.Where(a => a.CaseId == caseId && a.Id != applicationId).ToList();
            foreach (var app in otherApps)
            {
                app.Status = "Rejected";

                _context.Notifications.Add(new Notification
                {
                    UserId = app.CaregiverUserId,
                    Title = "💬 案件媒合結果",
                    Message = $"遺憾！案件【{caseItem.Title}】已由其他照服員接單。",
                    Url = $"/Case/Details/{caseId}",
                    CreatedAt = DateTime.Now
                });
            }

            caseItem.CaregiverUserId = selectedApp.CaregiverUserId;
            caseItem.Status = "InProgress";

            _context.Notifications.Add(new Notification
            {
                UserId = selectedApp.CaregiverUserId,
                Title = "🎉 恭喜媒合成功！",
                Message = $"家屬已確認由您接單案件：{caseItem.Title}，請前往查看詳情並聯繫家属。",
                Url = $"/Case/Details/{caseId}",
                CreatedAt = DateTime.Now
            });

            _context.SaveChanges();

            TempData["Success"] = $"🎉 成功確認媒合照服員【{selectedApp.CaregiverName}】！案件已轉為服務中。";

            return RedirectToAction("Details", new { id = caseId });
        }

        [HttpPost]
        public IActionResult AddComment(int caseId, string content)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "請先登入才能發表留言！";
                return RedirectToAction("Login", "Account");
            }

            int currentUserId = int.Parse(userIdStr);

            if (!string.IsNullOrWhiteSpace(content))
            {
                var comment = new CaseComment
                {
                    CaseId = caseId,
                    UserId = currentUserId,
                    UserName = userName ?? "使用者",
                    UserRole = userRole ?? "",
                    Content = content,
                    CreatedAt = DateTime.Now
                };

                _context.CaseComments.Add(comment);

                var caseItem = _context.Cases.FirstOrDefault(c => c.Id == caseId);
                if (caseItem != null && caseItem.FamilyUserId != currentUserId)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = caseItem.FamilyUserId,
                        Title = "💬 案件有新留言",
                        Message = $"【{comment.UserName}】在您的案件【{caseItem.Title}】下發表了留言。",
                        Url = $"/Case/Details/{caseId}",
                        CreatedAt = DateTime.Now
                    });
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = caseId });
        }

        [HttpGet]
        public IActionResult MyCases()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userIdStr) || userRole != "Family")
            {
                return RedirectToAction("Login", "Account");
            }

            int familyId = int.Parse(userIdStr);
            var myCases = _context.Cases
                .Where(c => c.FamilyUserId == familyId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View(myCases);
        }

        [HttpGet]
        public IActionResult MyJobs()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userIdStr) || userRole != "Caregiver")
            {
                return RedirectToAction("Login", "Account");
            }

            int caregiverId = int.Parse(userIdStr);

            var appliedCaseIds = _context.CaseApplications
                .Where(a => a.CaregiverUserId == caregiverId && a.Status != "Cancelled")
                .Select(a => a.CaseId)
                .ToList();

            var myJobs = _context.Cases
                .Where(c => appliedCaseIds.Contains(c.Id))
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View(myJobs);
        }

        [HttpPost]
        public IActionResult DeleteDraft(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userIdStr) || userRole != "Family")
            {
                return RedirectToAction("Login", "Account");
            }

            int familyId = int.Parse(userIdStr);
            var caseItem = _context.Cases.FirstOrDefault(c => c.Id == id && c.FamilyUserId == familyId);

            if (caseItem == null) return NotFound();

            if (caseItem.Status == "Draft")
            {
                _context.Cases.Remove(caseItem);
                _context.SaveChanges();
                TempData["Success"] = "🗑️ 草稿已成功刪除！";
            }
            else
            {
                TempData["Error"] = "只有草稿狀態的案件才能刪除喔！";
            }

            return RedirectToAction("MyCases");
        }

        [HttpPost]
        public IActionResult CancelApply(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userIdStr) || userRole != "Caregiver")
            {
                return RedirectToAction("Login", "Account");
            }

            int caregiverId = int.Parse(userIdStr);
            var app = _context.CaseApplications.FirstOrDefault(a => a.CaseId == id && a.CaregiverUserId == caregiverId && a.Status == "Pending");

            if (app != null)
            {
                app.Status = "Cancelled";

                var caseItem = _context.Cases.FirstOrDefault(c => c.Id == id && c.CaregiverUserId == caregiverId);
                if (caseItem != null)
                {
                    caseItem.CaregiverUserId = null;
                    caseItem.Status = "Open";
                }

                _context.SaveChanges();
                TempData["Success"] = "已取消應徵！";
            }

            return RedirectToAction("MyJobs");
        }
    }
}