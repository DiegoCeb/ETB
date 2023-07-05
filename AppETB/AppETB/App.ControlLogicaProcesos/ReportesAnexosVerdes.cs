using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.ControlInsumos;

namespace App.ControlLogicaProcesos
{
    /// <summary>
    /// Clase ReportesAnexosVerdes
    /// </summary>
    public class ReportesAnexosVerdes
    {

        #region Variables
        // Utiles
        public static Dictionary<string, List<string>> DiccionarioExtractosReporte = new Dictionary<string, List<string>>();
        string cuenta = string.Empty;
        string factura = string.Empty;
        string rutaSalida = string.Empty;
        string lote = string.Empty;

        // Variables SMS
        int contadorRegistrosSMS = 0;
        string nombreArchivoDetSMS = string.Empty;
        List<string> lisCiclosSMS = new List<string>();

        // Listas Reportes
        List<string> listaReporteMaestra = new List<string>();
        List<string> listaReporteDistrEspecial = new List<string>();
        List<string> listaReporteInsertos = new List<string>();
        #endregion

        #region Construcctores
        /// <summary>
        /// 
        /// </summary>
        public ReportesAnexosVerdes()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRutaSalida"></param>
        /// <param name="pLote"></param>
        public ReportesAnexosVerdes(Dictionary<string, List<string>> pDatosImprimir, string pRutaSalida, string pLote)
        {
            #region ProcesoMasivos
            try
            {
                rutaSalida = pRutaSalida;
                lote = pLote;
                Ejecutar(pDatosImprimir);
            }
            catch (Exception ex)
            {
                DatosError StructError = Helpers.ExtraerExcepcion(ex);
                Helpers.EscribirLogVentana(StructError, true);
            }
            #endregion
        } 
        #endregion

        /// <summary>
        /// Metodo Ejecutar
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        public void Ejecutar(Dictionary<string, List<string>> pDatosImprimir)
        {
            #region Ejecutar
            Helpers.CrearCarpeta(rutaSalida + @"\Reportes");
            CargarDiccionario();
            ExtraccionReportes(DiccionarioExtractosReporte);            
            #endregion
        }

        /// <summary>
        /// Metodo CargarDiccionario
        /// </summary>
        private void CargarDiccionario()
        {
            #region CargarDiccionario
            DiccionarioExtractosReporte = Variables.Variables.DiccionarioExtractosFormateados;

            // Agrego los datos de No Imprimir
            foreach (var keyNoImprimir in Variables.Variables.CuentasNoImprimir.Keys)
            {
                DiccionarioExtractosReporte.Add(keyNoImprimir, new List<string>(Variables.Variables.CuentasNoImprimir[keyNoImprimir]));
            }

            // Agrego los datos de ErrorLTE
            foreach (var keyErrorLTE in Variables.Variables.DatosErrorLTE.Keys)
            {
                DiccionarioExtractosReporte.Add(keyErrorLTE, new List<string>(Variables.Variables.DatosErrorLTE[keyErrorLTE]));
            } 
            #endregion
        }

        #region Obtener Reporte

