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
    public class ProcesoAnexosVerdes : IProcess
    {
        public ProcesoAnexosVerdes(string pRutaArchivo)
        {
            #region ProcesoAnexosVerdes
            try
            {
                Ejecutar(pRutaArchivo);
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
        public ProcesoAnexosVerdes()
        { }

        public void CargueFormateoArchivo(string pRutaArchivo)
        {
            #region CargueFormateoArchivo
            var archivos = from busqueda in Directory.GetFiles(pRutaArchivo) select busqueda;
            List<string> datosExtractoFormateo = new List<string>();
            string llaveCruce = string.Empty;

            foreach (var archivo in archivos)
            {
                List<string> DatosArchivo = File.ReadAllLines(archivo, Encoding.Default).ToList();

                //if (Path.GetFileNameWithoutExtension(archivo).Contains("DBASI"))
                //{
                //    foreach (var lineaDatos in DatosArchivo)
                //    {
                //        llaveCruce = lineaDatos.Substring(7).Trim();

                //        AgregarDiccionario(llaveCruce, FormatearArchivo(llaveCruce, new List<string> { lineaDatos }));
                //    }
                //}
                if (Path.GetFileNameWithoutExtension(archivo).Contains("MOVTO"))
                {
                    foreach (var lineaDatos in DatosArchivo)
                    {
                        llaveCruce = lineaDatos.Substring(89, 7).Trim();

                        AgregarDiccionario(llaveCruce, FormatearArchivo(llaveCruce, new List<string> { lineaDatos }));
                    }
                }
            }
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

        public void Ejecutar(string pRutaArchivo)
        {
            CargueFormateoArchivo(pRutaArchivo);
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

            resultadoFormateoLinea = FormateoCanal1EEA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1EEE(datosOriginales);

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
            List<string> ListaCanal1AAA = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();

            string linea = datosOriginales.FirstOrDefault();
            string telefono =  linea.Substring(89, 7);

           


            ListaCanal1AAA.Add("1AAA");
            ListaCanal1AAA.Add("KitXXXX");
            ListaCanal1AAA.Add(telefono);

            string datosAux = Helpers.GetValueInsumoCadena(Variables.Variables.DatosAuxAnexosVerdes, $"{telefono}") ?? string.Empty;

            if (!string.IsNullOrEmpty(datosAux))
            {
                listaCortes.Clear();
                listaCortes.Add(new PosCortes(7, 50));
                listaCortes.Add(new PosCortes(57, 40));
                listaCortes.Add(new PosCortes(97, 0));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, datosAux));
            }
            else
            {
                ListaCanal1AAA.Add(string.Empty);
                ListaCanal1AAA.Add(string.Empty);
                ListaCanal1AAA.Add(string.Empty);
            }


            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1EEE(List<string> datosOriginales)
        {
            #region FormateoCanal1EEE
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1EEA(List<string> datosOriginales)
        {
            #region FormateoCanal1EEA
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        #endregion
    }
}
