using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.ControlInsumos;

namespace App.ControlLogicaProcesos
{
    public class ReportesMasivos
    {
        public static Dictionary<string, List<string>> DiccionarioExtractosReporte = new Dictionary<string, List<string>>();
        string cuenta = string.Empty;
        string rutaSalida = string.Empty;
        string lote = string.Empty;
        public ReportesMasivos()
        { }

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

        public void Ejecutar(Dictionary<string, List<string>> pDatosImprimir)
        {
            Helpers.CrearCarpeta(rutaSalida + @"\Reportes");
            CargarDiccionario();

            ExtraccionReportes(DiccionarioExtractosReporte);           

        }

        private void CargarDiccionario()
        {
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
        }

        private void ExtraccionReportes(Dictionary<string, List<string>> pDatosProcesados)
        {
            List<string> listReporte = new List<string>();
            
            foreach (var datosSal in pDatosProcesados.Values)            
            {
                // Rpt Maestra
                listReporte.AddRange(GetReporteMaestra(datosSal.ToList()));
                EscribirReporteMaestra(listReporte);
                listReporte.Clear();

                // Rpt Maestra Insertos
                listReporte.AddRange(GetReporteMaestraInserto(datosSal.ToList()));
                EscribirReporteMaestraInserto(listReporte);
                listReporte.Clear();

                // Rpt Resumen Distribucion Especial
                listReporte = GetReporteDistribucionEspecial(datosSal.ToList());
                EscribirReporteDistribucionEspecial(listReporte);
                listReporte.Clear();

                // Rpt Resumen SMS
                listReporte = GetReporteSMS(datosSal.ToList());
                EscribirReporteSMS(listReporte);
                listReporte.Clear();
            }

            // Rpt Resumen Maestra
            listReporte = GetLineaResumenMaestra(DiccionarioExtractosReporte);
            EscribirReporteResumenMaestra(listReporte);
            listReporte.Clear();

            // Rpt Resumen Estadistico
            listReporte = GetReporteEstadistico();
            EscribirReporteEstadistico(listReporte);
            listReporte.Clear();            
        }

        private List<string> GetReporteMaestra(List<string> pDatosImprimir)
        {
            #region GetReporteMaestra
            List<string> lineaMaestra = new List<string>();
            lineaMaestra.Add(GetLineaMaestra(pDatosImprimir));

            return lineaMaestra;
            #endregion
        }

        private List<string> GetReporteMaestraInserto(List<string> pDatosImprimir)
        {
            #region GetReporteMaestraInserto
            List<string> lineaMaestraInserto = new List<string>();
            string lineaInsert = GetLineaMaestraInserto(pDatosImprimir);

            if(!string.IsNullOrEmpty(lineaInsert))
            {
                lineaMaestraInserto.Add(GetLineaMaestraInserto(pDatosImprimir));
            }

            return lineaMaestraInserto;
            #endregion
        }

        private List<string> GetReporteResumenMaestra(Dictionary<string, List<string>> pDatosImprimir)
        {
            #region GetReporteResumenMaestra
            List<string> lineaMaestraResumen = new List<string>();

            lineaMaestraResumen = GetReporteResumenMaestra(pDatosImprimir);

            return lineaMaestraResumen;
            #endregion
        }

        private List<string> GetReporteEstadistico()
        {
            #region GetReporteEstadistico
            List<string> lineaMaestraResumen = new List<string>();

            lineaMaestraResumen = GetLineaEstadistico();

            return lineaMaestraResumen;
            #endregion
        }

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
                string[] camposCUFE = resultCUFE.FirstOrDefault().Split('|');

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
                                    dicTranspromo.Add(lineaActual.Split('|')[33], 0);
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
                                    dicCupones.Add(lineaActual.Split('|')[36], 0);
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
                                dicCartas.Add(lineaActual.Split('|')[1], 0);
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
                lineasResumen.Add("Cupones" + keyCupones + "|" + dicTranspromo[keyCupones].ToString());
            }

            return lineasResumen;
            #endregion
        }

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
                }

                lineaSMS = campos1AAA[15].Trim() + mensaje + mensaje2;
            }

            return lineaSMS;
            #endregion
        }

        #region Metodos Propios
        private string GetArchivoSalida()
        {
            #region GetArchivoSalida
            string nombreArchivo = string.Empty;

            List<string> archivos = new List<string>();

            archivos = Directory.GetFiles(rutaSalida).ToList();

            foreach (string archivoActual in archivos)
            {
                if (archivoActual.Contains("COMPLETO"))
                {
                    continue;
                }

                var resultArchivos = from busqueda in File.ReadAllLines(archivoActual)
                                     where busqueda.Length > 5 && busqueda.Split('|')[0].Trim().Equals("1AAA") && busqueda.Split('|')[7].Trim().Equals(cuenta)
                                     select busqueda;

                if (resultArchivos.Any())
                {
                    nombreArchivo = Path.GetFileNameWithoutExtension(archivoActual);
                    break;
                }
            }

            return nombreArchivo;
            #endregion
        }

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

            if(!File.Exists(rutaReportes))
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

            if(!File.Exists(rutaReportes))
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

            if(!File.Exists(rutaReportes))
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

            if(!File.Exists(rutaReportes))
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
            #region EscribirReporteDistribucionEspecial

            List<string> resultado = new List<string>();
            string rutaReportes = string.Empty;
            int contadorRegistros = 0;
            string nombreArchivo = "FACDisp_" + DateTime.Now.ToString("ddMMyyyy") + "_16_17_31_32_34_35_38_39_40_45_46_51_52_53_68_70_71_87_92_93_1.txt";

            resultado.AddRange(pDatosImprimir);
            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivo);

            Helpers.EscribirEnArchivo(rutaReportes, resultado);

            contadorRegistros = resultado.Count();
            resultado.Clear();

            // Reporte global
            nombreArchivo = "FACDisp_" + DateTime.Now.ToString("ddMMyyyy") + "_T.txt";
            rutaReportes = Path.Combine(rutaSalida, "Reportes", nombreArchivo);

            resultado.Add("Cantidad de archivos:  1;Cantidad de registros totales: " + contadorRegistros + ";");
            Helpers.EscribirEnArchivo(rutaReportes, resultado);


            #endregion
        }
    }
}
