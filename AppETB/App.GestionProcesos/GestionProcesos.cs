using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using App.ControlEjecucion;
using App.ControlInsumos;
using DLL_Utilidades;

namespace App.ControlProcesos
{
    public class GestionProcesos : Variables.Variables, IControl, IDisposable
    {
        private bool _disposed = false;
        private readonly Procesamiento _objProcesamiento = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public GestionProcesos()
        {
            #region GestionProcesos
            //CargueInsumos
            _objProcesamiento = new Procesamiento();
            #endregion
        }

        public void Ejecutar(string pProceso)
        {
            //TODO: Creacion de carpeta de salida Dinamica en base al proceso
            Helpers.RutaProceso = Directory.CreateDirectory($"{Utilidades.LeerAppConfig("RutaSalida")}\\{DateTime.Now:yyyyMMdd}").FullName;
            //TODO: Mover archivos de entrada a original y los la carpeta completa de insumos, guardar en variable y enviarla al constructor de procesamiento y trabajar con los de original para liberar entrada

            //Identificar Proceso para enviar a Procesamiento
            switch (pProceso)
            {
                case "1": //Masivos
                    //Inicio cargue archivos y proceso de formateo
                    _objProcesamiento.EjecutarProcesoMasivo(Helpers.RutaOriginales);

                    break;
            }


            Helpers.EscribirVentanaLog("Final Existoso del Proceso, revise la carpeta salidas !!!");
            Helpers.EscribirVentanaLog("Presione una tecla para cerrar...");
            Console.ReadKey();
            Environment.Exit(1);
        }

        /// <summary>
        /// Metodo para pintar el encabezado al abrir la aplicación
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Inicio()
        {
            #region Inicio
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem?.GetName();
            Version ver = assemName?.Version;

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()?.Location ?? throw new InvalidOperationException());

            string companyName = versionInfo.CompanyName;

            string version = ver?.ToString();

            Console.WriteLine("");
            Console.WriteLine(" **************");
            Console.WriteLine(" *    *****   *");
            Console.WriteLine(" *   **   **  *      " + "Procesador ETB");
            Console.WriteLine(" *  **        *      " + "Nombre:\tETB");
            Console.WriteLine(" *  **        *      " + "Version:\t" + version);
            Console.WriteLine(" *   **   **  *      " + "Compañia:\t" + companyName);
            Console.WriteLine(" *    *****   *");
            Console.WriteLine(" **************");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.Title = "Procesos ETB " + version;
            #endregion
        }

        /// <summary>
        /// Metodo para mostrar el menu de la aplicación
        /// </summary>
        public void Menu()
        {
            #region Menu
            Inicio();
            Console.WriteLine("Seleccione el proceso que desea ejecutar:");
            Console.WriteLine("");
            Console.WriteLine("1. Masivos");
            Console.WriteLine("");
            Proceso = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine("");

            switch (Proceso)
            {
                case "1":
                    Ejecutar("1");
                    break;

                case "2":

                    break;

                default:
                    Console.WriteLine("Ingrese el numero de proceso valido");
                    System.Threading.Thread.Sleep(1500);
                    Console.Clear();
                    Menu();
                    break;
            }
            #endregion
        }

        /// <summary>
        /// Metodo para liberar Memoria
        /// </summary>
        public void Dispose()
        {
            #region Dispose
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
            #endregion
        }

        // Protected implementation of Dispose pattern.
        /// <summary>
        /// Metodo para liberar Memoria
        /// </summary>
        /// <param name="disposing">Bandera para limpiar variables</param>
        protected virtual void Dispose(bool disposing)
        {
            #region Dispose
            if (_disposed)
                return;

            if (disposing)
            {
                //Free OBJECTS

            }

            // Free any unmanaged objects here.
            _disposed = true;
            #endregion
        }
    }
}
