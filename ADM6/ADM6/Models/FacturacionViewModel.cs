namespace ADM6.Models;

public class FacturacionViewModel
{
    public static DateTime ObtenerFechaCortePredeterminada()
    {
        DateTime hoy = DateTime.Today;
        int maxDias = DateTime.DaysInMonth(hoy.Year, hoy.Month);
        int diaCorte = Math.Min(28, maxDias);
        return new DateTime(hoy.Year, hoy.Month, diaCorte);
    }

    public static DateTime ObtenerFechaMaxPagoPredeterminada()
    {
        DateTime hoy = DateTime.Today;
        int maxDias = DateTime.DaysInMonth(hoy.Year, hoy.Month);
        return new DateTime(hoy.Year, hoy.Month, maxDias);
    }

    public DateTime FechaInicio { get; set; } = ObtenerFechaCortePredeterminada();

    public DateTime FechaFin { get; set; } = ObtenerFechaMaxPagoPredeterminada();

    public DateTime? UltimaFacturacion { get; set; }

    public bool FacturacionRealizada { get; set; }

    public string? Mensaje { get; set; }

    public bool EsError { get; set; }

    public string PeriodoUltimaFacturacion
    {
        get
        {
            if (!UltimaFacturacion.HasValue)
            {
                return "No hay información disponible";
            }

            return UltimaFacturacion.Value
                .ToString("dd/MM/yyyy");
        }
    }
}