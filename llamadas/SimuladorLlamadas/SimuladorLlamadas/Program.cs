namespace SimuladorLlamadas
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                ApplicationConfiguration.Initialize();

                Application.ThreadException += (sender, error) =>
                {
                    MessageBox.Show(
                        error.Exception.ToString(),
                        "Error del programa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                };

                AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
                {
                    MessageBox.Show(
                        error.ExceptionObject.ToString(),
                        "Error no controlado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                };

                SimuladorLlamadas.Configuracion.ConfiguracionSistema.cargar_configuracion();

                Application.Run(new FrmMenuPrincipal());
            }
            catch (Exception error)
            {
                MessageBox.Show(
                    error.ToString(),
                    "Error al iniciar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}