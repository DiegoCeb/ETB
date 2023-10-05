using DLL_Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.ControlLogicaProcesos;
using App.ControlInsumos;
using System.Threading;

namespace App.ControlEjecucion
{
    /// <summary>
    /// Clase Procesamiento
    /// </summary>
    public class Procesamiento
    {
        /// <summary>
        /// constructor Procesamiento
        /// </summary>
        public Procesamiento()
        { }

        /// <summary>
        /// Metodo para adecuar el ambiente de trabajo segun proceso
        /// </summary>
        /// <param name="pIdentificadorProceso">Identificador del Proceso</param>
        public void AdecuarTrabajoApp(string pIdentificadorProceso)
        {
            #region AdecuarTrabajoApp
            //Creacion Carpeta Salida
            Helpers.RutaProceso = Directory.CreateDirectory($"{Utilidades.LeerAppConfig("RutaSalida")}\\{pIdentificadorProceso}{DateTime.Now:yyyyMMddhhmmss}").FullName;
            Helpers.EscribirVentanaLog($"Creación Ruta de Salida: {Helpers.RutaProceso}");

            //Mover de entrada a originales y trabajar con estos archivos
            Helpers.EscribirVentanaLog("Mover Archivos de entrada a originales");
            Helpers.RutaOriginales = Directory.CreateDirectory($"{Utilidades.LeerAppConfig("RutaOriginales")}\\{pIdentificadorProceso}Originales{DateTime.Now:yyyyMMddhhmmss}").FullName;
            Helpers.MoverArchivos(Utilidades.LeerAppConfig("RutaEntrada"), Helpers.RutaOriginales);

            switch (pIdentificadorProceso)
            {
                case "Hipotecario":
                    break;

                default:
                    //Mover archivos de entrada a original y la carpeta completa de insumos
                    Helpers.EscribirVentanaLog("Inicia Carga de Insumos");
                    Helpers.RutaInsumos = $"{Helpers.RutaOriginales}\\Insumos";
                    Helpers.CopiarCarpetaCompleta(Utilidades.LeerAppConfig("RutaInsumos"), Helpers.RutaInsumos, true);

                    //Cargue Lista Insumos para despues usar en el formateo desde originales
                    CargueGeneralInsumos(Helpers.RutaInsumos);
                    Helpers.EscribirVentanaLog("Final Carga de Insumos");
                    Helpers.EscribirVentanaLog(" ");
                    break;
            }


            #endregion
        }

        /// <summary>
        /// Metodo para ejecutar el proceso Masivo
        /// </summary>
        /// <param name="pRutaArchivosProcesar">Ruta donde se encuentran los archivos originales</param>
        public void EjecutarProcesoMasivo(string pRutaArchivosProcesar)
        {
            #region EjecutarProcesoMasivo
            var archivos = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                           where !busqueda.Contains("CONTEO") && !busqueda.Contains("IDENTIFICACION") && !busqueda.ToLower().Contains("premaestra") //TODO: se omiten estos archivos peor ahi que revisar para que sirven
                           select busqueda;

            var archivoPeriodo = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                                 where busqueda.Contains("IDENTIFICACION")
                                 select busqueda;

            string periodo = File.ReadAllLines(archivoPeriodo.FirstOrDefault()).ToList().ElementAt(0).Split('\t').ElementAt(1).PadLeft(6, '0');
            string lote = File.ReadAllLines(archivoPeriodo.FirstOrDefault()).ToList().ElementAt(0).Split('\t').ElementAt(5);

            Helpers.EscribirVentanaLog($"Archivos Cargados Para Proceso: {archivos.Count().ToString()}");
            Helpers.EscribirVentanaLog(" ");

            Helpers.EscribirVentanaLog($"Inicia Formateo de Archivos");
            Helpers.EscribirVentanaMismaLinea($"Formateando Archivos: ");
            foreach (var archivo in archivos)
            {
                _ = new ProcesoMasivos(archivo, periodo);
                Helpers.EscribirVentanaMismaLinea($"X", false);
            }
            Helpers.EscribirVentanaMismaLinea(System.Environment.NewLine, false);

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            Helpers.EscribirVentanaLog($"Inicia Escritura de Salidas");
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "1", lote);

            // Se crean los reportes
            Helpers.EscribirVentanaLog($"Inicia Proceso Reportes");
            _ = new ReportesMasivos(Variables.Variables.DiccionarioExtractosFormateados, App.ControlInsumos.Helpers.RutaProceso, lote);

            #endregion
        }

        /// <summary>
        /// Metodo para ejecutar el proceso Datos
        /// </summary>
        /// <param name="pRutaArchivosProcesar">Ruta donde se encuentran los archivos originales</param>
        public void EjecutarProcesoDatos(string pRutaArchivosProcesar)
        {
            #region EjecutarProcesoDatos

            var archivos = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                           where !busqueda.Contains("CONTEO") && !busqueda.Contains("IDENTIFICACION") && !busqueda.ToLower().Contains("premaestra") //TODO: se omiten estos archivos peor ahi que revisar para que sirven
                           select busqueda;

            var archivoPeriodo = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                                 where busqueda.Contains("IDENTIFICACION")
                                 select busqueda;

            string lote = File.ReadAllLines(archivoPeriodo.FirstOrDefault()).ToList().ElementAt(0).Split('\t').ElementAt(5);

            Helpers.EscribirVentanaLog($"Archivos Cargados Para Proceso: {archivos.Count().ToString()}");
            Helpers.EscribirVentanaLog(" ");

            Helpers.EscribirVentanaLog($"Inicia Formateo de Archivos");
            Helpers.EscribirVentanaMismaLinea($"Formateando Archivos: ");
            foreach (var archivo in archivos)
            {
                _ = new ProcesoDatos(archivo);
                Helpers.EscribirVentanaMismaLinea($"X", false);
            }
            Helpers.EscribirVentanaMismaLinea(System.Environment.NewLine, false);

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            Helpers.EscribirVentanaLog($"Inicia Escritura de Salidas");
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "2", lote);

            // Se crean los reportes
            Helpers.EscribirVentanaLog($"Inicia Proceso Reportes");
            _ = new ReportesDatos(Variables.Variables.DiccionarioExtractosFormateados, App.ControlInsumos.Helpers.RutaProceso, lote);

