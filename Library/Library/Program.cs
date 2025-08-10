using Autofac;
using Autofac.Extensions.DependencyInjection;

using LibraryRepository;
using LibraryRepository.Common;
using LibraryRepositroy.Common;
using LibraryService;
using LibraryService.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());


// Add services to the container.

builder.Services.AddControllers();

// JWT config
var jwtSettings = builder.Configuration.GetSection("Jwt");
// Ensure the "Key" value is not null by using null-coalescing operator or throwing an exception if null
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured."));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => 
{
    options.TokenValidationParameters = new TokenValidationParameters { 
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterType<BookRepository>().As<IBookRepository>().InstancePerLifetimeScope();

    container.RegisterType<BookService>().As<IBookService>().InstancePerLifetimeScope();

    container.RegisterType<CategoryRepository>().As<ICategoryRepository>().InstancePerLifetimeScope();
    container.RegisterType<CategoryService>().As<ICategoryService>().InstancePerLifetimeScope();
    container.RegisterType<BookCategoryRepository>().As<IBookCategoryRepository>().InstancePerLifetimeScope();
    container.RegisterType<BookCategoryService>().As<IBookCategoryService>().InstancePerLifetimeScope();
    container.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
    container.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
    container.RegisterType<AuthorRepository>().As<IAuthorRepository>().InstancePerLifetimeScope();
    container.RegisterType<AuthorService>().As<IAuthorService>().InstancePerLifetimeScope();
    container.RegisterType<MemberRepository>().As<IMemberRepository>().InstancePerLifetimeScope();
    container.RegisterType<MemberService>().As<IMemberService>().InstancePerLifetimeScope();

});

// Register AutoMapper with all assemblies in the current AppDomain for broader profile discovery
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAllOrigins");

// 🌐 Middleware
if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        
    

