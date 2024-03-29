﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using App.ControlEjecucion;
using App.ControlInsumos;
using DLL_Utilidades;

namespace App.ControlProcesos
{
    /// <summary>
    /// Clase GestionProcesos
    /// </summary>
    public class GestionProcesos : IControl, IDisposable
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

        /// <summary>
        /// Metodo Ejecutar
        /// </summary>
        /// <param name="pProceso"></param>
        public void Ejecutar(string pProceso)
        {
            #region Ejecutar
            try
            {
                //Identificar Proceso para enviar a Procesamiento
                switch (pProceso)
                {
                    case "1": //Masivos
                              //Se crea carpeta salida se mueven datos de entrada e insumos a originales
                        _objProcesamiento.AdecuarTrabajoApp("Masivos");
                        //Inicio cargue archivos y proceso de formateo
                        _objProcesamiento.EjecutarProcesoMasivo(Helpers.RutaOriginales);
                        break;

                    case "2": //Datos
                              //Se crea carpeta salida se mueven datos de entrada e insumos a originales
                        _objProcesamiento.AdecuarTrabajoApp("Datos");
                        //Inicio cargue archivos y proceso de formateo
                        _objProcesamiento.EjecutarProcesoDatos(Helpers.RutaOriginales);
                        break;

                    case "3": //Gobiernos
                              //Se crea carpeta salida se mueven datos de entrada e insumos a originales
                        _objProcesamiento.AdecuarTrabajoApp("Gobiernos");
                        //Inicio cargue archivos y proceso de formateo
                        _objProcesamiento.EjecutarProcesoGobiernos(Helpers.RutaOriginales);
                        break;

                    case "4": //LLanos
                              //Se crea carpeta salida se mueven datos de entrada e insumos a originales
                        _objProcesamiento.AdecuarTrabajoApp("LLanos");
                        //Inicio cargue archivos y proceso de formateo
                        _objProcesamiento.EjecutarProcesoLLanos(Helpers.RutaOriginales);
                        break;

                    case "5": //Hipotecario
                              //Se crea carpeta salida se mueven datos de entrada e insumos a originales
                        _objProcesamiento.AdecuarTrabajoApp("Hipotecario");
                        //Inicio cargue archivos y proceso de formateo
                        _objProcesamiento.EjecutarProcesoCreditoHipotecario(Helpers.RutaOriginales);
                        break;

                    case "6": //Anexos Verdes
                              //Se crea carpeta salida se mueven datos de entrada e insumos a originales
                        _objProcesamiento.AdecuarTrabajoApp("AnexosVerdes");
                        //Inicio cargue archivos y proceso de formateo
                        _objProcesamiento.EjecutarProcesoAnexosVerdes(Helpers.RutaOriginales);
                        break;
                    case "7": //Anexos Verdes
                              //Se crea carpeta salida se mueven datos de entrada e insumos a originales
                        _objProcesamiento.AdecuarTrabajoApp("LteCorp");
                        //Inicio cargue archivos y proceso de formateo
                        _objProcesamiento.EjecutarProcesoLteCorp(Helpers.RutaOriginales);
                        break;
                }

                Helpers.EscribirVentanaLog("Final Existoso del Proceso, revise la carpeta salidas !!!");
                Helpers.EscribirVentanaLog($"Ruta Salidas: {Helpers.RutaProceso}");

            }
            catch (Exception ex)
            {
                DatosError StructError = Helpers.ExtraerExcepcion(ex);

                Helpers.EscribirLogVentana(StructError, true);
            }
            finally
            {
                Helpers.EscribirVentanaLog("Presione una tecla para cerrar...");
            }

            Console.ReadKey();
            Environment.Exit(1);
            #endregion
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
            Console.WriteLine("2. Datos");
            Console.WriteLine("");
            Console.WriteLine("3. Gobiernos");
            Console.WriteLine("");
            Console.WriteLine("4. Llanos");
            Console.WriteLine("");
            Console.WriteLine("5. Credito Hipotecario");
            Console.WriteLine("");
            Console.WriteLine("6. Anexos Verdes");
            Console.WriteLine("");
            Console.WriteLine("7. LTE Corporativo");
            Console.WriteLine("");
            Variables.Variables.Proceso = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine("");

            switch (Variables.Variables.Proceso)
            {
                case "1":
                    Helpers.EscribirVentanaLog("Proceso Seleccionado 1. Masivos");
                    Helpers.EscribirVentanaLog(" ");
                    Helpers.EscribirVentanaLog("Inicio Proceso Masivos");
                    Ejecutar("1");
                    break;

                case "2":
                    Helpers.EscribirVentanaLog("Proceso Seleccionado 2. Datos");
                    Helpers.EscribirVentanaLog(" ");
                    Helpers.EscribirVentanaLog("Inicio Proceso Datos");
                    Ejecutar("2");
                    break;

                case "3":
                    Helpers.EscribirVentanaLog("Proceso Seleccionado 3. Gobiernos");
                    Helpers.EscribirVentanaLog(" ");
                    Helpers.EscribirVentanaLog("Inicio Proceso Gobiernos");
                    Ejecutar("3");
                    break;

                case "4":
                    Helpers.EscribirVentanaLog("Proceso Seleccionado 4. Llanos");
                    Helpers.EscribirVentanaLog(" ");
                    Helpers.EscribirVentanaLog("Inicio Proceso Llanos");
                    Ejecutar("4");
                    break;

                case "5":
                    Helpers.EscribirVentanaLog("Proceso Seleccionado 5. Credito Hipotecario");
                    Helpers.EscribirVentanaLog(" ");
                    Helpers.EscribirVentanaLog("Inicio Proceso Credito Hipotecario");
                    Ejecutar("5");
                    break;

                case "6":
                    Helpers.EscribirVentanaLog("Proceso Seleccionado 6. Anexos Verdes");
                    Helpers.EscribirVentanaLog(" ");
                    Helpers.EscribirVentanaLog("Inicio Proceso Anexos Verdes");
                    Ejecutar("6");
                    break;

                case "7":
                    Helpers.EscribirVentanaLog("Proceso Seleccionado 7. LTE Corporativo");
                    Helpers.EscribirVentanaLog(" ");
                    Helpers.EscribirVentanaLog("Inicio LTE Corporativo");
                    Ejecutar("7");
                    break;

                default:
                    Helpers.EscribirVentanaLog($"Ingrese el numero de proceso valido, Proceso Ingresado {Variables.Variables.Proceso}");
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
