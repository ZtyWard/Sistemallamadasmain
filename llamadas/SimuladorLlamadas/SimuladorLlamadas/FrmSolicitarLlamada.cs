using System;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using SimuladorLlamadas.Configuracion;
using SimuladorLlamadas.Estilos;
using SimuladorLlamadas.Modelos;
using SimuladorLlamadas.Servicios;

namespace SimuladorLlamadas
{
    public partial class FrmSolicitarLlamada : Form
    {
        
        // PANELES PRINCIPALES
        
        private Panel pnl_contenedor = null!;
        private Panel pnl_pantalla = null!;
        private Panel pnl_barra_estado = null!;
        private Panel pnl_tarjeta_telefono = null!;
        private Panel pnl_destino = null!;
        private Panel pnl_resultado = null!;

        
        // CONTROLES DEL FORMULARIO
        

        private Label lbl_hora = null!;
        private Label lbl_senal = null!;

        private Label lbl_titulo = null!;
        private Label lbl_subtitulo = null!;

        private Label lbl_seccion_telefono = null!;
        private Label lbl_numero_telefono = null!;
        private Label lbl_identificador_telefono = null!;
        private Label lbl_identificador_chip = null!;
        private Label lbl_coordenadas = null!;

        private Label lbl_seccion_llamada = null!;
        private Label lbl_telefono_destino = null!;
        private Label lbl_seccion_resultado = null!;

        private ComboBox cmb_telefono_origen = null!;

        private TextBox txt_numero_telefono = null!;
        private TextBox txt_identificador_telefono = null!;
        private TextBox txt_identificador_chip = null!;
        private TextBox txt_coordenadas = null!;
        private TextBox txt_telefono_destino = null!;
        private TextBox txt_resultado = null!;

        private Button btn_solicitar_llamada = null!;
        private Button btn_limpiar = null!;
        private Button btn_cerrar = null!;

        private System.Windows.Forms.Timer temporizador_hora = null!;

        public FrmSolicitarLlamada()
        {
            InitializeComponent();
            construir_interfaz();
        }

        private void construir_interfaz()
        {
            configurar_formulario();
            crear_controles();
            aplicar_estilos();
            agregar_controles();
            aplicar_bordes_redondeados();
            cargar_telefonos_origen();
            configurar_temporizador_hora();
        }

        private void configurar_formulario()
        {
            this.ClientSize = new Size(430, 840);

            TemaVisual.aplicar_estilo_formulario_celular(
                this,
                "Solicitar llamada"
            );
        }

