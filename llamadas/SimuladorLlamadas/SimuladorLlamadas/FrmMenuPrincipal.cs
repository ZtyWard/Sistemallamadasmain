using System;
using System.Drawing;
using System.Windows.Forms;
using SimuladorLlamadas.Configuracion;
using SimuladorLlamadas.Estilos;
using SimuladorLlamadas.Modelos;
using SimuladorLlamadas.Servicios;

namespace SimuladorLlamadas
{
    public partial class FrmMenuPrincipal : Form
    {
        private Panel pnl_contenedor = null!;
        private Panel pnl_pantalla = null!;
        private Panel pnl_barra_estado = null!;
        private Panel pnl_tarjeta_telefono = null!;
        private Panel pnl_tarjeta_conexion = null!;
        private Panel pnl_tarjeta_llamada = null!;

        private Label lbl_hora = null!;
        private Label lbl_senal = null!;
        private Label lbl_titulo = null!;
        private Label lbl_subtitulo = null!;
        private Label lbl_telefono_info = null!;
        private Label lbl_cliente_info = null!;
        private Label lbl_proveedor_info = null!;
        private Label lbl_estado_info = null!;
        private Label lbl_conexion = null!;
        private Label lbl_llamada_actual = null!;

        private ComboBox cmb_telefono_origen = null!;

        private Button btn_probar_conexion = null!;
        private Button btn_marcar_numero = null!;
        private Button btn_solicitar_llamada = null!;
        private Button btn_iniciar_llamada = null!;
        private Button btn_finalizar_llamada = null!;
        private Button btn_consultar_saldo = null!;
        private Button btn_configuracion = null!;
        private Button btn_salir = null!;

        private System.Windows.Forms.Timer temporizador_hora = null!;
        private System.Windows.Forms.Timer temporizador_estado = null!;

        public FrmMenuPrincipal()
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
            configurar_temporizadores();
            _ = probar_conexion_async();
        }

        private void configurar_formulario()
        {
            this.ClientSize = new Size(430, 900);
            TemaVisual.aplicar_estilo_formulario_celular(this, "Simulador de Llamadas Telefonicas");
        }

        private void crear_controles()
        {
            pnl_contenedor = new Panel { Location = new Point(40, 20), Size = new Size(350, 850) };
            pnl_pantalla = new Panel { Location = new Point(14, 14), Size = new Size(322, 822) };
            pnl_barra_estado = new Panel { Location = new Point(0, 0), Size = new Size(322, 38) };

            lbl_hora = new Label
            {
                Text = DateTime.Now.ToString("HH:mm"),
                Location = new Point(18, 8),
                Size = new Size(80, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lbl_senal = new Label
            {
                Text = "LTE   100%",
                Location = new Point(190, 8),
                Size = new Size(110, 22),
                TextAlign = ContentAlignment.MiddleRight
            };

            lbl_titulo = new Label
            {
                Text = "Phone Simulator",
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 48),
                Size = new Size(282, 34)
            };

            lbl_subtitulo = new Label
            {
                Text = "Flujo completo de llamadas",
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 80),
                Size = new Size(282, 24)
            };

            pnl_tarjeta_telefono = new Panel { Location = new Point(22, 116), Size = new Size(278, 160) };
            cmb_telefono_origen = new ComboBox { Location = new Point(14, 14), Size = new Size(250, 25) };
            cmb_telefono_origen.SelectedIndexChanged += cmb_telefono_origen_selected_index_changed;

            lbl_telefono_info = crear_label_info(14, 48);
            lbl_cliente_info = crear_label_info(14, 72);
            lbl_proveedor_info = crear_label_info(14, 96);
            lbl_estado_info = crear_label_info(14, 120);

            pnl_tarjeta_conexion = new Panel { Location = new Point(22, 292), Size = new Size(278, 92) };
            lbl_conexion = new Label
            {
                Text = "Probando conexion...",
                Location = new Point(14, 12),
                Size = new Size(250, 42),
                TextAlign = ContentAlignment.MiddleCenter
            };

            btn_probar_conexion = new Button
            {
                Text = "PROBAR CONEXION",
                Location = new Point(32, 58),
                Size = new Size(214, 28)
            };
            btn_probar_conexion.Click += async (_, _) => await probar_conexion_async();

            pnl_tarjeta_llamada = new Panel { Location = new Point(22, 400), Size = new Size(278, 88) };
            lbl_llamada_actual = new Label
            {
                Text = "Llamadas activas:\r\nNo hay llamada activa.",
                Location = new Point(14, 10),
                Size = new Size(250, 70),
                TextAlign = ContentAlignment.MiddleCenter
            };

            btn_marcar_numero = crear_boton_menu("MARCAR NUMERO", 510);
            btn_marcar_numero.Click += btn_marcar_numero_click;

            btn_consultar_saldo = crear_boton_menu("CONSULTAR SALDO", 560);
            btn_consultar_saldo.Click += btn_consultar_saldo_click;

            btn_solicitar_llamada = crear_boton_menu("SOLICITAR LLAMADA", 610);
            btn_solicitar_llamada.Click += btn_solicitar_llamada_click;

            btn_iniciar_llamada = crear_boton_menu("INICIAR LLAMADA", 660);
            btn_iniciar_llamada.Click += btn_iniciar_llamada_click;

            btn_finalizar_llamada = crear_boton_menu("FINALIZAR LLAMADA", 710);
            btn_finalizar_llamada.Click += btn_finalizar_llamada_click;

            btn_configuracion = crear_boton_menu("CONFIGURACION", 760);
            btn_configuracion.Click += btn_configuracion_click;

            btn_salir = new Button
            {
                Text = "SALIR",
                Location = new Point(205, 760),
                Size = new Size(65, 36)
            };
            btn_salir.Click += btn_salir_click;
        }

