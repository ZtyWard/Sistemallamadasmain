using System;
using System.Drawing;
using System.Windows.Forms;
using SimuladorLlamadas.Configuracion;
using SimuladorLlamadas.Estilos;
using SimuladorLlamadas.Servicios;

namespace SimuladorLlamadas
{
    public partial class FrmConfiguracion : Form
    {
 
        // PANELES PRINCIPALES
      

        private Panel pnl_contenedor = null!;
        private Panel pnl_pantalla = null!;
        private Panel pnl_barra_estado = null!;
        private Panel pnl_conexion = null!;
        private Panel pnl_seguridad = null!;

        
        // CONTROLES DEL FORMULARIO
        

        private Label lbl_hora = null!;
        private Label lbl_senal = null!;

        private Label lbl_titulo = null!;
        private Label lbl_subtitulo = null!;

        private Label lbl_seccion_conexion = null!;
        private Label lbl_ip_identificador = null!;
        private Label lbl_puerto_identificador = null!;

        private Label lbl_seccion_seguridad = null!;
        private Label lbl_llave_aes = null!;
        private Label lbl_nota_seguridad = null!;

        private TextBox txt_ip_identificador = null!;
        private TextBox txt_puerto_identificador = null!;
        private TextBox txt_llave_aes = null!;

        private Button btn_generar_llave = null!;
        private Button btn_probar_conexion = null!;
        private Button btn_guardar = null!;
        private Button btn_cancelar = null!;

        private System.Windows.Forms.Timer temporizador_hora = null!;

        public FrmConfiguracion()
        {
            InitializeComponent();
            construir_interfaz();
            cargar_configuracion_actual();
        }

        private void construir_interfaz()
        {
            configurar_formulario();
            crear_controles();
            aplicar_estilos();
            agregar_controles();
            aplicar_bordes_redondeados();
            configurar_temporizador_hora();
        }

        private void configurar_formulario()
        {
            this.ClientSize = new Size(430, 820);

            TemaVisual.aplicar_estilo_formulario_celular(
                this,
                "Configuración"
            );
        }

