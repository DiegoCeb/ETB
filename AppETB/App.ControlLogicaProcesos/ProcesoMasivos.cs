using App.ControlInsumos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLL_Utilidades;

namespace App.ControlLogicaProcesos
{
    public class ProcesoMasivos : IProcess
    {
        private bool IsResidencial { get; set; }
        private bool IsGobierno { get; set; }
        private bool IsFibra { get; set; }
        private bool IsDatos { get; set; }
        private bool IsLte { get; set; }
        private bool IsLteCorporativo { get; set; }
        private bool IsAnexoFibra { get; set; }
        private string Cuenta { get; set; }
        private string Ciclo { get; set; }
        private string Estrato { get; set; }
        private string PeriodoFacturacion { get; set; }
        private string FechaDesde { get; set; }
        private string FechaHasta { get; set; }
        private string FechaExpedicion { get; set; }

        public ProcesoMasivos(string pArchivo, string pPeridoFacturacion)
        {
            #region ProcesoMasivos
            try
            {
                PeriodoFacturacion = pPeridoFacturacion;
                Ejecutar(pArchivo);
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

        /// <summary>
        /// Constructor General
        /// </summary>
        public ProcesoMasivos()
        { }

        public void CargueFormateoArchivo(string pArchivo)
        {
            #region CargueFormateoArchivo
            List<string> DatosArchivo = File.ReadAllLines(pArchivo, Encoding.Default).ToList();
            List<string> datosExtractoFormateo = new List<string>();
            bool extractoCompleto = false;
            string llaveCruce = string.Empty;

            foreach (var linea in DatosArchivo)
            {
                if (linea.Substring(0, 6) == "000000") //Inicio Extracto
                {
                    extractoCompleto = false;

                    if (datosExtractoFormateo.Count > 1)
                    {
                        extractoCompleto = true;
                    }

                    if (extractoCompleto)
                    {
                        llaveCruce = datosExtractoFormateo.ElementAt(1).Substring(117, 20).Trim();

                        if (!string.IsNullOrEmpty(Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoCuentasExtraer, llaveCruce)))
                        {
                            //Cuenta Retenida
                            datosExtractoFormateo.Clear();
                        }
                        else
                        {
                            AgregarDiccionario(llaveCruce, FormatearArchivo(llaveCruce, datosExtractoFormateo));

                            datosExtractoFormateo.Clear();
                        }
                    }

                    datosExtractoFormateo.Add(linea);
                }
                else
                {
                    datosExtractoFormateo.Add(linea);
                }
            }

            //Ultimo Extracto
            if (datosExtractoFormateo.Count > 1)
            {
                llaveCruce = datosExtractoFormateo.ElementAt(1).Substring(117, 20).Trim();

                if (!string.IsNullOrEmpty(Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoCuentasExtraer, llaveCruce)))
                {
                    //Cuenta Retenida
                    datosExtractoFormateo.Clear();
                }
                else
                {
                    AgregarDiccionario(llaveCruce, FormatearArchivo(llaveCruce, datosExtractoFormateo));

                    datosExtractoFormateo.Clear();
                }
            }

            DatosArchivo.Clear();
            #endregion
        }

        /// <summary>
        /// Metodo que agrega al Dicionario General.
        /// </summary>
        /// <param name="pLlaveCruce">llave de cruce (Cedula)</param>
        /// <param name="pTemp">Lista del extracto</param>
        private void AgregarDiccionario(string pLlaveCruce, List<string> pTemp)
        {
            #region AgregarDiccionario
            if (Variables.Variables.DiccionarioExtractosFormateados.ContainsKey(pLlaveCruce))
            {
                Variables.Variables.DiccionarioExtractosFormateados[pLlaveCruce].AddRange(pTemp);
            }
            else
            {
                Variables.Variables.DiccionarioExtractosFormateados.Add(pLlaveCruce, new List<string>(pTemp));
            }
            #endregion
        }

        public void Ejecutar(string pArchivo)
        {
            CargueFormateoArchivo(pArchivo);
        }

        public List<string> FormatearArchivo(string pLLaveCruce, List<string> datosOriginales)
        {
            List<string> resultado = new List<string>();

            //if (pLLaveCruce == "2484047")
            //{

            //}

            FormatearPropiedadesExtracto();

            string canal1AAA = MapeoCanal1AAA(datosOriginales);
            

            //if (!string.IsNullOrEmpty(canal1AAA))
            //{
            //    resultado.Add(canal1AAA);
            //}

            //string canal1CMP = FormateoCanal1CMP(datosOriginales);

            //if (!string.IsNullOrEmpty(canal1CMP))
            //{
            //    resultado.Add(canal1CMP);
            //}

            //string canal1PPP = FormateoCanal1PPP(datosOriginales);

            //if (!string.IsNullOrEmpty(canal1PPP))
            //{
            //    resultado.Add(canal1PPP);
            //}

            //string canal1ADNC = MapeoCanalADNC(datosOriginales);

            //if (!string.IsNullOrEmpty(canal1ADNC))
            //{
            //    resultado.Add(canal1ADNC);
            //}

            //IEnumerable<string> canalADN1 = FormateoCanalADN1(datosOriginales);

            //if (canalADN1.Any())
            //{
            //    resultado.AddRange(canalADN1);
            //}

            //string canalNTC0 = FormateoCanalNTC0(datosOriginales);

            //if (!string.IsNullOrEmpty(canalNTC0))
            //{
            //    resultado.Add(canalNTC0);
            //}

            //string canalNTC1 = FormateoCanalNTC1(datosOriginales);

            //if (!string.IsNullOrEmpty(canalNTC1))
            //{
            //    resultado.Add(canalNTC1);
            //}

            //string canalNTC2 = MapeoCanalNTC2(datosOriginales);

            //if (!string.IsNullOrEmpty(canalNTC2))
            //{
            //    resultado.Add(canalNTC2);
            //}

            //string canalNTC3 = MapeoCanalNTC3(datosOriginales);

            //if (!string.IsNullOrEmpty(canalNTC3))
            //{
            //    resultado.Add(canalNTC3);
            //}

            //string canalNTC4 = MapeoCanalNTC4(datosOriginales);

            //if (!string.IsNullOrEmpty(canalNTC4))
            //{
            //    resultado.Add(canalNTC4);
            //}

            //IEnumerable<string> canalNTC5 = MapeoCanalNTC5(datosOriginales);

            //if (canalNTC5.Any())
            //{
            //    resultado.AddRange(canalNTC5);
            //}

            //string canal1ODC = FormateoCanal1ODC(datosOriginales);

            //if (!string.IsNullOrEmpty(canal1ODC))
            //{
            //    resultado.Add(canal1ODC);
            //}

            //IEnumerable<string> canal1ODD = FormateoCanal1ODD(datosOriginales);

            //if (canal1ODD.Any())
            //{
            //    resultado.AddRange(canal1ODD);
            //}

            List<string> linea1BBB = new List<string> { "1BBB|Valor factura anterior|$ 29.900,00 | ",
                                                        "1BBB|Gracias por su pago|-$ 29.900,00 | | ",
                                                        "1BBB|Servicios etb con IVA|$ 32.500,19 | ",
                                                        "1BBB|Ajuste a la decena|-$ 0,19 | "};

            //resultado.Add(MapeoCanal1BBA(datosOriginales,linea1BBB.ToList()));
            ////resultado.AddRange(MapeoCanal1CCC(datosOriginales));
            resultado.Add(MapeoCanal1CCM(datosOriginales));
            resultado.Add(MapeoCanal1AFI(datosOriginales));
            resultado.AddRange(MapeoCanal1BFI(datosOriginales));
            resultado.AddRange(MapeoCanal1CFI(datosOriginales));
            resultado.Add(MapeoCanal1BBA(datosOriginales, linea1BBB));

            return resultado;
        }

        private void FormatearPropiedadesExtracto()
        {
            IsResidencial = false;
            IsDatos = false;
            IsFibra = false;
            IsGobierno = false;
            IsLte = false;
            IsLteCorporativo = false;
            Cuenta = string.Empty;
            Ciclo = string.Empty;
            Estrato = string.Empty;
            FechaExpedicion = string.Empty;
    }

