using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.ControlInsumos;

namespace App.ControlLogicaProcesos
{
    public class ReportesMasivos
    {

        #region Variables
        // Utiles
        public static Dictionary<string, List<string>> DiccionarioExtractosReporte = new Dictionary<string, List<string>>();
        string cuenta = string.Empty;
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
        List<string> listaReporteSMS = new List<string>();

        // Reporte RecaudoEsperado final
        Dictionary<string, List<string>> dicRecaudoEsperadoEscribir = new Dictionary<string, List<string>>();
        #endregion

        #region Construcctores
        /// <summary>
        /// 
        /// </summary>
        public ReportesMasivos()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRutaSalida"></param>
        /// <param name="pLote"></param>
        public ReportesMasivos(Dictionary<string, List<string>> pDatosImprimir, string pRutaSalida, string pLote)
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
                DatosError StructError = new DatosError
                {
                    Clase = nameof(ProcesoMasivos),
                    Metodo = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetMethod().ToString(),
                    LineaError = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber(),
                    Error = ex.Message
                };

                Helpers.EscribirLogVentana(StructError, true);
            }
            #endregion
        } 
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        public void Ejecutar(Dictionary<string, List<string>> pDatosImprimir)
        {
            #region Ejecutar
            Helpers.CrearCarpeta(rutaSalida + @"\Reportes");
            Helpers.CrearCarpeta(rutaSalida + @"\Reportes\RecaudoEsperado");
            CargarDiccionario();
            ExtraccionReportes(DiccionarioExtractosReporte);            
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private void CargarDiccionario()
        {
            #region CargarDiccionario
            DiccionarioExtractosReporte = Variables.Variables.DiccionarioExtractosFormateados;

            // Agrego los datos de No Imprimir
            foreach (var keyNoImprimir in Variables.Variables.CuentasNoImprimir.Keys)
            {
                if (!DiccionarioExtractosReporte.ContainsKey(keyNoImprimir))
                    DiccionarioExtractosReporte.Add(keyNoImprimir, new List<string>(Variables.Variables.CuentasNoImprimir[keyNoImprimir]));
            }

            // Agrego los datos de ErrorLTE
            foreach (var keyErrorLTE in Variables.Variables.DatosErrorLTE.Keys)
            {
                if (!DiccionarioExtractosReporte.ContainsKey(keyErrorLTE))
                    DiccionarioExtractosReporte.Add(keyErrorLTE, new List<string>(Variables.Variables.DatosErrorLTE[keyErrorLTE]));
            } 
            #endregion
        }

        #region Obtener Reporte

        /// <summary>
        /// 
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

                // Rpt Maestra Insertos
                listReporte.AddRange(GetReporteMaestraInserto(datosSal.ToList()));
                if (listReporte.Count > 0)
                    listaReporteInsertos.AddRange(listReporte);
                listReporte.Clear();

                // Rpt Resumen Distribucion Especial
                listReporte = GetReporteDistribucionEspecial(datosSal.ToList());
                if (listReporte.Count > 0)
                    listaReporteDistrEspecial.AddRange(listReporte.ToList());
                listReporte.Clear();

                // Rpt Resumen SMS
                listReporte = GetReporteSMS(datosSal.ToList());
                if (listReporte.Count > 0)
                    listaReporteSMS.AddRange(listReporte);                    
                listReporte.Clear();

                //Rpt RecaudoEsperado
                GetLineaRecaudoEsperado(datosSal.ToList());
            }

            // Escribir Reparto Esperado
            EscribirReporteRecaudoEsperado();

            // Escribir Maestra
            EscribirReporteMaestra(listaReporteMaestra);
            listaReporteMaestra.Clear();

            // Escribir Distribucion Especial
            EscribirReporteDistribucionEspecial(listaReporteDistrEspecial);
            listaReporteDistrEspecial.Clear();

            // Escribir Insertos
            EscribirReporteMaestraInserto(listaReporteInsertos);
            listaReporteInsertos.Clear();

            // Escribir SMS Det
            EscribirReporteSMS(listaReporteSMS);

            // Rpt Resumen SMS
            EscribirReporteSMSTotal();

            // Cambiar nombre arvchivo SMS Detallado
            RenombrarArchivoSMSDet();

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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
                cuenta = campos1AAA[7];

                camposLinea.Add(campos1AAA[16]); // Telefono
                camposLinea.Add(campos1AAA[7]); // Cuenta                
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
                camposLinea.Add(string.Empty); // Fecb
                camposLinea.Add(string.Empty); // Fecc
                camposLinea.Add(GetTotalIva(pExtracto)); // Total Iva
                
                camposLinea.Add(campos1AAA[1]); // Total IVA Otros Operadores

                camposLinea.Add(campos1AAA[33]); // Insertos
                camposLinea.Add(campos1AAA[10]); // Valor Pagar Mes
                camposLinea.Add(campos1AAA[27]); // Actividad

                camposLinea.Add(campos1AAA[1]); // Logo TIC

                camposLinea.Add("$ 0.00"); // Valor Subsidiado
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
                camposLinea.Add(string.Empty); // ConceptoFinanciacion
                camposLinea.Add(string.Empty); // SaldoFinanciacion
                camposLinea.Add(string.Empty); // cuotaFinanciacion
                camposLinea.Add(GetValorFacturaAnterior(pExtracto)); // ValorfacturaAnterior
                camposLinea.Add(GetGraciasPago(pExtracto)); // GraciasPorSuPago
                camposLinea.Add(campos1AAA[24].Split(' ')[0].Trim()); // PorcentajeMora
                camposLinea.Add(GetRetencion()); // Retencion
                camposLinea.Add(GetOrderCourrier()); // ORDER_COURRIER
                camposLinea.Add(GetCourrierAsignado()); // CourerAsignado
                camposLinea.Add(string.Empty); // MarcaRoaming
                camposLinea.Add(campos1AAA[21].Replace("(", "").Replace(")", "")); // CodigoBarra                
                camposLinea.Add(GetContador12M(pExtracto)); // NroLineas12M
                camposLinea.Add(GetMarcaPaqueteHBO(pExtracto)); // MarcaPaqueteHBO
                camposLinea.Add(GetMinutosConMes(pExtracto)); // MinutosConsumoMes
                camposLinea.Add(string.Empty); // CuentaVencidaAnticipada
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
        /// 
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
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <returns></returns>
        private List<string> GetLineaResumenMaestra(Dictionary<string, List<string>> pDatosImprimir)
        {
            #region GetLineaResumenMaestra

            List<string> lineasResumen = new List<string>();

            Dictionary<string, int> dicCartas = new Dictionary<string, int>();
            Dictionary<string, int> dicTranspromo = new Dictionary<string, int>();
            Dictionary<string, int> dicCupones = new Dictionary<string, int>();


            foreach (List<string> extracto in pDatosImprimir.Values)
            {
                var result = from busqueda in extracto
                             where busqueda.Length > 5 && (busqueda.Substring(0, 5).Equals("1AAA|") || busqueda.Substring(0, 5).Equals("CART|"))
                             select busqueda;

                if (result.Any())
                {
                    foreach (var lineaActual in result)
                    {
                        if (lineaActual.Substring(0, 4) == "1AAA")
                        {
                            // Llena diccionario Transpromo
                            if (!string.IsNullOrEmpty(lineaActual.Split('|')[33].Trim()))
                            {
                                if (dicTranspromo.ContainsKey(lineaActual.Split('|')[33]))
                                {
                                    dicTranspromo[lineaActual.Split('|')[33]] = dicTranspromo[lineaActual.Split('|')[33]] + 1;
                                }
                                else
                                {
                                    dicTranspromo.Add(lineaActual.Split('|')[33], 1);
                                }
                            }

                            // Llena diccionario Cupones
                            if (!string.IsNullOrEmpty(lineaActual.Split('|')[36].Trim()))
                            {
                                if (dicCupones.ContainsKey(lineaActual.Split('|')[36]))
                                {
                                    dicCupones[lineaActual.Split('|')[36]] = dicCupones[lineaActual.Split('|')[36]] + 1;
                                }
                                else
                                {
                                    dicCupones.Add(lineaActual.Split('|')[36], 1);
                                }
                            }
                        }
                        else if (lineaActual.Substring(0, 4) == "CART")
                        {
                            // Llena diccionario Cartas
                            if (dicCartas.ContainsKey(lineaActual.Split('|')[1]))
                            {
                                dicCartas[lineaActual.Split('|')[1]] = dicCartas[lineaActual.Split('|')[1]] + 1;
                            }
                            else
                            {
                                dicCartas.Add(lineaActual.Split('|')[1], 1);
                            }
                        }
                    }
                }
            }


            // llenar lista con resultados del diccionario

            foreach (var keyCartas in dicCartas.Keys)
            {
                lineasResumen.Add("Cartas" + keyCartas + "|" + dicCartas[keyCartas].ToString());
            }

            foreach (var keyTranspromo in dicTranspromo.Keys)
            {
                lineasResumen.Add("BASETRANSPROMO" + keyTranspromo + "|" + dicTranspromo[keyTranspromo].ToString());
            }

            foreach (var keyCupones in dicCupones.Keys)
            {
                lineasResumen.Add("Cupones" + keyCupones + "|" + dicCupones[keyCupones].ToString());
            }

            return lineasResumen;
            #endregion
        }

        /// <summary>
        /// 
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
        /// 
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
        /// 
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private void GetLineaRecaudoEsperado(List<string> pExtracto)
        {
            #region GetLineaRecaudoEsperado            

            string lineaReporte = string.Empty;

            #region camposReporte

            string ValorPagarMes = string.Empty;
            string fecha_pago = string.Empty;
            string fecha_extemporaneo = string.Empty;

            string tmp_recesp_cuenta = string.Empty;
            string tmp_recesp_nro_factura = string.Empty;
            string tmp_recesp_nro_cuota = string.Empty;
            string tmp_recesp_ciclo = string.Empty;
            string tmp_recesp_valor_pagar_principal = string.Empty;
            string tmp_recesp_cod_emp_cobranza = string.Empty;
            string tmp_recesp_valor_pagar_adicional = string.Empty;
            string tmp_recesp_fecha_venc = string.Empty;
            string tmp_recesp_identificacion_EFR = string.Empty;
            string tmp_recesp_cuenta_cliente_receptor = string.Empty;
            string tmp_recesp_tipo_cuenta_receptor = string.Empty;
            string tmp_recesp_nro_identificacion_cliente = string.Empty;
            string tmp_recesp_nombre_cliente = string.Empty;
            string tmp_recesp_codigo_entidad_financiera = string.Empty;
            string tmp_recesp_reserva = string.Empty;

            string llaveDiccionario = string.Empty;

            #endregion

            #region Busqueda
            var result1AAA = from busqueda in pExtracto
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1AAA|")
                             select busqueda;
            #endregion

            if (result1AAA.Any())
            {
                string[] campos1AAA = result1AAA.FirstOrDefault().Split('|');

                if (campos1AAA[10].Replace("$", "").Replace(".", "").Replace(",", "").Trim() != "000")
                {
                    #region Asignacion Valores Reporte

                    tmp_recesp_cuenta = campos1AAA[7].PadLeft(48, '0');
                    tmp_recesp_nro_factura = campos1AAA[8].PadLeft(30, '0');
                    tmp_recesp_nro_cuota = campos1AAA[12].PadLeft(2, '0');
                    tmp_recesp_ciclo = campos1AAA[9].PadLeft(3, ' ');
                    tmp_recesp_valor_pagar_principal = campos1AAA[31].Replace("$", "").Replace(".", "").Replace(",", "").Trim();
                    tmp_recesp_valor_pagar_principal = tmp_recesp_valor_pagar_principal.PadLeft(14, '0');
                    tmp_recesp_cod_emp_cobranza = tmp_recesp_cod_emp_cobranza.PadLeft(13, '0');
                    tmp_recesp_valor_pagar_adicional = campos1AAA[10].Replace("$", "").Replace(".", "").Replace(",", "").Trim();
                    tmp_recesp_valor_pagar_adicional = tmp_recesp_valor_pagar_adicional.PadLeft(14, '0');
                    tmp_recesp_fecha_venc = Convert.ToDateTime(campos1AAA[17]).ToString("yyyyMMdd");
                    tmp_recesp_identificacion_EFR = tmp_recesp_identificacion_EFR.PadLeft(8, '0');
                    tmp_recesp_cuenta_cliente_receptor = tmp_recesp_cuenta_cliente_receptor.PadLeft(17, ' ');
                    tmp_recesp_tipo_cuenta_receptor = tmp_recesp_tipo_cuenta_receptor.PadLeft(2, '0');
                    tmp_recesp_nro_identificacion_cliente = tmp_recesp_nro_identificacion_cliente.PadLeft(10, ' ');
                    tmp_recesp_nombre_cliente = tmp_recesp_nombre_cliente.PadLeft(22, ' ');
                    tmp_recesp_codigo_entidad_financiera = tmp_recesp_codigo_entidad_financiera.PadLeft(3, '0');
                    tmp_recesp_reserva = Convert.ToDateTime(campos1AAA[18]).ToString("yyyyMMdd");

                    llaveDiccionario = "ETB_" + tmp_recesp_fecha_venc + "_" + campos1AAA[9].PadLeft(2, '0') + "_" + lote + "_FECHAGENERACION" + "_" + tmp_recesp_reserva + ".as";

                    #endregion


                    lineaReporte = "06" + tmp_recesp_cuenta + tmp_recesp_nro_factura + tmp_recesp_nro_cuota + tmp_recesp_ciclo + tmp_recesp_valor_pagar_principal +
                                   tmp_recesp_cod_emp_cobranza + tmp_recesp_valor_pagar_adicional + tmp_recesp_fecha_venc + tmp_recesp_identificacion_EFR +
                                   tmp_recesp_cuenta_cliente_receptor + tmp_recesp_tipo_cuenta_receptor + tmp_recesp_nro_identificacion_cliente + tmp_recesp_nombre_cliente +
                                   tmp_recesp_codigo_entidad_financiera + tmp_recesp_reserva;

                    if (dicRecaudoEsperadoEscribir.ContainsKey(llaveDiccionario))
                    {
                        dicRecaudoEsperadoEscribir[llaveDiccionario].Add(lineaReporte);
                    }
                    else
                    {
                        dicRecaudoEsperadoEscribir.Add(llaveDiccionario, new List<string> { lineaReporte });
                    }
                }
            }

            #endregion
        }
        #endregion

        #region Metodos Propios
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetArchivoSalida()
        {
            #region GetArchivoSalida
            string nombreArchivo = string.Empty;

            if (Variables.Variables.ArchivoSalidaFinal.ContainsKey(cuenta))
            {
                nombreArchivo = Path.GetFileNameWithoutExtension(Variables.Variables.ArchivoSalidaFinal[cuenta]);

                if (!nombreArchivo.Contains("OTROS"))
                {
                    nombreArchivo = nombreArchivo.Substring(0, nombreArchivo.Length - 4);
                }
            }

            return nombreArchivo;
            #endregion
        }

        /// <summary>
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetRetencion()
        {
            #region GetRetencion
            string resultado = string.Empty;

            if (Variables.Variables.CuentasNoImprimir.ContainsKey(cuenta))
            {
                if (Variables.Variables.DatosInsumoCuentasExtraer.ContainsKey(cuenta))
                {
                    resultado = "Retencion_Insumo";
                }
                else
                {
                    resultado = "Retencion_Valor";
                }
            }

            return resultado;
            #endregion
        }
        #endregion

        #region Escribir Archivos
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        private void EscribirReporteMaestra(List<string> pDatosImprimir)
        {
            #region EscribirReporteMaestra

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = lote + "_Maestra.txt";
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
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirReporteMaestraInserto(List<string> pDatosImprimir)
        {
            #region EscribirReporteMaestraInserto

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = lote + "_Maestra_Insertos.txt";
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
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirReporteResumenMaestra(List<string> pDatosImprimir)
        {
            #region EscribirReporteResumenMaestra

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = lote + "_Resumen_Maestra.txt";
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
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pNombreArchivo"></param>
        private void EscribirReporteEstadistico(List<string> pDatosImprimir)
        {
            #region EscribirReporteEstadistico

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = lote + "Estadistico.txt";
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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

            if (File.Exists(nombreAnterior))
                File.Move(nombreAnterior, nombreNuevo);
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private void EscribirReporteRecaudoEsperado()
        {
            #region EscribirReporteRecaudoEsperado

            #region Variables

            List<string> lineasReporte = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = string.Empty;
            string fechaGeneracion = DateTime.Now.ToString("yyyyMMdd");

            List<string> listaSumaServPrincipal = new List<string>();
            List<string> listaSumaServAdicional = new List<string>();

            string lineaDatos01 = string.Empty;
            string lineaDatos05 = string.Empty;

            string lineaDatos08 = string.Empty;
            string lineaDatos09 = string.Empty;
            string loteReporte = string.Empty;

            List<string> rptControlRecaudo = new List<string>();
            string lineaControlRecaudo = string.Empty;
            string fecha1ControlRecaudo = string.Empty;
            string fecha2ControlRecaudo = string.Empty;

            if (lote.Length == 6)
            {
                loteReporte = lote.Substring(2);
            }
            else if (lote.Length == 5)
            {
                loteReporte = lote.Substring(1);
            }
            else
            {
                loteReporte = lote;
            }
            #endregion

            #region Armo Linea01
            lineaDatos01 = "01";
            lineaDatos01 += "8999991158";
            lineaDatos01 += "0000000000";
            lineaDatos01 += "000";
            lineaDatos01 += DateTime.Now.ToString("yyyyMMddHHmm");
            lineaDatos01 += "A";
            lineaDatos01 = lineaDatos01.PadRight(220, ' ');
            #endregion

            #region Armo Linea05
            lineaDatos05 = "05";
            lineaDatos05 += "7707181500017";
            lineaDatos05 += loteReporte.PadLeft(4, '0');
            lineaDatos05 += "0001TELECOMUNIC";
            lineaDatos05 = lineaDatos05.PadRight(220, ' ');
            #endregion

            // Se escribe el Uno a uno
            foreach (var dicActual in dicRecaudoEsperadoEscribir)
            {
                rutaReportes = string.Empty;
                nombreArchivo = dicActual.Key.Replace("FECHAGENERACION", fechaGeneracion);
                rutaReportes = Path.Combine(rutaSalida, "Reportes", "RecaudoEsperado", nombreArchivo);

                lineasReporte.Add(lineaDatos01);
                lineasReporte.Add(lineaDatos05);
                lineasReporte.AddRange(dicActual.Value);

                foreach (var lineaActual in dicActual.Value)
                {
                    listaSumaServPrincipal.Add(lineaActual.Substring(85, 14));
                    listaSumaServAdicional.Add(lineaActual.Substring(112, 14));

                    fecha1ControlRecaudo = lineaActual.Substring(126, 8);
                    fecha2ControlRecaudo = lineaActual.Substring(196, 8);
                }


                #region Armo Linea08
                lineaDatos08 = "08";
                lineaDatos08 += dicActual.Value.Count().ToString().PadLeft(9, '0');
                lineaDatos08 += Helpers.SumarCampos(listaSumaServPrincipal).Replace("$", "").Replace(".", "").Replace(",", "").Trim().PadLeft(18, '0');
                lineaDatos08 += Helpers.SumarCampos(listaSumaServAdicional).Replace("$", "").Replace(".", "").Replace(",", "").Trim().PadLeft(18, '0');
                lineaDatos08 += loteReporte.PadLeft(4, '0');
                lineaDatos08 += "".PadLeft(169, ' ');
                #endregion

                #region Armo Linea09
                lineaDatos09 = "09";
                lineaDatos09 += dicActual.Value.Count().ToString().PadLeft(9, '0');
                lineaDatos09 += Helpers.SumarCampos(listaSumaServPrincipal).Replace("$", "").Replace(".", "").Replace(",", "").Trim().PadLeft(18, '0');
                lineaDatos09 += Helpers.SumarCampos(listaSumaServAdicional).Replace("$", "").Replace(".", "").Replace(",", "").Trim().PadLeft(18, '0');
                lineaDatos09 += "".PadLeft(173, ' ');
                #endregion

                lineasReporte.Add(lineaDatos08);
                lineasReporte.Add(lineaDatos09);

                Helpers.EscribirEnArchivo(rutaReportes, lineasReporte);

                #region linea Control Recaudo
                lineaControlRecaudo = dicActual.Key.Replace("FECHAGENERACION", fechaGeneracion).Replace(".as", "") + "|";
                lineaControlRecaudo += dicActual.Value.Count().ToString().PadLeft(9, '0') + "|";
                lineaControlRecaudo += Helpers.SumarCampos(listaSumaServPrincipal).Replace("$", "").Replace(".", "").Replace(",", "").Trim().PadLeft(18, '0') + "|";
                lineaControlRecaudo += Helpers.SumarCampos(listaSumaServAdicional).Replace("$", "").Replace(".", "").Replace(",", "").Trim().PadLeft(18, '0') + "|";
                lineaControlRecaudo += fecha1ControlRecaudo + "|";
                lineaControlRecaudo += fecha2ControlRecaudo + "|";
                lineaControlRecaudo += lote;

                rptControlRecaudo.Add(lineaControlRecaudo);
                #endregion

                listaSumaServPrincipal.Clear();
                listaSumaServAdicional.Clear();

                lineasReporte.Clear();

            }

            #region Escribir Rpt Recaudo esperado
            nombreArchivo = "Control_ReporteRecaurdoEsperado.txt";
            rutaReportes = Path.Combine(rutaSalida, "Reportes", "RecaudoEsperado", nombreArchivo);

            Helpers.EscribirEnArchivo(rutaReportes, rptControlRecaudo);
            #endregion

            #endregion
        }
        #endregion
    }
}
