using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddNewtonsoftJson();

// -- Create RSA with custom private key
RSA rsa = RSA.Create(2048);
rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(builder.Configuration["JWT:PrivateKey"]), 
                          out int bytesRead);
RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(rsa);
builder.Services.AddSingleton(rsaSecurityKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer("Bearer", options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = rsaSecurityKey,
        ValidAlgorithms = new[] {"RS256"},
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,         // "iss"
        ValidateAudience = false,       // "aud"
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();
app.Services.GetService<RsaSecurityKey>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(policy =>
    {
        policy.AllowAnyOrigin() //WithOrigins("http://www.google.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders(new string[] { "Location" });
    });

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();