        /// <summary>
        /// Linea que obtiene canal 1BBA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1BBA(List<string> datosOriginales, List<string> Lineas1BBB)
        {
            #region Canal 1BBA

            // Calcuar el subtotal del 1BBB
            double subtotal1BBB = 0;

            if (Lineas1BBB.Count > 0)
            {
                string valorActual = string.Empty;

                foreach (string LineaActual1BBB in Lineas1BBB)
                {
                    valorActual = LineaActual1BBB.Split('|')[2].Replace("$", "").Replace(".", "").Replace(",", "").Replace(" ", "").Trim();
                    subtotal1BBB += Convert.ToDouble(valorActual);
                }
            }


            // Creacion Canal 1BBA
            string Linea1BBA = string.Empty;

            List<PosCortes> listaCortes = new List<PosCortes>();

            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                         select busqueda;

            if (result.Any())
            {
                // Cortes                
                listaCortes.Add(new PosCortes(154, 14, TiposFormateo.Decimal01));

                // Linea Retornar
                Linea1BBA = "1BBA";

                // Se vaida que el Subtotal del 1BBB se
                if (subtotal1BBB > 0)
                {
                    Linea1BBA += IsFibra ? "|Valor total a pagar" : "|Total de la Factura ETB";
                }
                else
                {
                    Linea1BBA += IsFibra ? "|Valor total a pagar" : "|Saldo a Favor";
                }

                Linea1BBA += "|" + Helpers.ExtraccionCamposSpool(listaCortes, result.FirstOrDefault());
                Linea1BBA += "| ";
            }

            return Linea1BBA;

            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la linea formateada de Canal 1AAA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1AAA(List<string> datosOriginales)
        {
            #region MapeoCanal1AAA
            string Linea1AAA = string.Empty;
            string Linea010000 = string.Empty;
            string Linea040000 = string.Empty;

            List<string> ListaCanal1AAA = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();


            ListaCanal1AAA.Add("1AAA");
            ListaCanal1AAA.Add("KitXXXX");

            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                         select busqueda;

            var result040000 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                               select busqueda;

            if (result.Any())
            {
                Linea010000 = result.FirstOrDefault();
                Linea040000 = result040000.FirstOrDefault();

                listaCortes.Add(new PosCortes(6, 50));
                listaCortes.Add(new PosCortes(56, 12));
                listaCortes.Add(new PosCortes(68, 40));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));

                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"DANC{Linea010000.Substring(108, 5).Trim()}").FirstOrDefault()?.Substring(9).Trim() ?? string.Empty);
                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"DAND{Linea010000.Substring(108, 2).Trim()}").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty);
                

                listaCortes.Clear();
                listaCortes.Add(new PosCortes(117, 20));
                Cuenta = Linea010000.Substring(117, 20).Trim();// Cuenta
                listaCortes.Add(new PosCortes(139, 12));
                listaCortes.Add(new PosCortes(151, 4));
                Ciclo = Linea010000.Substring(151, 4).Trim().TrimStart('0'); // Asignamos Ciclo a variable Global
                listaCortes.Add(new PosCortes(155, 13, TiposFormateo.Decimal01));
                listaCortes.Add(new PosCortes(168, 8, TiposFormateo.Fecha01));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));
                ListaCanal1AAA.Add(GetMesMora(Linea010000.Substring(176, 2).Trim()));

                listaCortes.Clear();
                listaCortes.Add(new PosCortes(178, 8, TiposFormateo.Fecha01));
                FechaDesde = Linea010000.Substring(178, 8).Trim();
                listaCortes.Add(new PosCortes(186, 8, TiposFormateo.Fecha01));
                FechaHasta = Linea010000.Substring(186, 8).Trim();
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));

                ListaCanal1AAA.Add(GetTelefono(datosOriginales)); //TODO: Verificar Reglas
                ListaCanal1AAA.Add(IsFibra ? (string.IsNullOrEmpty(Linea010000.Substring(218, 20).Trim()) ? " " : Linea010000.Substring(218, 20).Trim()) : " ");
                
                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECP{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);
                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECL{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);
                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECX{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);// TODO: Validar


                ListaCanal1AAA.Add(GetNumeroReferencia(Linea010000.Substring(139, 12)));
                ListaCanal1AAA.AddRange(GetCodigosBarras(Linea010000.Substring(139, 12), Linea010000)); //TODO: Verificar valor a pagar
                ListaCanal1AAA.Add(GetTipoEtapas(Linea010000.Substring(151, 3)));
                //ListaCanal1AAA.Add(GetTasaInteres(Linea040000));
                listaCortes.Clear();
                listaCortes.Add(new PosCortes(108, 5));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));
                ListaCanal1AAA.Add(string.Empty); // TODO: Anexos Publicitarios - Verificar regla
                ListaCanal1AAA.Add(GetActividad(Linea040000));
                //ListaCanal1AAA.Add(GetEstrato(Linea040000));
                ListaCanal1AAA.AddRange(GetBarrioLocalidad());
                ListaCanal1AAA.Add(string.Empty);
                ListaCanal1AAA.Add(GetLogoInternet());
                ListaCanal1AAA.Add(GetMarcaAnexosPublicidad());
                ListaCanal1AAA.AddRange(GetEmailTipoEmal());
                ListaCanal1AAA.AddRange(GetMarcaCupones());
                ListaCanal1AAA.Add(string.Empty); // TODO: Numero de Hojas
                ListaCanal1AAA.Add(GetNivelReclamacion());

            }

            Linea1AAA = Helpers.ListaCamposToLinea(ListaCanal1AAA,'|');

            return Linea1AAA;
            #endregion
        }

        #region Metodos Mapeos 1AAA

        /// <summary>
        /// Regla de Mes Mora
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private string GetMesMora(string pCampo)
        {
            #region GetMesMora
            int mesMora = Convert.ToInt32(pCampo);
            mesMora += 1;
            mesMora = mesMora > 4 ? 4 : mesMora;
            return mesMora.ToString();
            #endregion
        }

        /// <summary>
        /// Reglas Get Telefono
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string GetTelefono(List<string> datosOriginales)
        {
            #region GetTelefono
            string telefono = string.Empty;

            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                         select busqueda;

            if (result != null)
            {
                telefono = result.FirstOrDefault().Substring(6, 20).Trim();
            }

            return telefono;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene el Numero de Referencia
        /// </summary>
        /// <param name="pNumReferencia"></param>
        /// <returns></returns>
        private string GetNumeroReferencia(string pNumReferencia, bool SeparadoGuiones = true)
        {
            #region GetNumeroReferencia

            string numFijoRef = Utilidades.LeerAppConfig("numeroReferencia");

            string numReferencia = $"{numFijoRef}{pNumReferencia}";

            int digitoVerificacion = 0;

            for (int i = 1; i <= numReferencia.Length; i++)
            {
                if (i % 2 == 0)
                {
                    digitoVerificacion += Convert.ToInt32(numReferencia.Substring((i - 1), 1)) * 3;
                }
                else
                {
                    digitoVerificacion += Convert.ToInt32(numReferencia.Substring((i - 1), 1)) * 1;
                }
            }

            var Multiplo = Math.Ceiling((decimal)digitoVerificacion / 10) * 10;

            digitoVerificacion = (int)Multiplo - digitoVerificacion;

            string resultado = string.Empty;
            if (SeparadoGuiones)
            { resultado = $"{numFijoRef}-{pNumReferencia}-{digitoVerificacion.ToString()}"; }
            else
            { resultado = $"{numFijoRef}{pNumReferencia}{digitoVerificacion.ToString()}"; }
            return resultado;

            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene Los Codigos de Barras
        /// </summary>
        /// <param name="pNumReferencia"></param>
        /// <param name="pLinea010000"></param>
        /// <returns></returns>
        private List<string> GetCodigosBarras(string pNumReferencia, string pLinea010000)
        {
            #region GetCodigosBarras
            List<string> result = new List<string>();
            string numeroETB = Utilidades.LeerAppConfig("numeroETB");
            string totalPagar = pLinea010000.Substring(155, 11).PadLeft(10, '0');
            string fechaPago = Helpers.FormatearCampos(TiposFormateo.Fecha08, Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECL{Helpers.FormatearCampos(TiposFormateo.Fecha02, pLinea010000.Substring(168, 8).Trim())}{pLinea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);
            string numReferencia = GetNumeroReferencia(pNumReferencia, false);

            string CodeBar1 = $"(415){numeroETB}(8020){numReferencia}(3900){totalPagar}(96){fechaPago}";
            //"(415)$numero_etb(8020){$valores_temp["Total1BBB"]["numero_referencia"]}(3900)0000000000(96)" . formatear_fecha($valores_temp["fecha_pago_extemporaneo"], 4);
            result.Add(CodeBar1);

            string CodeBar2 = $"(415){numeroETB}(8020){numReferencia}(3900){totalPagar}(96){fechaPago}";
            //"(415)$numero_etb(8020){$valores_temp["Total1BBB"]["numero_referencia"]}(3900)0000000000(96)" . formatear_fecha($valores_temp["fecha_pago_extemporaneo"], 4);
            result.Add(CodeBar2);
            return result;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene el Tipo de Etapa
        /// </summary>
        /// <param name="pCiclo"></param>
        /// <returns></returns>
        private string GetTipoEtapas(string pCiclo)
        {
            #region GetTipoEtapas
            string tipociclo = string.Empty;

            Int16 ciclo = Convert.ToInt16(pCiclo);

            switch (ciclo)
            {
                case 15:
                case 16:
                case 87:
                    tipociclo = "FIB";
                    IsFibra = true;
                    break;

                case 17:
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                case 36:
                case 38:
                case 39:
                case 40:
                case 44:
                case 45:
                case 46:
                case 47:
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                case 58:
                case 59:
                case 60:
                case 61:
                case 62:
                case 63:
                case 68:
                case 70:
                case 71:
                    tipociclo = "PAR";
                    IsResidencial = true;
                    break;

                case 18:
                case 19:
                case 37:
                case 64:
                case 67:
                case 69:
                    tipociclo = "GOB";
                    IsGobierno = true;
                    break;

                case 66:
                case 79:
                case 80:
                case 81:
                case 82:
                case 84:
                case 85:
                case 86:
                case 88:
                    tipociclo = "DAT";
                    IsDatos = true;
                    break;

                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 90:
                case 91:
                case 92:
                case 93:
                case 94:
                case 95:
                    tipociclo = "LTE";
                    IsLte = true;
                    break;

                case 96:
                case 97:
                    tipociclo = "LTE_CORP";
                    IsLteCorporativo = true;
                    break;
            }

            return tipociclo;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene la TasaInteres
        /// </summary>
        /// <param name="pLinea040000"></param>
        /// <returns></returns>
       private string GetTasaInteres(string pLinea040000)
        {
            #region GetTasaInteres
            string idActividad = GetIdActividad(pLinea040000.Substring(124, 2));
           string tasaInteresTablaSustitucion = string.Empty;

           tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM{idActividad}").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
            tasaInteresTablaSustitucion.Replace("TASAS DE MORA POR ACTIVIDAD", string.Empty);
            
           if (!string.IsNullOrEmpty(tasaInteresTablaSustitucion))
            {
                return tasaInteresTablaSustitucion;
            }
            else
            {
                if (IsDatos || IsLteCorporativo)
                {
					tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM10").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
                    return tasaInteresTablaSustitucion;
                }
                else if (IsLte)
                {
                    tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM08").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
                    return tasaInteresTablaSustitucion;
                }
                else if (IsResidencial || IsFibra || IsGobierno)
                {
                    tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM08").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
                    tasaInteresTablaSustitucion += $" {Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM10").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty}";

                   return tasaInteresTablaSustitucion;
                }
                else
                {
                    return string.Empty;
                }
            }
           #endregion
        }

        /// <summary>
        /// Metodo que Obtiene el ID Actividad
        /// </summary>
        /// <param name="pActividadSpool"></param>
        /// <returns></returns>
        private string GetIdActividad(string pActividadSpool)
        {
            #region GetIdActividad
            string actividad = pActividadSpool;

            if ((IsLte || IsLteCorporativo) && !string.IsNullOrEmpty(actividad))
            {
                actividad = "00";
            }

            if ((Ciclo == "66" || Ciclo == "79" || (Ciclo.Length == 2 && (Ciclo != "8" && Ciclo.Substring(0, 1) == "8"))) && Ciclo != "87")
            {
                actividad = "10";
            }

            return actividad;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene la Actividad
        /// </summary>
        /// <param name="pLinea040000"></param>
        /// <returns></returns>
        private string GetActividad(string pLinea040000)
        {
            #region GetActividad
            string idActividad = GetIdActividad(pLinea040000.Substring(124, 2));

            string actividadTablaSustitucion = string.Empty;

            actividadTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"ACTR{idActividad}").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
            actividadTablaSustitucion = actividadTablaSustitucion.Replace("ACTIVIDAD REVCHAIN", string.Empty);

            if (!string.IsNullOrEmpty(actividadTablaSustitucion) && (IsResidencial || IsFibra || IsGobierno))
            {
                return actividadTablaSustitucion;
            }
            else
            {
                return string.Empty;
            }



            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene el Estrato
        /// </summary>
        /// <param name="pLinea040000"></param>
        /// <returns></returns>
        private string GetEstrato(string pLinea040000)
        {
            #region GetEstrato
            string Estrato = pLinea040000.Substring(123, 1);

            if (!string.IsNullOrEmpty(Estrato.Trim()))
            {
                try
                {
                    int estratoInt = Convert.ToInt16(Estrato);
                    estratoInt = estratoInt > 6 ? 6 : estratoInt;
                    Estrato = estratoInt.ToString();
                    return estratoInt.ToString();
                }
                catch
                {
                    return string.Empty;
                }

            }
            else
            {
                return string.Empty;
            }
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene el Estrato
        /// </summary>
        /// <returns></returns>
        private List<string> GetBarrioLocalidad()
        {
            #region GetBarrioLocalidad
            List<string> resultado = new List<string>();

            string barrio = string.Empty;
            string localidad = string.Empty;
            string locBar = string.Empty;

            locBar = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoLocBar, $"{Cuenta}").FirstOrDefault();

            if (!string.IsNullOrEmpty(locBar))
            {
                List<string> camposlocBar = locBar.Split('|').ToList();

                string llaveBarrio = $"CODX{camposlocBar[1].TrimStart('0')}{camposlocBar[2]}";
                string llaveLocalidad = $"CODL{camposlocBar[1]}";

                if (!string.IsNullOrEmpty(llaveBarrio))
                {
                    barrio = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveBarrio).FirstOrDefault()?.Substring(12).Trim() ?? string.Empty;

                }

                if (!string.IsNullOrEmpty(llaveLocalidad))
                {
                    localidad = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveLocalidad).FirstOrDefault()?.Substring(7).Trim() ?? string.Empty;
                }
            }


            resultado.Add(barrio);
            resultado.Add(localidad);

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene el MarcaAnexosPublicidad
        /// </summary>
        /// <returns></returns>
        private string GetLogoInternet()
        {
            #region GetLogoInternet
            string logoIntener = string.Empty;

            if ((IsFibra || IsResidencial) && Estrato != "" && (Estrato.Trim() == "1" || Estrato.Trim() == "2")) //Regla de validacion del logo de MinTic en facturas fibra y que estrato sea igual a 2
            {
                logoIntener = "LogInt";
            }


            return logoIntener;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene el MarcaAnexosPublicidad
        /// </summary>
        /// <returns></returns>
        private string GetMarcaAnexosPublicidad()
        {
            #region GetMarcaAnexosPublicidad
            string marca = string.Empty;

            marca = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoBaseTranspromo, $"{Cuenta}").FirstOrDefault() ?? string.Empty;

            if (!string.IsNullOrEmpty(marca))
            {
                string[] camposMarca = marca.Split('|');

                if (camposMarca.Length > 1)
                {
                    marca = camposMarca[1];
                }
            }


            return marca;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene el Email y Tipo Email
        /// </summary>
        /// <returns></returns>
        private List<string> GetEmailTipoEmal()
        {
            #region GetEmailTipoEmal
            List<string> emailTipoEmail = new List<string>();
            string email = string.Empty;
            string tipoEmail = string.Empty;

            string lineaEmail = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoDistribucionEmailRevchain, $"{Cuenta}").FirstOrDefault() ?? string.Empty;

            if (!string.IsNullOrEmpty(lineaEmail))
            {
                string[] camposEmail = lineaEmail.Split('|');

                if (camposEmail.Length > 5)
                {
                    for (int i = 1; i < camposEmail.Length -1; i++)
                    {
                        if (!string.IsNullOrEmpty(camposEmail[i]))
                        {
                            if (string.IsNullOrEmpty(email))
                            { email += $"{camposEmail[i]}"; }
                            else
                            { email += $",{camposEmail[i]}"; }
                            
                        }

                    }
                }
            }
            
            if (!string.IsNullOrEmpty(email))
            {
                string lineaDistribucionDoble = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoDistribucionDobleRevchain, $"{Cuenta}") ?? string.Empty;

                if (!string.IsNullOrEmpty(lineaDistribucionDoble))
                { tipoEmail = "dual"; }
                else 
                { tipoEmail = "solo_email";  }
            }

            emailTipoEmail.Add(email);
            emailTipoEmail.Add(tipoEmail);

            return emailTipoEmail;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene Marca Cupones
        /// </summary>
        /// <returns></returns>
        private List<string> GetMarcaCupones()
        {
            #region GetMarcaCupones
            List<string> marcaCupones = new List<string>();
            string cupones = string.Empty;
            string cupones2 = string.Empty;

            string lineaBaseCupones = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoBaseCupones, $"{Cuenta}").FirstOrDefault() ?? string.Empty;

            if (!string.IsNullOrEmpty(lineaBaseCupones))
            {
                string[] camposCupones = lineaBaseCupones.Split('|');

                if (camposCupones.Length > 1)
                {
                    cupones = camposCupones[1];
                }
            }

            marcaCupones.Add(cupones);
            marcaCupones.Add(cupones2);

            return marcaCupones;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene Nivel de Reclamacion
        /// </summary>
        /// <returns></returns>
        private string GetNivelReclamacion()
        {
            #region GetNivelReclamacion
            string nivelReclamacion = string.Empty;

            string lineaNivelReclamacion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoNivelReclamacion, $"{Cuenta}").FirstOrDefault() ?? string.Empty;

            if (!string.IsNullOrEmpty(lineaNivelReclamacion.Trim()))
            {
                string[] camposNivelReclamacion = lineaNivelReclamacion.Split('|');

                if (camposNivelReclamacion.Length > 0)
                {
                    nivelReclamacion = camposNivelReclamacion[1];
                }
            }

            return nivelReclamacion;
            #endregion
        }

        #endregion Metodos 1AAA

        /// <summary>
        /// Linea que obtiene canal 1CCC
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanal1CCC(List<string> datosOriginales)
        {
            #region Canal 1CCC
            IEnumerable<string> Linea1CCC = null;

            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && (busqueda.Substring(0, 6).Equals("06T931") || busqueda.Substring(0, 6).Equals("06T935"))
                         select busqueda;

            if (result != null)
            {

            }

            return Linea1CCC;
            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal 1BBB
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanal1BBB(List<string> datosOriginales)
        {
            #region MapeoCanal1BBB
            IEnumerable<string> Lineas1BBB = new List<string>();
            return Lineas1BBB;
            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1CCM
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1CCM(List<string> datosOriginales)
        {
            #region Canal 1CCM
            string Linea1CCM = string.Empty;
            string cargosCCM = string.Empty;

            if (Ciclo == "70")
            {
                cargosCCM = "Cargos: " + FechaExpedicion;
            }

            if (!IsLte && !IsLteCorporativo)
            {
                Linea1CCM = "1CCM| |" + cargosCCM + " | ";
            }
            else
            {
                Linea1CCM = "1CCM| | | ";
            }

            return Linea1CCM;
            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1CCA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1CCA(List<string> datosOriginales)
        {
            #region MapeoCanal1CCA
            string Linea1CCA = string.Empty;
            return Linea1CCA;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1KKK
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1KKK(List<string> datosOriginales)
        {
            #region Canal 1KKK
            string Linea1KKK = string.Empty;
            return Linea1KKK;
            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1CPA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1CPA(List<string> datosOriginales)
        {
            #region MapeoCanal1CPA
            string Linea1CPA = string.Empty;
            return Linea1CPA;
            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1MMM
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanal1MMM(List<string> datosOriginales)
        {
            #region Canal 1MMM
            IEnumerable<string> Linea1MMM = null;
            return Linea1MMM;
            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal CUFE
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanalCUFE(List<string> datosOriginales)
        {
            #region Canal 1CUFE
            string LineaCUFE = string.Empty;
            return LineaCUFE;
            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1AFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1AFI(List<string> datosOriginales)
        {
            #region MapeoCanal1AFI      

            #region Variables con Valores a pintar
            List<PosCortes> listaCortes = new List<PosCortes>();

            string Linea1AFI = string.Empty;
            string letrasPaquetes = string.Empty;
            bool banPrimer11C304 = false;
            string periodo = string.Empty;
            string valorBase = string.Empty;
            string valorIva = string.Empty;
            string valorTotal = string.Empty;
            string subsidio = " ";
            string periodoLargo = string.Empty;
            string periodoDesde = string.Empty;
            string periodoHasta = string.Empty;

            List<string> listaFechas11C304 = new List<string>();
            List<string> listaFechasDesde = new List<string>();
            List<string> listaFechasHasta = new List<string>();

            #endregion

            var result = from busqueda in datosOriginales
                               where busqueda.Length > 6 && (busqueda.Substring(0, 6).Equals("11C101") ||
                                                             busqueda.Substring(0, 6).Equals("11C304") ||
                                                             busqueda.Substring(0, 6).Equals("11C401") ||
                                                             busqueda.Substring(0, 6).Equals("11C410") ||
                                                             busqueda.Substring(0, 6).Equals("11C411") ||
                                                             busqueda.Substring(0, 6).Equals("11C412") ||
                                                             busqueda.Substring(0, 6).Equals("02T004"))
                               select busqueda;


            if (result.Any())
            {
                // se capturan las letras de los paquetes
                foreach (var registroActual in result)
                {

                    #region Toma de Letras Paquete
                    string llaveUno = $"{registroActual.Substring(6, 10)}";
                    string llaveDos = $"{registroActual.Substring(6, 6)}";
                    string valor = string.Empty;

                    if (Variables.Variables.DatosInsumoParametrizacionPaquetesFibra.ContainsKey(llaveUno))
                    {
                        valor = Variables.Variables.DatosInsumoParametrizacionPaquetesFibra[llaveUno];
                    }
                    else if (Variables.Variables.DatosInsumoParametrizacionPaquetesFibra.ContainsKey(llaveDos))
                    {
                        valor = Variables.Variables.DatosInsumoParametrizacionPaquetesFibra[llaveDos];
                    }

                    if (!string.IsNullOrEmpty(valor))
                    {
                        if (!letrasPaquetes.Contains(valor.Split('|')[2]))
                        {
                            letrasPaquetes += valor.Split('|')[2];
                        }
                    }
                    #endregion
                                        
                    #region Captura Periodo 
                    if (registroActual.Substring(0, 6) == "11C304" && banPrimer11C304 == false)
                    {
                        periodo = registroActual.Substring(128, 6);
                        banPrimer11C304 = true;
                    }
                    #endregion

                    #region Llena Diccionarios y listas de fechas
                    if (registroActual.Substring(0, 6) == "11C304")
                    {
                        listaFechas11C304.Add(registroActual.Substring(128, 19));
                        listaFechasDesde.Add(registroActual.Substring(128, 8));
                        listaFechasHasta.Add(registroActual.Substring(139, 8));
                    }
                    #endregion
                    
                    #region Captura Totales
                    if (registroActual.Substring(0, 6) == "02T004")
                    {
                        listaCortes.Add(new PosCortes(6, 14, TiposFormateo.Decimal01));
                        valorBase = Helpers.ExtraccionCamposSpool(listaCortes, registroActual);
                        listaCortes.Clear();

                        listaCortes.Add(new PosCortes(34, 14, TiposFormateo.Decimal01));
                        valorIva = Helpers.ExtraccionCamposSpool(listaCortes, registroActual);
                        listaCortes.Clear();

                        List<string> listCamposSumar = new List<string>();
                        listCamposSumar.Add(registroActual.Substring(6, 14));
                        listCamposSumar.Add(registroActual.Substring(34, 14));
                        valorTotal = Helpers.SumarCampos(listCamposSumar);
                    } 
                    #endregion
                }

                // Capturar el nombre del paquete
                string nombrePaquete = GetNombrePaquete(letrasPaquetes);

                string fechaReciente = Helpers.GetFechaMasReciente(listaFechas11C304);

                // Se captura fecha mas reciente
                if (!string.IsNullOrEmpty(fechaReciente))
                {                    
                    periodoLargo = "(" + Helpers.FormatearCampos(TiposFormateo.Fecha04, fechaReciente.Substring(4, 4)) + " - " + Helpers.FormatearCampos(TiposFormateo.Fecha04, fechaReciente.Substring(13, 4)) + ")";
                }
                else
                {
                    periodoLargo = " ";
                }
                

                // Se captura la Fecha periodoDesde se toma la fecha mas antigua
                periodoDesde = Helpers.GetFechaMaximaOMinima(listaFechasDesde, 2);

                // Se captura la Fecha periodoHasta se toma la fecha mas Reciente
                periodoHasta = Helpers.GetFechaMaximaOMinima(listaFechasHasta, 1);

                Linea1AFI = "1AFI|";
                Linea1AFI += periodo + "|";
                Linea1AFI += nombrePaquete + "|";
                Linea1AFI += valorBase + "|";
                Linea1AFI += valorIva + "|";
                Linea1AFI += valorTotal + "|";
                Linea1AFI += subsidio + "|";
                Linea1AFI += periodoLargo + "| |";
                Linea1AFI += periodoDesde + "|";
                Linea1AFI += periodoHasta + "| ";
            }

            return Linea1AFI;

            #endregion
        }

        private string GetNombrePaquete(string letras)
        {
            #region GetNombrePaquete

            string llave = string.Empty;
            string valor = string.Empty;

            if (letras.Contains("A") && letras.Contains("B") && letras.Contains("C"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["ABC"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("A") && letras.Contains("B"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["AB"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("A") && letras.Contains("C"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["AC"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("B") && letras.Contains("C"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["BC"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("F") && letras.Contains("G"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["FG"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("A") && letras.Contains("G"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["AG"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("B") && letras.Contains("F"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["BF"];
                return valor.Split('|')[1];
            }
            else
            {
                return " ";
            }


            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal 1BFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanal1BFI(List<string> datosOriginales)
        {
            #region MapeoCanal1BFI

            IEnumerable<string> Lineas1CFI = null;
            string lineaTemp1CFI = string.Empty;
            List<string> resultadoLinea1CFI = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();

            #region Busqueda            

            var result11C101 =  from busqueda in datosOriginales
                                where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("11C101")                         
                                select busqueda;

            var result11C304 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("11C304")
                               select busqueda;

            var result11C401 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("11C401")
                               select busqueda;




            #endregion

            if (IsResidencial || IsFibra)
            {
                // Procesamiento Canal 11C101
                if (result11C101.Any())
                {
                    foreach (var linea11C101 in result11C101)
                    {
                        lineaTemp1CFI = Logica1BFI(linea11C101);

                        if(!string.IsNullOrEmpty(lineaTemp1CFI))
                        {
                            resultadoLinea1CFI.Add(lineaTemp1CFI);
                        }
                    }
                }

                // Procesamiento Canal 11C304
                if (result11C304.Any())
                {
                    foreach (var linea11C304 in result11C304)
                    {
                        lineaTemp1CFI = Logica1BFI(linea11C304);

                        if (!string.IsNullOrEmpty(lineaTemp1CFI))
                        {
                            resultadoLinea1CFI.Add(lineaTemp1CFI);
                        }
                    }
                }

                // Procesamiento Canal 11C401
                if (result11C401.Any())
                {
                    foreach (var linea11C401 in result11C401)
                    {
                        lineaTemp1CFI = Logica1BFI(linea11C401);

                        if (!string.IsNullOrEmpty(lineaTemp1CFI))
                        {
                            resultadoLinea1CFI.Add(lineaTemp1CFI);
                        }
                    }
                }
            }

            //string SVAS 

            Lineas1CFI = resultadoLinea1CFI;

            return Lineas1CFI;

            #endregion
        }

        private string Logica1BFI(string lineaProcesar)
        {
            string lineaResultado = string.Empty;

            // Se valida que sea diferente de 0 pa continuar
            if (Convert.ToDouble(lineaProcesar.Substring(16, 14).Trim()) > 0)
            {
                string llave = string.Empty;
                string valor = string.Empty;

                // Llave con los 10 digitos 
                llave = lineaProcesar.Substring(6, 10);

                // Buscar en el insumo Codigos_Univer_SASV y Cuentas Svas FueraBundle si se encuentra se omite este registro
                if (Variables.Variables.DatosInsumoCodigosUniverSvas.ContainsKey(llave) && Variables.Variables.DatosInsumoCuentasSvasFueraBundle.ContainsKey(Cuenta))
                {
                    return string.Empty;
                }                

                // Buscar en el insumo ExcluirServiciosAdicionales
                if (Variables.Variables.DatosInsumoExcluirServiciosAdicionales.ContainsKey(llave.TrimStart('0')))
                {
                    return string.Empty;
                }

                #region Busqueda concepto en tabla Sustitución
                llave = $"CODF{lineaProcesar.Substring(6, 10)}";

                if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llave))
                {
                    valor = Variables.Variables.DatosInsumoTablaSustitucion[llave].FirstOrDefault().Substring(14).Trim();
                }
                #endregion

                // validar lel concepto trae la palabra SVA se omite
                if (valor.Contains("SVA"))
                {
                    return string.Empty;
                }

                if (Convert.ToDouble(lineaProcesar.Substring(16, 14)) != 0 ||
                        Convert.ToDouble(lineaProcesar.Substring(44, 14)) != 0 ||
                        (Convert.ToDouble(lineaProcesar.Substring(16, 14)) + Convert.ToDouble(lineaProcesar.Substring(44, 14)) != 0))
                {
                    
                    lineaResultado = "1BFI| |";

                    if (lineaProcesar.Substring(128, 19).Contains('-'))
                    {
                        lineaResultado += valor + " ( " + Helpers.FormatearCampos(TiposFormateo.Fecha04, lineaProcesar.Substring(132, 4)) + " - " + Helpers.FormatearCampos(TiposFormateo.Fecha04, lineaProcesar.Substring(143, 4)) + " ) |";
                    }
                    else
                    {
                        lineaResultado += valor + "|";
                    }
                    
                    lineaResultado += " | | | | ";
                    
                    return lineaResultado;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Linea que obtiene canal ADNC
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanalADNC(List<string> datosOriginales)
        {
            #region Canal ADNC
            string resultado = string.Empty;

            var linea40000 = from busqueda in datosOriginales
                             where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                             select busqueda;

            if (linea40000.Any())
            {
                if (IsLte || IsLteCorporativo && GetTipo(linea40000.FirstOrDefault().Substring(6, 20).Trim()) != "Cuenta")
                {
                    resultado = $"ADNC|{linea40000.FirstOrDefault().Substring(26, 49).Trim()}";
                }
                else if (IsDatos || IsGobierno)
                {
                    resultado = $"ADNC|{linea40000.FirstOrDefault().Substring(26, 49).Trim()}";
                }
            }

            return resultado;
            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal 1CFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanal1CFI(List<string> datosOriginales)
        {
            #region MapeoCanal1CFI
            IEnumerable<string> Lineas1CFI = null;
            string lineaTemp1CFI = string.Empty;

            List<string> resultadoLinea1CFI = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();


            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && (busqueda.Substring(0, 6).Equals("11C101") || busqueda.Substring(0, 6).Equals("11C304") || busqueda.Substring(0, 6).Equals("11C401"))
                         let x = busqueda
                         group x by busqueda.Substring(6, 10) into busqueda
                         select busqueda;

            if (result != null)
            {
                foreach (var resultadoActual in result.SelectMany(x => x))
                {
                    if (Convert.ToInt32(resultadoActual.Substring(16, 14).Trim()) > 0)
                    {
                        string llave = $"CODF{resultadoActual.Substring(6, 10)}";
                        string valor = string.Empty;

                        if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llave))
                        {
                            valor = Variables.Variables.DatosInsumoTablaSustitucion[llave].FirstOrDefault().Trim();
                        }


                        // Tabla Suti = CODF0163730650 SVA Access Point Wifi Plus

                        //    /// 1. Si esta en Codigos_Univer_SVAS pinto, y en elnombre de sustitusion SI este la palabra SVA/                        
                        //    /// 2. Validar en este archivo ExcluirServiciosAdicionales_JUL2022
                        //    // Se valida que es anexo fibra
                        //    if (IsAnexoFibra)
                        //    {

                        //    }

                    }
                }
            }

            #region Ajuste a la decena
            // Llenar el canal con el ajuste a la decena si lo tiene
            var resultAjusteDecena = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T003")
                                     select busqueda;

            // Validar que tenga Ajuste a la decena
            if (resultAjusteDecena.Any())
            {
                listaCortes.Clear();

                listaCortes.Add(new PosCortes(6, 14, TiposFormateo.Decimal01));

                lineaTemp1CFI = string.Empty;
                lineaTemp1CFI = "1CFI| |Ajuste Decena|";
                lineaTemp1CFI += Helpers.ExtraccionCamposSpool(listaCortes, resultAjusteDecena.FirstOrDefault());
                lineaTemp1CFI += "| |";
                lineaTemp1CFI += Helpers.ExtraccionCamposSpool(listaCortes, resultAjusteDecena.FirstOrDefault());
                lineaTemp1CFI += "| | ";

                resultadoLinea1CFI.Add(lineaTemp1CFI);
            }
            #endregion


            //string SVAS 

            Lineas1CFI = resultadoLinea1CFI;

            return Lineas1CFI;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal NTC2
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanalNTC2(List<string> datosOriginales)
        {
            #region Canal NTC2
            string resultado = string.Empty;

            var linea30000 = from busqueda in datosOriginales
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("30000")
                             select busqueda;

            if (linea30000.Any())
            {
                resultado = Helpers.ValidarPipePipe($"NTC2|{linea30000.FirstOrDefault().Substring(5, 20).Trim()}|{linea30000.FirstOrDefault().Substring(25, 20).Trim()}|" +
                    $"{linea30000.FirstOrDefault().Substring(45, 20).Trim()}|{linea30000.FirstOrDefault().Substring(65, 20).Trim()}|{linea30000.FirstOrDefault().Substring(85, 20).Trim()}| ");
            }

            return resultado;
            #endregion
        }


        /// Metodo que obtiene la linea formateada de Canal 1TFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1TFI(List<string> datosOriginales)
        {
            #region MapeoCanal1TFI
            string Linea1TFI = string.Empty;
            return Linea1TFI;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal NTC3
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanalNTC3(List<string> datosOriginales)
        {
            #region Canal NTC3
            string resultado = string.Empty;

            var linea30001 = from busqueda in datosOriginales
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("30001")
                             select busqueda;

            if (linea30001.Any())
            {
                resultado = Helpers.ValidarPipePipe($"NTC3|{linea30001.FirstOrDefault().Substring(5, 20).Trim()}|{linea30001.FirstOrDefault().Substring(25, 10).Trim()}| ");
            }

            return resultado;
            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1CDP
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1CDP(List<string> datosOriginales)
        {
            #region MapeoCanal1CDP
            string Linea1CDP = string.Empty;
            return Linea1CDP;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal NTC4
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanalNTC4(List<string> datosOriginales)
        {
            #region Canal NTC4
            string resultado = string.Empty;

            var linea30002 = from busqueda in datosOriginales
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("30002")
                             select busqueda;

            if (linea30002.Any())
            {
                resultado = Helpers.ValidarPipePipe($"NTC4|{linea30002.FirstOrDefault().Substring(5, 10).Trim()}|{linea30002.FirstOrDefault().Substring(15, 70).Trim()}|" +
                    $"{linea30002.FirstOrDefault().Substring(85, 20).Trim()}| ");
            }

            return resultado;
            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal CART
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanalCART(List<string> datosOriginales)
        {
            #region MapeoCanalCART
            IEnumerable<string> LineasCART = null;
            return LineasCART;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1OPL
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1OPL(List<string> datosOriginales)
        {
            #region Canal 1OPL
            string Linea1OPL = string.Empty;
            return Linea1OPL;
            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal 1OMV
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanal1OMV(List<string> datosOriginales)
        {
            #region MapeoCanal1OMV
            IEnumerable<string> Lineas1OMV = null;
            return Lineas1OMV;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1OOB
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanal1OOB(List<string> datosOriginales)
        {
            #region Canal 1OOB
            IEnumerable<string> Linea1OOB = null;
            return Linea1OOB;
            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal CONS
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanalCONS(List<string> datosOriginales)
        {
            #region MapeoCanalCONS
            IEnumerable<string> LineasCONS = null;
            return LineasCONS;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1PLA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1PLA(List<string> datosOriginales)
        {
            #region Canal 1PLA
            string Linea1PLA = string.Empty;
            return Linea1PLA;
            #endregion
        }
        /// Metodo que obtiene las lineas formateadas de Canal NTC5
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanalNTC5(List<string> datosOriginales)
        {
            #region MapeoCanalNTC5
            List<string> resultado = new List<string>();

            var linea30003 = from busqueda in datosOriginales
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("30003")
                             select busqueda;

            if (linea30003.Any())
            {
                foreach (var lineaDet in linea30003)
                {
                    resultado.Add(Helpers.ValidarPipePipe($"NTC5|{linea30003.FirstOrDefault().Substring(5, 2).Trim()}|{linea30003.FirstOrDefault().Substring(7, 5).Trim()}|" +
                                $"{linea30003.FirstOrDefault().Substring(12, 20).Trim()}| "));
                }
            }

            return resultado;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1EE1
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanal1EE1(List<string> datosOriginales)
        {
            #region Canal 1EE1
            IEnumerable<string> Linea1EE1 = null;
            return Linea1EE1;
            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1OOA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1OOA(List<string> datosOriginales)
        {
            #region MapeoCanal1OOA
            string Linea1OOA = string.Empty;
            return Linea1OOA;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1EE2
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public List<string> MapeoCanal1EE2(List<string> datosOriginales)
        {
            #region Canal 1EE2
            List<string> Linea1EE2 = new List<string>();
            return Linea1EE2;
            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1HIS
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1HIS(List<string> datosOriginales)
        {
            #region MapeoCanal1HIS
            string Linea1HIS = string.Empty;
            return Linea1HIS;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1EE3
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanal1EE3(List<string> datosOriginales)
        {
            #region Canal 1EE3
            IEnumerable<string> Linea1EE3 = null;
            return Linea1EE3;
            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1HDT
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1HDT(List<string> datosOriginales)
        {
            #region MapeoCanal1HDT
            string Linea1HDT = string.Empty;
            return Linea1HDT;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1FFA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1FFA(List<string> datosOriginales)
        {
            #region Canal 1FFA
            string Linea1FFA = string.Empty;
            return Linea1FFA;
            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal 1DET
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1DET(List<string> datosOriginales)
        {
            #region MapeoCanal1DET
            string Lineas1DET = string.Empty;
            return Lineas1DET;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal 1LLL
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1LLL(List<string> datosOriginales)
        {
            #region MapeoCanal1LLL
            string Lineas1LLL = string.Empty;
            return Lineas1LLL;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal 1CIC
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1CIC(List<string> datosOriginales)
        {
            #region MapeoCanal1CIC
            string Lineas1CIC = string.Empty;
            return Lineas1CIC;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal 1RRT
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1RRT(List<string> datosOriginales)
        {
            #region MapeoCanal1RRT
            string Lineas1RRT = string.Empty;
            return Lineas1RRT;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal 1RRR
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1RRR(List<string> datosOriginales)
        {
            #region MapeoCanal1RRR
            string Lineas1RRR = string.Empty;
            return Lineas1RRR;
            #endregion
        }

        private string FormateoCanal1CMP(List<string> datosOriginales)
        {
            #region FormateoCanal1CMP
            string resultado = string.Empty;

            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("060000")
                         select busqueda;

            if (result.Any())
            {
                var valorMinutosConsumo = result.FirstOrDefault().Substring(22, 8).TrimStart('0').Trim();

                if (IsResidencial || IsFibra)
                {
                    var resultCanal = from busqueda in datosOriginales
                                      where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                      select busqueda;

                    string minutosPlan = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"MINC{resultCanal.FirstOrDefault().Substring(133, 10).Trim()}").FirstOrDefault()?.Substring(14).Trim() ?? "";

                    resultado += $"1CMP|Minutos del Plan {minutosPlan}|Minutos Consumidos {result.FirstOrDefault().Substring(22, 8).TrimStart('0').Trim()}| ";
                }
            }

            return resultado;
            #endregion
        }

        private string FormateoCanal1PPP(List<string> datosOriginales)
        {
            #region FormateoCanal1PPP
            string resultado = string.Empty;

            var linea60000 = from busqueda in datosOriginales
                             where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("060000")
                             select busqueda;

            if (linea60000.Any())
            {
                if (IsResidencial || IsFibra)
                {
                    var linea10000 = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                                     select busqueda;

                    var linea40000 = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                     select busqueda;

                    string valor = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"VMIN{linea40000.FirstOrDefault().Substring(133, 10)}").FirstOrDefault()?.Trim() ?? "";

                    var valorPromedio = linea60000.FirstOrDefault().Substring(78, 8).TrimStart('0').Trim() == "" ? "0" : linea60000.FirstOrDefault().Substring(78, 8).TrimStart('0').Trim();

                    resultado += Helpers.ValidarPipePipe($"1PPP|Promedio Historico|{valorPromedio}| |" +
                        $"{ArmarMesesHistograma(Helpers.FormatearCampos(TiposFormateo.Fecha01, linea10000.FirstOrDefault().Substring(168, 8)))}| |" +
                        $"{ArmarValoresHistograma(linea60000.FirstOrDefault())}| |{GetValorMinutoPlan(linea40000.FirstOrDefault(), valor)}| ");
                }
            }

            return resultado;
            #endregion
        }

        private string ArmarMesesHistograma(string pFechaReferencia)
        {
            #region ArmarMesesHistograma
            string resultado = string.Empty;
            List<string> meses = new List<string>();
            CultureInfo culture = new CultureInfo("es-CO");

            if (!string.IsNullOrEmpty(pFechaReferencia))
            {
                byte mesFacturacion = Convert.ToByte(pFechaReferencia.Split('/').ElementAt(1));
                mesFacturacion--;

                for (int i = mesFacturacion; i <= mesFacturacion; i--)
                {
                    if (i == 0)
                        break;

                    meses.Add(Helpers.FormatearCampos(TiposFormateo.LetraCapital, new DateTime(DateTime.Now.Year, i, 1).ToString("MMM", culture).Replace(".", string.Empty)));
                }

                meses.Reverse();

                resultado = $"{meses.ElementAt(0)}|{meses.ElementAt(1)}|{meses.ElementAt(2)}|{meses.ElementAt(3)}|{meses.ElementAt(4)}|{meses.ElementAt(5)}";
            }
            else
            {
                //No deberia llegar aca 
                resultado = "";
            }

            return resultado;
            #endregion
        }

        private string ArmarValoresHistograma(string pLineaDatos)
        {
            #region ArmarValoresHistograma
            string resultado = string.Empty;

            string valor1 = pLineaDatos.Substring(30, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(30, 8).TrimStart('0');
            string valor2 = pLineaDatos.Substring(38, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(38, 8).TrimStart('0');
            string valor3 = pLineaDatos.Substring(46, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(46, 8).TrimStart('0');
            string valor4 = pLineaDatos.Substring(54, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(54, 8).TrimStart('0');
            string valor5 = pLineaDatos.Substring(62, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(62, 8).TrimStart('0');
            string valor6 = pLineaDatos.Substring(70, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(70, 8).TrimStart('0');

            resultado = $"{valor1}|{valor2}|{valor3}|{valor4}|{valor5}|{valor6}";

            return resultado;
            #endregion
        }

        private string GetValorMinutoPlan(string pLineaDatos, string pLineaTablaSustitucion)
        {
            #region GetValorMinutoPlan
            string resultado = " ";

            string estrato = pLineaDatos.Substring(123, 1);

            if (!string.IsNullOrEmpty(pLineaTablaSustitucion))
            {
                switch (estrato)
                {
                    #region Estrato
                    case "1":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal01, pLineaTablaSustitucion.Substring(14, 10));
                        break;

                    case "2":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal01, pLineaTablaSustitucion.Substring(24, 10));
                        break;

                    case "3":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal01, pLineaTablaSustitucion.Substring(34, 10));
                        break;

                    case "4":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal01, pLineaTablaSustitucion.Substring(44, 10));
                        break;

                    case "5":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal01, pLineaTablaSustitucion.Substring(54, 10));
                        break;

                    case "6":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal01, pLineaTablaSustitucion.Substring(64, 10));
                        break;

                    default:
                        //Por defecto va con estrato 6
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal01, pLineaTablaSustitucion.Substring(64, 10));
                        break;
                        #endregion
                }
            }

            return resultado;
            #endregion
        }

        private List<string> FormateoCanalADN1(List<string> datosOriginales)
        {
            #region FormateoCanalADN1
            List<string> resultado = new List<string>();

            var linea40000 = from busqueda in datosOriginales
                             where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                             select busqueda;

            if (IsLte || IsLteCorporativo && GetTipo(linea40000.FirstOrDefault().Substring(6, 20).Trim()) != "Cuenta")
            {
                resultado.Add(Helpers.ValidarPipePipe($"ADN1|{linea40000.FirstOrDefault().Substring(6, 20).Trim()}|{GetTipo(linea40000.FirstOrDefault().Substring(6, 20).Trim())}|" +
                    $"{Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea40000.FirstOrDefault().Substring(76, 39).Trim())}| | | "));
            }
            else if (IsDatos || IsGobierno)
            {
                resultado.Add(Helpers.ValidarPipePipe($"ADN1|{linea40000.FirstOrDefault().Substring(6, 20).Trim()}|{GetTipo(linea40000.FirstOrDefault().Substring(6, 20).Trim())}|" +
                    $"{Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea40000.FirstOrDefault().Substring(76, 39).Trim())}| | | "));
            }

            return resultado;
            #endregion
        }

        private string GetTipo(string pConexion)
        {
            #region GetTipo
            string resultado = string.Empty;

            if (Cuenta == pConexion)
            {
                resultado = "Cuenta";
            }
            else
            {
                resultado = "Conexion";
            }

            return resultado;
            #endregion
        }

        private string FormateoCanal1OMT(List<string> datosOriginales)
        {
            string resultado = string.Empty;



            return resultado;
        }

        private string FormateoCanalNTC0(List<string> datosOriginales)
        {
            #region FormateoCanalNTC0
            string resultado = string.Empty;

            var linea28000 = from busqueda in datosOriginales
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("28000")
                             select busqueda;

            if (linea28000.Any())
            {
                resultado = Helpers.ValidarPipePipe($"NTC0|{linea28000.FirstOrDefault().Substring(5, 12).Trim()}|{linea28000.FirstOrDefault().Substring(21, 15).Trim()}|{linea28000.FirstOrDefault().Substring(50).Trim()}| ");
            }

            return resultado;
            #endregion
        }

        private string FormateoCanalNTC1(List<string> datosOriginales)
        {
            #region FormateoCanalNTC1
            string resultado = string.Empty;

            var linea29000 = from busqueda in datosOriginales
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("29000")
                             select busqueda;

            if (linea29000.Any())
            {
                resultado = Helpers.ValidarPipePipe($"NTC1|{linea29000.FirstOrDefault().Substring(5, 12).Trim()}|{linea29000.FirstOrDefault().Substring(18, 10).Trim()}|" +
                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, linea29000.FirstOrDefault().Substring(29, 20).Trim())}|{linea29000.FirstOrDefault().Substring(49).Trim()}| ");
            }

            return resultado;
            #endregion
        }

        private string FormateoCanal1ODC(List<string> datosOriginales)
        {
            #region FormateoCanal1ODC
            string resultado = string.Empty;

            var linea888888 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("888888")
                              select busqueda;

            if (linea888888.Any())
            {
                var linea3000401 = from busqueda in datosOriginales
                                   where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000401")
                                   select busqueda;

                string iva = Helpers.FormatearCampos(TiposFormateo.Decimal01, "00");

                if (linea3000401.Any())
                {
                    iva = Helpers.FormatearCampos(TiposFormateo.Decimal01, linea3000401.FirstOrDefault().Substring(12, 20).Trim());
                }

                var linea3000404 = from busqueda in datosOriginales
                                   where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000404")
                                   select busqueda;

                string impuesto = Helpers.FormatearCampos(TiposFormateo.Decimal01, "00");

                if (linea3000404.Any())
                {
                    impuesto = Helpers.FormatearCampos(TiposFormateo.Decimal01, linea3000401.FirstOrDefault().Substring(12, 20).Trim());
                }

                resultado = Helpers.ValidarPipePipe($"1ODC|{linea888888.FirstOrDefault().Substring(6, 10)}|Total|{Helpers.FormatearCampos(TiposFormateo.Decimal01, linea888888.FirstOrDefault().Substring(82, 14))}|" +
                    $"{iva}|{impuesto}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, linea888888.FirstOrDefault().Substring(110, 14))}|{Helpers.FormatearCampos(TiposFormateo.Fecha04, PeriodoFacturacion)}| ");

            }

            return resultado;
            #endregion
        }

        private IEnumerable<string> FormateoCanal1ODD(List<string> datosOriginales)
        {
            #region FormateoCanal1ODD
            List<string> resultado = new List<string>();
            string numeroConexion = string.Empty;
            string periodo = string.Empty;
            string llaveCruceSustitucion = string.Empty;
            string @base = string.Empty;
            string iva = string.Empty;
            string impuesto = string.Empty;
            string CodigoCanalPrimario = string.Empty;
            string CodigoCanal = string.Empty;
            string conceptoPrimario = string.Empty;
            string concepto = string.Empty;
            bool recargoMoraGenerado = false;

            if (IsLte || IsLteCorporativo)
            {
                #region Busqueda Canales
                var linea06T942 = from busqueda in datosOriginales //Con este cubro resultados del 11C901
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("06T942")
                                  select busqueda;

                var linea06T112 = from busqueda in datosOriginales //Con este cubro resultados del 11C103
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("06T112")
                                  select busqueda;

                var linea040000 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                  select busqueda;

                //Identificar canales detalles

                var linea11C901 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("11C901") && !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                                  select busqueda;

                var linea11C103 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("11C103") && !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                                  select busqueda;

                #endregion

                #region Busqueda Periodo

                var linea11C901Periodo = from busqueda in datosOriginales
                                         where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("11C901")
                                         select busqueda;

                if (linea11C901Periodo.Any())
                {
                    //Buscar Periodo
                    var busquedaperiodo = from n in linea11C901Periodo
                                          let comp = n.Substring(128, 19).Trim()
                                          where !string.IsNullOrEmpty(comp)
                                          select n;

                    if (busquedaperiodo.Any())
                    {
                        periodo = busquedaperiodo.FirstOrDefault() != "" ? busquedaperiodo.FirstOrDefault().Substring(128, 19).Trim() : "";
                    }
                }

                if (string.IsNullOrEmpty(periodo) || !periodo.Contains("-"))
                {
                    FechaDesde = Helpers.FormatearCampos(TiposFormateo.Fecha01, FechaDesde);
                    FechaHasta = Helpers.FormatearCampos(TiposFormateo.Fecha01, FechaHasta);

                    periodo = $"{FechaDesde.Split('/').ElementAt(2)}{FechaDesde.Split('/').ElementAt(1)}{FechaDesde.Split('/').ElementAt(0)} - " +
                        $"{FechaHasta.Split('/').ElementAt(2)}{FechaHasta.Split('/').ElementAt(1)}{FechaHasta.Split('/').ElementAt(0)}";
                }
                #endregion

                if (linea06T942.Any())
                {
                    #region Busqueda Numero Conexion
                    foreach (var lineaDatos in linea040000)
                    {
                        if (lineaDatos.Substring(6, 20).Trim() != Cuenta)
                        {
                            numeroConexion = lineaDatos.Substring(6, 20).Trim();
                        }
                    }
                    #endregion

                    #region Logica Canal 06T942
                    foreach (var linea in linea06T942)
                    {
                        if (!recargoMoraGenerado)
                        {
                            recargoMoraGenerado = false;
                        }

                        if (string.IsNullOrEmpty(linea.Substring(6, 14).Trim().TrimStart('0')))
                        {
                            //Si Viene 0 no debo buscar concepto primario Armar recargo mora
                            if (!string.IsNullOrEmpty(linea.Substring(20, 14).Trim().TrimStart('0')))
                            {
                                if (!recargoMoraGenerado)
                                {
                                    resultado.Add(GetRecargoMora(linea, periodo, numeroConexion));
                                    recargoMoraGenerado = true;
                                }
                            }
                        }
                        else
                        {
                            //Buscar concepto primario

                            var linea040011 = from busqueda in datosOriginales.Take(datosOriginales.FindIndex(x => x == linea) + 1)
                                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040011")
                                              select busqueda;

                            if (linea040011.Any())
                            {
                                CodigoCanalPrimario = linea040011.FirstOrDefault().Substring(167, 10).Trim();

                                //Buscar los del codigo para ver si no estan en 0 
                                var validarConceptoConDatos = from busqueda in datosOriginales
                                                              where busqueda.Length > 270
                                                              let comp = busqueda.Substring(271, 10).Trim()
                                                              where comp == CodigoCanalPrimario && !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                                                              select busqueda;

                                if (validarConceptoConDatos.Any())
                                {
                                    conceptoPrimario = Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea040011.FirstOrDefault().Substring(6, 35).Trim());

                                    @base = linea.Substring(6, 14).Trim().TrimStart('0');
                                    iva = linea.Substring(34, 14).Trim().TrimStart('0');
                                    impuesto = linea.Substring(118, 14).Trim().TrimStart('0');

                                    resultado.Add(Helpers.ValidarPipePipe($"1ODD|{Helpers.FormatearCampos(TiposFormateo.Fecha06, periodo.Split('-').ElementAt(0).Trim())} a {Helpers.FormatearCampos(TiposFormateo.Fecha06, periodo.Split('-').ElementAt(1).Trim())}|" +
                                        $"{conceptoPrimario} ({Helpers.FormatearCampos(TiposFormateo.Fecha07, periodo.Split('-').ElementAt(0).Trim())} - {Helpers.FormatearCampos(TiposFormateo.Fecha07, periodo.Split('-').ElementAt(1).Trim())})|" +
                                        $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, iva)}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, impuesto)}|" +
                                        $"{Helpers.SumarCampos(new List<string> { @base, iva, impuesto })}|" +
                                        $"{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|" +
                                        $"{numeroConexion}| "));
                                }

                            }

                            if (!recargoMoraGenerado)
                            {
                                if (!string.IsNullOrEmpty(linea.Substring(20, 14).Trim().TrimStart('0')))
                                {
                                    resultado.Add(GetRecargoMora(linea, periodo, numeroConexion));
                                    recargoMoraGenerado = true;
                                }
                            }
                        }
                    }
                    #endregion

                    #region Recorro canal del detalle para buscar mas conceptos

                    if (linea11C901.Any())
                    {
                        linea11C901 = from busqueda in linea11C901
                                      where busqueda.Substring(271, 10).Trim() != CodigoCanalPrimario
                                      select busqueda;

                        if (linea11C901.Any())
                        {
                            string codigoanterior = CodigoCanalPrimario;

                            foreach (var linea in linea11C901)
                            {
                                if (codigoanterior != /*Codigo Actual*/linea.Substring(271, 10).Trim())
                                {
                                    codigoanterior = linea.Substring(271, 10).Trim();

                                    CodigoCanal = linea.Substring(271, 10).Trim();

                                    concepto = Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea.Substring(281, 35).Trim());

                                    if (string.IsNullOrEmpty(concepto))
                                    {
                                        llaveCruceSustitucion = $"CODF{linea.Substring(6, 10).Trim()}";

                                        concepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruceSustitucion).FirstOrDefault()?.Substring(14).Trim() ?? "";
                                    }

                                    @base = linea.Substring(16, 14).Trim().TrimStart('0');
                                    iva = linea.Substring(44, 14).Trim().TrimStart('0');
                                    impuesto = linea.Substring(172, 14).Trim().TrimStart('0');

                                    resultado.Add(Helpers.ValidarPipePipe($"1ODD|{Helpers.FormatearCampos(TiposFormateo.Fecha06, periodo.Split('-').ElementAt(0).Trim())} a {Helpers.FormatearCampos(TiposFormateo.Fecha06, periodo.Split('-').ElementAt(1).Trim())}|" +
                                        $"{concepto} ({Helpers.FormatearCampos(TiposFormateo.Fecha07, periodo.Split('-').ElementAt(0).Trim())} - {Helpers.FormatearCampos(TiposFormateo.Fecha07, periodo.Split('-').ElementAt(1).Trim())})|" +
                                        $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, iva)}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, impuesto)}|" +
                                        $"{Helpers.SumarCampos(new List<string> { @base, iva, impuesto })}|" +
                                        $"{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|" +
                                        $"{numeroConexion}| "));
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region Logica Canal linea06T112
                if (linea06T112.Any())
                {
                    //Buscar concepto en tabla sustitucion
                    concepto = string.Empty;

                    linea11C901 = from busqueda in datosOriginales
                                  where busqueda.Length > 270
                                  let comp = busqueda.Substring(271, 10).Trim()
                                  where comp != CodigoCanalPrimario
                                  select busqueda;

                    if (linea11C103.Any())
                    {
                        llaveCruceSustitucion = $"CODF{linea11C103.FirstOrDefault().Substring(6, 10).Trim()}";
                    }
                    else
                    {
                        llaveCruceSustitucion = $"CODF{linea11C901.FirstOrDefault().Substring(6, 10).Trim()}";
                    }

                    concepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruceSustitucion).FirstOrDefault()?.Substring(14).Trim() ?? "";

                    @base = linea06T112.FirstOrDefault().Substring(6, 14).Trim().TrimStart('0');
                    iva = linea06T112.FirstOrDefault().Substring(34, 14).Trim().TrimStart('0');
                    impuesto = linea06T112.FirstOrDefault().Substring(118, 14).Trim().TrimStart('0');

                    resultado.Add($"1ODD|{Helpers.FormatearCampos(TiposFormateo.Fecha06, periodo.Split('-').ElementAt(0).Trim())} a {Helpers.FormatearCampos(TiposFormateo.Fecha06, periodo.Split('-').ElementAt(1).Trim())}|" +
                        $"{concepto} ({Helpers.FormatearCampos(TiposFormateo.Fecha07, periodo.Split('-').ElementAt(0).Trim())} - {Helpers.FormatearCampos(TiposFormateo.Fecha07, periodo.Split('-').ElementAt(1).Trim())})|" +
                        $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, iva)}|" +
                        $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, impuesto)}|{Helpers.SumarCampos(new List<string> { @base, iva, impuesto })}|" +
                        $"{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|" +
                        $"{numeroConexion}| ");

                }

                #endregion
            }

            return resultado; 
            #endregion
        }

        private string GetRecargoMora(string pLinea, string pPeriodo, string pNumeroConexion)
        {
            #region GetRecargoMora
            string resultado = string.Empty;

            string @base = pLinea.Substring(20, 14).Trim().TrimStart('0');
            string iva = pLinea.Substring(34, 14).Trim().TrimStart('0');

            resultado = $"1ODD|{Helpers.FormatearCampos(TiposFormateo.Fecha06, pPeriodo.Split('-').ElementAt(0).Trim())} a {Helpers.FormatearCampos(TiposFormateo.Fecha06, pPeriodo.Split('-').ElementAt(1).Trim())}|" +
                $"Recargo de Mora ({Helpers.FormatearCampos(TiposFormateo.Fecha07, pPeriodo.Split('-').ElementAt(0).Trim())} - {Helpers.FormatearCampos(TiposFormateo.Fecha07, pPeriodo.Split('-').ElementAt(1).Trim())})|" +
                $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, iva)}|$ 0,00|{Helpers.SumarCampos(new List<string> { @base, iva })}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|" +
                $"{pNumeroConexion}| ";

            return resultado;
            #endregion
        }


        private IEnumerable<string> FormateoCanal1OOO(List<string> datosOriginales)
        {
            IEnumerable<string> resultado = null;



            return resultado;
        }

        private IEnumerable<string> FormateoCanal1FFF(List<string> datosOriginales)
        {
            IEnumerable<string> resultado = null;



            return resultado;
        }
    }
}
