using System;
using System.Runtime.InteropServices;
using App.ControlInsumos;
using App.ControlProcesos;

namespace AppETB
{
    /// <summary>
    /// Clase Program
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            #region Main
            using (GestionProcesos objProcesos = new GestionProcesos())
            {
                Helpers.EscribirLogUsuario(Environment.UserName);
                objProcesos.Menu();
            }
            #endregion
        }
       
    }
}
