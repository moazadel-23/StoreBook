using Castle.Core.Smtp;
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
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                )
            );
            builder.Services.AddTransient<IEmailSender, EmailSender>();
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