        private void crear_controles()
        {
        
            // MARCO TIPO TELÉFONO
          

            pnl_contenedor = new Panel();
            pnl_contenedor.Location = new Point(40, 20);
            pnl_contenedor.Size = new Size(350, 770);

            pnl_pantalla = new Panel();
            pnl_pantalla.Location = new Point(14, 14);
            pnl_pantalla.Size = new Size(322, 742);

            
            // BARRA SUPERIOR
           

            pnl_barra_estado = new Panel();
            pnl_barra_estado.Location = new Point(0, 0);
            pnl_barra_estado.Size = new Size(322, 38);

            lbl_hora = new Label();
            lbl_hora.Text = DateTime.Now.ToString("HH:mm");
            lbl_hora.Location = new Point(18, 8);
            lbl_hora.Size = new Size(80, 22);
            lbl_hora.TextAlign = ContentAlignment.MiddleLeft;

            lbl_senal = new Label();
            lbl_senal.Text = "LTE   100%";
            lbl_senal.Location = new Point(190, 8);
            lbl_senal.Size = new Size(110, 22);
            lbl_senal.TextAlign = ContentAlignment.MiddleRight;

            // TÍTULO
       

            lbl_titulo = new Label();
            lbl_titulo.Text = "Configuración";
            lbl_titulo.TextAlign = ContentAlignment.MiddleCenter;
            lbl_titulo.Location = new Point(20, 48);
            lbl_titulo.Size = new Size(282, 34);

            lbl_subtitulo = new Label();
            lbl_subtitulo.Text = "Conexión y seguridad del simulador";
            lbl_subtitulo.TextAlign = ContentAlignment.MiddleCenter;
            lbl_subtitulo.Location = new Point(20, 80);
            lbl_subtitulo.Size = new Size(282, 24);

            // PANEL DE CONEXIÓN
       

            pnl_conexion = new Panel();
            pnl_conexion.Location = new Point(22, 120);
            pnl_conexion.Size = new Size(278, 170);

            lbl_seccion_conexion = new Label();
            lbl_seccion_conexion.Text = "Datos de conexión";
            lbl_seccion_conexion.Location = new Point(14, 12);
            lbl_seccion_conexion.Size = new Size(250, 24);
            lbl_seccion_conexion.TextAlign = ContentAlignment.MiddleLeft;

            lbl_ip_identificador = new Label();
            lbl_ip_identificador.Text = "IP del Identificador";
            lbl_ip_identificador.Location = new Point(20, 48);
            lbl_ip_identificador.Size = new Size(238, 20);
            lbl_ip_identificador.TextAlign = ContentAlignment.MiddleLeft;

            txt_ip_identificador = new TextBox();
            txt_ip_identificador.Location = new Point(20, 72);
            txt_ip_identificador.Size = new Size(238, 28);

            lbl_puerto_identificador = new Label();
            lbl_puerto_identificador.Text = "Puerto";
            lbl_puerto_identificador.Location = new Point(20, 108);
            lbl_puerto_identificador.Size = new Size(238, 20);
            lbl_puerto_identificador.TextAlign = ContentAlignment.MiddleLeft;

            txt_puerto_identificador = new TextBox();
            txt_puerto_identificador.Location = new Point(20, 132);
            txt_puerto_identificador.Size = new Size(238, 28);
            txt_puerto_identificador.KeyPress += txt_puerto_identificador_key_press;

         
            // PANEL DE SEGURIDAD
          

            pnl_seguridad = new Panel();
            pnl_seguridad.Location = new Point(22, 310);
            pnl_seguridad.Size = new Size(278, 210);

            lbl_seccion_seguridad = new Label();
            lbl_seccion_seguridad.Text = "Seguridad AES";
            lbl_seccion_seguridad.Location = new Point(14, 12);
            lbl_seccion_seguridad.Size = new Size(250, 24);
            lbl_seccion_seguridad.TextAlign = ContentAlignment.MiddleLeft;

            lbl_llave_aes = new Label();
            lbl_llave_aes.Text = "Llave AES Base64";
            lbl_llave_aes.Location = new Point(20, 48);
            lbl_llave_aes.Size = new Size(238, 20);
            lbl_llave_aes.TextAlign = ContentAlignment.MiddleLeft;

            txt_llave_aes = new TextBox();
            txt_llave_aes.Location = new Point(20, 72);
            txt_llave_aes.Size = new Size(238, 28);

            btn_generar_llave = new Button();
            btn_generar_llave.Text = "GENERAR LLAVE";
            btn_generar_llave.Location = new Point(20, 112);
            btn_generar_llave.Size = new Size(238, 40);
            btn_generar_llave.Click += btn_generar_llave_click;

            lbl_nota_seguridad = new Label();
            lbl_nota_seguridad.Text = "La llave AES se usa para cifrar los datos sensibles antes de enviarlos.";
            lbl_nota_seguridad.Location = new Point(20, 160);
            lbl_nota_seguridad.Size = new Size(238, 38);
            lbl_nota_seguridad.TextAlign = ContentAlignment.MiddleCenter;

           
            // BOTONES PRINCIPALES
           

            btn_probar_conexion = new Button();
            btn_probar_conexion.Text = "PROBAR CONEXION";
            btn_probar_conexion.Location = new Point(35, 535);
            btn_probar_conexion.Size = new Size(252, 40);
            btn_probar_conexion.Click += btn_probar_conexion_click;

            btn_guardar = new Button();
            btn_guardar.Text = "GUARDAR";
            btn_guardar.Location = new Point(35, 590);
            btn_guardar.Size = new Size(252, 48);
            btn_guardar.Click += btn_guardar_click;

            btn_cancelar = new Button();
            btn_cancelar.Text = "CANCELAR";
            btn_cancelar.Location = new Point(35, 655);
            btn_cancelar.Size = new Size(252, 40);
            btn_cancelar.Click += btn_cancelar_click;
        }

        private void aplicar_estilos()
        {
            TemaVisual.aplicar_estilo_marco_telefono(pnl_contenedor);
            TemaVisual.aplicar_estilo_pantalla_telefono(pnl_pantalla);

            pnl_barra_estado.BackColor = TemaVisual.color_telefono_pantalla;
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_hora);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_senal);

