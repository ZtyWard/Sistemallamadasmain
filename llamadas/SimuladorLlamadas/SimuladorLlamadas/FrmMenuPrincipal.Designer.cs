namespace SimuladorLlamadas
{
    partial class FrmMenuPrincipal
    {
        /// <summary>
        ///  Variable requerida por el diseñador.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Limpia los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si se deben liberar los recursos administrados; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        ///  Método requerido por el diseñador.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Text = "Simulador de Llamadas Telefónicas";
        }

        #endregion
    }
}