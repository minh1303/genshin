using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using genshin.DbContexts;
using genshin.DTOs;
using genshin.Helpers;
using genshin.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


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

app.MapGet("/characters", async (AppDbContext db) =>  await db.Characters.ToListAsync());
app.MapPost("/characters", async (AppDbContext db, CharacterRecord characterRecord) =>
{
    var element = await db.Elements.FirstAsync(e => e.Name == characterRecord.ElementName);
    var nameError = ValidationHelper.ValidateName(characterRecord.Name);
    var rarityError = ValidationHelper.ValidateRarity(characterRecord.Rarity);
    if (nameError != null) return Results.BadRequest(nameError);
    if (rarityError != null) return Results.BadRequest(rarityError);

    var character = new Character
    {
        Name = characterRecord.Name,
        Rarity = characterRecord.Rarity,
        ElementId = element.Id,
    };
    await db.Characters.AddAsync(character);
    await db.SaveChangesAsync();
    return Results.Created($"/characters/{character.Id}", character.Id);

}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPatch("/characters/{id}", async ( int id, AppDbContext db, UpdateCharacterRecord characterRecord) =>
{
    var character = await db.Characters.FindAsync(id);
    Console.WriteLine(character);
    if (character == null)
    {
        // Count total characters for debugging
        var total = await db.Characters.CountAsync();
        Console.WriteLine($"Character {id} not found");
        return Results.NotFound($"Character {id} not found. Total characters in DB: {total}");
    }
    if (characterRecord.Name != null)
    {
        var nameError = ValidationHelper.ValidateName(characterRecord.Name);
        if (nameError != null) return Results.BadRequest(nameError);
        character.Name= characterRecord.Name;
    }

    if (characterRecord.Rarity != null)
    {
        var rarityError = ValidationHelper.ValidateRarity(characterRecord.Rarity.Value);
        if (rarityError != null) return Results.BadRequest(rarityError);
        character.Rarity = characterRecord.Rarity.Value;
    }

    await db.SaveChangesAsync();
    return Results.Ok(character);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/characters/{id}", async (AppDbContext db, int id, UpdateCharacterRecord characterRecord) =>
{
    var character = await db.Characters.FindAsync(id);
    if (character == null) return  Results.NotFound($"Character {id} not found");  
    db.Characters.Remove(character);    
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/elements/bulk",async  (AppDbContext db, List<Element> elements) =>
{
    elements.ForEach(element => db.Add(element));
    await db.SaveChangesAsync();
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.Run();