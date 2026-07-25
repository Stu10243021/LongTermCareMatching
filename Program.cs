using LongTermCareMatching.Data; // ?? 引用你的 Data 資料夾
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. 註冊 MVC 與 Session 所需服務
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor(); // 給 _Layout 讀 Session 用
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 2. ??【關鍵修正】註冊 ApplicationDbContext 資料庫服務！
// (請確認 appsettings.json 裡面的 ConnectionStrings 檔名，預設通常叫 DefaultConnection)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. 啟用 Session
app.UseSession();

app.UseAuthorization();

// 4. 設定預設開啟登入頁
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();