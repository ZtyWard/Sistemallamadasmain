using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ADM6.Pages.Administradores;

public class GestionModel : PageModel
{
    private readonly AutenticacionWebServiceClient _autenticacion;
    public GestionModel(AutenticacionWebServiceClient autenticacion) =>
        _autenticacion = autenticacion;

    public List<UsuarioListadoWeb> Administradores { get; set; } = new();
    [BindProperty] public AdministradorEntrada Entrada { get; set; } = new();
    public string Mensaje { get; set; } = "";
    public bool EsError { get; set; }

    public async Task OnGetAsync() => await CargarAsync();

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (!DatosValidos(requiereContrasena: true)) return await ErrorAsync(
            "Datos incorrectos. La contraseña debe tener exactamente 14 caracteres y cumplir todas las reglas.");
        try
        {
            ResultadoOperacionWeb r = await _autenticacion.CrearUsuarioAsync(Modelo(), 1);
            Mensaje = r.Exitoso ? "Registro exitoso" : r.Mensaje;
            EsError = !r.Exitoso;
            if (r.Exitoso) Entrada = new();
        }
        catch { Mensaje = "No fue posible comunicarse con el Web Service."; EsError = true; }
        await CargarAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostModificarAsync()
    {
        if (!DatosValidos(requiereContrasena: false)) return await ErrorAsync("Datos incorrectos o incompletos.");
        try
        {
            ResultadoOperacionWeb r = await _autenticacion.ModificarUsuarioAsync(Modelo());
            Mensaje = r.Exitoso ? "Modificación exitosa" : r.Mensaje;
            EsError = !r.Exitoso;
        }
        catch { Mensaje = "No fue posible comunicarse con el Web Service."; EsError = true; }
        await CargarAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCambiarEstadoAsync(
        string identificacion, string estado)
    {
        try
        {
            string nuevo = estado.Equals("activo", StringComparison.OrdinalIgnoreCase)
                ? "inactivo" : "activo";
            ResultadoOperacionWeb r = await _autenticacion
                .CambiarEstadoAsync(identificacion, nuevo);
            Mensaje = r.Exitoso ? "Estado actualizado correctamente" : r.Mensaje;
            EsError = !r.Exitoso;
        }
        catch { Mensaje = "No fue posible comunicarse con el Web Service."; EsError = true; }
        await CargarAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostEliminarAsync(string identificacion)
    {
        try
        {
            ResultadoOperacionWeb r = await _autenticacion
                .EliminarUsuarioAsync(identificacion);
            Mensaje = r.Exitoso ? "Borrado exitoso" : r.Mensaje;
            EsError = !r.Exitoso;
        }
        catch { Mensaje = "No fue posible comunicarse con el Web Service."; EsError = true; }
        await CargarAsync();
        return Page();
    }

    private async Task CargarAsync()
    {
        try { Administradores = await _autenticacion.ListarUsuariosAsync(1); }
        catch { Administradores = new(); if (string.IsNullOrEmpty(Mensaje)) Mensaje = "No fue posible cargar administradores."; EsError = true; }
    }

    private UsuarioEdicionWeb Modelo() => new(
        Entrada.Identificacion.Trim(), Entrada.Nombre.Trim(),
        Entrada.PrimerApellido.Trim(), Entrada.SegundoApellido.Trim(),
        Entrada.CorreoElectronico.Trim(), Entrada.Usuario.Trim(), Entrada.Contrasena);

    private bool DatosValidos(bool requiereContrasena)
    {
        bool datosBase = !string.IsNullOrWhiteSpace(Entrada.Identificacion)
            && Regex.IsMatch(Entrada.Identificacion.Trim(), @"^\d+$")
            && NombreValido(Entrada.Nombre)
            && NombreValido(Entrada.PrimerApellido)
            && NombreValido(Entrada.SegundoApellido)
            && CorreoValido(Entrada.CorreoElectronico)
            && !string.IsNullOrWhiteSpace(Entrada.Usuario);

        if (!datosBase) return false;
        if (!requiereContrasena && string.IsNullOrWhiteSpace(Entrada.Contrasena)) return true;
        return ContrasenaValida(Entrada.Contrasena);
    }

    private static bool NombreValido(string valor) =>
        !string.IsNullOrWhiteSpace(valor)
        && Regex.IsMatch(valor.Trim(), @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$");

    private static bool CorreoValido(string valor) =>
        !string.IsNullOrWhiteSpace(valor)
        && Regex.IsMatch(valor.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    private static bool ContrasenaValida(string valor) =>
        valor.Length == 14
        && Regex.IsMatch(valor, "[A-Z]")
        && Regex.IsMatch(valor, "[a-z]")
        && Regex.IsMatch(valor, "[0-9]")
        && Regex.IsMatch(valor, "[^A-Za-z0-9]");

    private async Task<IActionResult> ErrorAsync(string mensaje)
    {
        Mensaje = mensaje; EsError = true; await CargarAsync(); return Page();
    }
}

public class AdministradorEntrada
{
    [Required, RegularExpression(@"^\d+$")] public string Identificacion { get; set; } = "";
    [Required] public string Nombre { get; set; } = "";
    [Required] public string PrimerApellido { get; set; } = "";
    [Required] public string SegundoApellido { get; set; } = "";
    [Required, EmailAddress] public string CorreoElectronico { get; set; } = "";
    [Required] public string Usuario { get; set; } = "";
    // La contraseña se valida manualmente porque al modificar puede conservarse vacía.
    public string Contrasena { get; set; } = "";
}
