using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Library.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
string connectionString = "";
if(builder.Environment.IsProduction())
{
    connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");  
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

builder.Services.Configure<FormOptions>(options =>
    {
        options.ValueCountLimit = 20000; // If library gets too big, increase this number.
    });

builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("IsLibrarian", policy => policy.RequireClaim("Role", "Librarian"));
            options.AddPolicy("IsUser", policy => policy.RequireClaim("Role", "User"));

            options.AddPolicy("IsUsingTempPassword", policy => policy.RequireClaim("UsingTempPassword"));

            //used for registration page
            options.AddPolicy("IsLibrarianOrNoUsersRegistered", policy =>
            {
                policy.AddRequirements(new IsLibrarianOrNoUsersRegisteredRequirement());
            });
        });

builder.Services.AddScoped<IAuthorizationHandler, IsLibrarianOrNoUsersRegisteredAuthorizationHandler>();

builder.Services.AddDistributedMemoryCache();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
//apply migrations if there's any to docker db
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

app.Run();