        /// <summary>
        /// Metodo que obtiene Extraccion Reportes
        /// </summary>
        /// <param name="pDatosProcesados"></param>
        private void ExtraccionReportes(Dictionary<string, List<string>> pDatosProcesados)
        {
            #region ExtraccionReportes
            List<string> listReporte = new List<string>();

            foreach (var datosSal in pDatosProcesados.Values)
            {
                // Rpt Maestra
                listReporte.AddRange(GetReporteMaestra(datosSal.ToList()));
                if (listReporte.Count > 0)
                    listaReporteMaestra.AddRange(listReporte.ToList());
                listReporte.Clear();

                // Rpt Resumen Distribucion Especial
                listReporte = GetReporteDistribucionEspecial(datosSal.ToList());
                if (listReporte.Count > 0)
                    listaReporteDistrEspecial.AddRange(listReporte.ToList());
                listReporte.Clear();
            }

            // Escribir Maestra
            EscribirReporteMaestra(listaReporteMaestra);
            listaReporteMaestra.Clear();

            // Escribir Distribucion Especial
            EscribirReporteDistribucionEspecial(listaReporteDistrEspecial);
            listaReporteDistrEspecial.Clear();

            // Rpt Resumen Maestra
            listReporte = GetLineaResumenMaestra(DiccionarioExtractosReporte);
            if (listReporte.Count > 0)
                EscribirReporteResumenMaestra(listReporte);
            listReporte.Clear();

            // Rpt Resumen Estadistico
            listReporte = GetReporteEstadistico();
            if (listReporte.Count > 0)
                EscribirReporteEstadistico(listReporte);
            listReporte.Clear();  
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Reporte Maestra
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <returns></returns>
        private List<string> GetReporteMaestra(List<string> pDatosImprimir)
        {
            #region GetReporteMaestra
            List<string> lineaMaestra = new List<string>();
            lineaMaestra.Add(GetLineaMaestra(pDatosImprimir));

            return lineaMaestra;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Reporte Maestra Inserto
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <returns></returns>
        private List<string> GetReporteMaestraInserto(List<string> pDatosImprimir)
        {
            #region GetReporteMaestraInserto
            List<string> lineaMaestraInserto = new List<string>();
            string lineaInsert = GetLineaMaestraInserto(pDatosImprimir);

            if (!string.IsNullOrEmpty(lineaInsert))
            {
                lineaMaestraInserto.Add(lineaInsert);
            }

            return lineaMaestraInserto;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Reporte Resumen Maestra
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <returns></returns>
        private List<string> GetReporteResumenMaestra(Dictionary<string, List<string>> pDatosImprimir)
        {
            #region GetReporteResumenMaestra
            List<string> lineaMaestraResumen = new List<string>();

            lineaMaestraResumen = GetReporteResumenMaestra(pDatosImprimir);

            return lineaMaestraResumen;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Reporte Estadistico
        /// </summary>
        /// <returns></returns>
        private List<string> GetReporteEstadistico()
        {
            #region GetReporteEstadistico
            List<string> lineaMaestraResumen = new List<string>();

            lineaMaestraResumen = GetLineaEstadistico();

            return lineaMaestraResumen;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Reporte Distribucion Especial
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private List<string> GetReporteDistribucionEspecial(List<string> pExtracto)
        {
            #region GetReporteDistribucionEspecial
            List<string> lineaReporte = new List<string>();
            string lineaTem = string.Empty;

            lineaTem = GetLineaDistribucionEspecial(pExtracto);
            if (!string.IsNullOrEmpty(lineaTem))
            {
                lineaReporte.Add(lineaTem);
            }

            return lineaReporte;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Reporte SMS
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private List<string> GetReporteSMS(List<string> pExtracto)
        {
            #region GetReporteMSM
            List<string> lineaReporte = new List<string>();
            string lineaTem = string.Empty;

            lineaTem = GetLineaSMS(pExtracto);
            if (!string.IsNullOrEmpty(lineaTem))
            {
                lineaReporte.Add(lineaTem);
            }

            return lineaReporte;
            #endregion
        }
        #endregion

        #region Obtener Lineas Reporte 
        /// <summary>
        /// Metodo que obtiene Linea Maestra
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetLineaMaestra(List<string> pExtracto)
        {
            #region GetLineaMaestra
            string LineaMaestra = string.Empty;
            List<string> camposLinea = new List<string>();

            #region Busquedas
            var result1AAA = from busqueda in pExtracto
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1AAA|")
                             select busqueda;

            var resultCUFE = from busqueda in pExtracto
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("CUFE|")
                             select busqueda;
            #endregion

            if (result1AAA.Any())
            {
                string[] campos1AAA = result1AAA.FirstOrDefault().Split('|');
                

                camposLinea.Add(campos1AAA[16]); // Telefono
                camposLinea.Add(campos1AAA[7]); // Cuenta
                cuenta = campos1AAA[7];
                camposLinea.Add(campos1AAA[2]); // Nombre
                camposLinea.Add(campos1AAA[4]); // Direccion
                camposLinea.Add(campos1AAA[40]); // Zona
                camposLinea.Add(campos1AAA[1]); // Consecutivo
                camposLinea.Add(GetArchivoSalida()); // Archivos
                camposLinea.Add(campos1AAA[8].TrimStart('0')); // Factura
                camposLinea.Add(campos1AAA[12]); // Mora
                camposLinea.Add(campos1AAA[28]); // Estrato
                camposLinea.Add(campos1AAA[40]); // Dane
                camposLinea.Add(campos1AAA[5]);  // Ciudad
                camposLinea.Add(campos1AAA[30]); // Localidad
                camposLinea.Add(campos1AAA[29]); // Barrio
                camposLinea.Add(campos1AAA[10].Split(',')[0].Replace("$", "").Replace(".", "").Trim()); // TotalFactura
                camposLinea.Add(campos1AAA[9]);  // Ciclo
                camposLinea.Add(campos1AAA[1].Split('_')[0]); // Lote
                camposLinea.Add(campos1AAA[19]); // Fecx
                camposLinea.Add(campos1AAA[17]); // Fecp

                camposLinea.Add(campos1AAA[1]); // Fecb
                camposLinea.Add(campos1AAA[1]); // Fecc

                camposLinea.Add(GetTotalIva(pExtracto)); // Total Iva

                camposLinea.Add(campos1AAA[1]); // Total IVA Otros Operadores

                camposLinea.Add(campos1AAA[33]); // Insertos
                camposLinea.Add(campos1AAA[10]); // Valor Pagar Mes
                camposLinea.Add(campos1AAA[27]); // Actividad

                camposLinea.Add(campos1AAA[1]); // Logo TIC
                camposLinea.Add(campos1AAA[1]); // Valor Subsidiado

                camposLinea.Add(campos1AAA[35]); // TipoEnvioCartaEmail
                camposLinea.Add(campos1AAA[34]); // Email
                camposLinea.Add(campos1AAA[18]); // FECL
                camposLinea.Add(campos1AAA[20].Replace("-", string.Empty)); // ReferenciaPago
                camposLinea.Add(campos1AAA[39]); // ProcedimientoReclamacion
                camposLinea.Add(GetLeyendaCarta(pExtracto)); // LeyendaCartera
                camposLinea.Add(campos1AAA[3]); // NIT/CED
                camposLinea.Add(campos1AAA[23]); // TipoProducto

                camposLinea.Add(campos1AAA[1]); // PlanPrimarioLTE

                camposLinea.Add(GetPlanActual(pExtracto)); // PlanActual

                camposLinea.Add(campos1AAA[1]); // ConceptoFinanciacion
                camposLinea.Add(campos1AAA[1]); // SaldoFinanciacion
                camposLinea.Add(campos1AAA[1]); // cuotaFinanciacion

                camposLinea.Add(GetValorFacturaAnterior(pExtracto)); // ValorfacturaAnterior
                camposLinea.Add(GetGraciasPago(pExtracto)); // GraciasPorSuPago
                camposLinea.Add(campos1AAA[24].Split(' ')[0].Trim()); // PorcentajeMora

                camposLinea.Add(campos1AAA[1]); // Retencion

                camposLinea.Add(GetOrderCourrier()); // ORDER_COURRIER
                camposLinea.Add(GetCourrierAsignado()); // CourerAsignado

                camposLinea.Add(campos1AAA[1]); // MarcaRoaming

                camposLinea.Add(campos1AAA[21].Replace("(", "").Replace(")", "")); // CodigoBarra                
                camposLinea.Add(GetContador12M(pExtracto)); // NroLineas12M
                camposLinea.Add(GetMarcaPaqueteHBO(pExtracto)); // MarcaPaqueteHBO
                camposLinea.Add(GetMinutosConMes(pExtracto)); // MinutosConsumoMes

                camposLinea.Add(campos1AAA[1]); // CuentaVencidaAnticipada

                camposLinea.Add(campos1AAA[47]); // Precis
                camposLinea.Add(campos1AAA[48]); // ChipCatastral
                camposLinea.Add(campos1AAA[49]); // Cordenadas

                if (resultCUFE.Any())
                {
                    string[] camposCUFE = resultCUFE.FirstOrDefault().Split('|');
                    camposLinea.Add(camposCUFE[2]); // CUFE
                    camposLinea.Add(camposCUFE[3]); // QR
                }
                else
                {
                    camposLinea.Add(string.Empty); // CUFE
                    camposLinea.Add(string.Empty); // QR
                }

                LineaMaestra = Helpers.ListaCamposToLinea(camposLinea, '|');
            }

            return LineaMaestra;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Linea Maestra Inserto
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetLineaMaestraInserto(List<string> pExtracto)
        {
            #region GetLineaMaestraInserto
            string LineaMaestraInserto = string.Empty;
            List<string> camposLinea = new List<string>();

            #region Busquedas
            var result1AAA = from busqueda in pExtracto
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1AAA|")
                             select busqueda;

            #endregion

            if (result1AAA.Any())
            {
                string[] campos1AAA = result1AAA.FirstOrDefault().Split('|');

                if (!string.IsNullOrEmpty(campos1AAA[33].Trim()))
                {
                    camposLinea.Add(campos1AAA[16]); // Telefono
                    camposLinea.Add(campos1AAA[7]); // Cuenta                
                    camposLinea.Add(campos1AAA[2]); // Nombre
                    camposLinea.Add(campos1AAA[4]); // Direccion
                    camposLinea.Add(campos1AAA[40]); // Zona
                    camposLinea.Add(campos1AAA[1]); // Consecutivo
                    camposLinea.Add(GetArchivoSalida()); // Archivos
                    camposLinea.Add(campos1AAA[8].TrimStart('0')); // Factura
                    camposLinea.Add(campos1AAA[10].Split(',')[0].Replace("$", "").Replace(".", "").Trim()); // TotalFactura
                    camposLinea.Add(campos1AAA[9]);  // Ciclo
                    camposLinea.Add(campos1AAA[33]);  // Inserto

                    LineaMaestraInserto = Helpers.ListaCamposToLinea(camposLinea, '|');
                }
            }

            return LineaMaestraInserto;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Linea Resumen Maestra
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <returns></returns>
        private List<string> GetLineaResumenMaestra(Dictionary<string, List<string>> pDatosImprimir)
        {
            #region GetLineaResumenMaestra

            List<string> lineasResumen = new List<string>();

            Dictionary<string, int> dicLocBar = new Dictionary<string, int>();
            Dictionary<string, int> dicFE = new Dictionary<string, int>();            
            Dictionary<string, int> dicTranspromo = new Dictionary<string, int>();
            Dictionary<string, int> dicDisEspecial = new Dictionary<string, int>();
            Dictionary<string, int> dicCtaExtraer = new Dictionary<string, int>();
            Dictionary<string, int> dicProcuni = new Dictionary<string, int>();
            Dictionary<string, int> dicFechaPagoFijo = new Dictionary<string, int>();

            string tipo = string.Empty;            
            bool valorInsumo = false;

            foreach (List<string> extracto in pDatosImprimir.Values)
            {

                var result = from busqueda in extracto
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1AAA|")
                             select busqueda;

                if (result.Any())
                {
                    cuenta = result.FirstOrDefault().Split('|')[7];
                    factura = result.FirstOrDefault().Split('|')[8].TrimStart('0').Trim();
                    tipo = result.FirstOrDefault().Split('|')[45];
                    valorInsumo = false;

                    #region Suma Cuentas FE
                    if (Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey($"{cuenta} {factura}"))
                    {
                        valorInsumo = true;
                    }

                    if (valorInsumo)
                    {
                        if (dicFE.ContainsKey(tipo))
                        {
                            dicFE[tipo] = dicFE[tipo] + 1;
                        }
                        else
                        {
                            dicFE.Add(tipo, 1);
                        }

                        valorInsumo = false;
                    }
                    #endregion

                    #region Suma LOC_BAR
                    if (Variables.Variables.DatosInsumoLocBar.ContainsKey(cuenta))
                    {
                        valorInsumo = true;
                    }

                    if (valorInsumo)
                    {
                        if (dicLocBar.ContainsKey(tipo))
                        {
                            dicLocBar[tipo] = dicLocBar[tipo] + 1;
                        }
                        else
                        {
                            dicLocBar.Add(tipo, 1);
                        }

                        valorInsumo = false;
                    }
                    #endregion

                    #region Suma Distri. Especial
                    if (Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(cuenta))
                    {
                        valorInsumo = true;
                    }

                    if (valorInsumo)
                    {
                        if (dicDisEspecial.ContainsKey(tipo))
                        {
                            dicDisEspecial[tipo] = dicDisEspecial[tipo] + 1;
                        }
                        else
                        {
                            dicDisEspecial.Add(tipo, 1);
                        }

                        valorInsumo = false;
                    }
                    #endregion

                    #region Suma Cta´s Extraer
                    if (Variables.Variables.DatosInsumoCuentasExtraer.ContainsKey(cuenta))
                    {
                        valorInsumo = true;
                    }

                    if (valorInsumo)
                    {
                        if (dicCtaExtraer.ContainsKey(tipo))
                        {
                            dicCtaExtraer[tipo] = dicCtaExtraer[tipo] + 1;
                        }
                        else
                        {
                            dicCtaExtraer.Add(tipo, 1);
                        }

                        valorInsumo = false;
                    }
                    #endregion

                    #region Suma Procuni
                    if (Variables.Variables.DatosInsumoProcuni.ContainsKey(cuenta))
                    {
                        valorInsumo = true;
                    }

                    if (valorInsumo)
                    {
                        if (dicProcuni.ContainsKey(tipo))
                        {
                            dicProcuni[tipo] = dicProcuni[tipo] + 1;
                        }
                        else
                        {
                            dicProcuni.Add(tipo, 1);
                        }

                        valorInsumo = false;
                    }
                    #endregion

                    #region Suma fecha Pago Fijo
                    if (Variables.Variables.DatosInsumoFechaPagoFijas.ContainsKey(cuenta))
                    {
                        valorInsumo = true;
                    }

                    if (valorInsumo)
                    {
                        if (dicFechaPagoFijo.ContainsKey(tipo))
                        {
                            dicFechaPagoFijo[tipo] = dicFechaPagoFijo[tipo] + 1;
                        }
                        else
                        {
                            dicFechaPagoFijo.Add(tipo, 1);
                        }

                        valorInsumo = false;
                    }
                    #endregion
                }
            }


            // llenar lista con resultados del diccionario

            foreach (string keyFE in dicFE.Keys)
            {
                lineasResumen.Add("ETB_Factura_Electronica|" + keyFE + "|" + dicFE[keyFE].ToString());
            }

            foreach (string keyLocBar in dicLocBar.Keys)
            {
                lineasResumen.Add("LOC_BAR|" + keyLocBar + "|" + dicLocBar[keyLocBar].ToString());
            }

            foreach (string keyDisEspecial in dicDisEspecial.Keys)
            {
                lineasResumen.Add("distribucion_especial|" + keyDisEspecial + "|" + dicDisEspecial[keyDisEspecial].ToString());
            }

            foreach (string keyFechaPagoFijo in dicFechaPagoFijo.Keys)
            {
                lineasResumen.Add("Fechas_Pago_Fijas|" + keyFechaPagoFijo + "|" + dicFechaPagoFijo[keyFechaPagoFijo].ToString());
            }

            foreach (string keyCtaExtraer in dicCtaExtraer.Keys)
            {
                lineasResumen.Add("cuentasExtraer|" + keyCtaExtraer + "|" + dicCtaExtraer[keyCtaExtraer].ToString());
            }

            foreach (string keyProcuni in dicProcuni.Keys)
            {
                lineasResumen.Add("PROCUNI_I|" + keyProcuni + "|" + dicProcuni[keyProcuni].ToString());
            }

            return lineasResumen;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Linea Estadistico
        /// </summary>
        /// <returns></returns>
        private List<string> GetLineaEstadistico()
        {
            #region GetLineaEstadistico

            List<string> lineasEstadistico = new List<string>();

            foreach (var archivoActual in Directory.GetFiles(rutaSalida))
            {
                if (archivoActual.Contains("COMPLETO"))
                    continue;


                var result1AAA = from busqueda in File.ReadAllLines(archivoActual)
                                 where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1AAA|")
                                 select busqueda;

                string inicio = result1AAA.FirstOrDefault().Split('|')[1];
                string fin = result1AAA.LastOrDefault().Split('|')[1];

                lineasEstadistico.Add(Path.GetFileName(archivoActual) + "|" + inicio + "|" + fin + "|" + result1AAA.Count() + "|Carvajal");
            }


            return lineasEstadistico;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Linea Distribucion Especial
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetLineaDistribucionEspecial(List<string> pExtracto)
        {
            #region GetLineaDistribucionEspecial
            string LineaDistribucionEspecial = string.Empty;
            List<string> camposLinea = new List<string>();

            #region Busquedas
            var result1AAA = from busqueda in pExtracto
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1AAA|")
                             select busqueda;

            #endregion

            if (result1AAA.Any())
            {
                string[] campos1AAA = result1AAA.FirstOrDefault().Split('|');
                cuenta = campos1AAA[7];
                LineaDistribucionEspecial = GetDistribucionEspecial();

            }

            return LineaDistribucionEspecial;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Linea SMS
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetLineaSMS(List<string> pExtracto)
        {
            #region GetLineaSMS
            string lineaSMS = string.Empty;
            List<string> camposLinea = new List<string>();
            string mensaje = string.Empty;
            string mensaje2 = string.Empty;

            #region Busquedas
            var result1AAA = from busqueda in pExtracto
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1AAA|")
                             select busqueda;

            #endregion

            if (result1AAA.Any())
            {
                string[] campos1AAA = result1AAA.FirstOrDefault().Split('|');

                if ((campos1AAA[9].Trim() == "3" || campos1AAA[9].Trim() == "4" || campos1AAA[9].Trim() == "5" || campos1AAA[9].Trim() == "6" || campos1AAA[9].Trim() == "7"
                  || campos1AAA[9].Trim() == "8" || campos1AAA[9].Trim() == "90" || campos1AAA[9].Trim() == "91" || campos1AAA[9].Trim() == "92" || campos1AAA[9].Trim() == "93"
                  || campos1AAA[9].Trim() == "94" || campos1AAA[9].Trim() == "95" || campos1AAA[9].Trim() == "96" || campos1AAA[9].Trim() == "97") // Valida LTE - LETCORP
                  && campos1AAA[10].Trim() != "$ 0,00" // Valida Valor a pagar
                  && campos1AAA[43].Trim() == "I" // Valida si es imprimir
                  && campos1AAA[15].Trim().Length <= 10) // Valida tamaño numero telefonico
                {
                    if (campos1AAA[34].Contains("@"))
                    {
                        mensaje = "&&LOS NUESTROS ESTAN AL DIA EN SU PAGO! Tu factura 4G ETB esta disponible en tu correo: ";
                    }
                    else
                    {
                        mensaje = "&&LOS NUESTROS ESTAN AL DIA EN SU PAGO!Tu factura 4G ETB esta disponible: ";
                    }

                    var vencimiento = campos1AAA[15].Trim() == "1" ? campos1AAA[17].Trim() : "Pago Inmediato";
                    mensaje2 = $"Pago electronico {cuenta} valor {campos1AAA[10].Replace("$", "").Trim()} vence: { vencimiento}";

                    lineaSMS = campos1AAA[15].Trim() + mensaje + mensaje2;

                    if (!lisCiclosSMS.Contains(campos1AAA[9].Trim()))
                    {
                        lisCiclosSMS.Add(campos1AAA[9].Trim());
                    }
                }

            }

            return lineaSMS;
            #endregion
        }
        #endregion

        #region Metodos Propios
        /// <summary>
        /// Metodo que obtiene Archivo Salida
        /// </summary>
        /// <returns></returns>
        private string GetArchivoSalida()
        {
            #region GetArchivoSalida
            string nombreArchivo = string.Empty;

            if (Variables.Variables.ArchivoSalidaFinal.ContainsKey(cuenta))
            {
                nombreArchivo = Path.GetFileNameWithoutExtension(Variables.Variables.ArchivoSalidaFinal[cuenta]);
            }

            return nombreArchivo;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Total Iva
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetTotalIva(List<string> pExtracto)
        {
            #region GetTotalIva
            string resultado = string.Empty;

            var resultCanales = from busqueda in pExtracto
                                where busqueda.Length > 5 && (busqueda.Split('|')[0].Trim().Equals("1TFI") || busqueda.Split('|')[0].Trim().Equals("CONS"))
                                select busqueda;

            if (resultCanales.Any())
            {
                foreach (string canalActual in resultCanales)
                {
                    if (canalActual.Contains("1TFI"))
                    {
                        resultado = canalActual.Split('|')[4];
                        break;
                    }
                    else if (canalActual.Contains("CONS|IVA"))
                    {
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal01, canalActual.Split('|')[3]);
                        break;
                    }
                }
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Valor Factura Anterior
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetValorFacturaAnterior(List<string> pExtracto)
        {
            #region GetValorFacturaAnterior
            string resultado = string.Empty;

            var result1BBB = from busqueda in pExtracto
                             where busqueda.Contains("1BBB|Valor Factura Anterior")
                             select busqueda;

            if (result1BBB.Any())
            {
                resultado = result1BBB.FirstOrDefault().Split('|')[2];
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Gracias Pago
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetGraciasPago(List<string> pExtracto)
        {
            #region GetGraciasPago
            string resultado = string.Empty;

            var result1BBB = from busqueda in pExtracto
                             where busqueda.Contains("1BBB|Gracias por su pago")
                             select busqueda;

            if (result1BBB.Any())
            {
                resultado = result1BBB.FirstOrDefault().Split('|')[2];
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Leyenda Carta
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetLeyendaCarta(List<string> pExtracto)
        {
            #region GetLeyendaCarta
            string resultado = string.Empty;

            var result1LLL = from busqueda in pExtracto
                             where busqueda.Contains("1LLL|")
                             select busqueda;

            if (result1LLL.Any())
            {
                string leyenda = result1LLL.FirstOrDefault().Split('|')[1];

                if (leyenda == Variables.RxGeneral.TextoMora_1)
                {
                    resultado = "Leyenda2_Aviso_Reconexion.";
                }
                else if (leyenda == Variables.RxGeneral.TextoMora_2)
                {
                    resultado = "Leyenda4_Suspención_Servicio.";
                }
                else if (leyenda == Variables.RxGeneral.TextoMora_3)
                {
                    resultado = "eyenda3_Reportes_Centrales_Riesgos.";
                }
                else if (leyenda == Variables.RxGeneral.TextoMora_4)
                {
                    resultado = "Leyenda3_Reportes_Centrales_Riesgos.";
                }
                else if (leyenda == Variables.RxGeneral.TextoMora_5)
                {
                    resultado = "Leyenda4_Firma_Presidente.";
                }
                else
                {
                    resultado = "Sin_Leyenda.";
                }

            }
            else
            {
                resultado = "Sin_Leyenda.";
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Contador 12M
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetContador12M(List<string> pExtracto)
        {
            #region GetContador12M
            string resultado = "0";

            var result1EE3 = from busqueda in pExtracto
                             where busqueda.Substring(0, 4).Equals("1EE3") && !busqueda.Contains("Consumo Fijo Etb A Móvil|")
                             select busqueda;

            if (result1EE3.Any())
            {
                resultado = result1EE3.Count().ToString();
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Minutos ConMes
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetMinutosConMes(List<string> pExtracto)
        {
            #region GetMinutosConMes
            string resultado = string.Empty;

            var result1CMP = from busqueda in pExtracto
                             where busqueda.Substring(0, 4).Equals("1CMP")
                             select busqueda;

            if (result1CMP.Any())
            {
                resultado = result1CMP.FirstOrDefault().Split('|')[2].Split(' ')[2].Trim();
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Courrier Asignado
        /// </summary>
        /// <returns></returns>
        private string GetCourrierAsignado()
        {
            #region GetCourrierAsignado
            string resultado = string.Empty;
            string valor = string.Empty;

            if (Variables.Variables.DatosInsumoProcuni.ContainsKey(cuenta))
            {
                valor = Variables.Variables.DatosInsumoProcuni[cuenta][0].Substring(63, 1);
            }

            switch (valor)
            {
                case "I":
                    resultado = "Interrapidisimo";
                    break;
                case "D":
                    resultado = "Computect Datacourier";
                    break;
                default:
                    break;
            }
            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Order Courrier
        /// </summary>
        /// <returns></returns>
        private string GetOrderCourrier()
        {
            #region GetOrderCourrier
            string resultado = string.Empty;

            if (Variables.Variables.DatosInsumoProcuni.ContainsKey(cuenta))
            {
                resultado = Variables.Variables.DatosInsumoProcuni[cuenta][0].Substring(39, 10);
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Plan Actual
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetPlanActual(List<string> pExtracto)
        {
            #region GetContador12M
            string resultado = "0";

            var result1CPA = from busqueda in pExtracto
                             where busqueda.Substring(0, 4).Equals("1CPA")
                             select busqueda;

            if (result1CPA.Any())
            {
                resultado = result1CPA.FirstOrDefault().Split('|')[2].Trim();
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Marca Paquete HBO
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetMarcaPaqueteHBO(List<string> pExtracto)
        {
            #region GetMarcaPaqueteHBO
            string resultado = string.Empty;

            var result1EE3 = from busqueda in pExtracto
                             where busqueda.Substring(0, 4).Equals("1CFI") && busqueda.Contains("HBO PACK")
                             select busqueda;

            if (result1EE3.Any())
            {
                resultado = "Paquete HBO Max";
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Distribucion Especial
        /// </summary>
        /// <returns></returns>
        private string GetDistribucionEspecial()
        {
            #region GetDistribucionEspecial
            string resultado = string.Empty;

            if (Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(cuenta))
            {
                resultado = Variables.Variables.DatosInsumoDistribucionEspecial[cuenta][0].Trim().Replace("/", "");
            }

            return resultado;
            #endregion
        }
        #endregion

        #region Escribir Archivos
        /// <summary>
        /// Metodo que Escribe Reporte Maestra 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        private void EscribirReporteMaestra(List<string> pDatosImprimir)
        {
            #region EscribirReporteMaestra

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = "CTAS_ESP_" + lote + "_Maestra.txt";
            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivo);

            if (!File.Exists(rutaReportes))
            {
                resultado.Add("Telefono|Cuenta|Nombre|Direccion|Zona|Consecutivo|Archivo|Factura|Mora|Estrato|Dane|Ciudad|Localidad|Barrio|TotalFactura|Ciclo|Lote|Fecx|Fecp|Fecb|Fecc|Total Iva|Total IVA Otros Operadores|Insertos|Valor Pagar Mes|Actividad|Logo TIC|Valor Subsidiado|TipoEnvioCartaEmail|Email|FECL|ReferenciaPago|ProcedimientoReclamacion|LeyendaCartera|NIT/CED|TipoProducto|PlanPrimarioLTE|PlanActual|ConceptoFinanciacion|SaldoFinanciacion|cuotaFinanciacion|ValorfacturaAnterior|GraciasPorSuPago|PorcentajeMora|Retencion|ORDER_COURRIER|CourerAsignado|MarcaRoaming|CodigoBarra|NroLineas12M|MarcaPaqueteHBO|MinutosConsumoMes|CuentaVencidaAnticipada|Precis|ChipCatastral|Cordenadas|CUFE|QR");
            }

            resultado.AddRange(pDatosImprimir);

            Helpers.EscribirEnArchivo(rutaReportes, resultado);

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Reporte Maestra Inserto 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirReporteMaestraInserto(List<string> pDatosImprimir)
        {
            #region EscribirReporteMaestraInserto

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = "CTAS_ESP_" + lote + "_Maestra_Insertos.txt";
            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivo);

            if (!File.Exists(rutaReportes))
            {
                resultado.Add("Telefono|Cuenta|Nombre|Direccion|Zona|CscImp|NomArchivo|Factura|ValoraPagar|Ciclo|Inserto");
            }

            resultado.AddRange(pDatosImprimir);

            Helpers.EscribirEnArchivo(rutaReportes, resultado);

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Reporte Resumen Maestra 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirReporteResumenMaestra(List<string> pDatosImprimir)
        {
            #region EscribirReporteResumenMaestra

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = "CTAS_ESP_" + lote + "_Resumen_Maestra.txt";
            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivo);

            if (!File.Exists(rutaReportes))
            {
                resultado.Add("Archivo|Tipo|Cantidad");
            }
            resultado.AddRange(pDatosImprimir);

            Helpers.EscribirEnArchivo(rutaReportes, resultado);

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Reporte Estadistico 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirReporteEstadistico(List<string> pDatosImprimir)
        {
            #region EscribirReporteEstadistico

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = "CTAS_ESP_" + lote + "Estadistico.txt";
            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivo);

            if (!File.Exists(rutaReportes))
            {
                resultado.Add("Archivo|Cons_Ini|Cons_Fin|Facturas|Impresor");
            }

            resultado.AddRange(pDatosImprimir);

            Helpers.EscribirEnArchivo(rutaReportes, resultado);

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Reporte Distribucion Especial 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirReporteDistribucionEspecial(List<string> pDatosImprimir)
        {
            #region EscribirReporteDistribucionEspecial

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = "DistribucionEspecial.txt";

            resultado.AddRange(pDatosImprimir);

            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivo);

            Helpers.EscribirEnArchivo(rutaReportes, resultado);

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Reporte SMS 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirReporteSMS(List<string> pDatosImprimir)
        {
            #region EscribirReporteSMS

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;

            nombreArchivoDetSMS = "FACDisp_" + DateTime.Now.ToString("ddMMyyyy") + "_CICLOS.txt";

            resultado.AddRange(pDatosImprimir);
            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivoDetSMS);

            Helpers.EscribirEnArchivo(rutaReportes, resultado);

            contadorRegistrosSMS += resultado.Count();
            resultado.Clear();

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Reporte SMS Total 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirReporteSMSTotal()
        {
            #region EscribirReporteSMSTotal

            List<string> resultado = new List<string>();
            string nombreArchivo = "FACDisp_" + DateTime.Now.ToString("ddMMyyyy") + "_T.txt";
            string rutaReportes = string.Empty;
            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivo);
            resultado.Add("Cantidad de archivos:  1;Cantidad de registros totales: " + contadorRegistrosSMS + ";");
            Helpers.EscribirEnArchivo(rutaReportes, resultado);

            #endregion
        }

        /// <summary>
        /// Metodo que Renombra Archivo SMS Det 
        /// </summary>
        private void RenombrarArchivoSMSDet()
        {
            #region RenombrarArchivoSMSDet
            string nombreAnterior = Path.Combine(rutaSalida, "Reportes", nombreArchivoDetSMS);
            string cambiar = string.Empty;

            foreach (var ciclo in lisCiclosSMS)
            {
                cambiar += "_" + ciclo;
            }

            string nombreNuevo = Path.Combine(rutaSalida, "Reportes", nombreArchivoDetSMS.Replace("_CICLOS", cambiar));
            
            if(File.Exists(nombreAnterior))
                File.Move(nombreAnterior, nombreNuevo);
            #endregion
        } 
        #endregion
    }
}
