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
    /// <summary>
    /// Clase ProcesoAnexosVerdes
    /// </summary>
    public class ProcesoAnexosVerdes : IProcess
    {
        #region Variables del proceso
        private string NombreArchivo { get; set; }
        private string Descripcion { get; set; }
        private string Telefono { get; set; }

        //private Dictionary<string, Dictionary<string,List<string>>> dicOrdenExtractos = new Dictionary<string, Dictionary<string, List<string>>>();
        private Dictionary<string, OrdenAnexosVerdes> dicOrdenExtractos = new Dictionary<string, OrdenAnexosVerdes>();

        #endregion
        public ProcesoAnexosVerdes(string pRutaArchivo)
        {
            #region ProcesoAnexosVerdes
            try
            {
                Ejecutar(pRutaArchivo);
            }
            catch (Exception ex)
            {
                DatosError StructError = Helpers.ExtraerExcepcion(ex);
                Helpers.EscribirLogVentana(StructError, true);
            }
            #endregion
        }

        /// <summary>
        /// Constructor General
        /// </summary>
        public ProcesoAnexosVerdes()
        { }

        /// <summary>
        /// Metodo Cargue Formateo Archivo
        /// </summary>
        /// <param name="pRutaArchivo"></param>
        public void CargueFormateoArchivo(string pRutaArchivo)
        {
            #region CargueFormateoArchivo
            var archivos = from busqueda in Directory.GetFiles(pRutaArchivo) select busqueda;
            List<string> datosExtractoFormateo = new List<string>();
            string llaveCruce = string.Empty;

            foreach (var archivo in archivos)
            {

                dicOrdenExtractos.Clear();
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
                    NombreArchivo = Path.GetFileName(archivo);
                    List<string> DatosArchivo = File.ReadAllLines(archivo, Encoding.Default).ToList();

                    foreach (var lineaDatos in DatosArchivo)
                    {
                        llaveCruce = lineaDatos.Substring(89, 7).Trim();

                        //AgregarDiccionario(llaveCruce, FormatearArchivo(llaveCruce, new List<string> { lineaDatos }));
                        OrganizarDatos(llaveCruce, lineaDatos);
                    }

                    ExtraerDatosOrganizados();
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

        /// <summary>
        /// Metodo Ejecutar
        /// </summary>
        /// <param name="pRutaArchivo"></param>
        public void Ejecutar(string pRutaArchivo)
        {
            CargueFormateoArchivo(pRutaArchivo);
        }

        /// <summary>
        /// Metodo Formatear Archivo
        /// </summary>
        /// <param name="pLLaveCruce"></param>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public List<string> FormatearArchivo(string pLLaveCruce, List<string> datosOriginales)
        {
            #region FormatearArchivo
            List<string> resultado = new List<string>();
            //dynamic resultadoFormateoLinea = null;
            #region Formateo Canales
            //resultadoFormateoLinea = FormateoCanal1AAA(datosOriginales);

            //if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            //{
            //    resultado.Add(resultadoFormateoLinea);
            //}

            //resultadoFormateoLinea = FormateoCanal1EEA(datosOriginales);

            //if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            //{
            //    resultado.Add(resultadoFormateoLinea);
            //}

            //resultadoFormateoLinea = FormateoCanal1EEE(datosOriginales);

            //if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            //{
            //    resultado.Add(resultadoFormateoLinea);
            //}
            #endregion

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo Organizar Datos
        /// </summary>
        /// <param name="pLLaveCruce"></param>
        /// <param name="datosOriginales"></param>
        private void OrganizarDatos(string pLLaveCruce, string datosOriginales)
        {

            #region OrganizarDatos
            string linea1AAA = FormateoCanal1AAA(datosOriginales);
            string linea1EEE = FormateoCanal1EEE(datosOriginales);

            if (dicOrdenExtractos.ContainsKey(Telefono))
            {
                if (dicOrdenExtractos[Telefono].Detalles.ContainsKey(Descripcion))
                {
                    dicOrdenExtractos[Telefono].Detalles[Descripcion].Add(linea1EEE);
                }
                else
                {
                    dicOrdenExtractos[Telefono].Detalles.Add(Descripcion, new List<string>() { linea1EEE });
                }
            }
            else
            {
                Dictionary<string, List<string>> dicAux = new Dictionary<string, List<string>>();
                dicAux.Add(Descripcion, new List<string>() { linea1EEE });
                dicOrdenExtractos.Add(Telefono, new OrdenAnexosVerdes(linea1AAA, dicAux));
            } 
            #endregion
        }

        /// <summary>
        /// Metodo que Extrae los Datos Organizados
        /// </summary>
        private void ExtraerDatosOrganizados()
        {
            #region ExtraerDatosOrganizados
            List<string> listaExtracto = new List<string>();
            List<string> lista1EEE;
            OrdenAnexosVerdes ordenAnexosVerdes;
            string Linea1EEA = string.Empty;
            foreach (string extracto in dicOrdenExtractos.Keys)
            {
                listaExtracto.Clear();
                ordenAnexosVerdes = dicOrdenExtractos[extracto];

                listaExtracto.Add(ordenAnexosVerdes.Linea1AAA);

                foreach (string descripcionPaquete in ordenAnexosVerdes.Detalles.Keys)
                {
                    lista1EEE = ordenAnexosVerdes.Detalles[descripcionPaquete];
                    Linea1EEA = FormateoCanal1EEA(lista1EEE);
                    listaExtracto.Add(Linea1EEA);
                    listaExtracto.AddRange(lista1EEE);

                }
                AgregarDiccionario(extracto, listaExtracto);
            } 
            #endregion
        }

        #region Canales Logica

        /// <summary>
        /// Metodo de Formateo Canal 1AAA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns>Canal Formateado</returns>
        private string FormateoCanal1AAA(string datosOriginales)
        {
            #region FormateoCanal1AAA
            string resultado = string.Empty;
            List<string> ListaCanal1AAA = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();

            string linea = datosOriginales;
            Telefono =  linea.Substring(89, 7);

            string[] fechasNombreArchivo = NombreArchivo.Split('_');

            ListaCanal1AAA.Add("1AAA");
            ListaCanal1AAA.Add("KitXXXXX");
            ListaCanal1AAA.Add(Telefono);

            string datosAux = Helpers.GetValueInsumoCadena(Variables.Variables.DatosAuxAnexosVerdes, $"{Telefono}") ?? string.Empty;

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

            ListaCanal1AAA.Add(string.Empty); // Ciudad
            ListaCanal1AAA.Add(string.Empty); // Depto
            ListaCanal1AAA.Add(Helpers.FormatearCampos(TiposFormateo.Fecha13, fechasNombreArchivo[2])); // Factura Mes
            ListaCanal1AAA.Add($"{Helpers.FormatearCampos(TiposFormateo.Fecha14, fechasNombreArchivo[3])} al {fechasNombreArchivo[4].Substring(6,2)}"); // Periodo Consumo
            ListaCanal1AAA.Add(string.Empty); // Vacio
            ListaCanal1AAA.Add(string.Empty); // Vacio
            ListaCanal1AAA.Add(string.Empty); // Vacio
            ListaCanal1AAA.Add(fechasNombreArchivo[0]); // Nombre Archivo
            ListaCanal1AAA.Add(string.Empty); // Ciclo Vacio
            ListaCanal1AAA.Add(string.Empty); // Ruta Vacio

            resultado = Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1AAA, '|'));

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo de Formateo Canal 1EEE
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns>Canal Formateado</returns>
        private string FormateoCanal1EEE(string datosOriginales)
        {
            #region FormateoCanal1EEE
            string resultado = string.Empty;

            string linea = datosOriginales;
            string telefono = linea.Substring(89, 7);

            Descripcion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"CODT{linea.Substring(0,6)}").FirstOrDefault()?.Substring(10).Trim() ?? string.Empty;

            List<string> ListaCanal1EEE = new List<string>();
            ListaCanal1EEE.Add("1EEE");
            ListaCanal1EEE.Add(Descripcion);
            ListaCanal1EEE.Add(Helpers.FormatearCampos(TiposFormateo.Fecha15, linea.Substring(6, 8)));
            ListaCanal1EEE.Add($"{linea.Substring(14, 2)}:{linea.Substring(16, 2)}:{linea.Substring(18, 2)}");
            ListaCanal1EEE.Add($"{linea.Substring(33, 3).Trim()} {linea.Substring(36, 7).Trim()}");
            ListaCanal1EEE.Add(linea.Substring(96, 30).Trim());
            ListaCanal1EEE.Add($"{linea.Substring(43, 2).TrimStart('0')}:{linea.Substring(45, 2)}");
            ListaCanal1EEE.Add(Helpers.FormatearCampos(TiposFormateo.Decimal01, linea.Substring(47, 9)));
            ListaCanal1EEE.Add(Helpers.FormatearCampos(TiposFormateo.Decimal01, linea.Substring(56, 9)));
            ListaCanal1EEE.Add(string.Empty); // Ultimo Vacio
            resultado = Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1EEE, '|'));

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo de Formateo Canal 1EEA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns>Canal Formateado</returns>
        private string FormateoCanal1EEA(List<string> pLista1EEE)
        {
            #region FormateoCanal1EEA
            string resultado = string.Empty;
            decimal valor = decimal.Zero;
            decimal total = decimal.Zero;
            string[] campos1EEE;
            foreach (string linea in pLista1EEE)
            {
                campos1EEE = linea.Split('|');
                valor = Convert.ToDecimal(campos1EEE[8].Replace("$","").Replace(".","").Replace(",",""));
                total += valor;
            }

            List<string> ListaCanal1EEE = new List<string>();
            ListaCanal1EEE.Add("1EEA");
            ListaCanal1EEE.Add($"Total {Descripcion}");
            ListaCanal1EEE.Add(Helpers.FormatearCampos(TiposFormateo.Decimal01, total.ToString()));
            ListaCanal1EEE.Add(string.Empty); // Ultimo Vacio

            resultado = Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1EEE, '|'));

            return resultado;
            #endregion
        }

        

        #endregion
    }

    /// <summary>
    /// Estructura de Datos extracto
    /// </summary>
    public struct OrdenAnexosVerdes
    {
        public Dictionary<string, List<string>> Detalles;
        public string Linea1AAA;

        public OrdenAnexosVerdes(string pLinea1AAA, Dictionary<string, List<string>> pDetalles)
        {
            Detalles = pDetalles;
            Linea1AAA = pLinea1AAA;
        }
    }
}
