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

        public ProcesoMasivos(string pArchivo)
        {
            #region ProcesoMasivos
            try
            {
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

                        AgregarDiccionario(llaveCruce, FormatearArchivo(llaveCruce, datosExtractoFormateo));

                        datosExtractoFormateo.Clear();
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

                AgregarDiccionario(llaveCruce, FormatearArchivo(llaveCruce, datosExtractoFormateo));

                datosExtractoFormateo.Clear();
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

            if (pLLaveCruce == "2484047")
            {

            }

            FormatearPropiedadesExtracto();

            string canal1CMP = FormateoCanal1CMP(datosOriginales);

            if (!string.IsNullOrEmpty(canal1CMP))
            {
                resultado.Add(canal1CMP);
            }

            string canal1PPP = FormateoCanal1PPP(datosOriginales);

            if (!string.IsNullOrEmpty(canal1PPP))
            {
                resultado.Add(canal1PPP);
            }

            //resultado.Add(MapeoCanal1AAA(datosOriginales));
            //resultado.Add(FormateoCanal1CMP(datosOriginales));
            //resultado.Add(FormateoCanal1PPP(datosOriginales));
            //resultado.AddRange(FormateoCanalADN1(datosOriginales));
            //resultado.Add(FormateoCanal1OMT(datosOriginales));
            //resultado.Add(FormateoCanalNTC0(datosOriginales));
            //resultado.Add(FormateoCanalNTC1(datosOriginales));
            //resultado.Add(FormateoCanal1ODC(datosOriginales));
            //resultado.AddRange(FormateoCanal1ODD(datosOriginales));
            //resultado.AddRange(FormateoCanal1OOO(datosOriginales));
            //resultado.AddRange(FormateoCanal1FFF(datosOriginales));


            List<string> linea1BBB = new List<string> { "1BBB|Valor factura anterior|$ 29.900,00 | ",
                                                        "1BBB|Gracias por su pago|-$ 29.900,00 | | ",
                                                        "1BBB|Servicios etb con IVA|$ 32.500,19 | ",
                                                        "1BBB|Ajuste a la decena|-$ 0,19 | "};

            resultado.Add(MapeoCanal1BBA(datosOriginales,linea1BBB.ToList()));
            //resultado.AddRange(MapeoCanal1CCC(datosOriginales));

            string Linea1AAA = "1AAA|176411_112369|PATRICIA HELENA FUENTES LOZANO|30668970|KR 18P BIS 67C 16 SUR|Bogotá|Cundinamarca|12054338377|000305487709|70|$ 144.700,00|17/07/2022|1|01/06/2022|30/06/2022|6017631210| |04/08/2022|08/08/2022|22072022 013000|1-000305487709-4|(415)7707181500017(8020)10003054877094(3900)0000144700(96)20220808|(415)7707181500017(8020)10003054877094(3900)0000034566(96)20220808|PAR|0.48|11001| |Residencial|3| | | | | |jesusbarros12102013@gmail.com|solo_email| | |FACTURA_SOLO_EMAIL|1\\3  |11001| | |I| | | | | | |https://tracking.carvajalcomunicacion.com/wdelta/w/m/aa/?ack=H1MTIwNTQzMzgzNzc=H1UEFUUklDSUEgSEVMRU5BIEZVRU5URVMgTE9aQU5P| | |20220701 - 20220714|**periodo_lte**|000000187640249555|22072022 013000|170720|22 00000000000000000| 0Hogares y mipymes|Plata|Hogares|Hogares| BA | ";
            resultado.Add(MapeoCanal1CCM(datosOriginales, Linea1AAA));

            resultado.AddRange(MapeoCanal1CFI(datosOriginales));

            //resultado.Add(MapeoCanal1BBA(datosOriginales));
            //resultado.AddRange(MapeoCanal1CCC(datosOriginales));

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
            
            if (result != null)            
            {
                // Cortes                
                listaCortes.Add(new PosCortes(154, 14, TiposFormateo.Decimal01));

                // Linea Retornar
                Linea1BBA = "1BBA";

                // Se vaida que el Subtotal del 1BBB se
                if(subtotal1BBB > 0)
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

            List<string> ListaCanal1AAA = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();


            ListaCanal1AAA.Add("1AAA");
            ListaCanal1AAA.Add("KitXXXX");

            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                         select busqueda;

            if (result != null)
            {
                Linea010000 = result.FirstOrDefault();

                listaCortes.Add(new PosCortes(6, 50));
                listaCortes.Add(new PosCortes(56, 12));
                listaCortes.Add(new PosCortes(68, 40));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));

                //ListaCanal1AAA.Add(Helpers.GetTablaSutitucion($"DAND{Linea010000.Substring(108, 2).Trim()}", "4").Resultados.FirstOrDefault().Substring(6).Trim());
                //ListaCanal1AAA.Add(Helpers.GetTablaSutitucion($"DANC{Linea010000.Substring(108, 5).Trim()}", "15").Resultados.FirstOrDefault().Substring(9).Trim());

                listaCortes.Clear();
                listaCortes.Add(new PosCortes(117, 20));
                listaCortes.Add(new PosCortes(151, 4));
                listaCortes.Add(new PosCortes(155, 13, TiposFormateo.Decimal01));
                listaCortes.Add(new PosCortes(168, 8, TiposFormateo.Fecha01));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));
                ListaCanal1AAA.Add(GetMesMora(Linea010000.Substring(176, 2).Trim()));

                listaCortes.Clear();
                listaCortes.Add(new PosCortes(178, 8, TiposFormateo.Fecha01));
                listaCortes.Add(new PosCortes(186, 8, TiposFormateo.Fecha01));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));

                ListaCanal1AAA.Add(GetTelefono(datosOriginales)); //TODO: Verificar Reglas
                ListaCanal1AAA.Add(IsFibra ? (string.IsNullOrEmpty(Linea010000.Substring(218, 20).Trim()) ? " " : Linea010000.Substring(218, 20).Trim()) : " ");
                ListaCanal1AAA.Add(Helpers.GetTablaSutitucion($"FECP{Helpers.FormatearCampos(TiposFormateo.Fecha02,Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}", "26").Resultados.FirstOrDefault().Substring(12).Trim());
                ListaCanal1AAA.Add(Helpers.GetTablaSutitucion($"FECL{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}", "27").Resultados.FirstOrDefault().Substring(12).Trim());
                ListaCanal1AAA.Add(Helpers.GetTablaSutitucion($"FECX{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}", "28").Resultados.FirstOrDefault().Substring(12).Trim()); // TODO: Validar
                ListaCanal1AAA.Add(GetNumeroReferencia(Linea010000.Substring(139, 12)));
                ListaCanal1AAA.AddRange(GetCodigosBarras(Linea010000.Substring(139, 12), Linea010000)); //TODO: Verificar valor a pagar
                ListaCanal1AAA.Add(GetTipoEtapas(Linea010000.Substring(151, 3))); 
                listaCortes.Clear();
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));

            }

            return Linea1AAA;
            #endregion
        }

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
        private string GetNumeroReferencia(string pNumReferencia)
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

            return $"{numFijoRef}-{pNumReferencia}-{digitoVerificacion.ToString()}";

            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene Los Codigos de Barras
        /// </summary>
        /// <param name="pNumReferencia"></param>
        /// <returns></returns>
        private List<string> GetCodigosBarras(string pNumReferencia, string pLinea010000)
        {
            #region GetCodigosBarras
            List<string> result = new List<string>();
            string numeroETB = Utilidades.LeerAppConfig("numeroETB");
            string numFijoRef = Utilidades.LeerAppConfig("numeroReferencia");
            string totalPagar = pLinea010000.Substring(155, 11).PadLeft(10, '0');
            string fechaPago = Helpers.GetTablaSutitucion($"FECL{Helpers.FormatearCampos(TiposFormateo.Fecha03, pLinea010000.Substring(168, 8).Trim())}{pLinea010000.Substring(151, 3).Trim().TrimStart('0')}", "27").Resultados.FirstOrDefault().Substring(12).Trim();

            string CodeBar1 = $"(415){numeroETB}(8020){numFijoRef}{pNumReferencia}(3900){totalPagar}(96){fechaPago}";
            //"(415)$numero_etb(8020){$valores_temp["Total1BBB"]["numero_referencia"]}(3900)0000000000(96)" . formatear_fecha($valores_temp["fecha_pago_extemporaneo"], 4);
            result.Add(CodeBar1);

            string CodeBar2 = $"(415){numeroETB}(8020){numFijoRef}{pNumReferencia}(3900){totalPagar}(96){fechaPago}";
            //"(415)$numero_etb(8020){$valores_temp["Total1BBB"]["numero_referencia"]}(3900)0000000000(96)" . formatear_fecha($valores_temp["fecha_pago_extemporaneo"], 4);
            result.Add(CodeBar2);
            return result;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene Los Codigos de Barras
        /// </summary>
        /// <param name="pNumReferencia"></param>
        /// <returns></returns>
        private string GetTipoEtapas(string pCiclo)
        {
            #region GetTipoEtapas
            string tipociclo = string.Empty;
            
            Int16 ciclo = Convert.ToInt16(pCiclo);

            switch (ciclo)
            {
                case 15: case 16: 
                case 87:
                    tipociclo = "FIB";
                    IsFibra = true;
                    break;

                case 17: 
                case 31: case 32: case 33: case 34: case 35: case 36: case 38: case 39:
                case 40: case 44: case 45: case 46: case 47: case 48: case 49: 
                case 50: case 51: case 52: case 53: case 54: case 55: case 56: case 57: case 58: case 59:
                case 60: case 61: case 62: case 63: case 68:
                case 70: case 71:
                    tipociclo = "PAR";
                    IsResidencial = true;
                    break;

                case 18: case 19:
                case 37:
                case 64: case 67: case 69:
                    tipociclo = "GOB";
                    IsGobierno = true;
                    break;

                case 66:
                case 79:
                case 80: case 81: case 82: case 84: case 85: case 86: case 88:
                    tipociclo = "DAT";
                    IsDatos = true;
                    break;

                case 3:  case 4:  case 5:  case 6:  case 7:  case 8: 
                case 90: case 91: case 92: case 93: case 94: case 95:
                    tipociclo = "LTE";
                    IsLte = true;
                    break;

                case 96: case 97:
                    tipociclo = "LTE_CORP";
                    IsLteCorporativo = true;
                    break;
            }

            return tipociclo;
            #endregion
        }

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
        public string MapeoCanal1CCM(List<string> datosOriginales, string linea1AAA)
        {
            #region Canal 1CCM
            string Linea1CCM = string.Empty;
            string cargosCCM = string.Empty;

            if (linea1AAA.Split('|')[9] == "70")
            {
                cargosCCM = "Cargos: " + linea1AAA.Split('|')[19];
            }

            if(!IsLte && !IsLteCorporativo)
            {
                Linea1CCM = "1CCM| |" + cargosCCM +" | ";
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

        /// Metodo que obtiene la linea formateada de Canal 1AFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1AFI(List<string> datosOriginales)
        {
            #region MapeoCanal1AFI
            string Linea1AFI = string.Empty;
            return Linea1AFI;

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

        /// Metodo que obtiene las lineas formateadas de Canal 1BFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanal1BFI(List<string> datosOriginales)
        {
            #region MapeoCanal1BFI
            IEnumerable<string> Lineas1BFI = null;
            return Lineas1BFI;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal ADNC
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanalADNC(List<string> datosOriginales)
        {
            #region Canal ADNC
            string LineaANDC = string.Empty;
            return LineaANDC;
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
                         where busqueda.Length > 6 && (busqueda.Substring(0, 6).Equals("11C10101") || busqueda.Substring(0, 6).Equals("11C304") || busqueda.Substring(0, 6).Equals("11C401"))
                         select busqueda;

            if(result != null)
            {
                // Se valida que es anexo fibra
                if(IsAnexoFibra)
                {

                }

                foreach (var resultadoActual in result)
                {
                    if(Convert.ToInt32(resultadoActual.Substring(16,14).Trim()) > 0)
                    {

                    }
                }
            }


            #region Ajuste a la decena
            // Llenar el canal con el ajuste a la decena si lo tiene
            var resultAjusteDecena = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T003")
                                     select busqueda;

            // Validar que tenga Ajuste a la decena
            if (resultAjusteDecena != null)
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
            string LineaNTC2 = string.Empty;
            return LineaNTC2;
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
            string LineaNTC3 = string.Empty;
            return LineaNTC3;
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
            string LineaNTC4 = string.Empty;
            return LineaNTC4;
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
            IEnumerable<string> LineasNTC5 = null;
            return LineasNTC5;

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

            if (result != null)
            {
                var valorMinutosConsumo = result.FirstOrDefault().Substring(22, 8).TrimStart('0').Trim();

                if (/*IsResidencial || IsFibra &&*/ true)
                {
                    var resultCanal = from busqueda in datosOriginales
                                      where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                      select busqueda;

                    string minutosPlan = string.Empty;
                    string llave = $"MINC{resultCanal.FirstOrDefault().Substring(133, 10).Trim()}";

                    if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llave))
                    {
                        minutosPlan = Variables.Variables.DatosInsumoTablaSustitucion[llave].FirstOrDefault().Substring(14).Trim();
                    }

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

            if (linea60000 != null)
            {
                if (/*IsResidencial || IsFibra &&*/ true)
                {
                    var linea10000 = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                                     select busqueda;

                    var linea40000 = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                     select busqueda;

                    string valor = string.Empty;
                    string llave = $"VMIN{linea40000.FirstOrDefault().Substring(133, 10)}";

                    if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llave))
                    {
                        valor = Variables.Variables.DatosInsumoTablaSustitucion[llave].FirstOrDefault().Trim();
                    }

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
                    meses.Add(Helpers.FormatearCampos(TiposFormateo.LetraCapital, new DateTime(DateTime.Now.Year, i, DateTime.Now.Day).ToString("MMM", culture).Replace(".", string.Empty)));
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

        private IEnumerable<string> FormateoCanalADN1(List<string> datosOriginales)
        {
            IEnumerable<string> resultado = null;



            return resultado;
        }

        private string FormateoCanal1OMT(List<string> datosOriginales)
        {
            string resultado = string.Empty;



            return resultado;
        }

        private string FormateoCanalNTC0(List<string> datosOriginales)
        {
            string resultado = string.Empty;



            return resultado;
        }

        private string FormateoCanalNTC1(List<string> datosOriginales)
        {
            string resultado = string.Empty;



            return resultado;
        }

        private string FormateoCanal1ODC(List<string> datosOriginales)
        {
            string resultado = string.Empty;



            return resultado;
        }

        private IEnumerable<string> FormateoCanal1ODD(List<string> datosOriginales)
        {
            IEnumerable<string> resultado = null;



            return resultado;
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
