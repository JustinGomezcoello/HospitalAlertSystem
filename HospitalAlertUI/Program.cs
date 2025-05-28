using HospitalAlertUI.Services;
using HospitalAlertUI.Rabbit;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Configura servicios
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 🧠 Servicios propios
builder.Services.AddSingleton<AlertService>();
builder.Services.AddHostedService<ServiceBusListener>();

var app = builder.Build();

// 🔐 Configuración del middleware HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// 🚀 Sirve contenido estático (CSS, JS, blazor.server.js)
app.UseHttpsRedirection();
app.UseStaticFiles();     // <-- ¡Esto es vital para Blazor!

// 🚦 Enrutamiento
app.UseRouting();

// 🔌 Mapeo de SignalR para Blazor Server
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
