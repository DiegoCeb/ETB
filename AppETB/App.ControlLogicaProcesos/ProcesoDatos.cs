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
    public class ProcesoDatos : IProcess
    {
        private string Cuenta { get; set; }
        public ProcesoDatos(string pArchivo)
        {
            #region ProcesoDatos
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
        public ProcesoDatos()
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

            resultadoFormateoLinea = FormateoCanalADNC(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormarPaqueteADN1(datosOriginales);

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

            resultadoFormateoLinea = FormateoCanal1JJK(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1KKK(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalCONS(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalCUFE(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalNTC0(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalNTC1(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalNTC2(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalNTC3(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalNTC4(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalNTC5(datosOriginales);

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

            resultadoFormateoLinea = FormateoCanalCART(datosOriginales);

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
        private string FormateoCanalCONS(List<string> datosOriginales)
        {
            #region FormateoCanalCONS
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanalCUFE(List<string> datosOriginales)
        {
            #region FormateoCanalCUFE
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanalNTC0(List<string> datosOriginales)
        {
            #region FormateoCanalNTC0
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanalNTC1(List<string> datosOriginales)
        {
            #region FormateoCanalNTC1
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanalNTC2(List<string> datosOriginales)
        {
            #region FormateoCanalNTC2
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanalNTC3(List<string> datosOriginales)
        {
            #region FormateoCanalNTC3
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanalNTC4(List<string> datosOriginales)
        {
            #region FormateoCanalNTC4
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanalNTC5(List<string> datosOriginales)
        {
            #region FormateoCanalNTC5
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
        private string FormateoCanalADNC(List<string> datosOriginales)
        {
            #region FormateoCanalADNC
            string resultado = string.Empty;

            var linea010000 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                              select busqueda;

            if (linea010000.Any())
            {
                resultado = Helpers.ValidarPipePipe($"ADNC|{linea010000.FirstOrDefault().Substring(6, 50).Trim()}");
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormarPaqueteADN1(List<string> datosOriginales)
        {
            #region FormarPaqueteADN1
            List<string> resultado = new List<string>();
            Dictionary<string, List<string>> paquetesInformacion = new Dictionary<string, List<string>>();
            bool banderaPaquete = false;
            string cuentaConexion = string.Empty;
            string busquedaPlanMinutos = string.Empty;
            string planMinutos = string.Empty;
            string llaveCruce = string.Empty;

            //TODO: ACA TOCA FORMAR UN PAQUETE de los canales ADN1 - 1DDD - 1DDA
            //EJEMPLO
            //ADN1|6015556991|Conexion | CL 95 48 40 | | Mundo Automaticos y Asist20 | |
            //1DDA|6015556991|Telefonía Local |$ 400.00 |$ 76.00 |$ 476.00 |
            //1DDD|6015556991|Consumo del mes|$ 400.00 |$ 76.00 |$ 400.00 | | |
            //1DDD|6015556991| | | | | | |
            //ADN1|7337013891|Cuenta | CL 95 48 40 | | Mundo Automaticos y Asist20 | |
            //1DDA|7337013891|Telefonía Local |$ 298,418.12 |$ 56,699.44 |$ 355,117.56 |
            //1DDD|7337013891|Cargo fijo del mes |$ 297,860.00 |$ 56,775.44 |$ 298,818.12 |$ 958.12 | |
            //1DDD|7337013891|Dcto consumo pne e1 - RDSI no c| -$ 400.00 | -$ 76.00 | -$ 400.00 | | |

            //Toda la info sale del paquete de canales que va desde el priemr 040000 hasta el siguiente 040000

            #region Organizacion Paquetes de Informacion
            foreach (var linea in datosOriginales)
            {
                if (linea.Substring(0, 6).Equals("040000"))
                {
                    banderaPaquete = true;
                    cuentaConexion = linea.Substring(6, 20).Trim();
                }

                if (linea.Substring(0, 6).Equals("031111"))
                {
                    banderaPaquete = false;
                }

                if (banderaPaquete)
                {
                    if (paquetesInformacion.ContainsKey(cuentaConexion))
                    {
                        paquetesInformacion[cuentaConexion].Add(linea);
                    }
                    else
                    {
                        paquetesInformacion.Add(cuentaConexion, new List<string> { linea });
                    }
                }
            }
            #endregion

            var linea06T112 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("06T112")
                              select busqueda;

            if (paquetesInformacion.Any())
            {
                foreach (var lineaDetalle in paquetesInformacion)
                {
                    #region Busqueda Informacion
                    var linea040000 = from busqueda in lineaDetalle.Value
                                      where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                      select busqueda;

                    var linea090000 = from busqueda in lineaDetalle.Value
                                      where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("090000")
                                      select busqueda;

                    busquedaPlanMinutos = linea040000.FirstOrDefault().Substring(133, 10).Trim();

                    if (!string.IsNullOrEmpty(busquedaPlanMinutos))
                    {
                        llaveCruce = $"PL{busquedaPlanMinutos}";
                        planMinutos = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(13).Trim() ?? string.Empty;
                    } 
                    #endregion

                    //resultado.Add(Helpers.ValidarPipePipe($"ADN1|{lineaDetalle.Key}|{GetTipo(lineaDetalle.Key)}|{linea040000.FirstOrDefault().Substring(76, 30).Trim()}|" +
                    //    $"{planMinutos}|{linea090000.FirstOrDefault().Substring(26, 27).Trim()}| | "));

                    //resultado.Add(Helpers.ValidarPipePipe($"1DDA|{lineaDetalle.Key}|"));
                }
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
        private IEnumerable<string> FormateoCanal1JJK(List<string> datosOriginales)
        {
            #region FormateoCanal1JJK
            List<string> resultado = new List<string>();

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
        private string FormateoCanalCART(List<string> datosOriginales)
        {
            #region FormateoCanalCART
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
