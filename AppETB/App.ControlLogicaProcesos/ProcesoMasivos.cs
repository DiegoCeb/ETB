using App.ControlInsumos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ControlLogicaProcesos
{
    public class ProcesoMasivos : Variables.Variables, IProcess
    {
        private bool IsResidencial { get; set; }
        private bool IsGobierno { get; set; }
        private bool IsFibra { get; set; }
        private bool IsDatos { get; set; }
        private bool IsLte { get; set; }
        private bool IsLteCorporativo { get; set; }

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
            Lector = new System.IO.StreamReader(pArchivo, Encoding.Default);
            List<string> datosExtractoFormateo = new List<string>();
            string linea = string.Empty;
            bool extractoCompleto = false;
            string llaveCruce = string.Empty;

            while ((linea = Lector.ReadLine()) != null)
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
                        llaveCruce = datosExtractoFormateo.ElementAt(1).Substring(117, 22).Trim();

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
                llaveCruce = datosExtractoFormateo.ElementAt(1).Substring(117, 22).Trim();

                AgregarDiccionario(llaveCruce, FormatearArchivo(llaveCruce, datosExtractoFormateo));

                datosExtractoFormateo.Clear();
            }

            Lector.Close();
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
            if (DiccionarioExtractosFormateados.ContainsKey(pLlaveCruce))
            {
                DiccionarioExtractosFormateados[pLlaveCruce].AddRange(pTemp);
            }
            else
            {
                DiccionarioExtractosFormateados.Add(pLlaveCruce, new List<string>(pTemp));
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

            FormatearPropiedadesExtracto();

            resultado.Add(FormateoCanal1PPP(datosOriginales));

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
        public string MapeoCanal1BBA(List<string> datosOriginales)
        {
            #region Canal 1BBA
            string Linea1BBA = string.Empty;
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
                ListaCanal1AAA.Add(Helpers.GetTablaSutitucion($"DANC{Linea010000.Substring(108, 5).Trim()}", "15").Resultados.FirstOrDefault());
                ListaCanal1AAA.Add(Helpers.GetTablaSutitucion($"DAND{Linea010000.Substring(108, 5).Trim()}", "4").Resultados.FirstOrDefault());
                listaCortes.Add(new PosCortes(117, 20));
                listaCortes.Add(new PosCortes(151, 4));
                listaCortes.Add(new PosCortes(155, 13)); //TODO: Formatear Decimal
                listaCortes.Add(new PosCortes(168, 8, TiposFormateo.Fecha01)); //TODO: Formatear Fecha
                listaCortes.Add(new PosCortes(176, 2)); //TODO: Construir regla
                listaCortes.Add(new PosCortes(178, 8, TiposFormateo.Fecha01)); //TODO: Formatear Fecha
                listaCortes.Add(new PosCortes(186, 8, TiposFormateo.Fecha01)); //TODO: Formatear Fecha
                listaCortes.Add(new PosCortes(176, 2)); //TODO: Construir regla


                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));

            }

            return Linea1AAA;
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
        public List<string> MapeoCanal1MMM(List<string> datosOriginales)
        {
            #region Canal 1MMM
            List<string> Linea1MMM = new List<string>();
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
            IEnumerable<string> Lineas1BFI = new List<string>();
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
            IEnumerable<string> Lineas1CFI = new List<string>();
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
            IEnumerable<string> LineasCART = new List<string>();
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
            IEnumerable<string> Lineas1OMV = new List<string>();
            return Lineas1OMV;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1OOB
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public List<string> MapeoCanal1OOB(List<string> datosOriginales)
        {
            #region Canal 1OOB
            List<string> Linea1OOB = new List<string>();
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
            IEnumerable<string> LineasCONS = new List<string>();
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
            IEnumerable<string> LineasNTC5 = new List<string>();
            return LineasNTC5;

            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1EE1
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public List<string> MapeoCanal1EE1(List<string> datosOriginales)
        {
            #region Canal 1EE1
            List<string> Linea1EE1 = new List<string>();
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
        public List<string> MapeoCanal1EE3(List<string> datosOriginales)
        {
            #region Canal 1EE3
            List<string> Linea1EE3 = new List<string>();
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

                if (IsResidencial || IsFibra && !string.IsNullOrEmpty(valorMinutosConsumo))
                {
                    var resultCanal = from busqueda in datosOriginales
                                      where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                      select busqueda;

                    resultado += $"1CMP|Minutos del Plan {Helpers.GetTablaSutitucion($"MINC{resultCanal.FirstOrDefault().Substring(133, 10).TrimStart('0').Trim()}", "39").Resultados.FirstOrDefault()}|Minutos Consumidos {result.FirstOrDefault().Substring(22, 8).TrimStart('0').Trim()}| ";
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
                var valorPromedio = linea60000.FirstOrDefault().Substring(78, 8).TrimStart('0').Trim();

                if (/*IsResidencial || IsFibra &&*/ !string.IsNullOrEmpty(valorPromedio))
                {
                    var linea10000 = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                                     select busqueda;

                    resultado += $"1PPP|Promedio Historico|{linea60000.FirstOrDefault().Substring(78, 8).TrimStart('0').Trim()}| |" +
                        $"{ArmarMesesHistograma(Helpers.FormatearCampos(TiposFormateo.Fecha01, linea10000.FirstOrDefault().Substring(168, 8)))}| |" +
                        $"{ArmarValoresHistograma(linea60000.FirstOrDefault())}|FALTA (Descuento)|FALTA (Valor Minuto)| ";
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

            if (IsResidencial || IsFibra)
            {
                resultado = $" |{pLineaDatos.Substring(39, 8).TrimStart('0')}|{pLineaDatos.Substring(47, 8).TrimStart('0')}|{pLineaDatos.Substring(55, 8).TrimStart('0')}|" +
                    $"{pLineaDatos.Substring(63, 8).TrimStart('0')}|{pLineaDatos.Substring(71, 8).TrimStart('0')}";
            }
            else if (IsLte || IsLteCorporativo)
            {
                resultado = $"{pLineaDatos.Substring(31, 8).TrimStart('0')}|{pLineaDatos.Substring(39, 8).TrimStart('0')}|{pLineaDatos.Substring(47, 8).TrimStart('0')}|" +
                    $"{pLineaDatos.Substring(55, 8).TrimStart('0')}|{pLineaDatos.Substring(63, 8).TrimStart('0')}|{pLineaDatos.Substring(71, 8).TrimStart('0')}";
            }
            else
            {
                //No deberia llegar aca
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