        private void crear_controles()
        {
            
            // MARCO TIPO TELÉFONO
            

            pnl_contenedor = new Panel();
            pnl_contenedor.Location = new Point(40, 20);
            pnl_contenedor.Size = new Size(350, 790);

            pnl_pantalla = new Panel();
            pnl_pantalla.Location = new Point(14, 14);
            pnl_pantalla.Size = new Size(322, 762);

            
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
            lbl_titulo.Text = "Solicitar llamada";
            lbl_titulo.TextAlign = ContentAlignment.MiddleCenter;
            lbl_titulo.Location = new Point(20, 48);
            lbl_titulo.Size = new Size(282, 34);

            lbl_subtitulo = new Label();
            lbl_subtitulo.Text = "Solicite autorización para llamar.";
            lbl_subtitulo.TextAlign = ContentAlignment.MiddleCenter;
            lbl_subtitulo.Location = new Point(20, 80);
            lbl_subtitulo.Size = new Size(282, 24);

           
            // TARJETA DE TELÉFONO ORIGEN
           

            pnl_tarjeta_telefono = new Panel();
            pnl_tarjeta_telefono.Location = new Point(22, 115);
            pnl_tarjeta_telefono.Size = new Size(278, 166);

            lbl_seccion_telefono = new Label();
            lbl_seccion_telefono.Text = "Teléfono origen";
            lbl_seccion_telefono.Location = new Point(14, 8);
            lbl_seccion_telefono.Size = new Size(250, 22);
            lbl_seccion_telefono.TextAlign = ContentAlignment.MiddleLeft;

            cmb_telefono_origen = new ComboBox();
            cmb_telefono_origen.Location = new Point(14, 34);
            cmb_telefono_origen.Size = new Size(250, 25);
            cmb_telefono_origen.SelectedIndexChanged += cmb_telefono_origen_selected_index_changed;

            lbl_numero_telefono = crear_etiqueta_tecnica("Número", 14, 68, 64);
            txt_numero_telefono = crear_caja_tecnica(78, 68, 186);
            txt_numero_telefono.ReadOnly = true;

            lbl_identificador_telefono = crear_etiqueta_tecnica("ID Tel.", 14, 92, 64);
            txt_identificador_telefono = crear_caja_tecnica(78, 92, 186);
            txt_identificador_telefono.ReadOnly = true;

            lbl_identificador_chip = crear_etiqueta_tecnica("Chip", 14, 116, 64);
            txt_identificador_chip = crear_caja_tecnica(78, 116, 186);
            txt_identificador_chip.ReadOnly = true;

            lbl_coordenadas = crear_etiqueta_tecnica("GPS", 14, 140, 64);
            txt_coordenadas = crear_caja_tecnica(78, 140, 186);
            txt_coordenadas.ReadOnly = true;

           
            // DATOS DE LA LLAMADA
           

            lbl_seccion_llamada = new Label();
            lbl_seccion_llamada.Text = "Destino de la llamada";
            lbl_seccion_llamada.Location = new Point(35, 300);
            lbl_seccion_llamada.Size = new Size(252, 24);
            lbl_seccion_llamada.TextAlign = ContentAlignment.MiddleCenter;

            pnl_destino = new Panel();
            pnl_destino.Location = new Point(35, 332);
            pnl_destino.Size = new Size(252, 96);

            lbl_telefono_destino = new Label();
            lbl_telefono_destino.Text = "Teléfono destino";
            lbl_telefono_destino.Location = new Point(10, 12);
            lbl_telefono_destino.Size = new Size(232, 22);
            lbl_telefono_destino.TextAlign = ContentAlignment.MiddleCenter;

            txt_telefono_destino = new TextBox();
            txt_telefono_destino.Location = new Point(25, 44);
            txt_telefono_destino.Size = new Size(202, 34);
            txt_telefono_destino.TextAlign = HorizontalAlignment.Center;
            txt_telefono_destino.KeyPress += txt_telefono_destino_key_press;

            
            // BOTONES
            

            btn_solicitar_llamada = new Button();
            btn_solicitar_llamada.Text = "SOLICITAR";
            btn_solicitar_llamada.Location = new Point(35, 450);
            btn_solicitar_llamada.Size = new Size(252, 48);
            btn_solicitar_llamada.Click += btn_solicitar_llamada_click;

            btn_limpiar = new Button();
            btn_limpiar.Text = "BORRAR";
            btn_limpiar.Location = new Point(35, 512);
            btn_limpiar.Size = new Size(122, 36);
            btn_limpiar.Click += btn_limpiar_click;

            btn_cerrar = new Button();
            btn_cerrar.Text = "SALIR";
            btn_cerrar.Location = new Point(165, 512);
            btn_cerrar.Size = new Size(122, 36);
            btn_cerrar.Click += btn_cerrar_click;

            
            // RESULTADO
          

            lbl_seccion_resultado = new Label();
            lbl_seccion_resultado.Text = "Resultado";
            lbl_seccion_resultado.Location = new Point(35, 568);
            lbl_seccion_resultado.Size = new Size(252, 22);
            lbl_seccion_resultado.TextAlign = ContentAlignment.MiddleCenter;

            pnl_resultado = new Panel();
            pnl_resultado.Location = new Point(35, 598);
            pnl_resultado.Size = new Size(252, 110);

            txt_resultado = new TextBox();
            txt_resultado.Location = new Point(10, 8);
            txt_resultado.Size = new Size(232, 94);
            txt_resultado.Multiline = true;
            txt_resultado.ScrollBars = ScrollBars.Vertical;
            txt_resultado.ReadOnly = true;
        }

        private Label crear_etiqueta_tecnica(string texto, int posicion_x, int posicion_y, int ancho)
        {
            Label etiqueta;

            etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.Location = new Point(posicion_x, posicion_y);
            etiqueta.Size = new Size(ancho, 20);
            etiqueta.TextAlign = ContentAlignment.MiddleLeft;

            return etiqueta;
        }

