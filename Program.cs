using DotNetEnv;
using FluentValidation;
using LibraryApi.Common;
using LibraryApi.Data;
using LibraryApi.Mappings;
using LibraryApi.Models;
using LibraryApi.Repositories;
using LibraryApi.Services;
using LibraryApi.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// Must run before CreateBuilder so the variables from .env are visible to configuration.
// NoClobber keeps a real environment variable (e.g. in production) ahead of the file.
Env.NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is missing in appsettings.json.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// Validated at startup so a missing or short key stops the app with a clear message instead of failing on the first login.
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(
        options => options.Key.Length >= JwtOptions.MinKeyLength,
        $"Jwt:Key is missing or shorter than {JwtOptions.MinKeyLength} characters. " +
        "Set Jwt__Key=<random 32+ characters> in the .env file (see .env.example) or as an environment variable.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Issuer) && !string.IsNullOrWhiteSpace(options.Audience),
        "Jwt:Issuer and Jwt:Audience must be set in appsettings.json.")
    .Validate(
        options => options.AccessTokenLifetime > TimeSpan.Zero && options.RefreshTokenLifetime > TimeSpan.Zero,
        "Jwt:AccessTokenLifetime and Jwt:RefreshTokenLifetime must be positive time spans (e.g. 00:15:00).")
    .ValidateOnStart();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Password rules live here only. Length 6, a digit, a lower and an upper case letter stay at the Identity defaults.
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Configured from the validated JwtOptions so the signing key is read once from configuration (.env or environment).
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Async suffix kept so action names match the Async naming rule (CreatedAtAction(nameof(...Async))).
// Implicit [Required] off so a missing field reaches FluentValidation and gets a per-field message. What is left for
// model state is an empty or malformed body (null request), answered here as a 400 in the ServiceResult envelope.
builder.Services.AddControllers(options =>
    {
        options.SuppressAsyncSuffixInActionNames = false;
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = _ =>
        new BadRequestObjectResult(ServiceResult.Fail(ErrorType.Validation, "Body request kosong atau bukan JSON yang valid.")));
// Lowercase generated URLs (e.g. the Location header) so they match the api/categories style.
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Http bearer scheme: the Authorize button takes the raw token and adds the "Bearer " prefix itself.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the accessToken from /api/auth/login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = []
    });
});

// Unexpected exceptions become a ProblemDetails 500 without the exception text or stack trace.
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    if (context.ProblemDetails.Status == StatusCodes.Status500InternalServerError)
        context.ProblemDetails.Title = "Terjadi kesalahan pada server.";
});

var app = builder.Build();

// Fail at startup on a broken mapping instead of on the first request that uses it.
app.Services.GetRequiredService<AutoMapper.IConfigurationProvider>().AssertConfigurationIsValid();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
