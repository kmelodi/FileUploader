using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "FileUploaderCookie";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("https://rastasamt.irrasta.ir/")
                   .SetPreflightMaxAge(TimeSpan.FromSeconds(360000)) // Increase the preflight max age
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.Services.Configure<IISOptions>(options =>
{
    options.ForwardClientCertificate = false;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 4147483648; // 4 GB
});

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Limits.MaxRequestBodySize = 4147483648; // 4 GB
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 4147483648; // 4 GB
});
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Limits.MaxRequestBodySize = 4147483648; // 4 GB
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c=> {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "aims_api v1");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
