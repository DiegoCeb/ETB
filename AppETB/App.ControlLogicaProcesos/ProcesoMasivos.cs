﻿using App.ControlInsumos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ControlLogicaProcesos
{
    public class ProcesoMasivos : Variables.Variables, IProcess
    {

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

        /// <summary>
        /// Metodo que obtiene la linea formateada de Canal 1AAA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1AAA(List<string> datosOriginales)
        {
            #region MapeoCanal1AAA
            string Linea1AAA = string.Empty;
            return Linea1AAA; 
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal 1BBB
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanal1BBB(List<string> datosOriginales)
        {
            #region MapeoCanal1BBB
            IEnumerable<string> Lineas1BBB = new List<string>();
            return Lineas1BBB; 
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la linea formateada de Canal 1CCA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1CCA(List<string> datosOriginales)
        {
            #region MapeoCanal1CCA
            string Linea1CCA = string.Empty;
            return Linea1CCA;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la linea formateada de Canal 1CPA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1CPA(List<string> datosOriginales)
        {
            #region MapeoCanal1CPA
            string Linea1CPA = string.Empty;
            return Linea1CPA;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la linea formateada de Canal 1AFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1AFI(List<string> datosOriginales)
        {
            #region MapeoCanal1AFI
            string Linea1AFI = string.Empty;
            return Linea1AFI;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal 1BFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanal1BFI(List<string> datosOriginales)
        {
            #region MapeoCanal1BFI
            IEnumerable<string> Lineas1BFI = new List<string>();
            return Lineas1BFI;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal 1CFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanal1CFI(List<string> datosOriginales)
        {
            #region MapeoCanal1CFI
            IEnumerable<string> Lineas1CFI = new List<string>();
            return Lineas1CFI;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la linea formateada de Canal 1TFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1TFI(List<string> datosOriginales)
        {
            #region MapeoCanal1TFI
            string Linea1TFI = string.Empty;
            return Linea1TFI;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la linea formateada de Canal 1CDP
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1CDP(List<string> datosOriginales)
        {
            #region MapeoCanal1CDP
            string Linea1CDP = string.Empty;
            return Linea1CDP;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal CART
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanalCART(List<string> datosOriginales)
        {
            #region MapeoCanalCART
            IEnumerable<string> LineasCART = new List<string>();
            return LineasCART;
            #endregion
        }

        /// <summary>
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
        /// Metodo que obtiene las lineas formateadas de Canal CONS
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanalCONS(List<string> datosOriginales)
        {
            #region MapeoCanalCONS
            IEnumerable<string> LineasCONS = new List<string>();
            return LineasCONS;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal NTC5
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public IEnumerable<string> MapeoCanalNTC5(List<string> datosOriginales)
        {
            #region MapeoCanalNTC5
            IEnumerable<string> LineasNTC5 = new List<string>();
            return LineasNTC5;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la linea formateada de Canal 1OOA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1OOA(List<string> datosOriginales)
        {
            #region MapeoCanal1OOA
            string Linea1OOA = string.Empty;
            return Linea1OOA;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la linea formateada de Canal 1HIS
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1HIS(List<string> datosOriginales)
        {
            #region MapeoCanal1HIS
            string Linea1HIS = string.Empty;
            return Linea1HIS;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene la linea formateada de Canal 1HDT
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1HDT(List<string> datosOriginales)
        {
            #region MapeoCanal1HDT
            string Linea1HDT = string.Empty;
            return Linea1HDT;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal 1DET
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1DET(List<string> datosOriginales)
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
        public string MapeoCanal1LLL(List<string> datosOriginales)
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
        public string MapeoCanal1CIC(List<string> datosOriginales)
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
        public string MapeoCanal1RRT(List<string> datosOriginales)
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
        public string MapeoCanal1RRR(List<string> datosOriginales)
        {
            #region MapeoCanal1RRR
            string Lineas1RRR = string.Empty;
            return Lineas1RRR;
            #endregion
        }

        private string FormateoCanal1CMP(List<string> datosOriginales)
        {
            string resultado = string.Empty;



            return resultado;
        }

        private string FormateoCanal1PPP(List<string> datosOriginales)
        {
            string resultado = string.Empty;



            return resultado;
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