        private Label crear_label_info(int x, int y)
        {
            return new Label
            {
                Location = new Point(x, y),
                Size = new Size(250, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private Button crear_boton_menu(string texto, int posicion_y)
        {
            return new Button
            {
                Text = texto,
                Size = new Size(270, 40),
                Location = new Point(26, posicion_y)
            };
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
            TemaVisual.aplicar_estilo_tarjeta_celular(pnl_tarjeta_conexion);
            TemaVisual.aplicar_estilo_tarjeta_celular(pnl_tarjeta_llamada);
            TemaVisual.aplicar_estilo_combo(cmb_telefono_origen);

            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_telefono_info);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_cliente_info);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_proveedor_info);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_estado_info);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_conexion);
            TemaVisual.aplicar_estilo_subtitulo_celular(lbl_llamada_actual);

            TemaVisual.aplicar_estilo_boton_app(btn_probar_conexion);
            TemaVisual.aplicar_estilo_boton_llamar(btn_marcar_numero);
            TemaVisual.aplicar_estilo_boton_app(btn_consultar_saldo);
            TemaVisual.aplicar_estilo_boton_app(btn_solicitar_llamada);
            TemaVisual.aplicar_estilo_boton_app(btn_iniciar_llamada);
            TemaVisual.aplicar_estilo_boton_finalizar(btn_finalizar_llamada);
            TemaVisual.aplicar_estilo_boton_app(btn_configuracion);
            TemaVisual.aplicar_estilo_boton_app(btn_salir);
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
            pnl_tarjeta_telefono.Controls.Add(cmb_telefono_origen);
            pnl_tarjeta_telefono.Controls.Add(lbl_telefono_info);
            pnl_tarjeta_telefono.Controls.Add(lbl_cliente_info);
            pnl_tarjeta_telefono.Controls.Add(lbl_proveedor_info);
            pnl_tarjeta_telefono.Controls.Add(lbl_estado_info);

            pnl_pantalla.Controls.Add(pnl_tarjeta_conexion);
            pnl_tarjeta_conexion.Controls.Add(lbl_conexion);
            pnl_tarjeta_conexion.Controls.Add(btn_probar_conexion);

            pnl_pantalla.Controls.Add(pnl_tarjeta_llamada);
            pnl_tarjeta_llamada.Controls.Add(lbl_llamada_actual);

            pnl_pantalla.Controls.Add(btn_marcar_numero);
            pnl_pantalla.Controls.Add(btn_consultar_saldo);
            pnl_pantalla.Controls.Add(btn_solicitar_llamada);
            pnl_pantalla.Controls.Add(btn_iniciar_llamada);
            pnl_pantalla.Controls.Add(btn_finalizar_llamada);
            pnl_pantalla.Controls.Add(btn_configuracion);
            pnl_pantalla.Controls.Add(btn_salir);
        }

        private void aplicar_bordes_redondeados()
        {
            TemaVisual.aplicar_borde_redondeado(pnl_contenedor, 34);
            TemaVisual.aplicar_borde_redondeado(pnl_pantalla, 26);
            TemaVisual.aplicar_borde_redondeado(pnl_tarjeta_telefono, 18);
            TemaVisual.aplicar_borde_redondeado(pnl_tarjeta_conexion, 18);
            TemaVisual.aplicar_borde_redondeado(pnl_tarjeta_llamada, 18);
            TemaVisual.aplicar_borde_redondeado(btn_probar_conexion, 12);
            TemaVisual.aplicar_borde_redondeado(btn_marcar_numero, 16);
            TemaVisual.aplicar_borde_redondeado(btn_consultar_saldo, 16);
            TemaVisual.aplicar_borde_redondeado(btn_solicitar_llamada, 16);
            TemaVisual.aplicar_borde_redondeado(btn_iniciar_llamada, 16);
            TemaVisual.aplicar_borde_redondeado(btn_finalizar_llamada, 16);
            TemaVisual.aplicar_borde_redondeado(btn_configuracion, 16);
            TemaVisual.aplicar_borde_redondeado(btn_salir, 12);
        }

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
            if (cmb_telefono_origen.SelectedItem is not TelefonoPrueba telefono)
            {
                return;
            }

            lbl_telefono_info.Text = "Telefono origen: " + telefono.numero_telefono;
            lbl_cliente_info.Text = "Cliente: " + telefono.cliente;
            lbl_proveedor_info.Text = "Proveedor: " + telefono.proveedor;
            lbl_estado_info.Text = "Estado: " + telefono.estado + " | " + telefono.tipo_servicio;
        }

        private void configurar_temporizadores()
        {
            temporizador_hora = new System.Windows.Forms.Timer { Interval = 1000 };
            temporizador_hora.Tick += (_, _) => lbl_hora.Text = DateTime.Now.ToString("HH:mm");
            temporizador_hora.Start();

            temporizador_estado = new System.Windows.Forms.Timer { Interval = 1000 };
            temporizador_estado.Tick += (_, _) => actualizar_llamada_actual();
            temporizador_estado.Start();

            this.FormClosed += frm_menu_principal_form_closed;
        }

        private async System.Threading.Tasks.Task probar_conexion_async()
        {
            lbl_conexion.Text = "Probando conexion...";
            bool conectado = await ServicioConexion.probar_identificador_async();

            if (conectado)
            {
                lbl_conexion.Text = "Conectado al identificador:\r\n" +
                    ConfiguracionSistema.ip_identificador + ":" +
                    ConfiguracionSistema.puerto_identificador;
            }
            else
            {
                lbl_conexion.Text = "No se pudo conectar\r\ncon el identificador.";
            }
        }

        private void actualizar_llamada_actual()
        {
            lbl_llamada_actual.Text = "Llamadas activas:\r\n" + EstadoLlamadaActual.resumen_llamada();
        }

        private void frm_menu_principal_form_closed(object? sender, FormClosedEventArgs e)
        {
            temporizador_hora?.Stop();
            temporizador_hora?.Dispose();
            temporizador_estado?.Stop();
            temporizador_estado?.Dispose();
        }

        private void cmb_telefono_origen_selected_index_changed(object? sender, EventArgs e)
        {
            cargar_datos_telefono_origen();
        }

        private void btn_solicitar_llamada_click(object? sender, EventArgs e)
        {
            new FrmSolicitarLlamada().ShowDialog();
            actualizar_llamada_actual();
        }

        private void btn_iniciar_llamada_click(object? sender, EventArgs e)
        {
            new FrmIniciarLlamada().ShowDialog();
            actualizar_llamada_actual();
        }

        private void btn_finalizar_llamada_click(object? sender, EventArgs e)
        {
            new FrmFinalizarLlamada().ShowDialog();
            actualizar_llamada_actual();
        }

        private void btn_consultar_saldo_click(object? sender, EventArgs e)
        {
            new FrmConsultarSaldo().ShowDialog();
        }

        private void btn_configuracion_click(object? sender, EventArgs e)
        {
            new FrmConfiguracion().ShowDialog();
            _ = probar_conexion_async();
        }

        private void btn_salir_click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_marcar_numero_click(object? sender, EventArgs e)
        {
            new FrmMarcarNumero().ShowDialog();
            actualizar_llamada_actual();
        }
    }
}
