using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StoreBook.Models;
using StoreBook.Repositories;
using StoreBook.Repositories.IRepository;
using StoreBook.Utilities;
using System.Diagnostics;

namespace StoreBook
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                )
            );

            // Configure ASP.NET Core Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailSender, StoreBook.Utilities.EmailSender>();
            builder.Services.AddScoped<IRepository<Category>, Repository<Category>>();
            builder.Services.AddScoped<IRepository<Brand>, Repository<Brand>>();
            builder.Services.AddScoped<IRepository<Auther>, Repository<Auther>>();
            builder.Services.AddScoped<IRepository<Book>, Repository<Book>>();
            builder.Services.AddScoped<IRepository<ApplicationUserOTP>, Repository<ApplicationUserOTP>>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();
       
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
      

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            var scalarUrl = "https://localhost:7088/scalar";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = scalarUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                // تجاهل لو فشل
            }
            app.Run();
        }
    }
}
