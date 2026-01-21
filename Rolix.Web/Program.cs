using Rolix.Web.Services;
using QuestPDF.Infrastructure;

// Set QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// Sessions
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
});
builder.Services.AddHttpContextAccessor();

// Dataverse (OAuth interactif)
builder.Services.AddSingleton<DataverseService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<QuoteService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<LeadService>();
builder.Services.AddScoped<SavService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
