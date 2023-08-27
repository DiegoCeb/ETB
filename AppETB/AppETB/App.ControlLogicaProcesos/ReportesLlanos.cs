using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using App.ControlInsumos;

namespace App.ControlLogicaProcesos
{
    /// <summary>
    /// Clase RepotrtesLlanos
    /// </summary>
    public class ReportesLlanos
    {
        #region Variables
        // Utiles
        public static Dictionary<string, List<string>> DiccionarioExtractosReporte = new Dictionary<string, List<string>>();
        string cuenta = string.Empty;
        string rutaSalida = string.Empty;

        // Listas Reportes
        List<LineaMaestra> listaReporteMaestra = new List<LineaMaestra>();
        List<string> listaReporteDistrEspecial = new List<string>();
        List<string> listaReporteTotalIVR = new List<string>();

        // Reporte RecaudoEsperado final
        Dictionary<string, List<string>> dicRecaudoEsperadoEscribir = new Dictionary<string, List<string>>();

        Dictionary<string, string> dicMeses = new Dictionary<string, string>();

        #endregion

        #region Construcctores
        /// <summary>
        /// 
        /// </summary>
        public ReportesLlanos()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pRutaSalida"></param>
        /// <param name="pNombreArchivo"></param>
        public ReportesLlanos(Dictionary<string, List<string>> pDatosImprimir, string pRutaSalida, string pNombreArchivo)
        {
            #region ReportesLlanos
            try
            {
                rutaSalida = pRutaSalida;
                Ejecutar(pDatosImprimir, pNombreArchivo);
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
        /// Metodo Ejectutar
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <param name="pNombreArchivo"></param>
        public void Ejecutar(Dictionary<string, List<string>> pDatosImprimir, string pNombreArchivo)
        {
            #region Ejecutar
            Helpers.CrearCarpeta(rutaSalida + @"\Reportes");
            Helpers.CrearCarpeta(rutaSalida + @"\Reportes\RecaudoEsperado");
            CargarDiccionario();
            ExtraccionReportes(DiccionarioExtractosReporte, pNombreArchivo);
            #endregion
        }

        /// <summary>
        /// Metodo Cargar Diccionario
        /// </summary>
        private void CargarDiccionario()
        {
            #region CargarDiccionario
            DiccionarioExtractosReporte = Variables.Variables.DiccionarioExtractosFormateados;

            dicMeses.Add("ENE", "01");
            dicMeses.Add("FEB", "02");
            dicMeses.Add("MAR", "03");
            dicMeses.Add("ABR", "04");
            dicMeses.Add("MAY", "05");
            dicMeses.Add("JUN", "06");
            dicMeses.Add("JUL", "07");
            dicMeses.Add("AGO", "08");
            dicMeses.Add("SEP", "09");
            dicMeses.Add("OCT", "10");
            dicMeses.Add("NOV", "11");
            dicMeses.Add("DIC", "12");
            #endregion
        }

        #region Obtener Reporte

        /// <summary>
        /// Metodo de Extraccion Reportes
        /// </summary>
        /// <param name="pDatosProcesados"></param>
        /// <param name="pNombreArchivo"></param>
        private void ExtraccionReportes(Dictionary<string, List<string>> pDatosProcesados, string pNombreArchivo)
        {
            #region ExtraccionReportes
            List<string> listReporte = new List<string>();
            List<LineaMaestra> listReporteMaestra = new List<LineaMaestra>();

            foreach (var datosSal in pDatosProcesados.Values)
            {
                // Rpt Maestra
                listReporteMaestra.AddRange(GetReporteMaestra(datosSal.ToList()));
                if (listReporteMaestra.Count > 0)
                    listaReporteMaestra.AddRange(listReporteMaestra.ToList());
                listReporteMaestra.Clear();

                // Rpt Total IVR
                listReporte.AddRange(GetReporteTotalIVR(datosSal.ToList()));
                if (listReporte.Count > 0)
                    listaReporteTotalIVR.AddRange(listReporte);
                listReporte.Clear();

                ////Rpt RecaudoEsperado
                GetLineaRecaudoEsperado(datosSal.ToList());
            }

            // Escribir Reparto Esperado
            EscribirReporteRecaudoEsperado();

            // Escribir Maestra
            EscribirReporteMaestra(listaReporteMaestra, pNombreArchivo);
            listaReporteMaestra.Clear();

            // Escribir TotalIVR
            EscribirReporteTotalIVR(listaReporteTotalIVR);
            listaReporteTotalIVR.Clear();
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Reporte Maestra
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <returns></returns>
        private List<LineaMaestra> GetReporteMaestra(List<string> pDatosImprimir)
        {
            #region GetReporteMaestra
            //List<string> lineaMaestra = new List<string>();
            List<LineaMaestra> lineaMaestra = new List<LineaMaestra>();
            lineaMaestra.Add(GetLineaMaestra(pDatosImprimir));

            return lineaMaestra;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Reporte Maestra Inserto
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        /// <returns></returns>
        private List<string> GetReporteTotalIVR(List<string> pDatosImprimir)
        {
            #region GetReporteMaestraInserto
            List<string> lineaMaestraInserto = new List<string>();
            string lineaInsert = GetLineaTotalIVR(pDatosImprimir);

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
        /// Metodo que obtiene Linea Recaudo Esperado
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

                if (campos1AAA[8].Replace("$", "").Replace(".", "").Replace(",", "").Trim() != "000")
                {
                    #region Asignacion Valores Reporte

                    tmp_recesp_cuenta = campos1AAA[6].PadLeft(48, '0');
                    tmp_recesp_nro_factura = campos1AAA[22].PadLeft(30, '0');
                    tmp_recesp_nro_cuota = campos1AAA[15].PadLeft(2, '0');
                    tmp_recesp_ciclo = string.Empty.PadLeft(3, ' ');
                    tmp_recesp_valor_pagar_principal = $"{GetValorPagar(campos1AAA[8])}00";
                    tmp_recesp_valor_pagar_principal = tmp_recesp_valor_pagar_principal.PadLeft(14, '0');
                    tmp_recesp_cod_emp_cobranza = tmp_recesp_cod_emp_cobranza.PadLeft(13, '0');
                    tmp_recesp_valor_pagar_adicional = $"{GetValorPagar(campos1AAA[8])}00";
                    tmp_recesp_valor_pagar_adicional = tmp_recesp_valor_pagar_adicional.PadLeft(14, '0');
                    tmp_recesp_fecha_venc = GetFechaPagoRecaudoEsperado(campos1AAA[10]);
                    tmp_recesp_identificacion_EFR = tmp_recesp_identificacion_EFR.PadLeft(8, '0');
                    tmp_recesp_cuenta_cliente_receptor = tmp_recesp_cuenta_cliente_receptor.PadLeft(17, ' ');
                    tmp_recesp_tipo_cuenta_receptor = tmp_recesp_tipo_cuenta_receptor.PadLeft(2, '0');
                    tmp_recesp_nro_identificacion_cliente = tmp_recesp_nro_identificacion_cliente.PadLeft(10, ' ');
                    tmp_recesp_nombre_cliente = tmp_recesp_nombre_cliente.PadLeft(22, ' ');
                    tmp_recesp_codigo_entidad_financiera = tmp_recesp_codigo_entidad_financiera.PadLeft(3, '0');
                    tmp_recesp_reserva = GetFechaPagoRecaudoEsperado(campos1AAA[10]).PadRight(24,' ');

                    llaveDiccionario = "LLA" + GetFechaPago(campos1AAA[16]) + "_01_"+ tmp_recesp_fecha_venc + ".as";

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

        #region Obtener Lineas Reporte 
        /// <summary>
        /// Metodo que obtiene Linea Maestra
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private LineaMaestra GetLineaMaestra(List<string> pExtracto)
        {
            #region GetLineaMaestra
            string linea = string.Empty;
            string periodo = string.Empty;
            LineaMaestra lineaMaestra = new LineaMaestra();
            List<string> camposLinea = new List<string>();

            #region Busquedas
            var result1AAA = from busqueda in pExtracto
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1AAA|")
                             select busqueda;

            var result1BBA = from busqueda in pExtracto
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1BBA|")
                             select busqueda;
            #endregion

            if (result1AAA.Any())
            {
                string[] campos1AAA = result1AAA.FirstOrDefault().Split('|');
                cuenta = campos1AAA[23];

                string[] campos1BBA = null;
                if (result1BBA.Any())
                {
                    campos1BBA = result1BBA.FirstOrDefault().Split('|');
                }

                camposLinea.Add(GetSinLetras(campos1AAA[27]).TrimStart('0')); // Consecutivo
                camposLinea.Add(GetSinLetras(campos1AAA[1])); // Num_Factura
                camposLinea.Add(campos1AAA[2]); // Nombre                
                camposLinea.Add(campos1AAA[3]); // Dir_1
                camposLinea.Add(campos1AAA[4]); // Dir_2
                camposLinea.Add(GetSinLetras(campos1AAA[5])); // Identif
                camposLinea.Add(campos1AAA[6]); // Telefono
                camposLinea.Add(GetValorPagar(campos1AAA[8])); // Valor Pagar
                if (campos1BBA != null && campos1BBA.Length > 10)
                {
                    camposLinea.Add(campos1BBA[10]); // Zona_Sector_Manzana
                }
                else
                {
                    camposLinea.Add(string.Empty); // Zona_Sector_Manzana
                }
                
                camposLinea.Add($"{campos1AAA[11]}-{campos1AAA[25]}"); // Zona_Postal
                camposLinea.Add(campos1AAA[14]); // Estrato
                camposLinea.Add(campos1AAA[15]); // Mes_Deuda
                camposLinea.Add(GetFechaPago(campos1AAA[10]));  // Fecha_Pago
                camposLinea.Add(campos1AAA[16]); // Fecha_Exp
                camposLinea.Add(string.Empty); // Inserto
                camposLinea.Add(GetDiferencia(pExtracto, campos1AAA[8])); // Diferencia
                camposLinea.Add(campos1AAA[22]);  // Cupon
                camposLinea.Add(campos1AAA[24].Substring(20,12)); // Referencia Pago
                camposLinea.Add(campos1AAA[30]); // Tipo Email
                camposLinea.Add(campos1AAA[31]); // Email
                camposLinea.Add(GetCodSuscriptor(campos1AAA[2])); // ID_SUSCRIPTOR
                camposLinea.Add(GetTipoEnvio(campos1AAA[6])); // ID_SUSCRIPTOR

                linea = Helpers.ListaCamposToLinea(camposLinea, '|').Replace('\t', ' ');
                periodo = GetFechaPeriodo(campos1AAA[9]);

                lineaMaestra = new LineaMaestra(periodo, Helpers.GetTextoSinTildes(linea));
            }

            return lineaMaestra;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Linea Maestra Inserto
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <returns></returns>
        private string GetLineaTotalIVR(List<string> pExtracto)
        {
            #region GetLineaMaestraInserto
            string lineaTotalIVR = string.Empty;
            List<string> camposLinea = new List<string>();

            #region Busquedas
            var result1AAA = from busqueda in pExtracto
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1AAA|")
                             select busqueda;

            #endregion

            if (result1AAA.Any())
            {
                string[] campos1AAA = result1AAA.FirstOrDefault().Split('|');

                if (campos1AAA[6].Contains("PRIVADO") || (campos1AAA[6].Length > 7 && campos1AAA[6].Length < 7) || Convert.ToInt16(campos1AAA[15]) <= 1)
                {
                    lineaTotalIVR = null;
                }
                else
                {
                    camposLinea.Add(GetFechaPago(campos1AAA[10])); // SegundaFechaPago
                    camposLinea.Add($"078{campos1AAA[6]}"); // Telefono              
                    camposLinea.Add("99"); // NumeroCampaña
                    camposLinea.Add(campos1AAA[8].Replace(",", "").Replace(".", "").PadLeft(10, '0')); // SaldoTotal

                    lineaTotalIVR = Helpers.ListaCamposToLinea(camposLinea, ',');
                }
            }

            return lineaTotalIVR;
            #endregion
        }

        #endregion

        #region Metodos Propios
        /// <summary>
        /// Metodo que obtiene el Valor Pagar
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private string GetValorPagar(string pCampo)
        {
            #region GetValorPagar

            string result = pCampo.Substring(0, pCampo.IndexOf(".")).Replace("$","").Replace(",","").Replace(".","");
            return result.Trim();
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene CuentaSinLetras
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private string GetSinLetras(string pCampo)
        {
            #region GetCuentaSinLetras
            string result = pCampo;
            if (!string.IsNullOrEmpty(pCampo))
            {
                result = string.Concat(pCampo.Where(c => Char.IsDigit(c)));

            }

            return result.Trim();
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene El Codigo del Suscriptor
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private string GetCodSuscriptor(string pCampo)
        {
            #region GetCodSuscriptor
            string result = pCampo;
            int ini = pCampo.IndexOf("(") + 1;
            int fin = pCampo.LastIndexOf(")");

            if (ini > 0 && fin > 0)
            {
                int cant = fin - ini;
                result = pCampo.Substring(ini, cant);

            }

            return result;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la Fecha Pago
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private string GetFechaPago(string pCampo)
        {
            #region GetFechaPago
            string result = string.Empty;
            string[] camposFecha = pCampo.Split('/');
            if (camposFecha.Length == 3)
            {
                result = $"{camposFecha[0]}{dicMeses[camposFecha[1]]}{camposFecha[2]}";
            }
            return result.Trim();
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la Fecha Pago
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private string GetFechaPagoRecaudoEsperado(string pCampo)
        {
            #region GetFechaPago
            string result = string.Empty;
            string[] camposFecha = pCampo.Split('/');
            if (camposFecha.Length == 3)
            {
                result = $"{camposFecha[2]}{dicMeses[camposFecha[1]]}{camposFecha[0]}";
            }
            return result.Trim();
            #endregion
        }

        /// <summary>
        /// Metodo que verifica si hay diferencia
        /// </summary>
        /// <param name="pExtracto"></param>
        /// <param name="pTotalPago"></param>
        /// <returns></returns>
        private string GetDiferencia(List<string> pExtracto, string pTotalPago)
        {
            #region GetDiferencia
            string result = string.Empty;

            var resultSumatoria = from busqueda in pExtracto
                                  where busqueda.Length > 5 &&
                                  (busqueda.Substring(0, 5).Equals("1CCC|") || busqueda.Substring(0, 5).Equals("1DDD|") || busqueda.Substring(0, 5).Equals("1EEE|"))
                                  select busqueda;

            decimal sumatoria = decimal.Zero;
            decimal totalPago = Convert.ToDecimal(pTotalPago);

            if (resultSumatoria.Any())
            {
                string[] campos;

                foreach (var linea in resultSumatoria)
                {
                    campos = linea.Split('|');

                    switch (campos[0])
                    {
                        case "1CCC":
                            if (campos[1].Contains("VALOR CARGOS ETB"))
                            {
                                sumatoria += Convert.ToDecimal(campos[2]);
                            }
                            break;
                        case "1DDD":
                            if (campos[1].Contains("PAGAR EMPRESAS LD Y MÓVIL"))
                            {
                                sumatoria += Convert.ToDecimal(campos[2]);
                            }
                            break;
                        case "1EEE":
                            if (campos[1].Contains("TOTAL FACTURAS VENCIDAS"))
                            {
                                sumatoria += Convert.ToDecimal(campos[2]);
                            }
                            break;
                    }
                }
            }

            if (Math.Abs(sumatoria - totalPago) > 5)
            {
                result = "Con Diferencia";
            }
            else
            {
                result = "Sin Diferencia";
            }

            return result; 
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la Fecha Periodo
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private string GetFechaPeriodo(string pCampo)
        {
            #region GetFechaPeriodo

            string result = null;

            string[] fechas = pCampo.Split('-');
            string[] fechaCampos = null;

            if (fechas.Length == 2)
            {
                fechaCampos = fechas[0].Split('/');
            }

            if (fechaCampos != null && fechaCampos.Length == 3)
            {
                result = $"{fechaCampos[1].Trim()}{fechaCampos[2].Trim()}";
            }

            return result.Trim();
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Tipo de Envio
        /// </summary>
        /// <returns></returns>
        private string GetTipoEnvio(string pTelefono)
        {
            #region GetOrderCourrier
            string resultado = string.Empty;

            if (Variables.Variables.DatosInsumoExtraerLlanos.ContainsKey(pTelefono))
            {
                resultado = "I_NO_IMPRIMIR";
            }
            else if (Variables.Variables.DatosInsumoLlanosEnvioSMS.ContainsKey(pTelefono))
            {
                resultado = "I_ENVIO_SMS";
            }
            else if (Variables.Variables.DatosInsumoDistribucionEmailLlanos.ContainsKey(pTelefono))
            {
                if (Variables.Variables.DatosInsumoDualLlanos.ContainsKey(pTelefono))
                {
                    resultado = "Courier00000000I";
                }
                else
                {
                    resultado = "I_FACTURA_SOLO_EMAIL";
                }
            }
            else
            {
                resultado = "Courier00000000I";
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
        /// <param name="pNombreArchivo"></param>
        private void EscribirReporteMaestra(List<LineaMaestra> pDatosImprimir, string pNombreArchivo)
        {
            #region EscribirReporteMaestra

            List<string> resultado = new List<string>();
            Dictionary<string, List<string>> resultadoPeriodo = new Dictionary<string, List<string>>();

            string rutaReportes = string.Empty;
            string rutaReportePeriodo = string.Empty;
            string nombreArchivoPeriodo = string.Empty;
            string nombreArchivo = "MAE_" + pNombreArchivo + ".txt";
            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivo);

            if (!File.Exists(rutaReportes))
            {
                resultado.Add("Consec.|Num_Factura|Nombre|Dir_1|Dir_2|Identif.|Telefono|Valor Pagar|Zona_Sector_Manzana|Zona_Postal|Estrato|Mes_Deuda|Fecha_Pago|Fecha_Exp|Inserto|Diferencia|Cupon|Referencia Pago|Tipo Email|Email|ID_SUSCRIPTOR|salida");
            }

            foreach (LineaMaestra linea in pDatosImprimir)
            {
                resultado.Add(linea.Linea);

                if (!resultadoPeriodo.ContainsKey(linea.Periodo))
                {
                    resultadoPeriodo.Add(linea.Periodo, new List<string>());
                    resultadoPeriodo[linea.Periodo].Add("Consec.|Num_Factura|Nombre|Dir_1|Dir_2|Identif.|Telefono|Valor Pagar|Zona_Sector_Manzana|Zona_Postal|Estrato|Mes_Deuda|Fecha_Pago|Fecha_Exp|Inserto|Diferencia|Cupon|Referencia Pago|Tipo Email|Email|ID_SUSCRIPTOR|salida");
                }

                resultadoPeriodo[linea.Periodo].Add(linea.Linea);

            }
            Helpers.EscribirEnArchivo(rutaReportes, resultado);

            foreach (var perido in resultadoPeriodo.Keys)
            {
                nombreArchivoPeriodo = "MAE_UNIFICADA_" + perido + "_REG_LLANOS.txt";
                rutaReportePeriodo = Path.Combine(rutaSalida, "Reportes", nombreArchivoPeriodo);
                Helpers.EscribirEnArchivo(rutaReportePeriodo, resultadoPeriodo[perido]);
            }

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Reporte Maestra Inserto 
        /// </summary>
        /// <param name="pDatosImprimir"></param>
        private void EscribirReporteTotalIVR(List<string> pDatosImprimir)
        {
            #region EscribirReporteTotalIVR

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            string nombreArchivo = "TOTAL_IVR.txt";
            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivo);

            if (!File.Exists(rutaReportes))
            {
                resultado.Add("SegundaFechaPago,Telefono,NumeroCampaña,SaldoTotal");
            }

            resultado.AddRange(pDatosImprimir);

            Helpers.EscribirEnArchivo(rutaReportes, resultado);

            #endregion
        }

        /// <summary>
        /// Metodo que Escribe Reporte Recaudo Esperado 
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
                nombreArchivo = dicActual.Key;
                rutaReportes = Path.Combine(rutaSalida, "Reportes", "RecaudoEsperado", nombreArchivo);

                lineasReporte.Add(lineaDatos01);
                lineasReporte.Add(lineaDatos05);
                lineasReporte.AddRange(dicActual.Value);

                foreach (var lineaActual in dicActual.Value)
                {
                    listaSumaServPrincipal.Add(lineaActual.Substring(85, 14));
                    listaSumaServAdicional.Add(lineaActual.Substring(112, 14));

                    if (string.IsNullOrEmpty(fecha1ControlRecaudo.Trim()))
                    {
                        fecha1ControlRecaudo = lineaActual.Substring(126, 8);
                    }

                    if (string.IsNullOrEmpty(fecha2ControlRecaudo.Trim()))
                    {
                        fecha2ControlRecaudo = lineaActual.Substring(196, 8);
                    }
                }


                #region Armo Linea08
                lineaDatos08 = "08";
                lineaDatos08 += dicActual.Value.Count().ToString().PadLeft(9, '0');
                lineaDatos08 += Helpers.SumarCampos(listaSumaServPrincipal).Replace("$", "").Replace(".", "").Replace(",", "").Trim().PadLeft(18, '0');
                lineaDatos08 += Helpers.SumarCampos(listaSumaServAdicional).Replace("$", "").Replace(".", "").Replace(",", "").Trim().PadLeft(18, '0');
                lineaDatos08 += loteReporte.PadLeft(9, '0');
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
                lineaControlRecaudo = dicActual.Key.Replace(".as", "") + "|";
                lineaControlRecaudo += dicActual.Value.Count().ToString().PadLeft(9, '0') + "|";
                lineaControlRecaudo += Helpers.SumarCampos(listaSumaServPrincipal).Replace("$", "").Replace(".", "").Replace(",", "").Trim().PadLeft(18, '0') + "|";
                lineaControlRecaudo += Helpers.SumarCampos(listaSumaServAdicional).Replace("$", "").Replace(".", "").Replace(",", "").Trim().PadLeft(18, '0') + "|";
                lineaControlRecaudo += fecha1ControlRecaudo + "|";
                lineaControlRecaudo += fecha2ControlRecaudo;

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

    /// <summary>
    /// Estructura de Linea Maestra
    /// </summary>
    public struct LineaMaestra
    {
        public string Periodo;
        public string Linea;

        public LineaMaestra(string pPeriodo, string pLinea)
        {
            this.Periodo = pPeriodo;
            this.Linea = pLinea;
        }
    }
}
