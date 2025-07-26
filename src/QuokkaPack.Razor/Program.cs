using QuokkaPack.API.Extensions;
using QuokkaPack.Razor.Tools;
using QuokkaPack.RazorPages.Tools;

var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddSession();

builder.Services.AddHttpClient("QuokkaApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7100"); // your API base URL
});
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

var environment = builder.Environment.EnvironmentName;
//builder.Configuration.AddJsonFile("appsettings.json", optional: false)
//                     .AddJsonFile($"appsettings.{environment}.json", optional: true);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSession();
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
