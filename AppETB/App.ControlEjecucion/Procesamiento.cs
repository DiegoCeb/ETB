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

            //Mover archivos de entrada a original y la carpeta completa de insumos
            Helpers.RutaInsumos = $"{Helpers.RutaOriginales}\\Insumos";
            Helpers.CopiarCarpetaCompleta(Utilidades.LeerAppConfig("RutaInsumos"), Helpers.RutaInsumos, true);

            //Cargue Lista Insumos para despues usar en el formateo desde originales
            CargueGeneralInsumos(Helpers.RutaInsumos);
            #endregion
        }

        public void EjecutarProcesoMasivo(string pRutaArchivosProcesar)
        {
            #region EjecutarProcesoMasivo

            //Llamada a App.ControlLogicaProcesos
            //TODO: si es muy lento esto se puede lanzar en hilos ya que se llena el diccionario de formateo y al finalizar con un ContinueWith del hilo se encadena la escritura

            var archivos = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                           where !busqueda.Contains("CONTEO") && !busqueda.Contains("IDENTIFICACION") && !busqueda.Contains("premaestra") //TODO: se omiten estos archivos peor ahi que revisar para que sirven
                           select busqueda;

            var archivoPeriodo = from busqueda in Directory.GetFiles(pRutaArchivosProcesar)
                                 where busqueda.Contains("IDENTIFICACION")
                                 select busqueda;

            string periodo = File.ReadAllLines(archivoPeriodo.FirstOrDefault()).ToList().ElementAt(0).Split('\t').ElementAt(1).PadLeft(6,'0');

#if DEBUG == false
            var result = Parallel.ForEach(archivos, archivo =>
    {
        var hilo = Task.Run(() =>
        {
            _ = new ProcesoMasivos(archivo, periodo);
        });

        hilo.Wait();
    }); 
#endif

            foreach (var archivo in archivos)
            {
                _ = new ProcesoMasivos(archivo, periodo);
            }

            //Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            EscribirSalidasProceso($"{App.ControlInsumos.Helpers.RutaProceso}", Variables.Variables.DiccionarioExtractosFormateados, "1");
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
        private void EscribirSalidasProceso(string pRutaSalida, Dictionary<string, List<string>> pDatosImprimir, string pIdentificadorProceso)
        {
            #region EscribirSalidasProceso
            switch (pIdentificadorProceso)
            {
                case "1": //Masivos
                    EscribirDatosSalidaCompleto(pDatosImprimir, $"{pRutaSalida}\\Completo", $"MasivoCompleto{DateTime.Now:yyyyMMddhhmm}.sal");
                    EscribirDatosSalidaNoImprimir($"{pRutaSalida}\\NoImprimir", $"MasivoNoImprimir{DateTime.Now:yyyyMMddhhmm}.sal");
                    EscribirDatosSalidaSms(pDatosImprimir, $"{pRutaSalida}\\SMS", $"MasivoSMS{DateTime.Now:yyyyMMddhhmm}.sal");
                    EscribirDatosSalidaEmail(pDatosImprimir, $"{pRutaSalida}\\Email", $"MasivoEmail{DateTime.Now:yyyyMMddhhmm}.sal");
                    EscribirDatosSalidaDistribucionEspecial(pDatosImprimir, $"{pRutaSalida}\\DistribucionEspecial", $"MasivoDistribucionEspecial{DateTime.Now:yyyyMMddhhmm}.sal");
                    EscribirDatosSalidaDiferencias(pDatosImprimir, $"{pRutaSalida}\\Diferencias", $"MasivoDiferencias{DateTime.Now:yyyyMMddhhmm}.sal");
                    EscribirDatosSalidaImpresion(pDatosImprimir, $"{pRutaSalida}\\Impresion", $"MasivoImpresion{DateTime.Now:yyyyMMddhhmm}.sal");
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
        private void EscribirDatosSalidaCompleto(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaCompleto
            List<string> resultado = new List<string>();
            string rutaCompleto = Directory.CreateDirectory(pRuta).FullName;

            foreach (var datoLinea in pDatosImprimir.SelectMany(x => x.Value))
            {
                resultado.Add(datoLinea);
            }

            Helpers.EscribirEnArchivo($"{rutaCompleto}\\{pNombreArchivo}", resultado);

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
            string rutaNoImprimir = string.Empty;

            if (Variables.Variables.CuentasNoImprimir.Any())
            {
                rutaNoImprimir = Directory.CreateDirectory(pRuta).FullName;

                foreach (var linea in Variables.Variables.CuentasNoImprimir.SelectMany(x => x.Value))
                {
                    resultado.Add(linea);
                }

                Helpers.EscribirEnArchivo($"{rutaNoImprimir}\\{pNombreArchivo}", resultado);

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
            string rutaSms = string.Empty;

            var busquedaCuentas = from busqueda in pDatosImprimir
                                  where Variables.Variables.DatosInsumoCuentasEnvioSms.ContainsKey(busqueda.Key)
                                  select busqueda;

            if (busquedaCuentas.Any())
            {
                rutaSms = Directory.CreateDirectory(pRuta).FullName;

                foreach (var linea in busquedaCuentas.SelectMany(x => x.Value))
                {
                    resultado.Add(linea);
                }

                Helpers.EscribirEnArchivo($"{rutaSms}\\{pNombreArchivo}", resultado);

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
            string rutaEmail = string.Empty;

            var busquedaCuentas = from busqueda in pDatosImprimir
                                  where Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(busqueda.Key)
                                  select busqueda;

            if (busquedaCuentas.Any())
            {
                rutaEmail = Directory.CreateDirectory(pRuta).FullName;

                foreach (var linea in busquedaCuentas.SelectMany(x => x.Value))
                {
                    resultado.Add(linea);
                }

                Helpers.EscribirEnArchivo($"{rutaEmail}\\{pNombreArchivo}", resultado);

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
            List<string> resultado = new List<string>();

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
            

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRuta"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirDatosSalidaImpresion(Dictionary<string, List<string>> pDatosImprimir, string pRuta, string pNombreArchivo)
        {
            #region EscribirDatosSalidaImpresion
            List<string> resultado = new List<string>();

            #endregion
        }
    }
}
