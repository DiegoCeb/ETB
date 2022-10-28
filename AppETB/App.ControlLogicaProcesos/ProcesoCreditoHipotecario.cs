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
    public class ProcesoCreditoHipotecario : IProcess
    {
        public ProcesoCreditoHipotecario(string pArchivo)
        {
            #region ProcesoCreditoHipotecario
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
        public ProcesoCreditoHipotecario()
        { }

        public void CargueFormateoArchivo(string pArchivo)
        {
            #region CargueFormateoArchivo
            List<string> DatosArchivo = File.ReadAllLines(pArchivo, Encoding.Default).ToList();
            List<string> datosExtractoFormateo = new List<string>();
            string llaveCruce = string.Empty;
            bool encabezado = true;

            var obtenerPaquete = from busqueda in DatosArchivo
                                 group busqueda by busqueda.Split(';').ElementAt(2) into resultado
                                 select resultado;

            foreach (var lineaDatos in obtenerPaquete.Select(x => x))
            {
                if (encabezado)
                {
                    encabezado = false;
                    continue;
                }

                AgregarDiccionario(lineaDatos.Key, FormatearArchivo(lineaDatos.Key, lineaDatos.ToList()));
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
            List<string> linea1AAA = new List<string>();

            string[] campos = datosOriginales[0].Split(';');

            linea1AAA.Add("1AAA");
            linea1AAA.Add("KitXXXXX");

            foreach (string campo in campos)
            {
                linea1AAA.Add(campo);
            }

            linea1AAA.Add(GetCodigoBarras(campos[0], campos[16], campos[17]));
            linea1AAA.Add(GetCartas(campos[2].Trim())); //TODO: Cruzar con insumo Cartas

            if (campos.Length > 20 && (!string.IsNullOrEmpty(campos[21].Trim())))
            {
                linea1AAA.Add("email1_2");
            }
            else
            {
                linea1AAA.Add(string.Empty);
            }

            linea1AAA.Add(string.Empty); // Vacio


            resultado = Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(linea1AAA, '|'));
            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que realiza el cruce de archivo de cartas
        /// </summary>
        /// <param name="pNumIdentificacion"></param>
        /// <returns></returns>
        private string GetCartas(string pNumIdentificacion)
        {
            #region GetCartas
            string Resultado = string.Empty;
            string carta = Helpers.GetValueInsumoCadena(Variables.Variables.DatosCartasHipotecario, $"{pNumIdentificacion}") ?? string.Empty;

            if (!string.IsNullOrEmpty(carta))
            {
                Resultado = "CART";
            }


            return Resultado; 
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene el codigo de barras
        /// </summary>
        /// <param name="pNumReferencia"></param>
        /// <param name="pTotalPagar"></param>
        /// <param name="pFechaPago"></param>
        /// <returns></returns>
        private string GetCodigoBarras(string pNumReferencia, string pTotalPagar, string pFechaPago)
        {
            #region GetCodigoBarras
            string CodeBar = string.Empty;

            string numeroETB = Utilidades.LeerAppConfig("numeroETB");
            string totalPagar = pTotalPagar.Split(',')[0];
            totalPagar = totalPagar.Replace(".", "");
            totalPagar = totalPagar.PadLeft(10, '0');
            string fechaPago = Helpers.FormatearCampos(TiposFormateo.Fecha17, pFechaPago);

            CodeBar = $"(415){numeroETB}(8020){pNumReferencia}(3900){totalPagar}(96){fechaPago}";

            return CodeBar; 
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
            string linea1BBB = string.Empty;
            for (int i = 1; i < datosOriginales.Count; i++)
            {
                linea1BBB = datosOriginales[i].Replace(";","|");
                resultado.Add($"1BBB|{linea1BBB}");
            }


            return resultado;
            #endregion
        }
        #endregion
    }
}
