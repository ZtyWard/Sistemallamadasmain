using ADM6.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ADM6.Pages.Cliente;

public class RegistroModel : PageModel
{
    private readonly AutenticacionWebServiceClient _autenticacion;

    public RegistroModel(AutenticacionWebServiceClient autenticacion) =>
        _autenticacion = autenticacion;

    [BindProperty]
    public RegistroCliente Entrada { get; set; } = new();

    public string Mensaje { get; set; } = "";
    public bool Exitoso { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Entrada.Identificacion)
            || !Regex.IsMatch(Entrada.Identificacion.Trim(), @"^\d+$"))
        {
            return Error("La identificación debe contener únicamente números.");
        }

        if (!NombreValido(Entrada.Nombre))
        {
            return Error("El nombre es obligatorio y solamente puede contener letras, espacios, apóstrofes o guiones.");
        }

        if (!NombreValido(Entrada.PrimerApellido))
        {
            return Error("El primer apellido es obligatorio y solamente puede contener letras, espacios, apóstrofes o guiones.");
        }

        if (!NombreValido(Entrada.SegundoApellido))
        {
            return Error("El segundo apellido es obligatorio y solamente puede contener letras, espacios, apóstrofes o guiones.");
        }

        if (string.IsNullOrWhiteSpace(Entrada.CorreoElectronico)
            || !new EmailAddressAttribute().IsValid(Entrada.CorreoElectronico.Trim()))
        {
            return Error("Debe ingresar un correo electrónico válido.");
        }

        if (string.IsNullOrWhiteSpace(Entrada.Usuario))
        {
            return Error("El usuario es obligatorio.");
        }

        if (!ContrasenaValida(Entrada.Contrasena))
        {
            return Error("La contraseña debe tener exactamente 14 caracteres, mayúscula, minúscula, número y carácter especial, sin espacios.");
        }

        if (Entrada.Contrasena != Entrada.ConfirmarContrasena)
        {
            return Error("Las contraseñas no coinciden.");
        }

        try
        {
            var usuario = new UsuarioEdicionWeb(
                Entrada.Identificacion.Trim(), Entrada.Nombre.Trim(),
                Entrada.PrimerApellido.Trim(), Entrada.SegundoApellido.Trim(),
                Entrada.CorreoElectronico.Trim(), Entrada.Usuario.Trim(),
                Entrada.Contrasena);

            ResultadoOperacionWeb resultado =
                await _autenticacion.CrearUsuarioAsync(usuario, 2);

            Exitoso = resultado.Exitoso;
            Mensaje = resultado.Exitoso ? "Registro exitoso" : resultado.Mensaje;
            if (Exitoso) Entrada = new();
            return Page();
        }
        catch
        {
            Mensaje = "No fue posible comunicarse con el servicio de autenticación.";
            return Page();
        }
    }

    private PageResult Error(string mensaje)
    {
        Mensaje = mensaje;
        Exitoso = false;
        return Page();
    }

    private static bool NombreValido(string valor) =>
        !string.IsNullOrWhiteSpace(valor)
        && Regex.IsMatch(valor.Trim(), @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$");

    private static bool ContrasenaValida(string valor) =>
        !string.IsNullOrWhiteSpace(valor)
        && valor.Length == 14
        && Regex.IsMatch(valor, "[A-Z]")
        && Regex.IsMatch(valor, "[a-z]")
        && Regex.IsMatch(valor, "[0-9]")
        && Regex.IsMatch(valor, "[^A-Za-z0-9]")
        && !Regex.IsMatch(valor, @"\s");
}

public class RegistroCliente
{
    [Required, RegularExpression(@"^\d+$")]
    public string Identificacion { get; set; } = "";
    [Required, RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$")]
    public string Nombre { get; set; } = "";
    [Required, RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$")]
    public string PrimerApellido { get; set; } = "";
    [Required, RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$")]
    public string SegundoApellido { get; set; } = "";
    [Required, EmailAddress]
    public string CorreoElectronico { get; set; } = "";
    [Required]
    public string Usuario { get; set; } = "";
    [Required, StringLength(14, MinimumLength = 14)]
    public string Contrasena { get; set; } = "";
    [Required]
    public string ConfirmarContrasena { get; set; } = "";
}
