using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SimuladorLlamadas.Estilos
{
    public static class TemaVisual
    {
        
        // COLORES GENERALES DEL SISTEMA
     

        public static readonly Color color_fondo = Color.FromArgb(229, 232, 236);
        public static readonly Color color_panel = Color.FromArgb(248, 250, 252);
        public static readonly Color color_primario = Color.FromArgb(15, 23, 42);
        public static readonly Color color_boton_principal = Color.FromArgb(37, 99, 235);
        public static readonly Color color_boton_secundario = Color.FromArgb(71, 85, 105);
        public static readonly Color color_texto = Color.FromArgb(30, 41, 59);
        public static readonly Color color_borde = Color.FromArgb(203, 213, 225);

        
        // COLORES TIPO CELULAR
        

        public static readonly Color color_telefono_marco = Color.FromArgb(10, 15, 25);
        public static readonly Color color_telefono_pantalla = Color.FromArgb(15, 23, 42);
        public static readonly Color color_telefono_panel = Color.FromArgb(30, 41, 59);
        public static readonly Color color_telefono_tarjeta = Color.FromArgb(51, 65, 85);

        public static readonly Color color_verde_llamada = Color.FromArgb(34, 197, 94);
        public static readonly Color color_rojo_finalizar = Color.FromArgb(239, 68, 68);
        public static readonly Color color_azul_accion = Color.FromArgb(59, 130, 246);
        public static readonly Color color_gris_tecla = Color.FromArgb(71, 85, 105);

        public static readonly Color color_texto_claro = Color.White;
        public static readonly Color color_texto_suave = Color.FromArgb(203, 213, 225);

        
        // FUENTES
        

        public static readonly Font fuente_titulo = new Font("Segoe UI", 20, FontStyle.Bold);
        public static readonly Font fuente_subtitulo = new Font("Segoe UI", 10, FontStyle.Regular);
        public static readonly Font fuente_normal = new Font("Segoe UI", 10, FontStyle.Regular);
        public static readonly Font fuente_boton = new Font("Segoe UI", 10, FontStyle.Bold);

        public static readonly Font fuente_titulo_celular = new Font("Segoe UI", 17, FontStyle.Bold);
        public static readonly Font fuente_subtitulo_celular = new Font("Segoe UI", 9, FontStyle.Regular);
        public static readonly Font fuente_boton_celular = new Font("Segoe UI", 11, FontStyle.Bold);
        public static readonly Font fuente_numero_grande = new Font("Segoe UI", 24, FontStyle.Bold);
        public static readonly Font fuente_teclado = new Font("Segoe UI", 18, FontStyle.Bold);
        public static readonly Font fuente_resultado = new Font("Consolas", 9, FontStyle.Regular);

        
        // ESTILOS GENERALES EXISTENTES
        

        public static void aplicar_estilo_formulario(Form formulario)
        {
            formulario.BackColor = color_fondo;
            formulario.Font = fuente_normal;
        }

        public static void aplicar_estilo_panel(Panel panel)
        {
            panel.BackColor = color_panel;
            panel.BorderStyle = BorderStyle.FixedSingle;
        }

        public static void aplicar_estilo_titulo(Label etiqueta)
        {
            etiqueta.ForeColor = color_primario;
            etiqueta.Font = fuente_titulo;
        }

        public static void aplicar_estilo_subtitulo(Label etiqueta)
        {
            etiqueta.ForeColor = color_texto;
            etiqueta.Font = fuente_subtitulo;
        }

        public static void aplicar_estilo_boton_principal(Button boton)
        {
            boton.BackColor = color_boton_principal;
            boton.ForeColor = Color.White;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = fuente_boton;
            boton.Cursor = Cursors.Hand;
        }

        public static void aplicar_estilo_boton_secundario(Button boton)
        {
            boton.BackColor = color_boton_secundario;
            boton.ForeColor = Color.White;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = fuente_boton;
            boton.Cursor = Cursors.Hand;
        }

        public static void aplicar_estilo_etiqueta(Label etiqueta)
        {
            etiqueta.ForeColor = color_texto;
            etiqueta.Font = fuente_normal;
        }

        public static void aplicar_estilo_caja_texto(TextBox caja_texto)
        {
            caja_texto.BackColor = Color.White;
            caja_texto.ForeColor = color_texto;
            caja_texto.BorderStyle = BorderStyle.FixedSingle;
            caja_texto.Font = fuente_normal;
        }

        public static void aplicar_estilo_resultado(TextBox caja_texto)
        {
            caja_texto.BackColor = Color.White;
            caja_texto.ForeColor = color_texto;
            caja_texto.BorderStyle = BorderStyle.FixedSingle;
            caja_texto.Font = fuente_resultado;
        }

        public static void aplicar_estilo_combo(ComboBox combo)
        {
            combo.BackColor = Color.White;
            combo.ForeColor = color_texto;
            combo.Font = fuente_normal;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
        }

      
        // ESTILOS TIPO CELULAR
    

        public static void aplicar_estilo_formulario_celular(Form formulario, string titulo)
        {
            formulario.Text = titulo;
            formulario.StartPosition = FormStartPosition.CenterScreen;
            formulario.BackColor = color_fondo;
            formulario.Font = fuente_normal;
            formulario.FormBorderStyle = FormBorderStyle.FixedSingle;
            formulario.MaximizeBox = false;
        }

        public static void aplicar_estilo_marco_telefono(Panel panel)
        {
            panel.BackColor = color_telefono_marco;
            panel.BorderStyle = BorderStyle.None;
        }

        public static void aplicar_estilo_pantalla_telefono(Panel panel)
        {
            panel.BackColor = color_telefono_pantalla;
            panel.BorderStyle = BorderStyle.None;
        }

        public static void aplicar_estilo_panel_celular(Panel panel)
        {
            panel.BackColor = color_telefono_panel;
            panel.BorderStyle = BorderStyle.None;
        }

        public static void aplicar_estilo_tarjeta_celular(Panel panel)
        {
            panel.BackColor = color_telefono_tarjeta;
            panel.BorderStyle = BorderStyle.None;
        }

        public static void aplicar_estilo_titulo_celular(Label etiqueta)
        {
            etiqueta.ForeColor = color_texto_claro;
            etiqueta.Font = fuente_titulo_celular;
        }

        public static void aplicar_estilo_subtitulo_celular(Label etiqueta)
        {
            etiqueta.ForeColor = color_texto_suave;
            etiqueta.Font = fuente_subtitulo_celular;
        }

        public static void aplicar_estilo_texto_celular(Label etiqueta)
        {
            etiqueta.ForeColor = color_texto_claro;
            etiqueta.Font = fuente_normal;
        }

        public static void aplicar_estilo_boton_app(Button boton)
        {
            boton.BackColor = color_telefono_panel;
            boton.ForeColor = color_texto_claro;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = fuente_boton_celular;
            boton.Cursor = Cursors.Hand;
            boton.TextAlign = ContentAlignment.MiddleCenter;
        }

        public static void aplicar_estilo_boton_llamar(Button boton)
        {
            boton.BackColor = color_verde_llamada;
            boton.ForeColor = Color.White;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = fuente_boton_celular;
            boton.Cursor = Cursors.Hand;
        }

        public static void aplicar_estilo_boton_finalizar(Button boton)
        {
            boton.BackColor = color_rojo_finalizar;
            boton.ForeColor = Color.White;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = fuente_boton_celular;
            boton.Cursor = Cursors.Hand;
        }

        public static void aplicar_estilo_boton_teclado(Button boton)
        {
            boton.BackColor = color_gris_tecla;
            boton.ForeColor = Color.White;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = fuente_teclado;
            boton.Cursor = Cursors.Hand;
        }

        public static void aplicar_estilo_caja_numero(TextBox caja_texto)
        {
            caja_texto.BackColor = color_telefono_pantalla;
            caja_texto.ForeColor = color_texto_claro;
            caja_texto.BorderStyle = BorderStyle.None;
            caja_texto.Font = fuente_numero_grande;
            caja_texto.TextAlign = HorizontalAlignment.Center;
        }

        public static void aplicar_estilo_resultado_celular(TextBox caja_texto)
        {
            caja_texto.BackColor = Color.FromArgb(15, 23, 42);
            caja_texto.ForeColor = color_texto_suave;
            caja_texto.BorderStyle = BorderStyle.None;
            caja_texto.Font = fuente_resultado;
        }

        
        // UTILIDAD PARA BORDES REDONDEADOS
        

        public static void aplicar_borde_redondeado(Control control, int radio)
        {
            Rectangle area;
            GraphicsPath ruta;

            area = new Rectangle(0, 0, control.Width, control.Height);
            ruta = obtener_ruta_redondeada(area, radio);

            control.Region = new Region(ruta);
        }

        private static GraphicsPath obtener_ruta_redondeada(Rectangle area, int radio)
        {
            GraphicsPath ruta;
            int diametro;

            ruta = new GraphicsPath();
            diametro = radio * 2;

            ruta.AddArc(area.X, area.Y, diametro, diametro, 180, 90);
            ruta.AddArc(area.Right - diametro, area.Y, diametro, diametro, 270, 90);
            ruta.AddArc(area.Right - diametro, area.Bottom - diametro, diametro, diametro, 0, 90);
            ruta.AddArc(area.X, area.Bottom - diametro, diametro, diametro, 90, 90);
            ruta.CloseFigure();

            return ruta;
        }
    }
}