using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _username;
    private readonly string _password;

    public FileUploadController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
    {
        _webHostEnvironment = webHostEnvironment;
        _username = configuration.GetSection("AppSettings:Username").Value;
        _password = configuration.GetSection("AppSettings:Password").Value;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // Check the file extension
        if (!Path.GetExtension(file.FileName).Equals(".7z", System.StringComparison.OrdinalIgnoreCase))
            return BadRequest("invalid file.");

        string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedFiles");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        string fileName = Path.Combine(uploadsFolder, file.FileName);
        using (var stream = new FileStream(fileName, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok($"File '{file.FileName}' uploaded successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Implement your own authentication logic here
        if (username == _username && password == _password)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var user = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddMinutes(3000),
                IsPersistent = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user, authProperties);
            return Ok("Logged in successfully.");
        }

        return Unauthorized("Invalid username or password.");
    }
}
