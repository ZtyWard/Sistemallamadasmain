using ADM6.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpClient("soap", cliente =>
    cliente.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddSingleton<ProveedorWebServiceClient>();
builder.Services.AddSingleton<AutenticacionWebServiceClient>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseStaticFiles();
app.UseSession();

app.Use(async (context, next) =>
{
    PathString ruta = context.Request.Path;

    bool rutaAdministrador =
        ruta.StartsWithSegments("/Facturacion")
        || ruta.StartsWithSegments("/NuevaLinea")
        || ruta.StartsWithSegments("/ActivarLinea")
        || ruta.StartsWithSegments("/DesactivarLinea")
        || ruta.StartsWithSegments("/Administradores/Gestion");

    if (rutaAdministrador
        && string.IsNullOrWhiteSpace(
            context.Session.GetString("AdministradorIdentificacion")))
    {
        context.Response.Redirect("/Administradores/Login");
        return;
    }

    await next();
});

app.MapRazorPages();

app.Run();
