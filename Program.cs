using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using genshin.DbContexts;
using genshin.DTOs;
using genshin.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var secretKey = builder.Configuration["Authentication:JwtSecretKey"];
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

//Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/register", async (AppDbContext db, UserRecord userRecord) =>
{  
   var user = await db.Users.FirstOrDefaultAsync(u => u.Username == userRecord.Username);
   if (user != null) return Results.BadRequest("Username already taken");
   var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRecord.Password);
   var newUser = new User { Username = userRecord.Username, Password = hashedPassword };
   await db.Users.AddAsync(newUser);
   await db.SaveChangesAsync();
   return Results.Created($"/users/{newUser.Id}", newUser.Id );
});


app.MapPost("/login", async (AppDbContext db, UserRecord userDetails) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == userDetails.Username);
    if (user == null)
        return Results.BadRequest("Invalid credentials");
    
    // Check password
    if (!BCrypt.Net.BCrypt.Verify(userDetails.Password, user.Password))
        return Results.BadRequest("Invalid credentials");
    var claims = new[]
    {
        new Claim(ClaimTypes.Name, user.Username),      // "name" claim
        new Claim(ClaimTypes.Role, user.Role)           // "role" claim
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        claims: claims, expires: DateTime.Now.AddHours(2), signingCredentials: credentials);
    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new 
    { 
        token = tokenString, 
        username = user.Username,
        role = user.Role  // Show role in response
    });
});

app.MapGet("/", async (AppDbContext db) =>  await db.Characters.ToListAsync());
app.MapPost("/", async (AppDbContext db, CharacterRecord characterRecord) =>
{
    var element = db.Elements.FirstAsync(e => e.Name == characterRecord.ElementName);
    var character = new Character
    {
        Name = characterRecord.Name,
        Rarity = characterRecord.Rarity,
        ElementId = element.Id,
        Element = element.Result
    };
    db.Characters.Add(character);
    await db.SaveChangesAsync();
    return Results.Created($"/characters/{character.Id}", character.Id);

}).RequireAuthorization(policy => policy.RequireRole("Admin"));
app.MapPost("/elements", (AppDbContext db, List<Element> elements) =>
{
    elements.ForEach(element => db.Add(element));
    db.SaveChanges();
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.Run();