            #endregion
        }

        /// <summary>
        /// Metodo para ejecutar el proceso Gobierno
        /// </summary>
        /// <param name="pRutaArchivosProcesar">Ruta donde se encuentran los archivos originales</param>
        public void EjecutarProcesoGobiernos(string pRutaArchivosProcesar)
        {
            #region EjecutarProcesoGobiernos

            var archivos = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                           where !busqueda.Contains("CONTEO") && !busqueda.Contains("IDENTIFICACION") && !busqueda.ToLower().Contains("premaestra") //TODO: se omiten estos archivos peor ahi que revisar para que sirven
                           select busqueda;

            var archivoPeriodo = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                                 where busqueda.Contains("IDENTIFICACION")
                                 select busqueda;

            string lote = File.ReadAllLines(archivoPeriodo.FirstOrDefault()).ToList().ElementAt(0).Split('\t').ElementAt(5);

            Helpers.EscribirVentanaLog($"Archivos Cargados Para Proceso: {archivos.Count().ToString()}");
            Helpers.EscribirVentanaLog(" ");

            Helpers.EscribirVentanaLog($"Inicia Formateo de Archivos");
            Helpers.EscribirVentanaMismaLinea($"Formateando Archivos: ");
            foreach (var archivo in archivos)
            {
                _ = new ProcesoGobiernos(archivo);
                Helpers.EscribirVentanaMismaLinea($"X", false);
            }
            Helpers.EscribirVentanaMismaLinea(System.Environment.NewLine, false);

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            Helpers.EscribirVentanaLog($"Inicia Escritura de Salidas");
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "3", lote);

            // Se crean los reportes
            Helpers.EscribirVentanaLog($"Inicia Proceso Reportes");
            _ = new ReportesGobiernos(Variables.Variables.DiccionarioExtractosFormateados, App.ControlInsumos.Helpers.RutaProceso, lote);
            #endregion
        }

        /// <summary>
        /// Metodo para ejecutar el proceso Llanos
        /// </summary>
        /// <param name="pRutaArchivosProcesar">Ruta donde se encuentran los archivos originales</param>
        public void EjecutarProcesoLLanos(string pRutaArchivosProcesar)
        {
            #region EjecutarProcesoLlanos

            var archivos = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                           select busqueda;

            Helpers.EscribirVentanaLog($"Archivos Cargados Para Proceso: {archivos.Count().ToString()}");
            Helpers.EscribirVentanaLog(" ");

            Helpers.EscribirVentanaLog($"Inicia Formateo de Archivos");
            foreach (var archivo in archivos)
            {
                _ = new ProcesoLlanos(archivo);


                //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
                Helpers.EscribirVentanaLog($"Inicia Escritura de Salidas");
                EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "4");

                // Se crean los reportes
                Helpers.EscribirVentanaLog($"Inicia Proceso Reportes");
                _ = new ReportesLlanos(Variables.Variables.DiccionarioExtractosFormateados, App.ControlInsumos.Helpers.RutaProceso, Path.GetFileNameWithoutExtension(archivo));
            }
            #endregion
        }

        /// <summary>
        /// Metodo para ejecutar el proceso Credito Hipotecario
        /// </summary>
        /// <param name="pRutaArchivosProcesar">Ruta donde se encuentran los archivos originales</param>
        public void EjecutarProcesoCreditoHipotecario(string pRutaArchivosProcesar)
        {
            #region EjecutarProcesoCreditoHipotecario

            var archivos = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                           where Path.GetExtension(busqueda).ToLower().Equals(".csv")
                           select busqueda;

            var archivosExcel = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                                where Path.GetExtension(busqueda).ToLower().Equals(".xlsx")
                                select busqueda;
            Helpers.GetCartasHipotecario(archivosExcel.ToList());

            Helpers.EscribirVentanaLog($"Inicia Formateo de Archivos");
            foreach (var archivo in archivos)
            {
                _ = new ProcesoCreditoHipotecario(archivo);
            }

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            Helpers.EscribirVentanaLog($"Inicia Escritura de Salidas");
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "5");

            Helpers.EscribirVentanaLog($"Inicia Proceso Reportes");

            #endregion
        }

        /// <summary>
        /// Metodo para ejecutar el proceso Anexos Verdes
        /// </summary>
        /// <param name="pRutaArchivosProcesar">Ruta donde se encuentran los archivos originales</param>
        public void EjecutarProcesoAnexosVerdes(string pRutaArchivosProcesar)
        {
            #region EjecutarProcesoAnexosVerdes

            var archivosAuxiliar = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                                   where busqueda.Contains("COSTAT_DBASI_")
                                   select busqueda;

            var archivos = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                           where busqueda.Contains("COSTAT_MOVTO_")
                           select busqueda;

            Helpers.GetAuxAnexosVerdes(archivosAuxiliar.ToList());

            Helpers.EscribirVentanaLog($"Inicia Formateo de Archivos");
            foreach (var archivo in archivos)
            {
                _ = new ProcesoAnexosVerdes(pRutaArchivosProcesar);
            }

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            Helpers.EscribirVentanaLog($"Inicia Escritura de Salidas");
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "6");

            // Se crean los reportes
            Helpers.EscribirVentanaLog($"Inicia Proceso Reportes");
            _ = new ReportesAnexosVerdes(Variables.Variables.DiccionarioExtractosFormateados, App.ControlInsumos.Helpers.RutaProceso, "");
            #endregion
        }

        /// <summary>
        /// Metodo para ejecutar el proceso LTE Corporativo
        /// </summary>
        /// <param name="pRutaArchivosProcesar">Ruta donde se encuentran los archivos originales</param>
        public void EjecutarProcesoLteCorp(string pRutaArchivosProcesar)
        {
            #region EjecutarProcesoLTECorporativo
            var archivos = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                           where !busqueda.Contains("CONTEO") && !busqueda.Contains("IDENTIFICACION") && !busqueda.Contains("premaestra") //TODO: se omiten estos archivos peor ahi que revisar para que sirven
                           select busqueda;

            var archivoPeriodo = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                                 where busqueda.Contains("IDENTIFICACION")
                                 select busqueda;

            string periodo = File.ReadAllLines(archivoPeriodo.FirstOrDefault()).ToList().ElementAt(0).Split('\t').ElementAt(1).PadLeft(6, '0');
            string lote = File.ReadAllLines(archivoPeriodo.FirstOrDefault()).ToList().ElementAt(0).Split('\t').ElementAt(5);

            Helpers.EscribirVentanaLog($"Archivos Cargados Para Proceso: {archivos.Count().ToString()}");
            Helpers.EscribirVentanaLog(" ");

            Helpers.EscribirVentanaLog($"Inicia Formateo de Archivos");
            Helpers.EscribirVentanaMismaLinea($"Formateando Archivos: ");
            foreach (var archivo in archivos)
            {
                _ = new ProcesoLteCorp(archivo, periodo);
                Helpers.EscribirVentanaMismaLinea($"X", false);
            }
            Helpers.EscribirVentanaMismaLinea(System.Environment.NewLine, false);

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            Helpers.EscribirVentanaLog($"Inicia Escritura de Salidas");
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "1", lote);

            // Se crean los reportes
            Helpers.EscribirVentanaLog($"Inicia Proceso Reportes");
            _ = new ReportesLteCorporativo(Variables.Variables.DiccionarioExtractosFormateados, App.ControlInsumos.Helpers.RutaProceso, lote);

            #endregion
        }

        /// <summary>
        /// Metodo para Cargar Insumos
        /// </summary>
        /// <param name="pRutaArchivosProcesar">Ruta donde se encuentran insumos del proceso</param>
        public void CargueGeneralInsumos(string pRuta)
        {
            #region CargueGeneralInsumos
            Helpers.EscribirVentanaMismaLinea($"Cargando Insumos: ");

            foreach (var Carpeta in Directory.GetDirectories(pRuta))
            {
                string NombreCarpeta = Path.GetFileName(Carpeta);

                Variables.Variables.DiccionarioInsumos.Add(NombreCarpeta, new Dictionary<string, Variables.DatosInsumos>());
            }

            foreach (var Carpeta in Variables.Variables.DiccionarioInsumos)
            {
                bool insumoConfiguradoExiste = false;

                foreach (var Archivo in Directory.GetFiles($"{Helpers.RutaInsumos}\\{Carpeta.Key}"))
                {
                    string NombreArchivo = Path.GetFileName(Archivo);

                    foreach (var EnumInsumo in Enum.GetValues(typeof(Variables.Insumos)))
                    {
                        //Si el insumo esta configurado
                        if (NombreArchivo.Contains(EnumInsumo.ToString()))
                        {
                            #region Configuracion Cargue Insumos
                            if (EnumInsumo.ToString() == Variables.Insumos.doc1tsub.ToString())
                            {
                                Helpers.GetTablaSutitucion(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ExcluirServiciosAdicionales.ToString())
                            {
                                Helpers.GetServiciosAdicionales(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ServiciosAdicionalesTV.ToString())
                            {
                                Helpers.GetServiciosAdicionalesTv(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.VelocidadFibra.ToString())
                            {
                                Helpers.GetVelocidadFibra(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ParametrizacionPaquetesFibra.ToString())
                            {
                                Helpers.GetParametrizacionPaquetesFibra(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.cuentas_Envio_SMS.ToString())
                            {
                                Helpers.GetCuentasEnvioSms(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.cuentas_Envio_Whatsapp.ToString())
                            {
                                Helpers.GetCuentasEnvioWhatsapp(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.Codigos_Univer_SVAS.ToString())
                            {
                                Helpers.GetCodigosUniverSvas(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.Cuentas_SVAS_FueradeBundle.ToString())
                            {
                                Helpers.GetCuentasSvasFueraBundle(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.cuentasExtraer.ToString())
                            {
                                Helpers.GetCuentasExtraer(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.distribucion_especial.ToString())
                            {
                                Helpers.GetDistribucionEspecial(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.CicloCourier.ToString())
                            {
                                Helpers.GetCicloCourier(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ClientesEspeciales.ToString())
                            {
                                Helpers.GetClientesEspecialesDatos(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.BaseTranspromo.ToString())
                            {
                                Helpers.GetBaseTranspromo(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ASIGNACION_CARTAS.ToString())
                            {
                                Helpers.GetAsignacionCartas(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.NIVEL_RECLAMACION.ToString())
                            {
                                Helpers.GetNivelReclamacion(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.Fechas_Pago_Fijas.ToString())
                            {
                                Helpers.GetFechaPagoFijas(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ETB_Horas_Exp.ToString())
                            {
                                Helpers.GetETBHorasExp(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.PromocionesLTE.ToString())
                            {
                                Helpers.GetPromocionesLTE(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.Cuentas_LTE.ToString())
                            {
                                Helpers.GetCuentasLTE(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.Clientes_Email_Privado.ToString())
                            {
                                Helpers.GetClientesEmailPrivado(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.BASE_CUPONES.ToString())
                            {
                                Helpers.GetBaseCupones(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.PROCUNI.ToString())
                            {
                                Helpers.GetProcuni(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.doc1_participaciones.ToString())
                            {
                                Helpers.GetDoc1Participaciones(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.CodigosBD.ToString())
                            {
                                Helpers.GetCodigosBD(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.LOC_BAR.ToString())
                            {
                                Helpers.GetLocBar(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.TelefonosPrivadosRevchain.ToString())
                            {
                                Helpers.GetTelefonosPrivadosRevchain(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.TRAMITES.ToString())
                            {
                                Helpers.GetTramites(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.FA_DISTRIBUCION_EMAIL_REVCHAIN.ToString())
                            {
                                Helpers.GetFADistribucionEmailRevchain(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.DistribucionDoble_REVCHAIN.ToString())
                            {
                                Helpers.GetDistribucionDobleRevchain(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ClausulaPermanencia.ToString())
                            {
                                Helpers.GetClausulaPermanencia(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.MENSAJE_CLIENTES_SUSPENDIDOS.ToString())
                            {
                                Helpers.GetMensajesClientesSuspendidos(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ETB_Cartas_Mora.ToString())
                            {
                                Helpers.GetETBCartasMora(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.EXCLUSION_PAGO_INMEDIATO.ToString())
                            {
                                Helpers.GetExclusionPagoInmediato(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ConformacionPaquetes.ToString())
                            {
                                Helpers.GetConformacionPaquetes(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ConfiguracionLlavesDoc1.ToString())
                            {
                                Helpers.GetConfiguracionLLavesDoc1(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.ETB_Factura_Electronica.ToString())
                            {
                                Helpers.GetETBFacturaElectronica(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.Dual_Llanos.ToString())
                            {
                                Helpers.GetDualLlanos(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.FA_DISTRIBUCION_EMAIL_LLANOS.ToString())
                            {
                                Helpers.GetDistribucionEmailLlanos(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.LLANOS_Envio_SMS.ToString())
                            {
                                Helpers.GetLlanosEnvioSMS(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.LLANOS_Extraer.ToString())
                            {
                                Helpers.GetExtraerLlanos(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.Llanos_min_plan.ToString())
                            {
                                Helpers.GetLlanosMinPlan(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            else if (EnumInsumo.ToString() == Variables.Insumos.Llanos_suple.ToString())
                            {
                                Helpers.GetLlanosSuple(File.ReadAllLines(Archivo, Encoding.Default).ToList());
                            }
                            #endregion

                            insumoConfiguradoExiste = true;
                            break;
                        }
                    }

                    if (!insumoConfiguradoExiste)
                    {
                        //TODO: No existe insumo configurado, Mostrar mensaje de error

                    }

                    Helpers.EscribirVentanaMismaLinea($"X", false);
                }

            }
            Helpers.EscribirVentanaMismaLinea(System.Environment.NewLine, false);
            #endregion
        }

        #region Logica y Escritura de Salidas del proceso

        /// <summary>
        /// Metodo que Escribe Salidas Proceso
        /// </summary>
        /// <param name="pRutaSalida"></param>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pIdentificadorProceso"></param>
        /// <param name="pLote"></param>
        private void EscribirSalidasProceso(string pRutaSalida, Dictionary<string, List<string>> pDatosImprimir, string pIdentificadorProceso, string pLote = "")
        {
            #region EscribirSalidasProceso
            switch (pIdentificadorProceso)
            {
                case "1": //Masivos
                    EscribirDatosSalidaCompleto(pDatosImprimir, $"{pRutaSalida}", $"COMPLETO.sal", pLote, "1");
                    EscribirDatosSalidaSinCufe(pDatosImprimir, $"{pRutaSalida}", $"I_SIN_CUFE.sal", "1", pLote);
                    EscribirDatosSalidaNoImprimir(pDatosImprimir, $"{pRutaSalida}", $"I_NO_IMPRIMIR.sal", "1", pLote);
                    EscribirDatosSalidaSms(pDatosImprimir, $"{pRutaSalida}", $"I_ENVIO_SMS.sal", "1", pLote);
                    EscribirDatosSalidaWhatsapp(pDatosImprimir, $"{pRutaSalida}", $"I_ENVIO_Whatsapp.sal", "1", pLote);
                    EscribirDatosSalidaErrorLte(pDatosImprimir, $"{pRutaSalida}", $"I_ERROR_FACTURA_LTE.sal", pLote);
                    EscribirDatosSalidaDiferencias(pDatosImprimir, $"{pRutaSalida}", $"I_DIFERENCIAS.sal", pLote);
                    EscribirDatosSalidaDistribucionEspecial(pDatosImprimir, $"{pRutaSalida}", $"I_DISTRIBUCION_ESPECIAL.sal", "1", pLote);
                    EscribirDatosSalidaEmail(pDatosImprimir, $"{pRutaSalida}", $"I_FACTURA_SOLO_EMAIL.sal", "1", pLote);
                    EscribirDatosSalidaOtros(pDatosImprimir, $"{pRutaSalida}", $"I_OTROS.sal", "1", pLote);
                    EscribirDatosSalidaImpresion(pDatosImprimir, $"{pRutaSalida}", $"Courier00000000I.sal", pLote, "1");
                    break;

                case "2": //Datos
                    EscribirDatosSalidaCompleto(pDatosImprimir, $"{pRutaSalida}", $"COMPLETO.sal", pLote, "2");
                    EscribirDatosSalidaSinCufe(pDatosImprimir, $"{pRutaSalida}", $"I_SIN_CUFE.sal", "1", pLote);
                    EscribirDatosSalidaNoImprimir(pDatosImprimir, $"{pRutaSalida}", $"I_NO_IMPRIMIR.sal", "2", pLote);
                    EscribirDatosSalidaNumHojas(pDatosImprimir, $"{pRutaSalida}", $"I_NUMHOJAS.sal", pLote);
                    EscribirDatosSalidaClientesEsepeciales(pDatosImprimir, $"{pRutaSalida}", $"INS_CLIENTES_ESPECIALES.sal", pLote);
                    EscribirDatosSalidaDistribucionEspecial(pDatosImprimir, $"{pRutaSalida}", $"I_DISTRIBUCION_ESPECIAL.sal", "2", pLote);
                    EscribirDatosSalidaEmail(pDatosImprimir, $"{pRutaSalida}", $"I_FACTURA_SOLO_EMAIL.sal", "2", pLote);
                    EscribirDatosSalidaOtros(pDatosImprimir, $"{pRutaSalida}", $"I_OTROS.sal", "2", pLote);
                    EscribirDatosSalidaImpresion(pDatosImprimir, $"{pRutaSalida}", $"Courier00000000I.sal", pLote, "2");
                    break;

                case "3": //Gobiernos
                    EscribirDatosSalidaCompleto(pDatosImprimir, $"{pRutaSalida}", $"COMPLETO.sal", pLote, "3");
                    EscribirDatosSalidaSinCufe(pDatosImprimir, $"{pRutaSalida}", $"I_SIN_CUFE.sal", "1", pLote);
                    EscribirDatosSalidaNoImprimir(pDatosImprimir, $"{pRutaSalida}", $"I_NO_IMPRIMIR.sal", "3", pLote);
                    EscribirDatosSalidaNumHojas(pDatosImprimir, $"{pRutaSalida}", $"I_NUMHOJAS.sal", pLote);
                    EscribirDatosSalidaDistribucionEspecial(pDatosImprimir, $"{pRutaSalida}", $"I_DISTRIBUCION_ESPECIAL.sal", "3", pLote);
                    EscribirDatosSalidaEmail(pDatosImprimir, $"{pRutaSalida}", $"I_FACTURA_SOLO_EMAIL.sal", "3", pLote);
                    EscribirDatosSalidaOtros(pDatosImprimir, $"{pRutaSalida}", $"I_OTROS.sal", "3", pLote);
                    EscribirDatosSalidaImpresion(pDatosImprimir, $"{pRutaSalida}", $"Courier00000000I.sal", pLote, "3");
                    break;

                case "4": //Llanos
                    EscribirDatosSalidaCompleto(pDatosImprimir, $"{pRutaSalida}", $"COMPLETO.sal", pLote, "4");
                    EscribirDatosSalidaSms(pDatosImprimir, $"{pRutaSalida}", $"I_ENVIO_SMS.sal", "4", pLote);
                    EscribirDatosSalidaNoImprimir(pDatosImprimir, $"{pRutaSalida}", $"I_NO_IMPRIMIR.sal", "4", pLote);
                    EscribirDatosSalidaEmail(pDatosImprimir, $"{pRutaSalida}", $"I_FACTURA_SOLO_EMAIL.sal", "4", pLote);
                    EscribirDatosSalidaImpresion(pDatosImprimir, $"{pRutaSalida}", $"Courier00000000I.sal", pLote, "4");
                    break;

                case "5": //Hipotecario
                    EscribirDatosSalidaImpresionNormal(pDatosImprimir, $"{pRutaSalida}", $"Courier00000000I.sal");
                    break;

                case "6": //Anexos Verdes
                    EscribirDatosSalidaImpresionNormal(pDatosImprimir, $"{pRutaSalida}", $"Courier00000000I.sal");
                    break;
            }
            #endregion
        }


        /// <summary>
        /// Metodo que Escribe Datos Salida Completo
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaCompleto(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pLote, string pTipoProceso)
        {
            #region EscribirDatosSalidaCompleto
            List<string> resultado = new List<string>();
            int consecutivo = 1;

            string cuenta = string.Empty;

            var datosImprimirFinal = new Dictionary<string, List<string>>(pDatosImprimir);

            switch (pTipoProceso)
            {
                case "1":
                case "2":
                case "3":
                case "4":
                    foreach (var datoLinea in datosImprimirFinal)
                    {
                        string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";

                        var nuevo1AAA = from n in datoLinea.Value
                                        where n.Substring(0, 4) == "1AAA"
                                        select n.Replace("KitXXXX", nuevoConsecutivo);

                        // Se actualiza el Consecutivo del diccionario Original formateado
                        Variables.Variables.DiccionarioExtractosFormateados[datoLinea.Key][0] = Variables.Variables.DiccionarioExtractosFormateados[datoLinea.Key][0].Replace("KitXXXX", nuevoConsecutivo);
                        resultado.AddRange(datoLinea.Value);
                        consecutivo++;
                    }

                    Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", resultado);
                    break;
            }


            resultado.Clear();
            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida SMS
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaSinCufe(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pTipoProceso, string pLote)
        {
            #region EscribirDatosSalidaSinCufe
            IEnumerable<KeyValuePair<string, List<string>>> objDatos = null;

            switch (pTipoProceso)
            {
                case "1":

                    objDatos = from busqueda in pDatosImprimir
                               let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                               let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                               where !Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}")
                               select busqueda;

                    if (objDatos.Any())
                    {
                        ProcesarSalidas(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "2":

                    objDatos = from busqueda in pDatosImprimir
                               let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                               let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                               where !Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}")
                               select busqueda;

                    if (objDatos.Any())
                    {
                        ProcesarSalidas(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "3":

                    objDatos = from busqueda in pDatosImprimir
                               let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                               let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                               where !Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}")
                               select busqueda;

                    if (objDatos.Any())
                    {
                        ProcesarSalidas(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

            }
            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida No Imprimir
        /// </summary>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaNoImprimir(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pTipoProceso, string pLote)
        {
            #region EscribirDatosSalidaNoImprimir
            List<string> resultado = new List<string>();
            int consecutivoInternoDivision = 0;
            int consecutivoInternoArchivo = 1;
            string cuenta = string.Empty;
            string archivoActual = string.Empty;
            int consecutivo = 1;

            switch (pTipoProceso)
            {
                case "1":
                case "2":
                case "3":

                    if (Variables.Variables.CuentasNoImprimir.Any())
                    {
                        foreach (var linea in Variables.Variables.CuentasNoImprimir.SelectMany(x => x.Value))
                        {
                            if (linea.Substring(0, 4) == "1AAA")
                            {
                                consecutivoInternoDivision++;
                                cuenta = linea.Split('|')[7].Trim();
                                archivoActual = $"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal";

                                // Se agrega la cuenta en el archivo final                                
                                if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(cuenta))
                                    Variables.Variables.ArchivoSalidaFinal.Add(cuenta, archivoActual);

                                if (consecutivoInternoDivision == 8001)
                                {
                                    archivoActual = $"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal";
                                    Helpers.EscribirEnArchivo(archivoActual, resultado);
                                    //Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);                                    
                                    consecutivoInternoArchivo++;
                                    consecutivoInternoDivision = 1;
                                    resultado.Clear();
                                }

                                string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";
                                consecutivo++;

                                resultado.Add(linea.Replace("KitXXXX", nuevoConsecutivo));

                                continue;
                            }

                            resultado.Add(linea);
                        }

                        Helpers.EscribirEnArchivo(archivoActual, resultado);
                        //Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);                        
                    }

                    break;

                case "4":

                    Dictionary<string, List<string>> datosOrdenadosTelefono = new Dictionary<string, List<string>>();

                    foreach (var DatoExtracto in pDatosImprimir)
                    {
                        string llave = DatoExtracto.Value.FirstOrDefault().Split('|').ElementAt(6).Trim();

                        if (!datosOrdenadosTelefono.ContainsKey(llave))
                        {
                            datosOrdenadosTelefono.Add(llave, DatoExtracto.Value);
                        }
                    }

                    var objDatos = from busqueda in datosOrdenadosTelefono
                                   where Variables.Variables.DatosInsumoExtraerLlanos.ContainsKey(busqueda.Key)
                                   select busqueda;

                    if (objDatos.Any())
                    {
                        foreach (var linea in objDatos.SelectMany(x => x.Value))
                        {
                            if (linea.Substring(0, 4) == "1AAA")
                            {
                                consecutivoInternoDivision++;

                                if (consecutivoInternoDivision == 8001)
                                {
                                    Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);
                                    consecutivoInternoArchivo++;
                                    consecutivoInternoDivision = 1;
                                    resultado.Clear();
                                }

                                string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";

                                linea.Replace("KitXXXX", nuevoConsecutivo);

                                consecutivo++;

                                resultado.Add(linea.Replace("****tipo_salida****", "_EXTRAER*"));
                                continue;
                            }

                            resultado.Add(linea);
                        }

                        Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);
                    }
                    break;
            }

            resultado.Clear();
            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida SMS
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaSms(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pTipoProceso, string pLote)
        {
            #region EscribirDatosSalidaSms
            IEnumerable<KeyValuePair<string, List<string>>> objDatos = null;

            switch (pTipoProceso)
            {
                case "1":

                    objDatos = from busqueda in pDatosImprimir
                               where Variables.Variables.DatosInsumoCuentasEnvioSms.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DatosInsumoCuentasEnvioWhatsapp.ContainsKey(busqueda.Key)
                               select busqueda;

                    if (objDatos.Any())
                    {
                        ProcesarSalidas(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "4":

                    Dictionary<string, List<string>> datosOrdenadosTelefono = new Dictionary<string, List<string>>();

                    foreach (var DatoExtracto in pDatosImprimir)
                    {
                        string llave = DatoExtracto.Value.FirstOrDefault().Split('|').ElementAt(6).Trim();

                        if (!datosOrdenadosTelefono.ContainsKey(llave))
                        {
                            datosOrdenadosTelefono.Add(llave, DatoExtracto.Value);
                        }
                    }

                    objDatos = from busqueda in datosOrdenadosTelefono
                               where Variables.Variables.DatosInsumoLlanosEnvioSMS.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DatosInsumoExtraerLlanos.ContainsKey(busqueda.Key)
                               select busqueda;

                    if (objDatos.Any())
                    {
                        ProcesarSalidas(objDatos, pRuta, pNombreArchivo, pLote, true, "_SMS*");
                    }
                    break;
            }
            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida Whatsapp
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaWhatsapp(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pTipoProceso, string pLote)
        {
            #region EscribirDatosSalidaWhatsapp
            IEnumerable<KeyValuePair<string, List<string>>> objDatos = null;

            switch (pTipoProceso)
            {
                case "1":

                    objDatos = from busqueda in pDatosImprimir
                               let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                               let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                               where Variables.Variables.DatosInsumoCuentasEnvioWhatsapp.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                               Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}")
                               select busqueda;

                    if (objDatos.Any())
                    {
                        ProcesarSalidas(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

            }
            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida Email
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaEmail(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pTipoProceso, string pLote)
        {
            #region EscribirDatosSalidaEmail

            IEnumerable<KeyValuePair<string, List<string>>> objDatos = null;

            switch (pTipoProceso)
            {
                case "1":
                    objDatos = from busqueda in pDatosImprimir
                               let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                               let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                               where Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DatosInsumoCuentasEnvioSms.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DatosInsumoCuentasEnvioWhatsapp.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DatosErrorLTE.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.Diferencias.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DiccionarioDual.ContainsKey(busqueda.Key)
                               select busqueda;

                    if (objDatos.Any())
                    {
                        ProcesarSalidas(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "2":
                    objDatos = from busqueda in pDatosImprimir
                               let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                               let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                               where Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey($"{busqueda.Key}-{busqueda.Value.FirstOrDefault().Split('|').ElementAt(9)}") &&
                               !Variables.Variables.DiccionarioDual.ContainsKey(busqueda.Key)
                               select busqueda;

                    if (objDatos.Any())
                    {
                        ProcesarSalidas(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "3":
                    objDatos = from busqueda in pDatosImprimir
                               let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                               let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                               where Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DiccionarioDual.ContainsKey(busqueda.Key)
                               select busqueda;

                    if (objDatos.Any())
                    {
                        ProcesarSalidas(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "4":

                    Dictionary<string, List<string>> datosOrdenadosTelefono = new Dictionary<string, List<string>>();

                    foreach (var DatoExtracto in pDatosImprimir)
                    {
                        string llave = DatoExtracto.Value.FirstOrDefault().Split('|').ElementAt(6).Trim();

                        if (!datosOrdenadosTelefono.ContainsKey(llave))
                        {
                            datosOrdenadosTelefono.Add(llave, DatoExtracto.Value);
                        }
                    }

                    objDatos = from busqueda in datosOrdenadosTelefono
                               where Variables.Variables.DatosInsumoDistribucionEmailLlanos.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DatosInsumoExtraerLlanos.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DatosInsumoLlanosEnvioSMS.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DatosInsumoDualLlanos.ContainsKey(busqueda.Key) &&
                               !Variables.Variables.DiccionarioDual.ContainsKey(busqueda.Key)
                               select busqueda;

                    if (objDatos.Any())
                    {
                        ProcesarSalidas(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;
            }

            #endregion
        }

        /// <summary>
        /// Metodo que Procesa Salidas
        /// </summary>
        /// <param name="pDatos"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void ProcesarSalidas(IEnumerable<KeyValuePair<string, List<string>>> pDatos, string pRuta, string pNombreArchivo, string pLote, bool pReemplazarMarcaSalida = false, string pMarcaReemplazo = "")
        {
            #region ProcesarSalidas
            List<string> resultado = new List<string>();
            int consecutivoInternoDivision = 0;
            int consecutivoInternoArchivo = 1;
            string cuenta = string.Empty;
            string archivoActual = string.Empty;
            int consecutivo = 1;

            foreach (var linea in pDatos.SelectMany(x => x.Value))
            {
                if (linea.Substring(0, 4) == "1AAA")
                {
                    consecutivoInternoDivision++;
                    cuenta = linea.Split('|')[7].Trim();
                    archivoActual = $"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal";

                    // Se agrega la cuenta en el archivo final                                
                    if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(cuenta))
                        Variables.Variables.ArchivoSalidaFinal.Add(cuenta, archivoActual);

                    if (consecutivoInternoDivision == 8001)
                    {
                        //Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);
                        Helpers.EscribirEnArchivo(archivoActual, resultado);
                        consecutivoInternoArchivo++;
                        consecutivoInternoDivision = 1;
                        resultado.Clear();
                    }

                    string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";
                    consecutivo++;

                    if (pReemplazarMarcaSalida)
                    {
                        resultado.Add(linea.Replace("****tipo_salida****", pMarcaReemplazo).Replace(linea.Split('|').ElementAt(1), nuevoConsecutivo));
                        continue;
                    }
                    else
                    {
                        resultado.Add(linea.Replace(linea.Split('|').ElementAt(1), nuevoConsecutivo));
                        continue;
                    }
                    
                }

                resultado.Add(linea);
            }

            Helpers.EscribirEnArchivo(archivoActual, resultado);
            //Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);

            resultado.Clear();
            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida Distribucion Especial
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaDistribucionEspecial(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pTipoProceso, string pLote)
        {
            #region EscribirDatosSalidaDistribucionEspecial
            IEnumerable<KeyValuePair<string, List<string>>> objDatos = null;

            switch (pTipoProceso)
            {
                case "1":
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoCuentasEnvioSms.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoCuentasEnvioWhatsapp.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosErrorLTE.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.Diferencias.ContainsKey(busqueda.Key) &&
                                //!Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                                Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}")
                                select busqueda).ToDictionary(x => x.Key).Values;

                    if (objDatos.Any())
                    {
                        ProcesarSalidasDistribucionEspecial(objDatos, pDatosImprimir, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "2":
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey($"{busqueda.Key}-{busqueda.Value.FirstOrDefault().Split('|').ElementAt(9)}") &&
                                //!Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                                Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}")
                                select busqueda).ToDictionary(x => x.Key).Values;

                    if (objDatos.Any())
                    {
                        ProcesarSalidasDistribucionEspecial(objDatos, pDatosImprimir, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "3":
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                //!Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key)&&
                                Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}")
                                select busqueda).ToDictionary(x => x.Key).Values;

                    if (objDatos.Any())
                    {
                        ProcesarSalidasDistribucionEspecial(objDatos, pDatosImprimir, pRuta, pNombreArchivo, pLote);
                    }
                    break;

            }

            #endregion
        }

        /// <summary>
        /// Metodo que Procesa Salidas Distribucion Especial
        /// </summary>
        /// <param name="pDatos"></param>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void ProcesarSalidasDistribucionEspecial(IEnumerable<KeyValuePair<string, List<string>>> pDatos, Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pLote)
        {
            #region ProcesarSalidasDistribucionEspecial
            List<string> datosFinales = new List<string>();
            int consecutivo = 1;

            foreach (var dato in pDatos)
            {
                if (pDatosImprimir.ContainsKey(dato.Key))
                {
                    string fechaPeriodoProceso = pDatosImprimir[dato.Key].FirstOrDefault().Split('|').ElementAt(11).Substring(3).Replace("/", "");
                    string fechaPeriodoInsumo = Variables.Variables.DatosInsumoDistribucionEspecial[dato.Key].FirstOrDefault().Split('|').ElementAt(2);

                    if (fechaPeriodoProceso == fechaPeriodoInsumo)
                    {
                        string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";

                        var nuevo1AAA = from n in dato.Value
                                        where n.Substring(0, 4) == "1AAA"
                                        select n.Replace("KitXXXX", nuevoConsecutivo);

                        // Se actualiza el Consecutivo del diccionario Original formateado
                        Variables.Variables.DiccionarioExtractosFormateados[dato.Key][0] = Variables.Variables.DiccionarioExtractosFormateados[dato.Key][0].Replace(Variables.Variables.DiccionarioExtractosFormateados[dato.Key][0].Split('|').ElementAt(1), nuevoConsecutivo);

                        consecutivo++;

                        datosFinales.AddRange(dato.Value);

                        // Se agrega la cuenta en el archivo final                                
                        if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(dato.Key))
                            Variables.Variables.ArchivoSalidaFinal.Add(dato.Key, $"{pRuta}\\{pNombreArchivo}");

                    }
                }
            }

            if (datosFinales.Any())
            {
                Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", datosFinales);
            }
            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida Error Lte
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaErrorLte(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pLote)
        {
            #region EscribirDatosSalidaErrorLte
            List<string> resultado = new List<string>();
            string cuenta = string.Empty;
            int consecutivo = 1;

            if (Variables.Variables.DatosErrorLTE.Any())
            {
                foreach (var linea in Variables.Variables.DatosErrorLTE.SelectMany(x => x.Value))
                {
                    if (linea.Split('|')[0] == "1AAA")
                    {
                        cuenta = linea.Split('|')[7];

                        string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";
                        consecutivo++;

                        // Se agrega la cuenta en el archivo final                                
                        if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(cuenta))
                            Variables.Variables.ArchivoSalidaFinal.Add(cuenta, $"{pRuta}\\{pNombreArchivo}");

                        resultado.Add(linea.Replace(linea.Split('|').ElementAt(1), nuevoConsecutivo));
                        continue;
                    }

                    resultado.Add(linea);
                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", resultado);

                resultado.Clear();
            }
            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida Diferencias
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaDiferencias(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pLote)
        {
            #region EscribirDatosSalidaDiferencias
            List<string> resultado = new List<string>();
            int consecutivo = 1;

            //var dfdf = (Convert.ToDecimal(Helpers.FormatearCampos(TiposFormateo.Decimal03, "$ 79.950,00")) - Convert.ToDecimal(Helpers.FormatearCampos(TiposFormateo.Decimal03, "$ 79.947,36"))) > 0.05m;

            var busquedaCuentas = (from busqueda in pDatosImprimir
                                   let canal1BBA = busqueda.Value.Find(x => x.Substring(0, 4).Equals("1BBA"))
                                   let total1AAA = busqueda.Value.FirstOrDefault().Split('|').ElementAt(10)
                                   let total1BBA = canal1BBA.Split('|').ElementAt(2)
                                   where total1AAA.Substring(0, 1) != "-" && total1BBA.Substring(0, 1) != "-"
                                   let RestTotales = (Convert.ToDecimal(Helpers.FormatearCampos(TiposFormateo.Decimal03, total1BBA )) - Convert.ToDecimal(Helpers.FormatearCampos(TiposFormateo.Decimal03, total1AAA)))
                                   where (RestTotales > 0.05m) &&
                                   !Variables.Variables.DatosErrorLTE.ContainsKey(busqueda.Key)
                                   select busqueda).ToDictionary(x => x.Key).Values;

            foreach (var dato in busquedaCuentas)
            {
                Variables.Variables.Diferencias.Add(dato.Key, string.Empty);

                // Se agrega la cuenta en el archivo final                                
                if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(dato.Key))
                    Variables.Variables.ArchivoSalidaFinal.Add(dato.Key, $"{pRuta}\\{pNombreArchivo}");


                if (pDatosImprimir.ContainsKey(dato.Key))
                {
                    string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";

                    var nuevo1AAA = from n in dato.Value
                                    where n.Substring(0, 4) == "1AAA"
                                    select n.Replace("KitXXXX", nuevoConsecutivo);

                    // Se actualiza el Consecutivo del diccionario Original formateado
                    Variables.Variables.DiccionarioExtractosFormateados[dato.Key][0] = Variables.Variables.DiccionarioExtractosFormateados[dato.Key][0].Replace(Variables.Variables.DiccionarioExtractosFormateados[dato.Key][0].Split('|').ElementAt(1), nuevoConsecutivo);

                    resultado.AddRange(dato.Value);

                    consecutivo++;
                }
            }

            if (resultado.Any())
            {
                Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", resultado);
            }

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida Otros
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaOtros(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pTipoProceso, string pLote)
        {
            #region EscribirDatosSalidaOtros
            IEnumerable<KeyValuePair<string, List<string>>> objDatos = null;

            switch (pTipoProceso)
            {
                case "1":
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoCuentasEnvioSms.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoCuentasEnvioWhatsapp.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosErrorLTE.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.Diferencias.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoProcuni.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DiccionarioDual.ContainsKey(busqueda.Key)&&
                                Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}")
                                select busqueda).ToDictionary(x => x.Key).Values;

                    if (objDatos.Any())
                    {
                        ProcesarSalidasOtrosProcesos(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "2":
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey($"{busqueda.Key}-{busqueda.Value.FirstOrDefault().Split('|').ElementAt(9)}") &&
                                !Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoProcuni.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DiccionarioDual.ContainsKey(busqueda.Key)&&
                                Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}")
                                select busqueda).ToDictionary(x => x.Key).Values;

                    if (objDatos.Any())
                    {
                        ProcesarSalidasOtrosProcesos(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "3":
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoProcuni.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DiccionarioDual.ContainsKey(busqueda.Key)&&
                                Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}")
                                select busqueda).ToDictionary(x => x.Key).Values;

                    if (objDatos.Any())
                    {
                        ProcesarSalidasOtrosProcesos(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

            }
            #endregion
        }

        /// <summary>
        /// Metodo que Procesa Salidas Otros Procesos
        /// </summary>
        /// <param name="pDatos"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void ProcesarSalidasOtrosProcesos(IEnumerable<KeyValuePair<string, List<string>>> pDatos, string pRuta, string pNombreArchivo, string pLote)
        {
            #region ProcesarSalidasOtrosProcesos
            List<string> resultado = new List<string>();
            int consecutivo = 1;

            foreach (var datoCuenta in pDatos)
            {
                string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";

                var nuevo1AAA = from n in datoCuenta.Value
                                where n.Substring(0, 4) == "1AAA"
                                select n.Replace("KitXXXX", nuevoConsecutivo);

                // Se actualiza el Consecutivo del diccionario Original formateado
                Variables.Variables.DiccionarioExtractosFormateados[datoCuenta.Key][0] = Variables.Variables.DiccionarioExtractosFormateados[datoCuenta.Key][0].Replace(Variables.Variables.DiccionarioExtractosFormateados[datoCuenta.Key][0].Split('|').ElementAt(1), nuevoConsecutivo);

                resultado.AddRange(datoCuenta.Value);

                consecutivo++;

                // Se agrega la cuenta en el archivo final                                
                if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(datoCuenta.Key))
                    Variables.Variables.ArchivoSalidaFinal.Add(datoCuenta.Key, $"{pRuta}\\{pNombreArchivo}");
            }

            Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", resultado);
            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida Impresion
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaImpresion(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pLote, string pTipoProceso)
        {
            #region EscribirDatosSalidaImpresion
            IEnumerable<KeyValuePair<string, List<string>>> objDatos = null;
            SortedDictionary<int, KeyValuePair<string, List<string>>> objDatosFinalOrdenado = new SortedDictionary<int, KeyValuePair<string, List<string>>>();
            List<KeyValuePair<string, List<string>>> objDatosFinal = new List<KeyValuePair<string, List<string>>>();

            switch (pTipoProceso)
            {
                case "1":
                    //Se buscan las de la procuni
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where Variables.Variables.DatosInsumoProcuni.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoCuentasEnvioSms.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoCuentasEnvioWhatsapp.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosErrorLTE.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.Diferencias.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key)
                                select busqueda).ToDictionary(x => x.Key).Values;

                    foreach (var item in objDatos)
                    {
                        objDatosFinal.Add(new KeyValuePair<string, List<string>>(item.Key, item.Value));
                    }

                    //Buscar las duales
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where Variables.Variables.DiccionarioDual.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoCuentasEnvioSms.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoCuentasEnvioWhatsapp.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosErrorLTE.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.Diferencias.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key)
                                select busqueda).ToDictionary(x => x.Key).Values;

                    foreach (var item in objDatos)
                    {
                        objDatosFinal.Add(new KeyValuePair<string, List<string>>(item.Key, item.Value));
                    }

                    #region Ordenamiento Procuni
                    foreach (var item in objDatosFinal)
                    {
                        int consecutivoProcuni = 0;

                        if (Variables.Variables.DatosInsumoProcuni.ContainsKey(item.Key))
                        {
                            consecutivoProcuni = Convert.ToInt32(Variables.Variables.DatosInsumoProcuni[item.Key].FirstOrDefault().Substring(39, 10).TrimStart('0'));

                            objDatosFinalOrdenado.Add(consecutivoProcuni, item);
                        }
                    }

                    if (objDatosFinalOrdenado.Count != objDatosFinal.Count)
                    {
                        foreach (var item in objDatosFinal)
                        {
                            if (Variables.Variables.DiccionarioDual.ContainsKey(item.Key))
                            {
                                int consecutivoProcuni = objDatosFinalOrdenado.Count + 1;

                                objDatosFinalOrdenado.Add(consecutivoProcuni, item);
                            }
                        }
                    }
                    
                    #endregion

                    if (objDatosFinal.Any())
                    {
                        ProcesoSalidasImpresion(objDatosFinalOrdenado.Values, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "2":

                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where Variables.Variables.DatosInsumoProcuni.ContainsKey(busqueda.Key) &&
                                (!Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey($"{busqueda.Key}-{busqueda.Value.FirstOrDefault().Split('|').ElementAt(9)}") &&
                                !Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key))
                                select busqueda).ToDictionary(x => x.Key).Values;

                    foreach (var item in objDatos)
                    {
                        objDatosFinal.Add(new KeyValuePair<string, List<string>>(item.Key, item.Value));
                    }

                    //Buscar las duales
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where Variables.Variables.DiccionarioDual.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey($"{busqueda.Key}-{busqueda.Value.FirstOrDefault().Split('|').ElementAt(9)}") &&
                                !Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key)
                                select busqueda).ToDictionary(x => x.Key).Values;

                    foreach (var item in objDatos)
                    {
                        objDatosFinal.Add(new KeyValuePair<string, List<string>>(item.Key, item.Value));
                    }

                    #region Ordenamiento Procuni
                    foreach (var item in objDatosFinal)
                    {
                        int consecutivoProcuni = 0;

                        if (Variables.Variables.DatosInsumoProcuni.ContainsKey(item.Key))
                        {
                            consecutivoProcuni = Convert.ToInt32(Variables.Variables.DatosInsumoProcuni[item.Key].FirstOrDefault().Substring(39, 10).TrimStart('0'));

                            objDatosFinalOrdenado.Add(consecutivoProcuni, item);
                        }
                    }

                    if (objDatosFinalOrdenado.Count != objDatosFinal.Count)
                    {
                        foreach (var item in objDatosFinal)
                        {
                            if (Variables.Variables.DiccionarioDual.ContainsKey(item.Key))
                            {
                                int consecutivoProcuni = objDatosFinalOrdenado.Count + 1;

                                objDatosFinalOrdenado.Add(consecutivoProcuni, item);
                            }
                        }
                    }
                    #endregion

                    if (objDatosFinal.Any())
                    {
                        ProcesoSalidasImpresion(objDatosFinalOrdenado.Values, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "3":
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where Variables.Variables.DatosInsumoProcuni.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key)
                                select busqueda).ToDictionary(x => x.Key).Values;

                    foreach (var item in objDatos)
                    {
                        objDatosFinal.Add(new KeyValuePair<string, List<string>>(item.Key, item.Value));
                    }

                    //Buscar las duales
                    objDatos = (from busqueda in pDatosImprimir
                                let cuenta = busqueda.Value.FirstOrDefault().Split('|').ElementAt(7)
                                let factura = busqueda.Value.FirstOrDefault().Split('|').ElementAt(8).TrimStart('0')
                                where Variables.Variables.DiccionarioDual.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key)
                                select busqueda).ToDictionary(x => x.Key).Values;

                    foreach (var item in objDatos)
                    {
                        objDatosFinal.Add(new KeyValuePair<string, List<string>>(item.Key, item.Value));
                    }

                    #region Ordenamiento Procuni
                    foreach (var item in objDatosFinal)
                    {
                        int consecutivoProcuni = 0;

                        if (Variables.Variables.DatosInsumoProcuni.ContainsKey(item.Key))
                        {
                            consecutivoProcuni = Convert.ToInt32(Variables.Variables.DatosInsumoProcuni[item.Key].FirstOrDefault().Substring(39, 10).TrimStart('0'));

                            objDatosFinalOrdenado.Add(consecutivoProcuni, item);
                        }
                    }

                    if (objDatosFinalOrdenado.Count != objDatosFinal.Count)
                    {
                        foreach (var item in objDatosFinal)
                        {
                            if (Variables.Variables.DiccionarioDual.ContainsKey(item.Key))
                            {
                                int consecutivoProcuni = objDatosFinalOrdenado.Count + 1;

                                objDatosFinalOrdenado.Add(consecutivoProcuni, item);
                            }
                        }
                    }
                    #endregion

                    if (objDatosFinal.Any())
                    {
                        ProcesoSalidasImpresion(objDatosFinalOrdenado.Values, pRuta, pNombreArchivo, pLote);
                    }
                    break;

                case "4":

                    Dictionary<string, List<string>> datosOrdenadosTelefono = new Dictionary<string, List<string>>();

                    foreach (var DatoExtracto in pDatosImprimir)
                    {
                        string llave = DatoExtracto.Value.FirstOrDefault().Split('|').ElementAt(6).Trim();

                        if (!datosOrdenadosTelefono.ContainsKey(llave))
                        {
                            datosOrdenadosTelefono.Add(llave, DatoExtracto.Value);
                        }
                    }

                    objDatos = (from busqueda in datosOrdenadosTelefono
                                where 
                                !Variables.Variables.DatosInsumoExtraerLlanos.ContainsKey(busqueda.Key) &&
                                !Variables.Variables.DatosInsumoLlanosEnvioSMS.ContainsKey(busqueda.Key) &&
                                (!Variables.Variables.DatosInsumoDistribucionEmailLlanos.ContainsKey(busqueda.Key) ||
                                (Variables.Variables.DatosInsumoDistribucionEmailLlanos.ContainsKey(busqueda.Key) && Variables.Variables.DatosInsumoDualLlanos.ContainsKey(busqueda.Key)))
                                select busqueda).ToDictionary(x => x.Key).Values;

                    if (objDatos.Any())
                    {
                        ProcesoSalidasImpresionLLanos(objDatos, pRuta, pNombreArchivo, pLote);
                    }
                    break;

            }

            #endregion
        }

        /// <summary>
        /// Metodo que Procesa Salidas Impresion
        /// </summary>
        /// <param name="pDatos"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        /// <param name="pLote"></param>
        private void ProcesoSalidasImpresion(IEnumerable<KeyValuePair<string, List<string>>> pDatos, string pRuta, string pNombreArchivo, string pLote)
        {
            #region ProcesoSalidasImpresion
            List<string> resultado = new List<string>();
            int consecutivo = 1;
            int consecutivoInternoDivision = 0;
            int consecutivoInternoArchivo = 1;
            string cuenta = string.Empty;
            string archivoActual = string.Empty;

            foreach (var datoCuenta in pDatos)
            {
                string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";

                if (datoCuenta.Value.FirstOrDefault().Substring(0, 4) == "1AAA")
                {
                    consecutivoInternoDivision++;
                    cuenta = datoCuenta.Value.FirstOrDefault().Split('|')[7].Trim();
                    archivoActual = $"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal";

                    // Se agrega la cuenta en el archivo final                                
                    if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(cuenta))
                        Variables.Variables.ArchivoSalidaFinal.Add(cuenta, archivoActual);

                    if (consecutivoInternoDivision == 8001)
                    {
                        //Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);
                        Helpers.EscribirEnArchivo(archivoActual, resultado);
                        consecutivoInternoArchivo++;
                        consecutivoInternoDivision = 1;
                        resultado.Clear();
                    }
                }

                Variables.Variables.DiccionarioExtractosFormateados[datoCuenta.Key][0] = Variables.Variables.DiccionarioExtractosFormateados[datoCuenta.Key][0].Replace("KitXXXX", nuevoConsecutivo);

                resultado.AddRange(datoCuenta.Value);

                consecutivo++;
            }

            Helpers.EscribirEnArchivo(archivoActual, resultado);
            //Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);

            resultado.Clear();
            #endregion
        }

        /// <summary>
        /// Metodo que Procesa Salidas Impresion LLanos
        /// </summary>
        /// <param name="pDatos"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        /// <param name="pLote"></param>
        private void ProcesoSalidasImpresionLLanos(IEnumerable<KeyValuePair<string, List<string>>> pDatos, string pRuta, string pNombreArchivo, string pLote)
        {
            #region ProcesoSalidasImpresion
            List<string> resultado = new List<string>();
            int consecutivo = 1;
            int consecutivoInternoDivision = 0;
            int consecutivoInternoArchivo = 1;
            string cuenta = string.Empty;
            string archivoActual = string.Empty;

            foreach (var datoCuenta in pDatos)
            {
                string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";

                var nuevo1AAA = from n in datoCuenta.Value
                                where n.Substring(0, 4) == "1AAA"
                                select n.Replace("KitXXXX", nuevoConsecutivo);

                if (datoCuenta.Value.FirstOrDefault().Substring(0, 4) == "1AAA")
                {
                    consecutivoInternoDivision++;
                    cuenta = datoCuenta.Value.FirstOrDefault().Split('|')[1].Substring(1).Trim();
                    archivoActual = $"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal";

                    // Se agrega la cuenta en el archivo final                                
                    if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(cuenta))
                        Variables.Variables.ArchivoSalidaFinal.Add(cuenta, archivoActual);

                    if (consecutivoInternoDivision == 8001)
                    {
                        //Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);
                        Helpers.EscribirEnArchivo(archivoActual, resultado);
                        consecutivoInternoArchivo++;
                        consecutivoInternoDivision = 1;
                        resultado.Clear();
                    }
                }

                Variables.Variables.DiccionarioExtractosFormateados[cuenta][0] = Variables.Variables.DiccionarioExtractosFormateados[cuenta][0].Replace("KitXXXX", nuevoConsecutivo);

                resultado.AddRange(datoCuenta.Value);

                consecutivo++;
            }

            Helpers.EscribirEnArchivo(archivoActual, resultado);
            //Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);

            resultado.Clear();
            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida Num Hojas
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaNumHojas(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pLote)
        {
            #region EscribirDatosSalidaNumHojas
            List<string> resultado = new List<string>();
            int consecutivo = 1;

            var busquedaCuentasImpresion = (from busqueda in pDatosImprimir
                                            where !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.Diferencias.ContainsKey(busqueda.Key) &&
                                            (from busquedaEE3 in busqueda.Value
                                             where busquedaEE3.Substring(0, 4).Equals("1EE3")
                                             select busquedaEE3).Count() >= 35000
                                            select busqueda).ToDictionary(x => x.Key).Values;

            if (busquedaCuentasImpresion.Any())
            {
                foreach (var datoCuenta in busquedaCuentasImpresion)
                {
                    Variables.Variables.NumHojas.Add(datoCuenta.Key, string.Empty);

                    string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";

                    var nuevo1AAA = from n in datoCuenta.Value
                                    where n.Substring(0, 4) == "1AAA"
                                    select n.Replace("KitXXXX", nuevoConsecutivo);

                    // Se actualiza el Consecutivo del diccionario Original formateado
                    Variables.Variables.DiccionarioExtractosFormateados[datoCuenta.Key][0] = Variables.Variables.DiccionarioExtractosFormateados[datoCuenta.Key][0].Replace(Variables.Variables.DiccionarioExtractosFormateados[datoCuenta.Key][0].Split('|').ElementAt(1), nuevoConsecutivo);

                    consecutivo++;
                    resultado.AddRange(datoCuenta.Value);

                    // Se agrega la cuenta en el archivo final
                    if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(datoCuenta.Key))
                        Variables.Variables.ArchivoSalidaFinal.Add(datoCuenta.Key, $"{pRuta}\\{pNombreArchivo}");

                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", resultado);
            }

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida Clientes Esepeciales
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaClientesEsepeciales(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pLote)
        {
            #region EscribirDatosSalidaClientesEsepeciales
            List<string> datosFinales = new List<string>();
            int consecutivo = 1;

            var busquedaCuentas = (from busqueda in pDatosImprimir
                                   where Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey($"{busqueda.Key}-{busqueda.Value.FirstOrDefault().Split('|').ElementAt(9)}")
                                   select busqueda).ToDictionary(x => x.Key).Values;

            foreach (var dato in busquedaCuentas)
            {
                if (pDatosImprimir.ContainsKey(dato.Key))
                {
                    string fechaPeriodoProceso = pDatosImprimir[dato.Key].FirstOrDefault().Split('|').ElementAt(11).Substring(3).Replace("/", "");
                    string fechaPeriodoInsumo = Variables.Variables.DatosInsumoClientesEspecialesDatos[$"{dato.Key}-{dato.Value.FirstOrDefault().Split('|').ElementAt(9)}"].Split('|').ElementAt(9);

                    string cicloProceso = pDatosImprimir[dato.Key].FirstOrDefault().Split('|').ElementAt(9);
                    string cicloInsumo = Variables.Variables.DatosInsumoClientesEspecialesDatos[$"{dato.Key}-{dato.Value.FirstOrDefault().Split('|').ElementAt(9)}"].Split('|').ElementAt(2);

                    if (fechaPeriodoProceso == fechaPeriodoInsumo && cicloProceso == cicloInsumo)
                    {

                        string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(6, '0')}";

                        var nuevo1AAA = from n in dato.Value
                                        where n.Substring(0, 4) == "1AAA"
                                        select n.Replace("KitXXXX", nuevoConsecutivo);

                        // Se actualiza el Consecutivo del diccionario Original formateado
                        Variables.Variables.DiccionarioExtractosFormateados[dato.Key][0] = Variables.Variables.DiccionarioExtractosFormateados[dato.Key][0].Replace("KitXXXX", nuevoConsecutivo);

                        consecutivo++;

                        datosFinales.AddRange(dato.Value);

                        // Se agrega la cuenta en el archivo final                                
                        if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(dato.Key))
                            Variables.Variables.ArchivoSalidaFinal.Add(dato.Key, $"{pRuta}\\{pNombreArchivo}");
                    }
                }
            }

            if (datosFinales.Any())
            {
                Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", datosFinales);
            }

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Datos Salida Impresion Normal
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaImpresionNormal(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaImpresionNormal
            List<string> resultado = new List<string>();
            int consecutivo = 1;
            string cuenta = string.Empty;

            var busquedaCuentasImpresion = (from busqueda in pDatosImprimir
                                            select busqueda).ToDictionary(x => x.Key).Values;

            if (busquedaCuentasImpresion.Any())
            {
                foreach (var datoCuenta in busquedaCuentasImpresion)
                {
                    string nuevoConsecutivo = $"KIT{consecutivo.ToString().PadLeft(6, '0')}";

                    var nuevo1AAA = from n in datoCuenta.Value
                                    where n.Substring(0, 4) == "1AAA"
                                    select n.Replace("KitXXXXX", nuevoConsecutivo);

                    cuenta = nuevo1AAA.FirstOrDefault().Split('|')[7];

                    // Se agrega la cuenta en el archivo final                                
                    if (!Variables.Variables.ArchivoSalidaFinal.ContainsKey(cuenta))
                        Variables.Variables.ArchivoSalidaFinal.Add(cuenta, $"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}.sal");

                    resultado.Add(nuevo1AAA.FirstOrDefault());

                    //datoCuenta.Value.RemoveAt(0);

                    Variables.Variables.DiccionarioExtractosFormateados[datoCuenta.Key][0] = Variables.Variables.DiccionarioExtractosFormateados[datoCuenta.Key][0].Replace("KitXXXX", nuevoConsecutivo);

                    resultado.AddRange(datoCuenta.Value);


                    consecutivo++;
                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}.sal", resultado);

                resultado.Clear();
            }

            #endregion
        }

        #endregion
    }
}
