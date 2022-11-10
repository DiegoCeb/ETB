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
        /// metodo Procesamiento
        /// </summary>
        public Procesamiento()
        { }

        public void AdecuarTrabajoApp(string pIdentificadorProceso)
        {
            #region AdecuarTrabajoApp
            //Creacion Carpeta Salida
            Helpers.RutaProceso = Directory.CreateDirectory($"{Utilidades.LeerAppConfig("RutaSalida")}\\{pIdentificadorProceso}{DateTime.Now:yyyyMMddhhmmss}").FullName;

            //Mover de entrada a originales y trabajar con estos archivos
            Helpers.RutaOriginales = Directory.CreateDirectory($"{Utilidades.LeerAppConfig("RutaOriginales")}\\{pIdentificadorProceso}Originales{DateTime.Now:yyyyMMddhhmmss}").FullName;
            Helpers.MoverArchivos(Utilidades.LeerAppConfig("RutaEntrada"), Helpers.RutaOriginales);

            switch (pIdentificadorProceso)
            {
                case "Hipotecario":
                    break;

                default:
                    //Mover archivos de entrada a original y la carpeta completa de insumos
                    Helpers.RutaInsumos = $"{Helpers.RutaOriginales}\\Insumos";
                    Helpers.CopiarCarpetaCompleta(Utilidades.LeerAppConfig("RutaInsumos"), Helpers.RutaInsumos, true);

                    //Cargue Lista Insumos para despues usar en el formateo desde originales
                    CargueGeneralInsumos(Helpers.RutaInsumos);
                    break;
            }


            #endregion
        }

        public void EjecutarProcesoMasivo(string pRutaArchivosProcesar)
        {
            #region EjecutarProcesoMasivo
            var archivos = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                           where !busqueda.Contains("CONTEO") && !busqueda.Contains("IDENTIFICACION") && !busqueda.Contains("premaestra") //TODO: se omiten estos archivos peor ahi que revisar para que sirven
                           select busqueda;

            var archivoPeriodo = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                                 where busqueda.Contains("IDENTIFICACION")
                                 select busqueda;

            string periodo = File.ReadAllLines(archivoPeriodo.FirstOrDefault()).ToList().ElementAt(0).Split('\t').ElementAt(1).PadLeft(6, '0');
            string lote = File.ReadAllLines(archivoPeriodo.FirstOrDefault()).ToList().ElementAt(0).Split('\t').ElementAt(5);


            foreach (var archivo in archivos)
            {
                _ = new ProcesoMasivos(archivo, periodo);
            }

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "1", lote);

            // Se crean los reportes
            string rutaReportes = Path.Combine(App.ControlInsumos.Helpers.RutaProceso, "Reportes");
            _ = new ReportesMasivos(Variables.Variables.DiccionarioExtractosFormateados, App.ControlInsumos.Helpers.RutaProceso,lote);         



        #endregion
        }

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

            foreach (var archivo in archivos)
            {
                _ = new ProcesoDatos(archivo);
            }

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "2", lote);
            #endregion
        }

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


            foreach (var archivo in archivos)
            {
                _ = new ProcesoGobiernos(archivo);
            }

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "3", lote);
            #endregion
        }

        public void EjecutarProcesoLLanos(string pRutaArchivosProcesar)
        {
            #region EjecutarProcesoGobiernos

            var archivos = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                           select busqueda;

            foreach (var archivo in archivos)
            {
                _ = new ProcesoLlanos(archivo);
            }

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "4");
            #endregion
        }

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

            foreach (var archivo in archivos)
            {
                _ = new ProcesoCreditoHipotecario(archivo);
            }

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "5");
            #endregion
        }

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

            foreach (var archivo in archivos)
            {
                _ = new ProcesoAnexosVerdes(pRutaArchivosProcesar);
            }

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "6");
            #endregion
        }

        public void CargueGeneralInsumos(string Pruta)
        {
            #region CargueGeneralInsumos
            foreach (var Carpeta in Directory.GetDirectories(Pruta))
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
                }

            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRutaSalida"></param>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pIdentificadorProceso"></param>
        private void EscribirSalidasProceso(string pRutaSalida, Dictionary<string, List<string>> pDatosImprimir, string pIdentificadorProceso, string pLote = "")
        {
            #region EscribirSalidasProceso
            switch (pIdentificadorProceso)
            {
                case "1": //Masivos
                    EscribirDatosSalidaCompleto(pDatosImprimir, $"{pRutaSalida}", $"COMPLETO.sal", pLote);
                    EscribirDatosSalidaNoImprimir($"{pRutaSalida}", $"I_NO_IMPRIMIR.sal");
                    EscribirDatosSalidaSms(pDatosImprimir, $"{pRutaSalida}", $"I_ENVIO_SMS.sal");
                    EscribirDatosSalidaErrorLte(pDatosImprimir, $"{pRutaSalida}", $"I_ERROR_FACTURA_LTE.sal");
                    EscribirDatosSalidaDiferencias(pDatosImprimir, $"{pRutaSalida}", $"I_DIFERENCIAS.sal");
                    EscribirDatosSalidaEmail(pDatosImprimir, $"{pRutaSalida}", $"I_FACTURA_SOLO_EMAIL.sal");
                    EscribirDatosSalidaDistribucionEspecial(pDatosImprimir, $"{pRutaSalida}", $"I_DISTRIBUCION_ESPECIAL.sal");
                    EscribirDatosSalidaImpresion(pDatosImprimir, $"{pRutaSalida}", $"Courier00000000I.sal", pLote);
                    EscribirDatosSalidaOtros(pDatosImprimir, $"{pRutaSalida}", $"I_OTROS.sal");
                    break;

                case "2": //Datos
                    EscribirDatosSalidaCompleto(pDatosImprimir, $"{pRutaSalida}", $"COMPLETO.sal", pLote);
                    EscribirDatosSalidaNoImprimir($"{pRutaSalida}", $"I_NO_IMPRIMIR.sal");
                    EscribirDatosSalidaNumHojas(pDatosImprimir, $"{pRutaSalida}", $"I_NUMHOJAS.sal");
                    EscribirDatosSalidaClientesEsepeciales(pDatosImprimir, $"{pRutaSalida}", $"INS_CLIENTES_ESPECIALES.sal");
                    EscribirDatosSalidaEmail(pDatosImprimir, $"{pRutaSalida}", $"I_FACTURA_SOLO_EMAIL.sal");
                    EscribirDatosSalidaDistribucionEspecial(pDatosImprimir, $"{pRutaSalida}", $"I_DISTRIBUCION_ESPECIAL.sal");
                    EscribirDatosSalidaImpresion(pDatosImprimir, $"{pRutaSalida}", $"Courier00000000I.sal", pLote);
                    EscribirDatosSalidaOtros(pDatosImprimir, $"{pRutaSalida}", $"I_OTROS.sal");
                    break;

                case "3": //Gobiernos
                    EscribirDatosSalidaCompleto(pDatosImprimir, $"{pRutaSalida}", $"COMPLETO.sal", pLote);
                    EscribirDatosSalidaNoImprimir($"{pRutaSalida}", $"I_NO_IMPRIMIR.sal");
                    EscribirDatosSalidaNumHojas(pDatosImprimir, $"{pRutaSalida}", $"I_NUMHOJAS.sal");
                    EscribirDatosSalidaEmail(pDatosImprimir, $"{pRutaSalida}", $"I_FACTURA_SOLO_EMAIL.sal");
                    EscribirDatosSalidaDistribucionEspecial(pDatosImprimir, $"{pRutaSalida}", $"I_DISTRIBUCION_ESPECIAL.sal");
                    EscribirDatosSalidaImpresion(pDatosImprimir, $"{pRutaSalida}", $"Courier00000000I.sal", pLote);
                    EscribirDatosSalidaOtros(pDatosImprimir, $"{pRutaSalida}", $"I_OTROS.sal");
                    break;

                case "4": //Llanos
                    EscribirDatosSalidaCompleto(pDatosImprimir, $"{pRutaSalida}", $"COMPLETO.sal", pLote);
                    EscribirDatosSalidaSms(pDatosImprimir, $"{pRutaSalida}", $"I_ENVIO_SMS.sal");
                    EscribirDatosSalidaNoImprimir($"{pRutaSalida}", $"I_NO_IMPRIMIR.sal");
                    EscribirDatosSalidaEmail(pDatosImprimir, $"{pRutaSalida}", $"I_FACTURA_SOLO_EMAIL.sal");
                    EscribirDatosSalidaImpresion(pDatosImprimir, $"{pRutaSalida}", $"Courier00000000I.sal", pLote);
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
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaCompleto(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pLote)
        {
            #region EscribirDatosSalidaCompleto
            List<string> resultado = new List<string>();
            int consecutivo = 1;

            var datosImprimirFinal = new Dictionary<string, List<string>>(pDatosImprimir);

            foreach (var datoLinea in datosImprimirFinal)
            {
                string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(8, '0')}";

                var nuevo1AAA = from n in datoLinea.Value
                                where n.Substring(0, 4) == "1AAA"
                                select n.Replace("KitXXXX", nuevoConsecutivo);

                string nuevo1AAAFinal = nuevo1AAA.FirstOrDefault();

                resultado.Add(nuevo1AAAFinal);

                datoLinea.Value.RemoveAt(0);

                resultado.AddRange(datoLinea.Value);

                datoLinea.Value.Insert(0, nuevo1AAAFinal.Replace(nuevoConsecutivo, "KitXXXX"));

                consecutivo++;
            }

            Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", resultado);

            resultado.Clear();
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaNoImprimir(string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaNoImprimir
            List<string> resultado = new List<string>();
            int consecutivoInternoDivision = 0;
            int consecutivoInternoArchivo = 1;

            if (Variables.Variables.CuentasNoImprimir.Any())
            {
                foreach (var linea in Variables.Variables.CuentasNoImprimir.SelectMany(x => x.Value))
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
                    }

                    resultado.Add(linea);
                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);

                resultado.Clear();
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaSms(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaSms
            List<string> resultado = new List<string>();
            int consecutivoInternoDivision = 0;
            int consecutivoInternoArchivo = 1;

            var busquedaCuentas = from busqueda in pDatosImprimir
                                  where Variables.Variables.DatosInsumoCuentasEnvioSms.ContainsKey(busqueda.Key) &&
                                   !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key)
                                  select busqueda;

            if (busquedaCuentas.Any())
            {
                foreach (var linea in busquedaCuentas.SelectMany(x => x.Value))
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
                    }

                    resultado.Add(linea);
                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);

                resultado.Clear();
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaEmail(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaEmail
            List<string> resultado = new List<string>();
            int consecutivoInternoDivision = 0;
            int consecutivoInternoArchivo = 1;

            var busquedaCuentas = from busqueda in pDatosImprimir
                                  where Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                                  !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                  !Variables.Variables.DatosErrorLTE.ContainsKey(busqueda.Key) &&
                                  !Variables.Variables.Diferencias.ContainsKey(busqueda.Key) &&
                                  !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                  !Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey(busqueda.Key)
                                  select busqueda;

            if (busquedaCuentas.Any())
            {
                foreach (var linea in busquedaCuentas.SelectMany(x => x.Value))
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
                    }

                    resultado.Add(linea);
                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);

                resultado.Clear();
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaDistribucionEspecial(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaDistribucionEspecial
            List<string> datosFinales = new List<string>();

            var busquedaCuentas = (from busqueda in pDatosImprimir
                                   where Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                   !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                   !Variables.Variables.NumHojas.ContainsKey(busqueda.Key)
                                   select busqueda).ToDictionary(x => x.Key).Values;

            foreach (var dato in busquedaCuentas)
            {
                if (pDatosImprimir.ContainsKey(dato.Key))
                {
                    string fechaPeriodoProceso = pDatosImprimir[dato.Key].FirstOrDefault().Split('|').ElementAt(11).Substring(3);
                    string fechaPeriodoInsumo = Variables.Variables.DatosInsumoDistribucionEspecial[dato.Key].FirstOrDefault().Split('|').ElementAt(2);

                    if (fechaPeriodoProceso == fechaPeriodoInsumo)
                    {
                        datosFinales.AddRange(dato.Value);
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
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaErrorLte(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaErrorLte
            List<string> resultado = new List<string>();

            if (Variables.Variables.DatosErrorLTE.Any())
            {
                foreach (var linea in Variables.Variables.DatosErrorLTE.SelectMany(x => x.Value))
                {
                    resultado.Add(linea);
                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", resultado);

                resultado.Clear();
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaDiferencias(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaDiferencias
            List<string> resultado = new List<string>();

            var busquedaCuentas = (from busqueda in pDatosImprimir
                                   let canal1BBA = busqueda.Value.Find(x => x.Substring(0, 4).Equals("1BBA"))
                                   where !string.IsNullOrEmpty(canal1BBA) && (Convert.ToDecimal(Helpers.FormatearCampos(TiposFormateo.Decimal03, busqueda.Value.FirstOrDefault().Split('|').ElementAt(10))) - Convert.ToDecimal(Helpers.FormatearCampos(TiposFormateo.Decimal03, canal1BBA.Split('|').ElementAt(2)))) > 5 &&
                                   !Variables.Variables.DatosErrorLTE.ContainsKey(busqueda.Key)
                                   select busqueda).ToDictionary(x => x.Key).Values;

            foreach (var dato in busquedaCuentas)
            {
                Variables.Variables.Diferencias.Add(dato.Key, string.Empty);

                if (pDatosImprimir.ContainsKey(dato.Key))
                {
                    resultado.AddRange(dato.Value);
                }
            }

            if (resultado.Any())
            {
                Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", resultado);
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaOtros(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaOtros
            List<string> resultado = new List<string>();

            var busquedaCuentasImpresion = (from busqueda in pDatosImprimir
                                            where !Variables.Variables.DatosInsumoProcuni.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.DatosErrorLTE.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.Diferencias.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.DatosInsumoCuentasEnvioSms.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey(busqueda.Key)
                                            select busqueda).ToDictionary(x => x.Key).Values;

            if (busquedaCuentasImpresion.Any())
            {
                foreach (var datoCuenta in busquedaCuentasImpresion)
                {
                    resultado.AddRange(datoCuenta.Value);
                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", resultado);
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaImpresion(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo, string pLote)
        {
            #region EscribirDatosSalidaImpresion
            List<string> resultado = new List<string>();
            int consecutivo = 1;
            int consecutivoInternoDivision = 0;
            int consecutivoInternoArchivo = 1;

            var busquedaCuentasImpresion = (from busqueda in pDatosImprimir
                                            where Variables.Variables.DatosInsumoProcuni.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.CuentasNoImprimir.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.DatosErrorLTE.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.Diferencias.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.NumHojas.ContainsKey(busqueda.Key) &&
                                            !Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey(busqueda.Key)
                                            select busqueda).ToDictionary(x => x.Key).Values;

            if (busquedaCuentasImpresion.Any())
            {
                foreach (var datoCuenta in busquedaCuentasImpresion)
                {
                    string nuevoConsecutivo = $"{pLote}_{consecutivo.ToString().PadLeft(8, '0')}";

                    var nuevo1AAA = from n in datoCuenta.Value
                                    where n.Substring(0, 4) == "1AAA"
                                    select n.Replace("KitXXXX", nuevoConsecutivo);

                    if (datoCuenta.Value.FirstOrDefault().Substring(0, 4) == "1AAA")
                    {
                        consecutivoInternoDivision++;

                        if (consecutivoInternoDivision == 8001)
                        {
                            Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);
                            consecutivoInternoArchivo++;
                            consecutivoInternoDivision = 1;
                            resultado.Clear();
                        }
                    }

                    resultado.Add(nuevo1AAA.FirstOrDefault());

                    datoCuenta.Value.RemoveAt(0);

                    resultado.AddRange(datoCuenta.Value);

                    consecutivo++;
                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}_{consecutivoInternoArchivo.ToString().PadLeft(3, '0')}.sal", resultado);

                resultado.Clear();
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaNumHojas(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaNumHojas
            List<string> resultado = new List<string>();

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
                    resultado.AddRange(datoCuenta.Value);
                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{pNombreArchivo}", resultado);
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaClientesEsepeciales(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaClientesEsepeciales
            List<string> datosFinales = new List<string>();

            var busquedaCuentas = (from busqueda in pDatosImprimir
                                   where Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey(busqueda.Key)
                                   select busqueda).ToDictionary(x => x.Key).Values;

            foreach (var dato in busquedaCuentas)
            {
                if (pDatosImprimir.ContainsKey(dato.Key))
                {
                    string fechaPeriodoProceso = pDatosImprimir[dato.Key].FirstOrDefault().Split('|').ElementAt(11).Substring(3).Replace("/", "");
                    string fechaPeriodoInsumo = Variables.Variables.DatosInsumoClientesEspecialesDatos[dato.Key].Split('|').ElementAt(9);

                    string cicloProceso = pDatosImprimir[dato.Key].FirstOrDefault().Split('|').ElementAt(9);
                    string cicloInsumo = Variables.Variables.DatosInsumoClientesEspecialesDatos[dato.Key].Split('|').ElementAt(2);

                    if (fechaPeriodoProceso == fechaPeriodoInsumo && cicloProceso == cicloInsumo)
                    {
                        datosFinales.AddRange(dato.Value);
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
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaImpresionNormal(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaImpresionNormal
            List<string> resultado = new List<string>();
            int consecutivo = 1;

            var busquedaCuentasImpresion = (from busqueda in pDatosImprimir
                                            select busqueda).ToDictionary(x => x.Key).Values;

            if (busquedaCuentasImpresion.Any())
            {
                foreach (var datoCuenta in busquedaCuentasImpresion)
                {
                    string nuevoConsecutivo = $"KIT{consecutivo.ToString().PadLeft(8, '0')}";

                    var nuevo1AAA = from n in datoCuenta.Value
                                    where n.Substring(0, 4) == "1AAA"
                                    select n.Replace("KitXXXXX", nuevoConsecutivo);

                    resultado.Add(nuevo1AAA.FirstOrDefault());

                    datoCuenta.Value.RemoveAt(0);

                    resultado.AddRange(datoCuenta.Value);

                    consecutivo++;
                }

                Helpers.EscribirEnArchivo($"{pRuta}\\{Path.GetFileNameWithoutExtension(pNombreArchivo)}.sal", resultado);

                resultado.Clear();
            }

            #endregion
        }
        

    }
}