        private TextBox crear_caja_tecnica(int posicion_x, int posicion_y, int ancho)
        {
            TextBox caja_texto;

            caja_texto = new TextBox();
            caja_texto.Location = new Point(posicion_x, posicion_y);
            caja_texto.Size = new Size(ancho, 20);

            return caja_texto;
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

            TemaVisual.aplicar_estilo_tarjeta_celular(pnl_tarjeta_telefono);
            TemaVisual.aplicar_estilo_texto_celular(lbl_seccion_telefono);

            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_numero_telefono);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_identificador_telefono);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_identificador_chip);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_coordenadas);

            TemaVisual.aplicar_estilo_combo(cmb_telefono_origen);

            aplicar_estilo_caja_tecnica(txt_numero_telefono);
            aplicar_estilo_caja_tecnica(txt_identificador_telefono);
            aplicar_estilo_caja_tecnica(txt_identificador_chip);
            aplicar_estilo_caja_tecnica(txt_coordenadas);

            TemaVisual.aplicar_estilo_texto_celular(lbl_seccion_llamada);

            TemaVisual.aplicar_estilo_tarjeta_celular(pnl_destino);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_telefono_destino);
            TemaVisual.aplicar_estilo_caja_numero(txt_telefono_destino);

            TemaVisual.aplicar_estilo_boton_llamar(btn_solicitar_llamada);
            TemaVisual.aplicar_estilo_boton_app(btn_limpiar);
            TemaVisual.aplicar_estilo_boton_app(btn_cerrar);

            TemaVisual.aplicar_estilo_texto_celular(lbl_seccion_resultado);

            TemaVisual.aplicar_estilo_tarjeta_celular(pnl_resultado);
            TemaVisual.aplicar_estilo_resultado_celular(txt_resultado);
        }

        private void aplicar_estilo_caja_tecnica(TextBox caja_texto)
        {
            caja_texto.BackColor = TemaVisual.color_telefono_tarjeta;
            caja_texto.ForeColor = TemaVisual.color_texto_suave;
            caja_texto.BorderStyle = BorderStyle.None;
            caja_texto.Font = new Font("Consolas", 8, FontStyle.Regular);
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

            pnl_pantalla.Controls.Add(pnl_tarjeta_telefono);
            pnl_tarjeta_telefono.Controls.Add(lbl_seccion_telefono);
            pnl_tarjeta_telefono.Controls.Add(cmb_telefono_origen);
            pnl_tarjeta_telefono.Controls.Add(lbl_numero_telefono);
            pnl_tarjeta_telefono.Controls.Add(txt_numero_telefono);
            pnl_tarjeta_telefono.Controls.Add(lbl_identificador_telefono);
            pnl_tarjeta_telefono.Controls.Add(txt_identificador_telefono);
            pnl_tarjeta_telefono.Controls.Add(lbl_identificador_chip);
            pnl_tarjeta_telefono.Controls.Add(txt_identificador_chip);
            pnl_tarjeta_telefono.Controls.Add(lbl_coordenadas);
            pnl_tarjeta_telefono.Controls.Add(txt_coordenadas);

            pnl_pantalla.Controls.Add(lbl_seccion_llamada);
            pnl_pantalla.Controls.Add(pnl_destino);
            pnl_destino.Controls.Add(lbl_telefono_destino);
            pnl_destino.Controls.Add(txt_telefono_destino);

            pnl_pantalla.Controls.Add(btn_solicitar_llamada);
            pnl_pantalla.Controls.Add(btn_limpiar);
            pnl_pantalla.Controls.Add(btn_cerrar);

            pnl_pantalla.Controls.Add(lbl_seccion_resultado);
            pnl_pantalla.Controls.Add(pnl_resultado);
            pnl_resultado.Controls.Add(txt_resultado);
        }

        private void aplicar_bordes_redondeados()
        {
            TemaVisual.aplicar_borde_redondeado(pnl_contenedor, 34);
            TemaVisual.aplicar_borde_redondeado(pnl_pantalla, 26);
            TemaVisual.aplicar_borde_redondeado(pnl_tarjeta_telefono, 18);
            TemaVisual.aplicar_borde_redondeado(pnl_destino, 18);
            TemaVisual.aplicar_borde_redondeado(pnl_resultado, 16);

            TemaVisual.aplicar_borde_redondeado(btn_solicitar_llamada, 18);
            TemaVisual.aplicar_borde_redondeado(btn_limpiar, 12);
            TemaVisual.aplicar_borde_redondeado(btn_cerrar, 12);
        }

        
        // TEMPORIZADOR DE HORA
        

        private void configurar_temporizador_hora()
        {
            temporizador_hora = new System.Windows.Forms.Timer();
            temporizador_hora.Interval = 1000;
            temporizador_hora.Tick += temporizador_hora_tick;
            temporizador_hora.Start();

            this.FormClosed += frm_solicitar_llamada_form_closed;
        }

        private void temporizador_hora_tick(object? sender, EventArgs e)
        {
            lbl_hora.Text = DateTime.Now.ToString("HH:mm");
        }

        private void frm_solicitar_llamada_form_closed(object? sender, FormClosedEventArgs e)
        {
            if (temporizador_hora != null)
            {
                temporizador_hora.Stop();
                temporizador_hora.Dispose();
            }
        }

       
        // CARGA DE DATOS
        

        private void cargar_telefonos_origen()
        {
            cmb_telefono_origen.DataSource = DatosPrueba.obtener_telefonos_prueba();

            if (cmb_telefono_origen.Items.Count > 0)
            {
                cmb_telefono_origen.SelectedIndex = 0;
                cargar_datos_telefono_origen();
            }
        }

        private void cargar_datos_telefono_origen()
        {
            TelefonoPrueba telefono_prueba;

            if (cmb_telefono_origen.SelectedItem == null)
            {
                return;
            }

            telefono_prueba = (TelefonoPrueba)cmb_telefono_origen.SelectedItem;

            txt_numero_telefono.Text = telefono_prueba.numero_telefono;
            txt_identificador_telefono.Text = telefono_prueba.identificador_telefono;
            txt_identificador_chip.Text = telefono_prueba.identificador_chip;
            txt_coordenadas.Text = telefono_prueba.coordenadas;
            txt_telefono_destino.Text = telefono_prueba.telefono_destino;
            txt_resultado.Clear();
        }

        
        // EVENTOS VISUALES
        

        private void cmb_telefono_origen_selected_index_changed(object? sender, EventArgs e)
        {
            cargar_datos_telefono_origen();
        }

        private void txt_telefono_destino_key_press(object? sender, KeyPressEventArgs e)
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

        private async void btn_solicitar_llamada_click(object? sender, EventArgs e)
        {
            string mensaje_json;
            string respuesta_servidor;

            if (!validar_datos())
            {
                return;
            }

            try
            {
                mensaje_json = construir_json_solicitud_llamada();

                txt_resultado.Text =
                    "Telefono origen: " + txt_numero_telefono.Text.Trim() + Environment.NewLine +
                    "Telefono destino: " + txt_telefono_destino.Text.Trim() + Environment.NewLine +
                    "Tipo: " + ServicioFormato.clasificar_destino(txt_telefono_destino.Text) +
                    Environment.NewLine + Environment.NewLine +
                    "Esperando respuesta del Identificador...";

                respuesta_servidor = await ServicioSocket.enviar_mensaje_async(mensaje_json);

                registrar_llamada_si_respuesta_ok(respuesta_servidor);

                txt_resultado.Text =
                    "Solicitud de llamada" + Environment.NewLine +
                    "Telefono origen: " + txt_numero_telefono.Text.Trim() + Environment.NewLine +
                    "Telefono destino: " + txt_telefono_destino.Text.Trim() + Environment.NewLine +
                    "Tipo: " + ServicioFormato.clasificar_destino(txt_telefono_destino.Text) +
                    Environment.NewLine + Environment.NewLine +
                    ServicioFormato.respuesta_amigable(respuesta_servidor) +
                    Environment.NewLine + Environment.NewLine +
                    "JSON tecnico:" + Environment.NewLine +
                    respuesta_servidor;
            }
            catch (Exception error)
            {
                txt_resultado.Text = "ERROR: " + error.Message;
            }
        }

        private void btn_limpiar_click(object? sender, EventArgs e)
        {
            txt_telefono_destino.Clear();
            txt_resultado.Clear();
            txt_telefono_destino.Focus();
        }

        private void btn_cerrar_click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void registrar_llamada_si_respuesta_ok(string respuesta_servidor)
        {
            JsonDocument documento_json;
            JsonElement raiz_json;
            JsonElement estado_json;
            JsonElement tiempo_json;
            JsonElement monto_json;
            JsonElement tarifa_json;

            string estado_respuesta;
            string tiempo_autorizado;
            string monto_autorizado;
            string tarifa;

            try
            {
                documento_json = JsonDocument.Parse(respuesta_servidor);
                raiz_json = documento_json.RootElement;

                if (!raiz_json.TryGetProperty("status", out estado_json))
                {
                    return;
                }

                estado_respuesta = estado_json.GetString() ?? string.Empty;

                if (estado_respuesta != "OK")
                {
                    return;
                }

                tiempo_autorizado = string.Empty;
                monto_autorizado = string.Empty;
                tarifa = string.Empty;

                if (raiz_json.TryGetProperty("tiempo", out tiempo_json))
                {
                    tiempo_autorizado = tiempo_json.GetString() ?? string.Empty;
                }

                if (raiz_json.TryGetProperty("monto_autorizado", out monto_json))
                {
                    monto_autorizado = monto_json.GetString() ?? string.Empty;
                }

                if (raiz_json.TryGetProperty("tarifa", out tarifa_json))
                {
                    tarifa = tarifa_json.GetString() ?? string.Empty;
                }

                EstadoLlamadaActual.registrar_llamada_pendiente(
                    txt_numero_telefono.Text.Trim(),
                    txt_telefono_destino.Text.Trim(),
                    tiempo_autorizado,
                    monto_autorizado,
                    tarifa,
                    respuesta_servidor,
                    ServicioFormato.clasificar_destino(txt_telefono_destino.Text)
                );
            }
            catch
            {
                // Si la respuesta no viene como JSON válido,
                // no se registra estado interno.
            }
        }

        private bool validar_datos()
        {
            if (cmb_telefono_origen.SelectedItem == null)
            {
                mostrar_advertencia("Debe seleccionar el teléfono origen.");
                return false;
            }

            if (!ServicioValidaciones.es_numero_telefono_valido(txt_numero_telefono.Text))
            {
                mostrar_advertencia("El número origen debe tener 8 dígitos.");
                return false;
            }

            if (!ServicioValidaciones.es_identificador_telefono_valido(txt_identificador_telefono.Text))
            {
                mostrar_advertencia("El identificador del teléfono debe tener 16 dígitos.");
                return false;
            }

            if (!ServicioValidaciones.es_identificador_chip_valido(txt_identificador_chip.Text))
            {
                mostrar_advertencia("El identificador del chip debe tener 19 dígitos.");
                return false;
            }

            if (!ServicioValidaciones.es_coordenada_valida(txt_coordenadas.Text))
            {
                mostrar_advertencia("Las coordenadas deben tener el formato latitud,longitud. Ejemplo: 9.8644,-83.9194");
                return false;
            }

            if (!ServicioValidaciones.es_numero_marcado_valido(txt_telefono_destino.Text) ||
                ServicioValidaciones.es_codigo_saldo(txt_telefono_destino.Text))
            {
                mostrar_advertencia("El telefono destino debe ser nacional de 8 digitos o internacional con 00.");
                return false;
            }

            if (!ServicioValidaciones.es_llave_aes_base64_valida(ConfiguracionSistema.llave_aes_base64))
            {
                mostrar_advertencia("Debe configurar una llave AES válida antes de solicitar una llamada.");
                return false;
            }

            return true;
        }

        private string construir_json_solicitud_llamada()
        {
            SolicitudLlamada solicitud_llamada;
            JsonSerializerOptions opciones_json;
            string mensaje_json;

            solicitud_llamada = new SolicitudLlamada();
            solicitud_llamada.transaccion = "solicitud";

            solicitud_llamada.telefono = ServicioAES.cifrar_texto(
                txt_numero_telefono.Text.Trim(),
                ConfiguracionSistema.llave_aes_base64
            );

            solicitud_llamada.identificador_tel = ServicioAES.cifrar_texto(
                txt_identificador_telefono.Text.Trim(),
                ConfiguracionSistema.llave_aes_base64
            );

            solicitud_llamada.identificador_chip = ServicioAES.cifrar_texto(
                txt_identificador_chip.Text.Trim(),
                ConfiguracionSistema.llave_aes_base64
            );

            solicitud_llamada.coordenadas = txt_coordenadas.Text.Trim();
            solicitud_llamada.telefono_destino = txt_telefono_destino.Text.Trim();

            opciones_json = new JsonSerializerOptions();
            opciones_json.WriteIndented = false;

            mensaje_json = JsonSerializer.Serialize(solicitud_llamada, opciones_json);

            return mensaje_json;
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
