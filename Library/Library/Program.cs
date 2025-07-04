using Autofac;
using Autofac.Extensions.DependencyInjection;
using BootcampApp.Repository.Common;
using LibraryRepository;
using LibraryRepository.Common;
using LibraryRepositroy.Common;
using LibraryService;
using LibraryService.Common;
using Microsoft.AspNetCore.Cors.Infrastructure;

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




// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterType<BooksRepository>().As<IBooksRepository>().InstancePerLifetimeScope();
    container.RegisterType<BooksService>().As<IBooksService>().InstancePerLifetimeScope();
    container.RegisterType<AuthorsRepository>().As<IAuthorsRepository>().InstancePerLifetimeScope();
    container.RegisterType<AuthorsService>().As<IAuthorsService>().InstancePerLifetimeScope();
    container.RegisterType<MemberRepository>().As<IMemberRepository>().InstancePerLifetimeScope();
    container.RegisterType<MemberService>().As<IMemberService>().InstancePerLifetimeScope();
    container.RegisterType<BookAuthorsRepository>().As<IBookAuthorsRepository>().InstancePerLifetimeScope();
    container.RegisterType<BookAuthorsService>().As<IBookAuthorsService>().InstancePerLifetimeScope();


    container.RegisterType<LoanRepository>().As<ILoanRepository>().InstancePerLifetimeScope();
    container.RegisterType<LoanService>().As<ILoanService>().InstancePerLifetimeScope();

    container.RegisterType<CategoryRepository>().As<ICategoryRepository>().InstancePerLifetimeScope();
    container.RegisterType<CategoryService>().As<ICategoryService>().InstancePerLifetimeScope();
    container.RegisterType<BookCategoryRepository>().As<IBookCategoryRepository>().InstancePerLifetimeScope();
    container.RegisterType<BookCategoryService>().As<IBookCategoryService>().InstancePerLifetimeScope();

});

// Register AutoMapper with all assemblies in the current AppDomain for broader profile discovery
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();
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
        
    

