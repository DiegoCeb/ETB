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
    public class ProcesoGobiernos : IProcess
    {
        public ProcesoGobiernos(string pArchivo)
        {
            #region ProcesoGobiernos
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
        public ProcesoGobiernos()
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
                            Variables.Variables.CuentasNoImprimir.Add(llaveCruce, FormatearArchivo(llaveCruce, datosExtractoFormateo));
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
            #region FormatearArchivo
            List<string> resultado = new List<string>();
            dynamic resultadoFormateoLinea = null;

            //Para Validaciones
            if (pLLaveCruce == "")
            {

            }

            #region Formateo Canales
            resultadoFormateoLinea = FormateoCanal1AAA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1BBB(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1BBA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CTF(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CRM(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CTT(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CIM(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CIV(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CST(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CCD(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormarPaqueteADNC(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormarPaqueteEEE(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1FFF(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1FFA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormarPaqueteSERV(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1III(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalTPNC(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1JJJ(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1JJA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1KKK(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CDP(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1LLL(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1MMM(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1QQQ(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            #endregion

            return resultado;
            #endregion
        }

        #region Canales Logica

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1AAA(List<string> datosOriginales)
        {
            #region FormateoCanal1AAA
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1BBB(List<string> datosOriginales)
        {
            #region FormateoCanal1BBB
            List<string> resultado = new List<string>();

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1BBA(List<string> datosOriginales)
        {
            #region FormateoCanal1BBA
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1KKK(List<string> datosOriginales)
        {
            #region FormateoCanal1KKK
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1CTF(List<string> datosOriginales)
        {
            #region FormateoCanal1CTF
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1CRM(List<string> datosOriginales)
        {
            #region FormateoCanal1CRM
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1CTT(List<string> datosOriginales)
        {
            #region FormateoCanal1CTT
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1CIM(List<string> datosOriginales)
        {
            #region FormateoCanal1CIM
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1CIV(List<string> datosOriginales)
        {
            #region FormateoCanal1CIV
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1CST(List<string> datosOriginales)
        {
            #region FormateoCanal1CST
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1CCD(List<string> datosOriginales)
        {
            #region FormateoCanal1CCD
            List<string> resultado = new List<string>();

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormarPaqueteADNC(List<string> datosOriginales)
        {
            #region FormarPaqueteADNC
            List<string> resultado = new List<string>();

            //TODO: ACA TOCA FORMAR UN PAQUETE de los canales ADNC - ADN1 - 1DDD - 1DDA
            //EJEMPLO
            //ADNC|VETPLUS MEDICINA PREPAGADA VETERINARIA SAS
            //ADN1|6015556991|Conexion | CL 95 48 40 | | Mundo Automaticos y Asist20 | |
            //1DDA|6015556991|Telefonía Local |$ 400.00 |$ 76.00 |$ 476.00 |
            //1DDD|6015556991|Consumo del mes|$ 400.00 |$ 76.00 |$ 400.00 | | |
            //1DDD|6015556991| | | | | | |
            //ADN1|7337013891|Cuenta | CL 95 48 40 | | Mundo Automaticos y Asist20 | |
            //1DDA|7337013891|Telefonía Local |$ 298,418.12 |$ 56,699.44 |$ 355,117.56 |
            //1DDD|7337013891|Cargo fijo del mes |$ 297,860.00 |$ 56,775.44 |$ 298,818.12 |$ 958.12 | |
            //1DDD|7337013891|Dcto consumo pne e1 - RDSI no c| -$ 400.00 | -$ 76.00 | -$ 400.00 | | |


            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormarPaqueteEEE(List<string> datosOriginales)
        {
            #region FormarPaqueteEEE
            List<string> resultado = new List<string>();

            //TODO: ACA TOCA FORMAR UN PAQUETE de los canales 1EE1 - 1EE2 - 1EE3
            //EJEMPLO
            //1EE1 | 6013817150 | Consumo Fijo Móvil |$ 65,628.00 |$ 12,469.32 |$ 80,722.44 |$ 2,625.12 |
            //1EE2 | 6013817150 | |
            //1EE3 | 6013817150 | 04Ago2021 | 13:59:58 | | 3142701036 | COMCEL COLO | 1:00 | 103.00 | 103.00 |

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormarPaqueteSERV(List<string> datosOriginales)
        {
            #region FormarPaqueteSERV
            List<string> resultado = new List<string>();

            //TODO: ACA TOCA FORMAR UN PAQUETE de los canales SERV - 1GGG - 1GGA - 1GGB
            //EJEMPLO
            //SERV | Cargo Fijo Voz | Administradas IP troncal| 3 |
            //1GGG | Cargo Fijo Voz| Administradas IP troncal| 12053276142 | | | 20210801 - 20210831 | | | | | |$ 0.00 |$ 354,435.00 |$ 67,342.65 |
            //1GGA | Total Administradas IP troncal |$ 354,435.00 |$ 67,342.65 |$ 421,777.65 |
            //1GGB | Total Cargo Fijo Voz |$ 982,935.00 |$ 186,757.65 |$ 1,169,692.65 |
            //SERV | Cargo Fijo Voz | Autoattendant | 0 |
            //1GGG | Cargo Fijo Voz| Autoattendant | 12053276142 | | | 20210801 - 20210831 | | | | | |$ 0.00 |$ 6,600.00 |$ 1,254.00 |
            //1GGA | Total Autoattendant |$ 6,600.00 |$ 1,254.00 |$ 7,854.00 |
            //1GGB | Total Cargo Fijo Voz |$ 982,935.00 |$ 186,757.65 |$ 1,169,692.65 |
            //SERV | Cargo Fijo Voz | Consola de recepcionista| 0 |
            //1GGG | Cargo Fijo Voz| Consola de recepcionista| 12053276142 | | | 20210801 - 20210831 | | | | | |$ 0.00 |$ 35,500.00 |$ 6,745.00 |
            //1GGA | Total Consola de recepcionista |$ 35,500.00 |$ 6,745.00 |$ 42,245.00 |
            //1GGB | Total Cargo Fijo Voz |$ 982,935.00 |$ 186,757.65 |$ 1,169,692.65 |



            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1JJJ(List<string> datosOriginales)
        {
            #region FormateoCanal1JJJ
            List<string> resultado = new List<string>();

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1JJA(List<string> datosOriginales)
        {
            #region FormateoCanal1JJA
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1LLL(List<string> datosOriginales)
        {
            #region FormateoCanal1LLL
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1CDP(List<string> datosOriginales)
        {
            #region FormateoCanal1CDP
            List<string> resultado = new List<string>();

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1FFF(List<string> datosOriginales)
        {
            #region FormateoCanal1FFF
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1FFA(List<string> datosOriginales)
        {
            #region FormateoCanal1FFA
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1III(List<string> datosOriginales)
        {
            #region FormateoCanal1III
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanalTPNC(List<string> datosOriginales)
        {
            #region FormateoCanalTPNC
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1MMM(List<string> datosOriginales)
        {
            #region FormateoCanal1MMM
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1QQQ(List<string> datosOriginales)
        {
            #region FormateoCanal1QQQ
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        #endregion
    }
}