            TemaVisual.aplicar_estilo_titulo_celular(lbl_titulo);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_subtitulo);

            TemaVisual.aplicar_estilo_tarjeta_celular(pnl_conexion);
            TemaVisual.aplicar_estilo_texto_celular(lbl_seccion_conexion);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_ip_identificador);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_puerto_identificador);

            aplicar_estilo_caja_configuracion(txt_ip_identificador);
            aplicar_estilo_caja_configuracion(txt_puerto_identificador);

            TemaVisual.aplicar_estilo_tarjeta_celular(pnl_seguridad);
            TemaVisual.aplicar_estilo_texto_celular(lbl_seccion_seguridad);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_llave_aes);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_nota_seguridad);

            aplicar_estilo_caja_configuracion(txt_llave_aes);

            TemaVisual.aplicar_estilo_boton_app(btn_generar_llave);
            TemaVisual.aplicar_estilo_boton_app(btn_probar_conexion);
            TemaVisual.aplicar_estilo_boton_llamar(btn_guardar);
            TemaVisual.aplicar_estilo_boton_app(btn_cancelar);
        }

        private void aplicar_estilo_caja_configuracion(TextBox caja_texto)
        {
            caja_texto.BackColor = TemaVisual.color_telefono_pantalla;
            caja_texto.ForeColor = TemaVisual.color_texto_claro;
            caja_texto.BorderStyle = BorderStyle.None;
            caja_texto.Font = new Font("Consolas", 10, FontStyle.Regular);
        }

        private void agregar_controles()
        {
            this.Controls.Clear();
            this.Controls.Add(pnl_contenedor);

            pnl_contenedor.Controls.Add(pnl_pantalla);

            pnl_pantalla.Controls.Add(pnl_barra_estado);
            pnl_barra_estado.Controls.Add(lbl_hora);
            pnl_barra_estado.Controls.Add(lbl_senal);

            pnl_pantalla.Controls.Add(lbl_titulo);
            pnl_pantalla.Controls.Add(lbl_subtitulo);

            pnl_pantalla.Controls.Add(pnl_conexion);
            pnl_conexion.Controls.Add(lbl_seccion_conexion);
            pnl_conexion.Controls.Add(lbl_ip_identificador);
            pnl_conexion.Controls.Add(txt_ip_identificador);
            pnl_conexion.Controls.Add(lbl_puerto_identificador);
            pnl_conexion.Controls.Add(txt_puerto_identificador);

            pnl_pantalla.Controls.Add(pnl_seguridad);
            pnl_seguridad.Controls.Add(lbl_seccion_seguridad);
            pnl_seguridad.Controls.Add(lbl_llave_aes);
            pnl_seguridad.Controls.Add(txt_llave_aes);
            pnl_seguridad.Controls.Add(btn_generar_llave);
            pnl_seguridad.Controls.Add(lbl_nota_seguridad);

            pnl_pantalla.Controls.Add(btn_probar_conexion);
            pnl_pantalla.Controls.Add(btn_guardar);
            pnl_pantalla.Controls.Add(btn_cancelar);
        }

        private void aplicar_bordes_redondeados()
        {
            TemaVisual.aplicar_borde_redondeado(pnl_contenedor, 34);
            TemaVisual.aplicar_borde_redondeado(pnl_pantalla, 26);
            TemaVisual.aplicar_borde_redondeado(pnl_conexion, 18);
            TemaVisual.aplicar_borde_redondeado(pnl_seguridad, 18);

            TemaVisual.aplicar_borde_redondeado(btn_generar_llave, 14);
            TemaVisual.aplicar_borde_redondeado(btn_probar_conexion, 14);
            TemaVisual.aplicar_borde_redondeado(btn_guardar, 18);
            TemaVisual.aplicar_borde_redondeado(btn_cancelar, 14);
        }

       
        // TEMPORIZADOR DE HORA
       

        private void configurar_temporizador_hora()
        {
            temporizador_hora = new System.Windows.Forms.Timer();
            temporizador_hora.Interval = 1000;
            temporizador_hora.Tick += temporizador_hora_tick;
            temporizador_hora.Start();

            this.FormClosed += frm_configuracion_form_closed;
        }

        private void temporizador_hora_tick(object? sender, EventArgs e)
        {
            lbl_hora.Text = DateTime.Now.ToString("HH:mm");
        }

        private void frm_configuracion_form_closed(object? sender, FormClosedEventArgs e)
        {
            if (temporizador_hora != null)
            {
                temporizador_hora.Stop();
                temporizador_hora.Dispose();
            }
        }

       
        // CARGA DE CONFIGURACIÓN
        

        private void cargar_configuracion_actual()
        {
            txt_ip_identificador.Text = ConfiguracionSistema.ip_identificador;
            txt_puerto_identificador.Text = ConfiguracionSistema.puerto_identificador.ToString();
            txt_llave_aes.Text = ConfiguracionSistema.llave_aes_base64;
        }

        
        // EVENTOS VISUALES
       

        private void txt_puerto_identificador_key_press(object? sender, KeyPressEventArgs e)
        {
            bool es_digito;
            bool es_control;

            es_digito = char.IsDigit(e.KeyChar);
            es_control = char.IsControl(e.KeyChar);

            if (!es_digito && !es_control)
            {
                e.Handled = true;
            }
        }

    

        private void btn_guardar_click(object? sender, EventArgs e)
        {
            int puerto_identificador;

            if (string.IsNullOrWhiteSpace(txt_ip_identificador.Text))
            {
                mostrar_advertencia("Debe ingresar la IP del Identificador.");
                return;
            }

            if (!int.TryParse(txt_puerto_identificador.Text, out puerto_identificador))
            {
                mostrar_advertencia("El puerto debe ser un número válido.");
                return;
            }

            if (!ServicioValidaciones.es_puerto_valido(puerto_identificador))
            {
                mostrar_advertencia("El puerto debe estar entre 1 y 65535.");
                return;
            }

            if (!ServicioValidaciones.es_llave_aes_base64_valida(txt_llave_aes.Text))
            {
                mostrar_advertencia("Debe ingresar o generar una llave AES válida.");
                return;
            }

            ConfiguracionSistema.ip_identificador = txt_ip_identificador.Text.Trim();
            ConfiguracionSistema.puerto_identificador = puerto_identificador;
            ConfiguracionSistema.llave_aes_base64 = txt_llave_aes.Text.Trim();

            ConfiguracionSistema.guardar_configuracion();

            MessageBox.Show(
                "Configuración guardada correctamente.",
                "Configuración",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            this.Close();
        }

        private void btn_cancelar_click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_generar_llave_click(object? sender, EventArgs e)
        {
            txt_llave_aes.Text = ServicioAES.generar_llave_aes_256();
        }

        private async void btn_probar_conexion_click(object? sender, EventArgs e)
        {
            int puerto_identificador;
            string ip_anterior;
            int puerto_anterior;

            if (!int.TryParse(txt_puerto_identificador.Text, out puerto_identificador))
            {
                mostrar_advertencia("El puerto debe ser un numero valido.");
                return;
            }

            ip_anterior = ConfiguracionSistema.ip_identificador;
            puerto_anterior = ConfiguracionSistema.puerto_identificador;

            ConfiguracionSistema.ip_identificador = txt_ip_identificador.Text.Trim();
            ConfiguracionSistema.puerto_identificador = puerto_identificador;

            btn_probar_conexion.Enabled = false;
            btn_probar_conexion.Text = "PROBANDO...";

            bool conectado = await ServicioConexion.probar_identificador_async();

            btn_probar_conexion.Enabled = true;
            btn_probar_conexion.Text = "PROBAR CONEXION";

            ConfiguracionSistema.ip_identificador = ip_anterior;
            ConfiguracionSistema.puerto_identificador = puerto_anterior;

            MessageBox.Show(
                conectado
                    ? "Conectado al identificador."
                    : "No se pudo conectar con el Identificador.\r\nRevise que Python este ejecutandose.",
                "Conexion",
                MessageBoxButtons.OK,
                conectado ? MessageBoxIcon.Information : MessageBoxIcon.Warning
            );
        }

        private void mostrar_advertencia(string mensaje)
        {
            MessageBox.Show(
                mensaje,
                "Validación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }
}
