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
        #region Variables del proceso
        private bool IsDatos { get; set; }
        private string Cuenta { get; set; }
        private string Ciclo { get; set; }
        private string Estrato { get; set; }
        private string PeriodoFacturacion { get; set; }
        private string FechaDesde { get; set; }
        private string FechaHasta { get; set; }
        private string FechaExpedicion { get; set; }
        private int? MesMora { get; set; }
        private string CodigoDANE { get; set; }
        private bool ban1CTF { get; set; }
        private ClientesEspeciales cE { get; set; }
        #endregion

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
            if (pArchivo.Contains("desktop.ini"))
            {
                return;
            }


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
                    Variables.Variables.CuentasNoImprimir.Add(llaveCruce, FormatearArchivo(llaveCruce, datosExtractoFormateo));
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
            if (pLLaveCruce == "12054016956" || pLLaveCruce == "7551235491")
            {

            }

            FormatearPropiedadesExtracto();

            #region Formateo Canales
            resultadoFormateoLinea = FormateoCanal1AAA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            if (cE.ReversionPago == "X")
            {
                int indice = datosOriginales.FindIndex(x => x.Substring(0, 6) == "02T016");

                if (indice > -1)
                {
                    datosOriginales.RemoveAt(indice);
                }
            }

            resultadoFormateoLinea = FormateoCanal1BBB(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CTF(datosOriginales);

            if(ban1CTF)
            {
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

            resultadoFormateoLinea = FormateoCanal1FFF(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormarPaqueteSERV(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1III(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.Add(resultadoFormateoLinea);
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

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
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

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
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

            resultadoFormateoLinea = FormateoCanalCONS(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalCART(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1MMM(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            #endregion

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private void FormatearPropiedadesExtracto()
        {
            #region FormatearPropiedadesExtracto
            IsDatos = false;
            Cuenta = string.Empty;
            Ciclo = string.Empty;
            Estrato = string.Empty;
            FechaExpedicion = string.Empty;
            MesMora = null;
            CodigoDANE = string.Empty;
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

                Cuenta = Linea010000.Substring(117, 20).Trim();// Cuenta
                Ciclo = Linea010000.Substring(151, 4).Trim().TrimStart('0'); // Asignamos Ciclo a variable Global

                cE = CargarClientesEspeciales(Cuenta, Ciclo);

                listaCortes.Add(new PosCortes(6, 50));
                listaCortes.Add(new PosCortes(56, 12));
                listaCortes.Add(new PosCortes(68, 40));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));

                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"DANC{Linea010000.Substring(108, 5).Trim()}").FirstOrDefault()?.Substring(9).Trim() ?? string.Empty);
                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"DAND{Linea010000.Substring(108, 2).Trim()}").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty);


                listaCortes.Clear();
                listaCortes.Add(new PosCortes(117, 20));
                listaCortes.Add(new PosCortes(139, 12));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));


                ListaCanal1AAA.Add(Ciclo);
                ListaCanal1AAA.Add(GetTotalPagar(datosOriginales));

                listaCortes.Clear();
                listaCortes.Add(new PosCortes(168, 8, TiposFormateo.Fecha01));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));
                ListaCanal1AAA.Add(GetMesMora(Linea010000.Substring(176, 2).Trim(), datosOriginales));

                listaCortes.Clear();
                listaCortes.Add(new PosCortes(178, 8, TiposFormateo.Fecha01));
                FechaDesde = Linea010000.Substring(178, 8).Trim();
                listaCortes.Add(new PosCortes(186, 8, TiposFormateo.Fecha01));
                FechaHasta = Linea010000.Substring(186, 8).Trim();
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));

                ListaCanal1AAA.Add(GetTelefono(datosOriginales));
                ListaCanal1AAA.Add(string.Empty); // Tipo Identificacion solo aplica para Fibra

                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECP{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);
                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECL{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);
                ListaCanal1AAA.Add(GetFechaExpedicion(Linea010000));

                ListaCanal1AAA.Add(GetNumeroReferencia(Linea010000.Substring(139, 12).Trim()));
                ListaCanal1AAA.AddRange(GetCodigosBarras(Linea010000.Substring(139, 12), Linea010000, datosOriginales));
                ListaCanal1AAA.Add(GetTipoEtapas(Linea010000.Substring(151, 3)));
                ListaCanal1AAA.Add(GetTasaInteres(Linea040000));
                listaCortes.Clear();
                listaCortes.Add(new PosCortes(108, 5));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));
                ListaCanal1AAA.Add(string.Empty); // TODO: Anexos Publicitarios - Verificar regla
                ListaCanal1AAA.Add(GetActividad(Linea040000));
                ListaCanal1AAA.Add(string.Empty); // Estrato en Datos Vacio
                ListaCanal1AAA.AddRange(GetBarrioLocalidad());
                ListaCanal1AAA.Add(GetValorPagarMes(datosOriginales)); //Valor Pagar Mes
                ListaCanal1AAA.Add(GetLogoInternet());
                ListaCanal1AAA.Add(GetMarcaAnexosPublicidad());
                ListaCanal1AAA.AddRange(GetEmailTipoEmal());
                ListaCanal1AAA.AddRange(GetMarcaCupones());
                ListaCanal1AAA.Add(GetMarcaNumHojas());
                ListaCanal1AAA.Add(GetNivelReclamacion());
                listaCortes.Clear();
                listaCortes.Add(new PosCortes(108, 5));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000)); // Codigo DANE
                CodigoDANE = Linea010000.Substring(108, 5);
                ListaCanal1AAA.Add(GetLogoCivico(datosOriginales));
                ListaCanal1AAA.Add(string.Empty); //TODO: Doble marca Transpromo Verificar
                ListaCanal1AAA.Add(GetMarcaCourier());
                ListaCanal1AAA.Add(GetCuentaVencidaAnticipada());
                ListaCanal1AAA.Add(GetMarcaParafiscales()); // Parafiscales
                ListaCanal1AAA.Add(string.Empty); //TODO: Tipo Email Prevalidador Verificar
                ListaCanal1AAA.AddRange(GetLocBar());
                ListaCanal1AAA.AddRange(GetQR(Linea010000.Substring(6, 50)));
                ListaCanal1AAA.Add(string.Empty); //CampoVacio
                ListaCanal1AAA.Add(string.Empty); // Fecha Corte No aplica para Datos
                ListaCanal1AAA.AddRange(GetPeriodoDesdeHasta(datosOriginales));

                listaCortes.Clear();
                listaCortes.Add(new PosCortes(258, 20)); // Adicionales Fe
                listaCortes.Add(new PosCortes(278, 17)); // Adicionales Fe
                listaCortes.Add(new PosCortes(295, 8));  // Adicionales Fe
                listaCortes.Add(new PosCortes(303, 20)); // Adicionales Fe

                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));

                if (Linea010000.Length > 325)
                {
                    listaCortes.Clear();
                    listaCortes.Add(new PosCortes(324, 30)); //ETB_UEN
                    listaCortes.Add(new PosCortes(354, 30)); //ETB_Categoria
                    listaCortes.Add(new PosCortes(384, 30)); //ETB_Segmento
                    listaCortes.Add(new PosCortes(414, 30)); //ETB_Segmento_UEN
                    ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));
                }
                else
                {
                    ListaCanal1AAA.Add(string.Empty); //ETB_UEN Vacio
                    ListaCanal1AAA.Add(string.Empty); //ETB_Categoria Vacio
                    ListaCanal1AAA.Add(string.Empty); //ETB_Segmento Vacio
                    ListaCanal1AAA.Add(string.Empty); //ETB_Segmento_UEN Vacio
                }


                ListaCanal1AAA.Add(string.Empty); //TODO: Validar Combos - ConformacionPaquetes
                ListaCanal1AAA.Add(string.Empty); //CampoVacio
            }

            Linea1AAA = Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1AAA, '|'));

            return Linea1AAA;
            #endregion
        }

        #region Metodos Mapeos 1AAA

        /// <summary>
        /// Metodo que el TotalPagar
        /// </summary>
        /// <param name="pNumReferencia"></param>
        /// <param name="pLinea010000"></param>
        /// <returns></returns>
        private string GetTotalPagar(List<string> pDatosOriginales)
        {
            #region GetTotalPagar
            string resultado = string.Empty;
            List<string> valoresPago = ObtenerDatosCanal1BBB(pDatosOriginales, true).ToList();
            string totalPagar = valoresPago[0].ToString().Trim().PadLeft(12, '0');

            resultado = Helpers.FormatearCampos(TiposFormateo.Decimal01, totalPagar);
            return resultado;
            #endregion
        }

        /// <summary>
        /// Regla de Mes Mora
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private string GetMesMora(string pCampo, List<string> pDatosOriginales)
        {
            #region GetMesMora
            int mesMora = Convert.ToInt32(pCampo);
            mesMora += 1;
            mesMora = mesMora > 4 ? 4 : mesMora;

            #region Ajuste Morosidad
            var result02T002 = from busqueda in pDatosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T002")
                               select busqueda;

            if (result02T002.Any())
            {
                if (mesMora > 1)
                {
                    mesMora = 1;
                }
            }

            #endregion

            MesMora = mesMora;
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

            string campoTelefono;
            if (result.Any())
            {
                foreach (var item in result.ToList())
                {
                    campoTelefono = item.Substring(6, 20).Trim();

                    if (GetTipo(item.Substring(6, 20).Trim()) == "Conexion")
                    {
                        if (item.Substring(6, 20).Trim().Length >= 8)
                        {
                            telefono = item.Substring(6, 20).Trim();
                            break;
                        }
                    }

                }
            }

            return telefono;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene el Numero de Referencia
        /// </summary>
        /// <param name="pNumReferencia"></param>
        /// <returns></returns>
        private string GetFechaExpedicion(string pLinea010000)
        {
            #region GetFechaExpedicion

            string fechaExpedicion = string.Empty;

            string lineaClientesEspeciales = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoClientesEspecialesDatos, $"{Cuenta}") ?? string.Empty;
            string fechaClientesEspeciales = string.Empty;

            if (!string.IsNullOrEmpty(lineaClientesEspeciales))
            {
                string[] camposClientesEspeciales = lineaClientesEspeciales.Split('|');
                fechaClientesEspeciales = Helpers.FormatearCampos(TiposFormateo.Fecha11, camposClientesEspeciales[10].Trim());
            }

            string fechaExpedicionInsumo = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECX{Helpers.FormatearCampos(TiposFormateo.Fecha02, pLinea010000.Substring(168, 8).Trim())}{pLinea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty;
            string fechaExpedicionLinea010000 = pLinea010000.Length > 278 ? pLinea010000.Substring(278, 17).Trim() : string.Empty;


            if (!string.IsNullOrEmpty(fechaClientesEspeciales))
            {
                fechaExpedicion = fechaClientesEspeciales;
            }
            else if (!string.IsNullOrEmpty(fechaExpedicionLinea010000))
            {
                fechaExpedicion = fechaExpedicionLinea010000;
            }
            else
            {
                fechaExpedicion = fechaExpedicionInsumo;
            }

            FechaExpedicion = fechaExpedicion;

            return fechaExpedicion;

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
        private List<string> GetCodigosBarras(string pNumReferencia, string pLinea010000, List<string> pDatosOriginales)
        {
            #region GetCodigosBarras
            List<string> result = new List<string>();
            string numeroETB = Utilidades.LeerAppConfig("numeroETB");
            string totalPagar = pLinea010000.Substring(155, 11).PadLeft(10, '0');
            string fechaPago = Helpers.FormatearCampos(TiposFormateo.Fecha08, Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECL{Helpers.FormatearCampos(TiposFormateo.Fecha02, pLinea010000.Substring(168, 8).Trim())}{pLinea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);
            string numReferencia = GetNumeroReferencia(pNumReferencia.Trim(), false);
            List<string> valoresPago = ObtenerDatosCanal1BBB(pDatosOriginales, true).ToList();
            string ValorPagarCB1 = valoresPago[0].ToString().Trim().PadLeft(12, '0');
            string ValorPagarCB2 = valoresPago[1].ToString().Trim().PadLeft(12, '0');

            string CodeBar1 = String.Empty;
            if (Convert.ToInt64(valoresPago[1]) >= 0)
            {
                CodeBar1 = $"(415){numeroETB}(8020){numReferencia}(3900){ValorPagarCB1.Substring(0, 10)}(96){fechaPago}";
            }
            else
            {
                CodeBar1 = $"(415){numeroETB}(8020){numReferencia}(3900)0000000000(96){fechaPago}";
            }

            result.Add(CodeBar1);

            string CodeBar2 = $"(415){numeroETB}(8020){numReferencia}(3900){ValorPagarCB2.Substring(0, 10)}(96){fechaPago}";

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

                default:
                    throw new Exception($"Error: La cuenta {Cuenta} que corresponde al ciclo:{ciclo}, no corresponde al proceso de DATOS.");

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
                if (IsDatos)
                {
                    tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM10").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
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

            //if (!string.IsNullOrEmpty(actividadTablaSustitucion) && (IsResidencial || IsFibra || IsGobierno))
            //{
            //    return actividadTablaSustitucion;
            //}
            //else
            //{
            //    return string.Empty;
            //}

            return string.Empty;

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


                string llaveBarrio = ($"{camposlocBar[1].TrimStart('0')}{camposlocBar[2]}").PadLeft(8, '0');
                llaveBarrio = $"CODX{llaveBarrio}";

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
        /// Metodo que Obtiene el Valor Pagar Mes
        /// </summary>
        /// <param name="pDatosOriginales"></param>
        /// <returns></returns>
        private string GetValorPagarMes(List<string> pDatosOriginales)
        {
            #region GetValorPagarMes
			
            List<string> valoresPago = ObtenerDatosCanal1BBB(pDatosOriginales, true).ToList();
            string ValorPagar = Helpers.FormatearCampos(TiposFormateo.Decimal05, valoresPago[1].ToString());
			
            return ValorPagar ?? string.Empty;
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

            //if ((IsFibra || IsResidencial) && Estrato != "" && (Estrato.Trim() == "1" || Estrato.Trim() == "2")) //Regla de validacion del logo de MinTic en facturas fibra y que estrato sea igual a 2
            //{
            //    logoIntener = "LogInt";
            //}


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
                    for (int i = 1; i < camposEmail.Length - 1; i++)
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
                {
                    tipoEmail = "dual";
                    Variables.Variables.DiccionarioDual.Add(Cuenta, Cuenta);
                }
                else
                { tipoEmail = "solo_email"; }
            }

            string lineaSMS = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoCuentasEnvioSms, $"{Cuenta}") ?? string.Empty;

            if (!string.IsNullOrEmpty(lineaSMS))
            {
                tipoEmail = "ENVIO_SMS";
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
        /// Metodo que Obtiene Marca NumHojas
        /// </summary>
        /// <returns></returns>
        private string GetMarcaNumHojas()
        {
            #region GetMarcaNumHojas
            string MarcaNumHojas = string.Empty;
            bool isDual = false;
            bool isSoloMail = false;

            string mail = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoDistribucionEmailRevchain, $"{Cuenta}").FirstOrDefault() ?? string.Empty;

            if (!string.IsNullOrEmpty(mail))
            {
                string dual = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoDistribucionDobleRevchain, $"{Cuenta}") ?? string.Empty;

                if (!string.IsNullOrEmpty(dual))
                { isDual = true; }
                else
                { isSoloMail = true; }
            }

            if (isDual)
            {
                MarcaNumHojas = "DISTRIBUCION_DUAL";
            }
            else if (isSoloMail)
            {
                MarcaNumHojas = "FACTURA_SOLO_EMAIL";
            }



            return MarcaNumHojas;
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

        /// <summary>
        /// Metodo que Obtiene Logo Civico
        /// </summary>
        /// <returns></returns>
        private string GetLogoCivico(List<string> pDatosOriginales)
        {
            #region GetLogoCivico
            string LogoCivico = string.Empty;

            var result11C = from busqueda in pDatosOriginales
                            where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C")
                            select busqueda;

            List<string> lista11C = result11C as List<string>;
            string campoValidar = string.Empty;
            if (lista11C != null && lista11C.Count > 0)
            {
                foreach (var linea in lista11C)
                {
                    campoValidar = linea.Substring(6, 10);

                    if (campoValidar == "3874040341" || campoValidar == "0164040342" || campoValidar == "3874041341" || campoValidar == "0164041342")
                    {
                        LogoCivico = "CIVICO";
                    }
                }
            }


            return LogoCivico;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene Logo Civico
        /// </summary>
        /// <returns></returns>
        private string GetMarcaCourier()
        {
            #region GetNivelReclamacion
            string marcaCourier = string.Empty;

            string lineaMarcaCourier = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoCicloCourier, $"{Ciclo}") ?? string.Empty;

            if (!string.IsNullOrEmpty(lineaMarcaCourier.Trim()))
            {
                string[] camposMarcaCourier = lineaMarcaCourier.Split('|');

                if (camposMarcaCourier.Length > 0)
                {
                    marcaCourier = camposMarcaCourier[1];
                }
            }

            return marcaCourier;
            #endregion
        }

        // <summary>
        /// Metodo que Obtiene CuentaVencidaAnticipada
        /// </summary>
        /// <returns></returns>
        private string GetCuentaVencidaAnticipada()
        {
            #region GetCuentaVencidaAnticipada
            string cuentaVencidaAnticipada = string.Empty;

            string lineacuentaVencidaAnticipada = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoCuentasLte, $"{Cuenta}") ?? string.Empty;

            if (!string.IsNullOrEmpty(lineacuentaVencidaAnticipada.Trim()))
            {
                cuentaVencidaAnticipada = "CuentaVencidaAnticipada";
            }

            return cuentaVencidaAnticipada;
            #endregion
        }

        // <summary>
        /// Metodo que Obtiene Marca Parafiscales
        /// </summary>
        /// <returns></returns>
        private string GetMarcaParafiscales()
        {
            #region GetMarcaParafiscales
            string marcaParafiscales = string.Empty;

            string cadenaCiclosParafiscales = Utilidades.LeerAppConfig("CiclosParafiscales");

            if (!string.IsNullOrEmpty(cadenaCiclosParafiscales))
            {
                List<string> ciclosParafiscales = cadenaCiclosParafiscales.Split(',').ToList();

                if (ciclosParafiscales.Count > 0)
                {
                    if (ciclosParafiscales.Contains(Ciclo))
                    {
                        marcaParafiscales = "LP";
                    }
                }

            }

            return marcaParafiscales;
            #endregion
        }

        // <summary>
        /// Metodo que Obtiene Localizacion
        /// </summary>
        /// <returns></returns>
        private List<string> GetLocBar()
        {
            #region GetLocBar
            List<string> listaLocBar = new List<string>();

            string Precis = string.Empty;
            string ChipCatastral = string.Empty;
            string Coordenadas = string.Empty;

            string lineaLocBar = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoLocBar, $"{Cuenta}").FirstOrDefault() ?? string.Empty;

            if (!string.IsNullOrEmpty(lineaLocBar.Trim()))
            {
                string[] camposLocBar = lineaLocBar.Split('|');

                if (camposLocBar.Length > 3)
                {
                    Precis = camposLocBar[3];
                    ChipCatastral = camposLocBar[4];
                    Coordenadas = camposLocBar[5];
                }
            }

            listaLocBar.Add(Precis);
            listaLocBar.Add(ChipCatastral);
            listaLocBar.Add(Coordenadas);

            return listaLocBar;
            #endregion
        }

        // <summary>
        /// Metodo que Obtiene QR
        /// </summary>
        /// <returns></returns>
        private List<string> GetQR(string pNombre)
        {
            #region GetLocBar
            List<string> listaFacturaElectronica = new List<string>();

            string url_qr = $"https://tracking.carvajalcomunicacion.com/wdelta/w/m/aa/?ack=H1" +
                $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(Cuenta))}" +
                $"H1" +
                $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(pNombre.Substring(0, 35).Trim()))}";

            listaFacturaElectronica.Add(url_qr);

            return listaFacturaElectronica;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene PeriodoDesdeHasta
        /// </summary>
        /// <param name="pDatosOriginales"></param>
        /// <returns></returns>
        private List<string> GetPeriodoDesdeHasta(List<string> pDatosOriginales)
        {
            #region PeriodoDesdeHasta
            List<string> periodos = new List<string>();
            string periodoDesdeHasta = string.Empty;
            string periodoDesdeHastaLTE = string.Empty;

            string fechaInicio = string.Empty;
            string fechaFin = string.Empty;

            var result11C = from busqueda in pDatosOriginales
                            where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C")
                            select busqueda;

            if (result11C.Any())
            {
                List<string> listaFechaInicio = new List<string>();
                List<string> listaFechaFin = new List<string>();
                foreach (string linea11C in result11C)
                {
                    if (!string.IsNullOrEmpty(linea11C.Substring(128, 20).Trim()) && !Helpers.GetContieneLetras(linea11C.Substring(128, 20).Trim()))
                    {
                        if (Convert.ToInt64(linea11C.Substring(132, 2)) != 0 && Convert.ToInt64(linea11C.Substring(134, 2)) != 0 && Convert.ToInt64(linea11C.Substring(143, 2)) != 0 && Convert.ToInt64(linea11C.Substring(145, 2)) != 0)
                        {
                            listaFechaInicio.Add(linea11C.Substring(128, 8));
                            listaFechaFin.Add(linea11C.Substring(139, 8));
                        }
                    }
                }

                if (listaFechaInicio.Count != 0 && listaFechaFin.Count != 0)
                {
                    fechaInicio = Helpers.GetFechaMaximaMinima(listaFechaInicio, 2);
                    fechaFin = Helpers.GetFechaMaximaMinima(listaFechaFin, 1);
                }

            }


            string cuentasLTE9697 = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoCuentasLte, $"{Cuenta}") ?? string.Empty;

            if (!string.IsNullOrEmpty(fechaInicio) && !string.IsNullOrEmpty(fechaFin))
            {
                periodoDesdeHasta = $"{fechaInicio} - {fechaFin}";

                if (!string.IsNullOrEmpty(cuentasLTE9697))
                {
                    periodoDesdeHastaLTE = $"PER LTE {periodoDesdeHasta}";
                    periodoDesdeHasta = string.Empty;
                }

            }


            periodos.Add(periodoDesdeHasta);
            periodos.Add(periodoDesdeHastaLTE);

            return periodos;
            #endregion
        }


        #endregion Metodos 1AAA

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1BBB(List<string> pDatosOriginales)
        {
            #region FormateoCanal1BBB
            IEnumerable<string> Lineas1BBB = ObtenerDatosCanal1BBB(pDatosOriginales, false);

            return Lineas1BBB; 
            #endregion
        }

        private IEnumerable<string> ObtenerDatosCanal1BBB(List<string> datosOriginales, bool sumatorias)
        {
            #region ObtenerDatosCanal1BBB
            List<string> Lineas1BBB = new List<string>();
            List<string> LineasSumatorias = new List<string>();
            string descripcion = string.Empty;
            string llave = string.Empty;
            decimal SubTotal1BBB = 0;
            decimal ValorPagarMes = 0;
            string lineaAjusteDecena = string.Empty;
            string lineaServiciosETBIVA = string.Empty;
            string lineaNotasCredito = string.Empty;
            decimal calculoAjusteDecena = decimal.Zero;
            decimal tempValorTotalIva = decimal.Zero;
            List<string> lineasFinanciacion = new List<string>();
            List<string> lineasFinales1BBB = new List<string>();

            #region Notas Crédito
            var result29000 = from busqueda in datosOriginales
                              where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("29000")
                              select busqueda;

            if (result29000.Any())
            {
                string linea29000 = result29000.FirstOrDefault() ?? string.Empty;

                if (!string.IsNullOrEmpty(linea29000))
                {
                    lineaNotasCredito = $"1BBB|Notas Crédito|{Helpers.FormatearCampos(TiposFormateo.Decimal05, linea29000.Substring(29, 20).TrimStart('0'))}| ";
                    SubTotal1BBB += Convert.ToInt64(linea29000.Substring(29, 20));
                }

            }
            #endregion

            #region Detalles resumen Facturacion ETB


            var result150001 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("150001")
                               select busqueda;

            string linea150001 = string.Empty;

            if (result150001.Any())
            {
                linea150001 = result29000.FirstOrDefault() ?? string.Empty;
            }

            var resultDetalles = from busqueda in datosOriginales
                                 where busqueda.Length > 4
                                 let comp4 = busqueda.Substring(0, 4).Trim()
                                 where
                                 comp4 == "02S0" ||
                                 (comp4 == "02T0" &&
                                 (Convert.ToInt16(busqueda.Substring(4, 2).Trim()) < 50 ||
                                 Convert.ToInt16(busqueda.Substring(4, 2).Trim()) > 76))
                                 select busqueda;

            if (resultDetalles.Any())
            {
                List<string> listaDetalles = resultDetalles.ToList();

                foreach (string detalle in listaDetalles)
                {

                    llave = detalle.Substring(0, 6).Trim();
                    descripcion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"CODT{detalle.Substring(0, 6).Trim()}")?.FirstOrDefault() ?? string.Empty;
                    descripcion = descripcion.Substring(10).Trim();

                    if (llave == "02T304" || llave == "02T309")
                    {
                        descripcion = descripcion.Substring(0, 23);
                    }
                    else if (llave == "02T112")
                    {
                        descripcion = descripcion.Substring(0, 31);
                    }

                    if (llave == "02T008")
                    { } // se omite el valor de subsidio
                    else if (llave == "02S000")
                    {
                        if (cE.Doc1SaldoGracias == "X" || (Convert.ToInt64(detalle.Substring(6, 14)) == 0 && Convert.ToInt64(detalle.Substring(34, 14)) == 0))
                        { continue; }

                        if (!string.IsNullOrEmpty(detalle.Substring(20, 14).Trim()) && Convert.ToInt64(detalle.Substring(20, 14)) != 0)
                        {
                            Lineas1BBB.Add($"1BBB|Traslado de Saldos|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(20, 14).TrimStart('0'))}| ");
                            SubTotal1BBB += Convert.ToInt64(detalle.Substring(20, 14));
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                        {
                            if (string.IsNullOrEmpty(linea150001) || (!string.IsNullOrEmpty(linea150001) && linea150001.Substring(6, 1) != "N")) // Regla No sumar saldo anterior
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14));
                            }
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(34, 14).Trim()) && Convert.ToInt64(detalle.Substring(34, 14)) != 0)
                        {
                            Lineas1BBB.Add($"1BBB|Ajuste De Pagos|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(34, 14).TrimStart('0'))}| ");
                            SubTotal1BBB += Convert.ToInt64(detalle.Substring(34, 14));
                        }
                    }
                    else
                    {
                        if (Convert.ToInt64(detalle.Substring(6, 14)) == 0 && Convert.ToInt64(detalle.Substring(20, 14)) == 0 &&
                        Convert.ToInt64(detalle.Substring(34, 14)) == 0 && Convert.ToInt64(detalle.Substring(48, 14)) == 0 &&
                        Convert.ToInt64(detalle.Substring(62, 14)) == 0 && Convert.ToInt64(detalle.Substring(118, 14)) == 0)
                        { continue; }

                        if (llave == "02T019")
                        {
                            if (cE.Estampilla == "X")
                            { continue; }

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T016")
                        {
                            if (cE.ReversionPago == "X")
                            { continue; }

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T002")
                        {
                            if (cE.Doc1SaldoGracias == "X")
                            { continue; }

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T003")
                        {
                            if (!string.IsNullOrEmpty(cE.AjusteDecena) && cE.AjusteDecena != "X")
                            {
                                calculoAjusteDecena = Convert.ToDecimal(cE.AjusteDecena.Replace(".", ""));
                            }
                            else
                            {
                                calculoAjusteDecena += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                            if (cE.AjusteDecena == "X")
                            {
                                SubTotal1BBB += calculoAjusteDecena;
                                ValorPagarMes += calculoAjusteDecena;
                            }
                            else
                            {
                                lineaAjusteDecena = $"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(6, 14).TrimStart('0'))}| ";
                                SubTotal1BBB += calculoAjusteDecena;
                            }
                        }
                        else
                        {
                            Int64 impuestoConsumo = Convert.ToInt64(detalle.Substring(118, 14));

                            decimal sumatoria = Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14)) + impuestoConsumo;

                            if (llave == "02T004")
                            {
                                lineaServiciosETBIVA = $"1BBB|{descripcion}|***| ";
                                tempValorTotalIva += sumatoria;
                                lineasFinales1BBB.Add(lineaServiciosETBIVA);

                            }
                            else if (llave == "02T020" || llave == "02T050")
                            { // Req 11052015 Campo Financiacion Mes
                                lineasFinanciacion.Add($"1BBF|{descripcion}|{sumatoria}| ");
                                continue;
                            }


                            if (llave != "02T004")
                            {
                                lineasFinales1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, sumatoria.ToString())}| ");
                            }

                            SubTotal1BBB += sumatoria;
                            ValorPagarMes += sumatoria;

                        }
                    }
                }

            }
            #endregion

            #region Servicios ETB IVA, AJuste Decena y Total
            if (lineasFinales1BBB.Count > 0)
            {
                string[] campos;
                foreach (var linea in lineasFinales1BBB)
                {
                    campos = linea.Split('|');

                    if (campos[2] == "***" || campos.Length == 2)
                    {
                        Lineas1BBB.Add(linea.Replace("***", $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, tempValorTotalIva.ToString())}"));
                    }
                    else
                    {
                        Lineas1BBB.Add(linea);
                    }
                }
            }
            if (!string.IsNullOrEmpty(lineaNotasCredito))
            {
                Lineas1BBB.Add(lineaNotasCredito);
            }
            if (!string.IsNullOrEmpty(lineaAjusteDecena))
            {
                Lineas1BBB.Add(lineaAjusteDecena);
            }

            if (SubTotal1BBB > 0)
            {
                Lineas1BBB.Add($"1BBA|Total de la Factura ETB|{Helpers.FormatearCampos(TiposFormateo.Decimal05, SubTotal1BBB.ToString())}| ");
            }
            else
            {
                Lineas1BBB.Add($"1BBA|Saldo a Favor|{Helpers.FormatearCampos(TiposFormateo.Decimal05, SubTotal1BBB.ToString())}| ");
            }

            if (lineasFinanciacion.Count > 0)
            {
                Lineas1BBB.AddRange(lineasFinanciacion);
            }

            #endregion

            LineasSumatorias.Add(SubTotal1BBB.ToString());
            LineasSumatorias.Add(ValorPagarMes.ToString());

            if (sumatorias)
            {
                return LineasSumatorias;
            }
            else
            {
                return Lineas1BBB;
            }
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
            string Linea1KKK = string.Empty;
            string oficinaPqr = string.Empty;

            oficinaPqr = GetOficinaPQR($"CZONA80000");
            Linea1KKK = $"1KKK|{oficinaPqr} | ";


            return Linea1KKK;
            #endregion
        }

        private string GetOficinaPQR(string pLlave)
        {
            #region GetOficinaPQR
            string lineaOficinaPQR = string.Empty;

            string oficinaPqr = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, pLlave).FirstOrDefault() ?? string.Empty;

            if (!string.IsNullOrEmpty(oficinaPqr))
            {
                lineaOficinaPQR = oficinaPqr.Replace(pLlave, string.Empty).Replace("|", " ").Trim();
            }

            return lineaOficinaPQR;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
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
                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, linea29000.FirstOrDefault().Substring(29, 20).Trim())}|{linea29000.FirstOrDefault().Substring(49).Trim()}| ");
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanalNTC3(List<string> datosOriginales)
        {
            #region FormateoCanalNTC3
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanalNTC4(List<string> datosOriginales)
        {
            #region FormateoCanalNTC4
            List<string> resultado = new List<string>();

            var linea30002 = from busqueda in datosOriginales
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("30002")
                             select busqueda;

            if (linea30002.Any())
            {
                foreach (var lineaDetalle in linea30002)
                {
                    resultado.Add(Helpers.ValidarPipePipe($"NTC4|{lineaDetalle.Substring(5, 10).Trim()}|{lineaDetalle.Substring(15, 70).Trim()}|" +
                        $"{lineaDetalle.Substring(85, 20).Trim()}| "));
                }
            }

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

            var linea30003 = from busqueda in datosOriginales
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("30003")
                             select busqueda;

            if (linea30003.Any())
            {
                resultado = Helpers.ValidarPipePipe($"NTC5|{linea30003.FirstOrDefault().Substring(5, 2).Trim()}|{linea30003.FirstOrDefault().Substring(7, 5).Trim()}|" +
                            $"{linea30003.FirstOrDefault().Substring(12, 20).Trim()}| ");
            }

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
            List<string> lisCamposSumar = new List<string>();
            string identificadorCanal = string.Empty;

            #region Busqueda            

            var result02TX = from busqueda in datosOriginales
                             where busqueda.Length > 4 && (busqueda.Substring(0, 4).Equals("02T1") ||
                                                           busqueda.Substring(0, 4).Equals("02T2") ||
                                                           busqueda.Substring(0, 4).Equals("02T3") ||
                                                           busqueda.Substring(0, 4).Equals("02T4") ||
                                                           busqueda.Substring(0, 4).Equals("02T5") ||
                                                           busqueda.Substring(0, 4).Equals("02T6") ||
                                                           busqueda.Substring(0, 4).Equals("02T7") ||
                                                           busqueda.Substring(0, 4).Equals("02T8") ||
                                                           busqueda.Substring(0, 4).Equals("02T9"))
                             select busqueda;
            #endregion


            if (result02TX.Any())
            {
                foreach (var lineaActual in result02TX)
                {
                    identificadorCanal = lineaActual.Substring(0, 6);

                    if (identificadorCanal == "02T112" || identificadorCanal == "02T222" || identificadorCanal == "02T309" || identificadorCanal == "02T409" ||
                       identificadorCanal == "02T576" || identificadorCanal == "02T801" || identificadorCanal == "02T942")
                    {
                        lisCamposSumar.Add(lineaActual.Substring(6, 14));
                        lisCamposSumar.Add(lineaActual.Substring(20, 14));
                        lisCamposSumar.Add(lineaActual.Substring(34, 14));
                        lisCamposSumar.Add(lineaActual.Substring(48, 14));
                        lisCamposSumar.Add(lineaActual.Substring(62, 14));
                        lisCamposSumar.Add(lineaActual.Substring(118, 14));
                    }
                }

                string sumaTotal = Helpers.SumarCampos(lisCamposSumar, "D");

                if(sumaTotal != "$ 0.00")
                {
                    ban1CTF = true;
                }

                resultado = "1CTF|TOTAL FACTURADO|" + sumaTotal + "| ";
            }

            return Helpers.ValidarPipePipe(resultado);
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
            List<string> lisCamposSumar = new List<string>();
            Dictionary<string, List<string>> dicValores = new Dictionary<string, List<string>>();
            string llavePrincipal = string.Empty;
            string concepto = "RECARGO MORA";
            string ivaFormateado = String.Empty;
            string identificadorCanal = string.Empty;
            decimal recargo = 0;
            decimal ivaRecargo = 0;
            decimal impuestoProducto = 0;
            string impuestoFormateado = String.Empty;
            decimal iva = 0;



            dicValores.Add("1", new List<string>());    // Telefonia Local
            dicValores.Add("2", new List<string>());    // Larga Distancia
            dicValores.Add("3", new List<string>());    // Internet y Datos
            dicValores.Add("4", new List<string>());    // TV
            dicValores.Add("5", new List<string>());    // Otros Operadores
            dicValores.Add("6", new List<string>());    // Alianzas
            dicValores.Add("7", new List<string>());    // Otros Operadores
            dicValores.Add("8", new List<string>());    // Alianzas
            dicValores.Add("9", new List<string>());    // Movilidad
            dicValores.Add("SUBTOTAL", new List<string>());
            dicValores.Add("IVA", new List<string>());
            dicValores.Add("IMPUESTOS", new List<string>());
            dicValores.Add("TOTAL", new List<string>());

            #region Busqueda            

            var result02TX = from busqueda in datosOriginales
                             where busqueda.Length > 4 && (busqueda.Substring(0, 4).Equals("02T1") ||
                                                           busqueda.Substring(0, 4).Equals("02T2") ||
                                                           busqueda.Substring(0, 4).Equals("02T3") ||
                                                           busqueda.Substring(0, 4).Equals("02T4") ||
                                                           busqueda.Substring(0, 4).Equals("02T5") ||
                                                           busqueda.Substring(0, 4).Equals("02T6") ||
                                                           busqueda.Substring(0, 4).Equals("02T7") ||
                                                           busqueda.Substring(0, 4).Equals("02T8") ||
                                                           busqueda.Substring(0, 4).Equals("02T9"))
                             select busqueda;
            #endregion

            if (result02TX.Any())
            {
                foreach (var lineaActual in result02TX)
                {
                    llavePrincipal = lineaActual.Substring(3, 1);
                    identificadorCanal = lineaActual.Substring(0, 6);

                    if (identificadorCanal == "02T112" || identificadorCanal == "02T222" || identificadorCanal == "02T309" || identificadorCanal == "02T409" ||
                       identificadorCanal == "02T576" || identificadorCanal == "02T801" || identificadorCanal == "02T942")
                    {
                        if (dicValores.Keys.Contains(llavePrincipal))
                        {
                            dicValores[llavePrincipal].Add(lineaActual.Substring(20, 14));
                            dicValores["SUBTOTAL"].Add(lineaActual.Substring(20, 14));
                            dicValores["IMPUESTOS"].Add("0");
                        }
                    }
                    else
                    {
                        if (Convert.ToDouble(lineaActual.Substring(6, 14)) != 0 || identificadorCanal == "02T317")
                        {
                            recargo = Convert.ToDecimal($"{lineaActual.Substring(20, 12)}.{lineaActual.Substring(32, 2)}");
                            iva = Convert.ToDecimal(lineaActual.Substring(34, 14));

                            if (recargo != 0 && iva != 0)
                            {
                                ivaRecargo = recargo * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeIva"));
                                ivaRecargo = Decimal.Round(ivaRecargo, 2);
                                ivaFormateado = ivaRecargo.ToString().Split('.')[0] + ivaRecargo.ToString().Split('.')[1].Substring(0, 2);
                                dicValores["IVA"].Add(ivaFormateado);
                                dicValores["IMPUESTOS"].Add("0");
                            }
                        }
                        else if (Convert.ToDouble(lineaActual.Substring(20, 14)) != 0)
                        {
                            // Se agrega Iva
                            dicValores["IVA"].Add(lineaActual.Substring(34, 14));

                            if (identificadorCanal == "02T582" || identificadorCanal == "02T507" || identificadorCanal == "02T510" || identificadorCanal == "02T511" ||
                                identificadorCanal == "02T517" || identificadorCanal == "02T502" || identificadorCanal == "02T504" || identificadorCanal == "02T505" || identificadorCanal == "02T118")
                            {
                                impuestoProducto = Convert.ToDecimal($"{lineaActual.Substring(34, 12)}.{lineaActual.Substring(44, 2)}");
                                impuestoProducto = impuestoProducto * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeImpuestoConsumo"));
                                impuestoProducto = Decimal.Round(impuestoProducto);
                                impuestoFormateado = impuestoProducto.ToString().Split('.')[0] + impuestoProducto.ToString().Split('.')[1].Substring(0, 2);
                                dicValores["IMPUESTOS"].Add(impuestoFormateado);
                            }
                        }
                    }
                }

                // Armar canal

                List<string> sumarCamposAux = new List<string>();

                resultado = "1CRM|";
                resultado += concepto + "|";
                resultado += Helpers.SumarCampos(dicValores["1"],"D") + "|";
                resultado += Helpers.SumarCampos(dicValores["3"],"D") + "|";
                resultado += Helpers.SumarCampos(dicValores["2"],"D") + "|";
                resultado += Helpers.SumarCampos(dicValores["9"],"D") + "|";
                resultado += Helpers.SumarCampos(dicValores["4"],"D") + "|";

                // Suma Conceptos 6 - 8
                sumarCamposAux.AddRange(dicValores["6"]);
                sumarCamposAux.AddRange(dicValores["8"]);
                resultado += Helpers.SumarCampos(sumarCamposAux,"D") + "|";
                sumarCamposAux.Clear();

                // Suma Conceptos 5 - 7
                sumarCamposAux.AddRange(dicValores["5"]);
                sumarCamposAux.AddRange(dicValores["7"]);
                resultado += Helpers.SumarCampos(sumarCamposAux,"D") + "|";
                sumarCamposAux.Clear();

                resultado += Helpers.SumarCampos(dicValores["SUBTOTAL"],"D") + "|";
                resultado += Helpers.SumarCampos(dicValores["IVA"],"D") + "|";
                resultado += Helpers.SumarCampos(dicValores["IMPUESTOS"],"D") + "|";

                // Suma Conceptos 
                sumarCamposAux.AddRange(dicValores["SUBTOTAL"]);
                sumarCamposAux.AddRange(dicValores["IVA"]);
                sumarCamposAux.AddRange(dicValores["IMPUESTOS"]);
                resultado += Helpers.SumarCampos(sumarCamposAux,"D") + "|";
                sumarCamposAux.Clear();
            }

            return Helpers.ValidarPipePipe(resultado).Replace("$ 0.00", "-");
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
            List<string> lisCamposSumar = new List<string>();
            Dictionary<string, List<string>> dicValores = new Dictionary<string, List<string>>();
            string llavePrincipal = string.Empty;
            string concepto = "TOTAL";
            string ivaFormateado = String.Empty;
            string identificadorCanal = string.Empty;
            decimal recargo = 0;
            decimal ivaRecargo = 0;
            decimal impuestoProducto = 0;
            string impuestoFormateado = String.Empty;
            decimal iva = 0;

            string llavelineaNegocio = string.Empty;
            string lineaNegocio = string.Empty;

            // Inicializa diccionario Con Valores
            dicValores.Add("1", new List<string>());    // Telefonia Local
            dicValores.Add("2", new List<string>());    // Larga Distancia
            dicValores.Add("3", new List<string>());    // Internet y Datos
            dicValores.Add("4", new List<string>());    // TV
            dicValores.Add("5", new List<string>());    // Otros Operadores
            dicValores.Add("6", new List<string>());    // Alianzas
            dicValores.Add("7", new List<string>());    // Otros Operadores
            dicValores.Add("8", new List<string>());    // Alianzas
            dicValores.Add("9", new List<string>());    // Movilidad
            dicValores.Add("SUBTOTAL", new List<string>());
            dicValores.Add("IVA", new List<string>());
            dicValores.Add("IMPUESTOS", new List<string>());
            dicValores.Add("TOTAL", new List<string>());

            List<string> lisSUBTOTAL = new List<string>();

            #region Busqueda            

            var result02TX = from busqueda in datosOriginales
                             where busqueda.Length > 4 && (busqueda.Substring(0, 4).Equals("02T1") ||
                                                           busqueda.Substring(0, 4).Equals("02T2") ||
                                                           busqueda.Substring(0, 4).Equals("02T3") ||
                                                           busqueda.Substring(0, 4).Equals("02T4") ||
                                                           busqueda.Substring(0, 4).Equals("02T5") ||
                                                           busqueda.Substring(0, 4).Equals("02T6") ||
                                                           busqueda.Substring(0, 4).Equals("02T7") ||
                                                           busqueda.Substring(0, 4).Equals("02T8") ||
                                                           busqueda.Substring(0, 4).Equals("02T9"))
                             select busqueda;
            #endregion

            if (result02TX.Any())
            {
                foreach (var lineaActual in result02TX)
                {
                    llavePrincipal = lineaActual.Substring(3, 1);
                    identificadorCanal = lineaActual.Substring(0, 6);

                    #region Toma Linea Negocio
                    llavelineaNegocio = lineaActual.Substring(3, 1);

                    if (llavelineaNegocio == "0")
                    {
                        llavelineaNegocio = "1";
                    }

                    llavelineaNegocio = "FACLIN" + llavelineaNegocio;

                    if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llavelineaNegocio))
                    {
                        lineaNegocio = Variables.Variables.DatosInsumoTablaSustitucion[llavelineaNegocio][0].Substring(8).Trim();
                    }
                    #endregion

                    if (identificadorCanal == "02T112" || identificadorCanal == "02T222" || identificadorCanal == "02T309" || identificadorCanal == "02T409" ||
                       identificadorCanal == "02T576" || identificadorCanal == "02T801" || identificadorCanal == "02T942")
                    {
                        if (dicValores.Keys.Contains(llavePrincipal))
                        {
                            dicValores[llavePrincipal].Add(lineaActual.Substring(6, 14));
                            dicValores[llavePrincipal].Add(lineaActual.Substring(20, 14));
                            dicValores[llavePrincipal].Add(lineaActual.Substring(34, 14));
                            dicValores[llavePrincipal].Add(lineaActual.Substring(48, 14));
                            dicValores[llavePrincipal].Add(lineaActual.Substring(62, 14));

                            dicValores["SUBTOTAL"].Add(lineaActual.Substring(6, 14));
                            dicValores["IVA"].Add(lineaActual.Substring(34, 14));
                            dicValores["IMPUESTOS"].Add("0");

                            dicValores["TOTAL"].Add(lineaActual.Substring(6, 14));
                            dicValores["TOTAL"].Add(lineaActual.Substring(20, 14));
                            dicValores["TOTAL"].Add(lineaActual.Substring(34, 14));
                            dicValores["TOTAL"].Add(lineaActual.Substring(48, 14));
                            dicValores["TOTAL"].Add(lineaActual.Substring(62, 14));
                            dicValores["TOTAL"].Add(lineaActual.Substring(118, 14));
                        }
                    }
                    else
                    {
                        if ((Convert.ToDouble(lineaActual.Substring(6, 14)) != 0 || identificadorCanal == "02T317"))
                        {
                            lisSUBTOTAL.Add(lineaActual.Substring(6, 14));

                            recargo = Convert.ToDecimal($"{lineaActual.Substring(20, 12)}.{lineaActual.Substring(32, 2)}");
                            iva = Convert.ToDecimal(lineaActual.Substring(34, 14));

                            if (recargo != 0 && iva != 0)
                            {
                                ivaRecargo = recargo * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeIva"));
                                //ivaRecargo = Decimal.Round(ivaRecargo, 2);
                                ivaFormateado = ivaRecargo.ToString().Split('.')[0] + ivaRecargo.ToString().Split('.')[1].Substring(0, 2);

                                if (ivaRecargo.ToString().Contains("."))
                                {
                                    if (impuestoProducto == 0)
                                    {
                                        impuestoProducto = 0.00m;
                                    }
                                    ivaFormateado = ivaRecargo.ToString().Split('.')[0] + impuestoProducto.ToString().Split('.')[1].Substring(0, 2);
                                }
                                else
                                {
                                    ivaFormateado = ivaRecargo.ToString().Replace(",", "").Replace(".", "");
                                }

                                dicValores["IVA"].Add("-" + ivaFormateado);
                            }

                            if (lineaNegocio == "LTE" && Convert.ToDouble(lineaActual.Substring(118, 4)) != 0)
                            {
                                dicValores["IMPUESTOS"].Add(lineaActual.Substring(118, 4));
                            }
                            else if (identificadorCanal == "02T582" || identificadorCanal == "02T507" || identificadorCanal == "02T510" || identificadorCanal == "02T511" ||
                                    identificadorCanal == "02T517" || identificadorCanal == "02T502" || identificadorCanal == "02T504" || identificadorCanal == "02T505" || identificadorCanal == "02T118")
                            {
                                string sumaSubtotal = Helpers.SumarCampos(lisSUBTOTAL, "D").Replace("$", "").Replace(".", "").Replace(",", "").Trim();

                                var entero = sumaSubtotal.Substring(0, sumaSubtotal.Length - 2);
                                var decima = sumaSubtotal.Substring(sumaSubtotal.Length - 2, 2);

                                impuestoProducto = Convert.ToDecimal($"{entero}.{decima}");
                                impuestoProducto = impuestoProducto * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeImpuestoConsumo"));

                                if (impuestoProducto.ToString().Contains("."))
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Split('.')[0] + impuestoProducto.ToString().Split('.')[1].Substring(0, 2);
                                }
                                else
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Replace(",", "").Replace(".", "");
                                }

                                dicValores["IMPUESTOS"].Add(impuestoFormateado);
                            }
                        }
                        else if (Convert.ToDouble(lineaActual.Substring(20, 14)) != 0)
                        {
                            if (lineaNegocio == "LTE" && Convert.ToDouble(lineaActual.Substring(118, 4)) != 0)
                            {
                                dicValores["IMPUESTOS"].Add(lineaActual.Substring(118, 4));
                            }
                            else if (identificadorCanal == "02T582" || identificadorCanal == "02T507" || identificadorCanal == "02T510" || identificadorCanal == "02T511" ||
                                    identificadorCanal == "02T517" || identificadorCanal == "02T502" || identificadorCanal == "02T504" || identificadorCanal == "02T505" || identificadorCanal == "02T118")
                            {
                                impuestoProducto = Convert.ToDecimal($"{lineaActual.Substring(34, 12)}.{lineaActual.Substring(44, 2)}");
                                impuestoProducto = impuestoProducto * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeImpuestoConsumo"));
                                //impuestoProducto = Decimal.Round(impuestoProducto);

                                if (impuestoProducto.ToString().Contains("."))
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Split('.')[0] + impuestoProducto.ToString().Split('.')[1].Substring(0, 2);
                                }
                                else
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Replace(",", "").Replace(".", "");
                                }

                                dicValores["IMPUESTOS"].Add(impuestoFormateado);
                            }
                        }
                    }
                }

                // Armar canal

                List<string> sumarCamposAux = new List<string>();

                resultado = "1CTT|";
                resultado += concepto + "|";
                resultado += Helpers.SumarCampos(dicValores["1"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["3"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["2"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["9"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["4"], "D") + "|";

                // Suma Conceptos 6 - 8
                sumarCamposAux.AddRange(dicValores["6"]);
                sumarCamposAux.AddRange(dicValores["8"]);
                resultado += Helpers.SumarCampos(sumarCamposAux, "D") + "|";
                sumarCamposAux.Clear();

                // Suma Conceptos 5 - 7
                sumarCamposAux.AddRange(dicValores["5"]);
                sumarCamposAux.AddRange(dicValores["7"]);
                resultado += Helpers.SumarCampos(sumarCamposAux, "D") + "|";
                sumarCamposAux.Clear();

                resultado += Helpers.SumarCampos(dicValores["SUBTOTAL"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["IVA"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["IMPUESTOS"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["TOTAL"], "D") + "| ";
            }

            return Helpers.ValidarPipePipe(resultado).Replace("$ 0.00", "-");
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
            List<string> lisCamposSumar = new List<string>();
            Dictionary<string, List<string>> dicValores = new Dictionary<string, List<string>>();
            string llavePrincipal = string.Empty;
            string concepto = "IMPUESTOS";
            string ivaFormateado = String.Empty;
            string identificadorCanal = string.Empty;
            decimal recargo = 0;
            decimal ivaRecargo = 0;
            decimal impuestoProducto = 0;
            string impuestoFormateado = String.Empty;
            decimal iva = 0;

            string llavelineaNegocio = string.Empty;
            string lineaNegocio = string.Empty;

            // Inicializa diccionario Con Valores
            dicValores.Add("1", new List<string>());    // Telefonia Local
            dicValores.Add("2", new List<string>());    // Larga Distancia
            dicValores.Add("3", new List<string>());    // Internet y Datos
            dicValores.Add("4", new List<string>());    // TV
            dicValores.Add("5", new List<string>());    // Otros Operadores
            dicValores.Add("6", new List<string>());    // Alianzas
            dicValores.Add("7", new List<string>());    // Otros Operadores
            dicValores.Add("8", new List<string>());    // Alianzas
            dicValores.Add("9", new List<string>());    // Movilidad
            dicValores.Add("SUBTOTAL", new List<string>());
            dicValores.Add("IVA", new List<string>());
            dicValores.Add("IMPUESTOS", new List<string>());
            dicValores.Add("TOTAL", new List<string>());

            List<string> lisSUBTOTAL = new List<string>();

            #region Busqueda            

            var result02TX = from busqueda in datosOriginales
                             where busqueda.Length > 4 && (busqueda.Substring(0, 4).Equals("02T1") ||
                                                           busqueda.Substring(0, 4).Equals("02T2") ||
                                                           busqueda.Substring(0, 4).Equals("02T3") ||
                                                           busqueda.Substring(0, 4).Equals("02T4") ||
                                                           busqueda.Substring(0, 4).Equals("02T5") ||
                                                           busqueda.Substring(0, 4).Equals("02T6") ||
                                                           busqueda.Substring(0, 4).Equals("02T7") ||
                                                           busqueda.Substring(0, 4).Equals("02T8") ||
                                                           busqueda.Substring(0, 4).Equals("02T9"))
                             select busqueda;
            #endregion

            if (result02TX.Any())
            {
                foreach (var lineaActual in result02TX)
                {
                    llavePrincipal = lineaActual.Substring(3, 1);
                    identificadorCanal = lineaActual.Substring(0, 6);

                    #region Toma Linea Negocio
                    llavelineaNegocio = lineaActual.Substring(3, 1);

                    if (llavelineaNegocio == "0")
                    {
                        llavelineaNegocio = "1";
                    }

                    llavelineaNegocio = "FACLIN" + llavelineaNegocio;

                    if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llavelineaNegocio))
                    {
                        lineaNegocio = Variables.Variables.DatosInsumoTablaSustitucion[llavelineaNegocio][0].Substring(8).Trim();
                    }
                    #endregion

                    if (identificadorCanal == "02T112" || identificadorCanal == "02T222" || identificadorCanal == "02T309" || identificadorCanal == "02T409" ||
                       identificadorCanal == "02T576" || identificadorCanal == "02T801" || identificadorCanal == "02T942")
                    {
                        if (dicValores.Keys.Contains(llavePrincipal))
                        {
                            dicValores[llavePrincipal].Add("0");
                            dicValores["SUBTOTAL"].Add("0");
                            dicValores["IVA"].Add("0");
                            dicValores["IMPUESTOS"].Add("0");
                            dicValores["TOTAL"].Add("0");
                        }
                    }
                    else
                    {
                        if ((Convert.ToDouble(lineaActual.Substring(6, 14)) != 0 || identificadorCanal == "02T317"))
                        {
                            lisSUBTOTAL.Add(lineaActual.Substring(6, 14));

                            if (lineaNegocio == "LTE" && Convert.ToDecimal(lineaActual.Substring(118, 14)) != 0)
                            {
                                dicValores[llavePrincipal].Add(lineaActual.Substring(118, 14));
                                dicValores["SUBTOTAL"].Add("0");
                                dicValores["IVA"].Add("0");
                                dicValores["IMPUESTOS"].Add(lineaActual.Substring(118, 14));
                                dicValores["TOTAL"].Add(lineaActual.Substring(118, 14));
                            }
                            else if (identificadorCanal == "02T582" || identificadorCanal == "02T507" || identificadorCanal == "02T510" || identificadorCanal == "02T511" ||
                                    identificadorCanal == "02T517" || identificadorCanal == "02T502" || identificadorCanal == "02T504" || identificadorCanal == "02T505" || identificadorCanal == "02T118")
                            {
                                string sumaSubtotal = Helpers.SumarCampos(lisSUBTOTAL, "D").Replace("$", "").Replace(".", "").Replace(",", "").Trim();

                                var entero = sumaSubtotal.Substring(0, sumaSubtotal.Length - 2);
                                var decima = sumaSubtotal.Substring(sumaSubtotal.Length - 2, 2);

                                impuestoProducto = Convert.ToDecimal($"{entero}.{decima}");
                                impuestoProducto = impuestoProducto * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeImpuestoConsumo"));

                                if (impuestoProducto.ToString().Contains("."))
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Split('.')[0] + impuestoProducto.ToString().Split('.')[1].Substring(0, 2);
                                }
                                else
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Replace(",", "").Replace(".", "");
                                }

                                dicValores[llavePrincipal].Add(impuestoFormateado);
                                dicValores["SUBTOTAL"].Add("0");
                                dicValores["IVA"].Add("0");
                                dicValores["IMPUESTOS"].Add(impuestoFormateado);
                                dicValores["TOTAL"].Add(impuestoFormateado);
                            }
                        }
                        else if (Convert.ToDouble(lineaActual.Substring(20, 14)) != 0)
                        {
                            if (lineaNegocio == "LTE" && Convert.ToDouble(lineaActual.Substring(118, 14)) != 0)
                            {
                                dicValores[llavePrincipal].Add(lineaActual.Substring(118, 14));
                                dicValores["SUBTOTAL"].Add("0");
                                dicValores["IVA"].Add("0");
                                dicValores["IMPUESTOS"].Add(lineaActual.Substring(118, 14));
                                dicValores["TOTAL"].Add(lineaActual.Substring(118, 14));
                            }
                            else if (identificadorCanal == "02T582" || identificadorCanal == "02T507" || identificadorCanal == "02T510" || identificadorCanal == "02T511" ||
                                     identificadorCanal == "02T517" || identificadorCanal == "02T502" || identificadorCanal == "02T504" || identificadorCanal == "02T505" || identificadorCanal == "02T118")
                            {
                                impuestoProducto = Convert.ToDecimal($"{lineaActual.Substring(34, 12)}.{lineaActual.Substring(44, 2)}");
                                impuestoProducto = impuestoProducto * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeImpuestoConsumo"));
                                //impuestoProducto = Decimal.Round(impuestoProducto);

                                if (impuestoProducto.ToString().Contains("."))
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Split('.')[0] + impuestoProducto.ToString().Split('.')[1].Substring(0, 2);
                                }
                                else
                                {
                                    impuestoFormateado = impuestoProducto.ToString();
                                }

                                dicValores[llavePrincipal].Add(impuestoFormateado);
                                dicValores["SUBTOTAL"].Add("0");
                                dicValores["IVA"].Add("0");
                                dicValores["IMPUESTOS"].Add(impuestoFormateado);
                                dicValores["TOTAL"].Add(impuestoFormateado);
                            }
                        }
                    }
                }

                // Armar canal

                List<string> sumarCamposAux = new List<string>();

                resultado = "1CIM|";
                resultado += concepto + "|";
                resultado += Helpers.SumarCampos(dicValores["1"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["3"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["2"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["9"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["4"], "D") + "|";

                // Suma Conceptos 6 - 8
                sumarCamposAux.AddRange(dicValores["6"]);
                sumarCamposAux.AddRange(dicValores["8"]);
                resultado += Helpers.SumarCampos(sumarCamposAux, "D") + "|";
                sumarCamposAux.Clear();

                // Suma Conceptos 5 - 7
                sumarCamposAux.AddRange(dicValores["5"]);
                sumarCamposAux.AddRange(dicValores["7"]);
                resultado += Helpers.SumarCampos(sumarCamposAux, "D") + "|";
                sumarCamposAux.Clear();

                resultado += Helpers.SumarCampos(dicValores["SUBTOTAL"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["IVA"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["IMPUESTOS"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["TOTAL"], "D") + "| ";
            }

            return Helpers.ValidarPipePipe(resultado).Replace("$ 0.00", "-");
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
            List<string> lisCamposSumar = new List<string>();
            Dictionary<string, List<string>> dicValores = new Dictionary<string, List<string>>();
            string llavePrincipal = string.Empty;
            string concepto = "IVA";
            string ivaFormateado = String.Empty;
            string identificadorCanal = string.Empty;
            decimal recargo = 0;
            decimal ivaRecargo = 0;
            decimal impuestoProducto = 0;
            string impuestoFormateado = String.Empty;
            decimal iva = 0;

            string llavelineaNegocio = string.Empty;
            string lineaNegocio = string.Empty;

            // Inicializa diccionario Con Valores
            dicValores.Add("1", new List<string>());    // Telefonia Local
            dicValores.Add("2", new List<string>());    // Larga Distancia
            dicValores.Add("3", new List<string>());    // Internet y Datos
            dicValores.Add("4", new List<string>());    // TV
            dicValores.Add("5", new List<string>());    // Otros Operadores
            dicValores.Add("6", new List<string>());    // Alianzas
            dicValores.Add("7", new List<string>());    // Otros Operadores
            dicValores.Add("8", new List<string>());    // Alianzas
            dicValores.Add("9", new List<string>());    // Movilidad
            dicValores.Add("SUBTOTAL", new List<string>());
            dicValores.Add("IVA", new List<string>());
            dicValores.Add("IMPUESTOS", new List<string>());
            dicValores.Add("TOTAL", new List<string>());

            List<string> lisSUBTOTAL = new List<string>();

            #region Busqueda            

            var result02TX = from busqueda in datosOriginales
                             where busqueda.Length > 4 && (busqueda.Substring(0, 4).Equals("02T1") ||
                                                           busqueda.Substring(0, 4).Equals("02T2") ||
                                                           busqueda.Substring(0, 4).Equals("02T3") ||
                                                           busqueda.Substring(0, 4).Equals("02T4") ||
                                                           busqueda.Substring(0, 4).Equals("02T5") ||
                                                           busqueda.Substring(0, 4).Equals("02T6") ||
                                                           busqueda.Substring(0, 4).Equals("02T7") ||
                                                           busqueda.Substring(0, 4).Equals("02T8") ||
                                                           busqueda.Substring(0, 4).Equals("02T9"))
                             select busqueda;
            #endregion

            if (result02TX.Any())
            {
                foreach (var lineaActual in result02TX)
                {
                    llavePrincipal = lineaActual.Substring(3, 1);
                    identificadorCanal = lineaActual.Substring(0, 6);

                    #region Toma Linea Negocio
                    llavelineaNegocio = lineaActual.Substring(3, 1);

                    if (llavelineaNegocio == "0")
                    {
                        llavelineaNegocio = "1";
                    }

                    llavelineaNegocio = "FACLIN" + llavelineaNegocio;

                    if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llavelineaNegocio))
                    {
                        lineaNegocio = Variables.Variables.DatosInsumoTablaSustitucion[llavelineaNegocio][0].Substring(8).Trim();
                    }
                    #endregion

                    if (identificadorCanal == "02T112" || identificadorCanal == "02T222" || identificadorCanal == "02T309" || identificadorCanal == "02T409" ||
                       identificadorCanal == "02T576" || identificadorCanal == "02T801" || identificadorCanal == "02T942")
                    {
                        if (dicValores.Keys.Contains(llavePrincipal))
                        {
                            dicValores[llavePrincipal].Add(lineaActual.Substring(34, 14));
                            dicValores["SUBTOTAL"].Add("0");
                            dicValores["IVA"].Add(lineaActual.Substring(34, 14));
                            dicValores["IMPUESTOS"].Add("0");
                            dicValores["TOTAL"].Add(lineaActual.Substring(34, 14));
                        }
                    }
                    else
                    {
                        if ((Convert.ToDouble(lineaActual.Substring(6, 14)) != 0 || identificadorCanal == "02T317"))
                        {
                            lisSUBTOTAL.Add(lineaActual.Substring(6, 14));

                            recargo = Convert.ToDecimal($"{lineaActual.Substring(20, 12)}.{lineaActual.Substring(32, 2)}");
                            iva = Convert.ToDecimal(lineaActual.Substring(34, 14));

                            if (recargo != 0 && iva != 0)
                            {
                                ivaRecargo = recargo * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeIva"));
                                //ivaRecargo = Decimal.Round(ivaRecargo, 2);
                                ivaFormateado = ivaRecargo.ToString().Split('.')[0] + ivaRecargo.ToString().Split('.')[1].Substring(0, 2);

                                if (ivaRecargo.ToString().Contains("."))
                                {
                                    if (impuestoProducto == 0)
                                    {
                                        impuestoProducto = 0.00m;
                                    }
                                    ivaFormateado = ivaRecargo.ToString().Split('.')[0] + impuestoProducto.ToString().Split('.')[1].Substring(0, 2);
                                }
                                else
                                {
                                    ivaFormateado = ivaRecargo.ToString().Replace(",", "").Replace(".", "");
                                }

                                dicValores[llavePrincipal].Add(lineaActual.Substring(34, 14));
                                dicValores["SUBTOTAL"].Add("0");
                                dicValores["IVA"].Add("-" + ivaFormateado);
                                dicValores["IMPUESTOS"].Add("0");
                                dicValores["TOTAL"].Add("-" + ivaFormateado);
                            }

                            if (identificadorCanal == "02T582" || identificadorCanal == "02T507" || identificadorCanal == "02T510" || identificadorCanal == "02T511" ||
                                identificadorCanal == "02T517" || identificadorCanal == "02T502" || identificadorCanal == "02T504" || identificadorCanal == "02T505" || identificadorCanal == "02T118")
                            {
                                string sumaSubtotal = Helpers.SumarCampos(lisSUBTOTAL, "D").Replace("$", "").Replace(".", "").Replace(",", "").Trim();

                                var entero = sumaSubtotal.Substring(0, sumaSubtotal.Length - 2);
                                var decima = sumaSubtotal.Substring(sumaSubtotal.Length - 2, 2);

                                impuestoProducto = Convert.ToDecimal($"{entero}.{decima}");
                                impuestoProducto = impuestoProducto * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeImpuestoConsumo"));

                                if (impuestoProducto.ToString().Contains("."))
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Split('.')[0] + impuestoProducto.ToString().Split('.')[1].Substring(0, 2);
                                }
                                else
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Replace(",", "").Replace(".", "");
                                }

                                dicValores[llavePrincipal].Add("-" + impuestoFormateado);
                                dicValores["SUBTOTAL"].Add("0");
                                dicValores["IVA"].Add("-" + impuestoFormateado);
                                dicValores["IMPUESTOS"].Add("0");
                                dicValores["TOTAL"].Add("-" + impuestoFormateado);
                            }
                        }
                        else if (Convert.ToDouble(lineaActual.Substring(20, 14)) != 0)
                        {
                            dicValores[llavePrincipal].Add("-" + lineaActual.Substring(34, 14));
                            dicValores["SUBTOTAL"].Add("0");
                            dicValores["IVA"].Add("-" + lineaActual.Substring(34, 14));
                            dicValores["IMPUESTOS"].Add("0");
                            dicValores["TOTAL"].Add("-" + lineaActual.Substring(34, 14));

                            if (identificadorCanal == "02T582" || identificadorCanal == "02T507" || identificadorCanal == "02T510" || identificadorCanal == "02T511" ||
                                identificadorCanal == "02T517" || identificadorCanal == "02T502" || identificadorCanal == "02T504" || identificadorCanal == "02T505" || identificadorCanal == "02T118")
                            {
                                impuestoProducto = Convert.ToDecimal($"{lineaActual.Substring(34, 12)}.{lineaActual.Substring(44, 2)}");
                                impuestoProducto = impuestoProducto * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeImpuestoConsumo"));
                                //impuestoProducto = Decimal.Round(impuestoProducto);

                                if (impuestoProducto.ToString().Contains("."))
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Split('.')[0] + impuestoProducto.ToString().Split('.')[1].Substring(0, 2);
                                }
                                else
                                {
                                    impuestoFormateado = impuestoProducto.ToString();
                                }

                                dicValores[llavePrincipal].Add("-" + impuestoFormateado);
                                dicValores["SUBTOTAL"].Add("0");
                                dicValores["IVA"].Add("-" + impuestoFormateado);
                                dicValores["IMPUESTOS"].Add("0");
                                dicValores["TOTAL"].Add("-" + impuestoFormateado);
                            }
                        }
                    }
                }

                // Armar canal

                List<string> sumarCamposAux = new List<string>();

                resultado = "1CIV|";
                resultado += concepto + "|";
                resultado += Helpers.SumarCampos(dicValores["1"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["3"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["2"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["9"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["4"], "D") + "|";

                // Suma Conceptos 6 - 8
                sumarCamposAux.AddRange(dicValores["6"]);
                sumarCamposAux.AddRange(dicValores["8"]);
                resultado += Helpers.SumarCampos(sumarCamposAux, "D") + "|";
                sumarCamposAux.Clear();

                // Suma Conceptos 5 - 7
                sumarCamposAux.AddRange(dicValores["5"]);
                sumarCamposAux.AddRange(dicValores["7"]);
                resultado += Helpers.SumarCampos(sumarCamposAux, "D") + "|";
                sumarCamposAux.Clear();

                resultado += Helpers.SumarCampos(dicValores["SUBTOTAL"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["IVA"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["IMPUESTOS"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["TOTAL"], "D") + "| ";
            }

            return Helpers.ValidarPipePipe(resultado).Replace("$ 0.00", "-");
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
            List<string> lisCamposSumar = new List<string>();
            Dictionary<string, List<string>> dicValores = new Dictionary<string, List<string>>();
            string llavePrincipal = string.Empty;
            string concepto = "SUBTOTAL";
            string ivaFormateado = String.Empty;
            string identificadorCanal = string.Empty;
            decimal recargo = 0;
            decimal ivaRecargo = 0;
            decimal impuestoProducto = 0;
            string impuestoFormateado = String.Empty;
            decimal iva = 0;

            string llavelineaNegocio = string.Empty;
            string lineaNegocio = string.Empty;

            // Inicializa diccionario Con Valores
            dicValores.Add("1", new List<string>());    // Telefonia Local
            dicValores.Add("2", new List<string>());    // Larga Distancia
            dicValores.Add("3", new List<string>());    // Internet y Datos
            dicValores.Add("4", new List<string>());    // TV
            dicValores.Add("5", new List<string>());    // Otros Operadores
            dicValores.Add("6", new List<string>());    // Alianzas
            dicValores.Add("7", new List<string>());    // Otros Operadores
            dicValores.Add("8", new List<string>());    // Alianzas
            dicValores.Add("9", new List<string>());    // Movilidad
            dicValores.Add("SUBTOTAL", new List<string>());
            dicValores.Add("IVA", new List<string>());
            dicValores.Add("IMPUESTOS", new List<string>());
            dicValores.Add("TOTAL", new List<string>());

            List<string> lisSUBTOTAL = new List<string>();

            #region Busqueda            

            var result02TX = from busqueda in datosOriginales
                             where busqueda.Length > 4 && (busqueda.Substring(0, 4).Equals("02T1") ||
                                                           busqueda.Substring(0, 4).Equals("02T2") ||
                                                           busqueda.Substring(0, 4).Equals("02T3") ||
                                                           busqueda.Substring(0, 4).Equals("02T4") ||
                                                           busqueda.Substring(0, 4).Equals("02T5") ||
                                                           busqueda.Substring(0, 4).Equals("02T6") ||
                                                           busqueda.Substring(0, 4).Equals("02T7") ||
                                                           busqueda.Substring(0, 4).Equals("02T8") ||
                                                           busqueda.Substring(0, 4).Equals("02T9"))
                             select busqueda;
            #endregion

            if (result02TX.Any())
            {
                foreach (var lineaActual in result02TX)
                {
                    llavePrincipal = lineaActual.Substring(3, 1);
                    identificadorCanal = lineaActual.Substring(0, 6);

                    #region Toma Linea Negocio
                    llavelineaNegocio = lineaActual.Substring(3, 1);

                    if (llavelineaNegocio == "0")
                    {
                        llavelineaNegocio = "1";
                    }

                    llavelineaNegocio = "FACLIN" + llavelineaNegocio;

                    if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llavelineaNegocio))
                    {
                        lineaNegocio = Variables.Variables.DatosInsumoTablaSustitucion[llavelineaNegocio][0].Substring(8).Trim();
                    }
                    #endregion

                    if (identificadorCanal == "02T112" || identificadorCanal == "02T222" || identificadorCanal == "02T309" || identificadorCanal == "02T409" ||
                       identificadorCanal == "02T576" || identificadorCanal == "02T801" || identificadorCanal == "02T942")
                    {
                        if (dicValores.Keys.Contains(llavePrincipal))
                        {
                            dicValores[llavePrincipal].Add(lineaActual.Substring(6, 14));
                            dicValores["SUBTOTAL"].Add(lineaActual.Substring(6, 14));
                            dicValores["IVA"].Add("0");
                            dicValores["IMPUESTOS"].Add("0");
                            dicValores["TOTAL"].Add(lineaActual.Substring(6, 14));
                        }
                    }
                }

                // Armar canal

                List<string> sumarCamposAux = new List<string>();

                resultado = "1CST|";
                resultado += concepto + "|";
                resultado += Helpers.SumarCampos(dicValores["1"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["3"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["2"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["9"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["4"], "D") + "|";

                // Suma Conceptos 6 - 8
                sumarCamposAux.AddRange(dicValores["6"]);
                sumarCamposAux.AddRange(dicValores["8"]);
                resultado += Helpers.SumarCampos(sumarCamposAux, "D") + "|";
                sumarCamposAux.Clear();

                // Suma Conceptos 5 - 7
                sumarCamposAux.AddRange(dicValores["5"]);
                sumarCamposAux.AddRange(dicValores["7"]);
                resultado += Helpers.SumarCampos(sumarCamposAux, "D") + "|";
                sumarCamposAux.Clear();

                resultado += Helpers.SumarCampos(dicValores["SUBTOTAL"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["IVA"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["IMPUESTOS"], "D") + "|";
                resultado += Helpers.SumarCampos(dicValores["TOTAL"], "D") + "| ";
            }

            return Helpers.ValidarPipePipe(resultado).Replace("$ 0.00", "-");
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
            string resultadoTemp = string.Empty;
            List<string> lista02T = new List<string>();

            #region Busqueda 11C y toma los cruces de DatosInsumoConfiguracionLLavesDoc1

            var result11C = from busqueda in datosOriginales
                            where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C")

                            select busqueda;

            if (result11C.Any())
            {
                string llaveDoc = string.Empty;
                string valorDoc = string.Empty;
                string valorDoc2 = string.Empty;

                foreach (var lineaActual11C in result11C)
                {
                    llaveDoc = lineaActual11C.Substring(6, 10);

                    if (Variables.Variables.DatosInsumoConfiguracionLLavesDoc1.ContainsKey(llaveDoc))
                    {
                        valorDoc = Variables.Variables.DatosInsumoConfiguracionLLavesDoc1[llaveDoc].ToString().Split('|')[13];
                        valorDoc2 = Variables.Variables.DatosInsumoConfiguracionLLavesDoc1[llaveDoc].ToString().Split('|')[15];

                        if (!lista02T.Contains(valorDoc))
                            lista02T.Add(valorDoc);

                        if (!lista02T.Contains(valorDoc2))
                            lista02T.Add(valorDoc2);
                    }
                }
            }

            #endregion


            List<string> lisCamposSumar = new List<string>();
            Dictionary<string, List<string>> dicValores = new Dictionary<string, List<string>>();
            string llavePrincipal = string.Empty;
            string llaveProducto = string.Empty;
            string concepto = "IVA";
            string ivaFormateado = String.Empty;
            string identificadorCanal = string.Empty;
            decimal recargo = 0;
            decimal ivaRecargo = 0;
            decimal impuestoProducto = 0;
            string impuestoFormateado = String.Empty;
            decimal iva = 0;

            string llavelineaNegocio = string.Empty;
            string lineaNegocio = string.Empty;

            // Inicializa diccionario Con Valores
            dicValores.Add("1", new List<string>());    // Telefonia Local
            dicValores.Add("2", new List<string>());    // Larga Distancia
            dicValores.Add("3", new List<string>());    // Internet y Datos
            dicValores.Add("4", new List<string>());    // TV
            dicValores.Add("5", new List<string>());    // Otros Operadores
            dicValores.Add("6", new List<string>());    // Alianzas
            dicValores.Add("7", new List<string>());    // Otros Operadores
            dicValores.Add("8", new List<string>());    // Alianzas
            dicValores.Add("9", new List<string>());    // Movilidad
            dicValores.Add("SUBTOTAL", new List<string>());
            dicValores.Add("IVA", new List<string>());
            dicValores.Add("IMPUESTOS", new List<string>());
            dicValores.Add("TOTAL", new List<string>());



            var result02T = from busqueda in datosOriginales
                            where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("02T")
                            select busqueda;

            //CODT
            if (result02T.Any())
            {
                foreach (var lineaResul02T in result02T)
                {
                    llavePrincipal = lineaResul02T.Substring(0, 6);

                    if (lista02T.Contains(llavePrincipal))
                    {
                        llaveProducto = lineaResul02T.Substring(3, 1);

                        #region Toma Linea Negocio                        

                        if (llaveProducto == "0")
                        {
                            llaveProducto = "1";
                        }

                        llavelineaNegocio = "FACLIN" + llaveProducto;

                        if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llavelineaNegocio))
                        {
                            lineaNegocio = Variables.Variables.DatosInsumoTablaSustitucion[llavelineaNegocio][0].Substring(8).Trim();
                        }
                        #endregion

                        // se toma el concepto

                        if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey("CODT" + llavePrincipal))
                        {
                            concepto = Variables.Variables.DatosInsumoTablaSustitucion["CODT" + llavePrincipal][0].Substring(11).Trim();
                        }

                        //En el caso de que toca reemplazar labels en larga distancia
                        if (llavePrincipal == "02T204" || llavePrincipal == "02T207")
                            concepto = "L.D. Nacional";

                        if (dicValores.Keys.Contains(llaveProducto))
                        {
                            dicValores[llaveProducto].Add(lineaResul02T.Substring(6, 14));
                            dicValores["SUBTOTAL"].Add(lineaResul02T.Substring(6, 14));

                            // Otros operadores
                            if (llaveProducto == "5" || llaveProducto == "7")
                            {
                                impuestoProducto = Convert.ToDecimal($"{lineaResul02T.Substring(6, 12)}.{lineaResul02T.Substring(18, 2)}");
                                impuestoProducto = impuestoProducto * Convert.ToDecimal(Utilidades.LeerAppConfig("porcentajeImpuestoConsumo"));

                                if (impuestoProducto.ToString().Contains("."))
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Split('.')[0] + impuestoProducto.ToString().Split('.')[1].Substring(0, 2);
                                }
                                else
                                {
                                    impuestoFormateado = impuestoProducto.ToString().Replace(",", "").Replace(".", "");
                                }

                                dicValores["IVA"].Add("-" + impuestoFormateado);
                                dicValores["IMPUESTOS"].Add(impuestoFormateado);
                            }


                            dicValores["IVA"].Add(lineaResul02T.Substring(34, 14));
                            dicValores["IMPUESTOS"].Add(lineaResul02T.Substring(118, 14));

                            dicValores["TOTAL"].Add(lineaResul02T.Substring(6, 14));
                            dicValores["TOTAL"].Add(lineaResul02T.Substring(34, 14));
                            dicValores["TOTAL"].Add(lineaResul02T.Substring(118, 14));
                        }


                        resultadoTemp = "1CCD|";
                        resultadoTemp += concepto + "|";
                        resultadoTemp += Helpers.SumarCampos(dicValores["1"], "D") + "|";
                        resultadoTemp += Helpers.SumarCampos(dicValores["3"], "D") + "|";
                        resultadoTemp += Helpers.SumarCampos(dicValores["2"], "D") + "|";
                        resultadoTemp += Helpers.SumarCampos(dicValores["9"], "D") + "|";
                        resultadoTemp += Helpers.SumarCampos(dicValores["4"], "D") + "|";

                        List<string> sumarCamposAux = new List<string>();

                        // Suma Conceptos 6 - 8
                        sumarCamposAux.AddRange(dicValores["6"]);
                        sumarCamposAux.AddRange(dicValores["8"]);
                        resultadoTemp += Helpers.SumarCampos(sumarCamposAux, "D") + "|";
                        sumarCamposAux.Clear();

                        // Suma Conceptos 5 - 7
                        sumarCamposAux.AddRange(dicValores["5"]);
                        sumarCamposAux.AddRange(dicValores["7"]);
                        resultadoTemp += Helpers.SumarCampos(sumarCamposAux, "D") + "|";
                        sumarCamposAux.Clear();

                        resultadoTemp += Helpers.SumarCampos(dicValores["SUBTOTAL"], "D") + "|";
                        resultadoTemp += Helpers.SumarCampos(dicValores["IVA"], "D") + "|";
                        resultadoTemp += Helpers.SumarCampos(dicValores["IMPUESTOS"], "D") + "|";
                        resultadoTemp += Helpers.SumarCampos(dicValores["TOTAL"], "D") + "| ";

                        resultado.Add(Helpers.ValidarPipePipe(resultadoTemp).Replace("$ 0,00", "-"));

                        // Inicializa diccionario Con Valores
                        dicValores["1"] = new List<string>();    // Telefonia Local
                        dicValores["2"] = new List<string>();    // Larga Distancia
                        dicValores["3"] = new List<string>();    // Internet y Datos
                        dicValores["4"] = new List<string>();    // TV
                        dicValores["5"] = new List<string>();    // Otros Operadores
                        dicValores["6"] = new List<string>();    // Alianzas
                        dicValores["7"] = new List<string>();    // Otros Operadores
                        dicValores["8"] = new List<string>();    // Alianzas
                        dicValores["9"] = new List<string>();    // Movilidad
                        dicValores["SUBTOTAL"] = new List<string>();
                        dicValores["IVA"] = new List<string>();
                        dicValores["IMPUESTOS"] = new List<string>();
                        dicValores["TOTAL"] = new List<string>();
                    }
                }
            }

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

            #region Variables Proceso
            List<string> resultado = new List<string>();
            List<string> sumaValoresBase = new List<string>();
            List<string> sumaValoresIva = new List<string>();
            List<string> sumaValoresSubsidio = new List<string>();
            List<string> sumaValoresTotal = new List<string>();
            Dictionary<string, List<string>> paquetesInformacion = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> llavesLineasNegocio = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> llavesDatosEEE = new Dictionary<string, List<string>>();
            bool banderaPaquete = false;
            string cuentaConexion = string.Empty;
            string busquedaPlanMinutos = string.Empty;
            string planMinutos = string.Empty;
            string llaveCruce = string.Empty;
            string lineaFacturaNegocio = string.Empty;
            string llaveBusquedaDescripcion = string.Empty;
            string descripcionTitulo = string.Empty;
            string llaveBusquedaNit = string.Empty;
            string nit = string.Empty;
            string descripcionSubtitulo = string.Empty;
            string lineaNegocio = string.Empty;
            string @base = string.Empty;
            string recargoMora = string.Empty;
            string iva = string.Empty;
            string impuesto = "00";
            string restaImpuesto = "-00";
            string canalTotales = string.Empty;
            #endregion

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

            if (paquetesInformacion.Any())
            {
                #region Logica
                foreach (var lineaDetalle in paquetesInformacion)
                {
                    #region Logica por paquete

                    var lineasNegocioFactura = from busqueda in lineaDetalle.Value
                                               where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C") &&
                                               !string.IsNullOrEmpty(busqueda.Substring(44, 14).Trim().TrimStart('0'))
                                               select busqueda;

                    if (lineasNegocioFactura.Any())
                    {
                        llavesLineasNegocio = new Dictionary<string, List<string>>();

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

                            llaveCruce = $"VMIN{busquedaPlanMinutos}";

                            string valorminuto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(14).Trim() ?? string.Empty;

                            if (!string.IsNullOrEmpty(valorminuto))
                            {
                                planMinutos += $" - Valor Minuto Adicional {Helpers.FormatearCampos(TiposFormateo.Decimal05, valorminuto.Substring(0, 10))}";
                            }
                        }

                        #endregion

                        resultado.Add(Helpers.ValidarPipePipe($"ADN1|{lineaDetalle.Key}|{GetTipo(lineaDetalle.Key)}|{linea040000.FirstOrDefault().Substring(76, 30).Trim()}|" +
                            $"{planMinutos}|{linea090000.FirstOrDefault().Substring(26, 27).Trim()}| | "));

                        #region ObtenerLineasNegocio

                        foreach (var linea in lineasNegocioFactura)
                        {
                            string llave = linea.Substring(3, 1);

                            if (llavesLineasNegocio.ContainsKey(llave))
                            {
                                llavesLineasNegocio[llave].Add(linea);
                            }
                            else
                            {
                                llavesLineasNegocio.Add(llave, new List<string> { linea });
                            }
                        }

                        foreach (var lineaNegocioInfo in llavesLineasNegocio)
                        {
                            if (lineaNegocioInfo.Key == "5")
                            {
                                continue;
                            }

                            #region Armar Canal 1DDA
                            if (lineaNegocioInfo.Key == "1")
                            {
                                canalTotales = "06T112";
                            }
                            else if (lineaNegocioInfo.Key == "2")
                            {
                                canalTotales = "06T222";
                            }
                            else if (lineaNegocioInfo.Key == "3")
                            {
                                canalTotales = "06T309";
                            }
                            else if (lineaNegocioInfo.Key == "4")
                            {
                                canalTotales = "06T411";
                            }
                            else if (lineaNegocioInfo.Key == "6")
                            {
                                canalTotales = "06T660";
                            }
                            else if (lineaNegocioInfo.Key == "8")
                            {
                                canalTotales = "06T801";
                            }
                            else if (lineaNegocioInfo.Key == "9")
                            {
                                canalTotales = "06T942";
                            }
                            else
                            {
                                //falta configurar
                            }

                            var lineaTotales = from busqueda in lineaDetalle.Value
                                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals(canalTotales)
                                               select busqueda;

                            if (!lineaTotales.Any())
                            {
                                if (lineaNegocioInfo.Key == "8")
                                {
                                    canalTotales = "06T850";

                                    lineaTotales = from busqueda in lineaDetalle.Value
                                                   where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals(canalTotales)
                                                   select busqueda;
                                }
                            }

                            llaveCruce = $"FACLIN{lineaNegocioInfo.Key}";

                            string DescripcionLineaNegocio = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(8).Trim() ?? string.Empty;

                            if (lineaNegocioInfo.Key == "8")
                            {
                                DescripcionLineaNegocio = "Alianzas";
                            }

                            if (!string.IsNullOrEmpty(lineaTotales.FirstOrDefault().Substring(20, 14).Trim().TrimStart('0')))
                            {
                                recargoMora = lineaTotales.FirstOrDefault().Substring(20, 14).Trim().TrimStart('0');
                            }
                            else
                            {
                                recargoMora = string.Empty;
                            }

                            @base = lineaTotales.FirstOrDefault().Substring(6, 14).Trim().TrimStart('0');
                            iva = lineaTotales.FirstOrDefault().Substring(34, 14).Trim().TrimStart('0');

                            resultado.Add(Helpers.ValidarPipePipe($"1DDA|{lineaDetalle.Key}|{DescripcionLineaNegocio}|" +
                                $"{Helpers.SumarCampos(new List<string> { @base, recargoMora }, "D")}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, iva)}|" +
                                $"{Helpers.SumarCampos(new List<string> { @base, iva, recargoMora }, "D")}| "));
                            #endregion

                            #region Armar Canal 1DDD

                            IEnumerable<IGrouping<string, string>> lineadetalles = null;

                            if (lineaNegocioInfo.Key == "1" || lineaNegocioInfo.Key == "3" || lineaNegocioInfo.Key == "8")
                            {
                                lineadetalles = from busqueda in lineaNegocioInfo.Value
                                                where !string.IsNullOrEmpty(busqueda.Substring(44, 14).Trim().TrimStart('0'))
                                                group busqueda by busqueda.Substring(274, 7).Trim() into busqueda
                                                select busqueda;
                            }
                            else if (lineaNegocioInfo.Key == "2" || lineaNegocioInfo.Key == "6" || lineaNegocioInfo.Key == "9" || lineaNegocioInfo.Key == "4")
                            {
                                lineadetalles = from busqueda in lineaNegocioInfo.Value
                                                where !string.IsNullOrEmpty(busqueda.Substring(44, 14).Trim().TrimStart('0'))
                                                group busqueda by busqueda.Substring(0, 16).Trim() into busqueda
                                                select busqueda;
                            }
                            else
                            {
                                //FALTA CONFIGURAR
                            }

                            foreach (var detalleAgrupado in lineadetalles.Select(x => x))
                            {
                                sumaValoresBase.Clear();
                                sumaValoresIva.Clear();
                                sumaValoresSubsidio.Clear();
                                sumaValoresTotal.Clear();

                                foreach (var lineadetalle in detalleAgrupado)
                                {
                                    sumaValoresBase.Add(lineadetalle.Substring(16, 14).Trim().TrimStart('0'));
                                    sumaValoresIva.Add(lineadetalle.Substring(44, 14).Trim().TrimStart('0'));
                                    sumaValoresSubsidio.Add(lineadetalle.Substring(30, 14).Trim().TrimStart('0'));
                                }

                                sumaValoresTotal.AddRange(sumaValoresSubsidio);
                                sumaValoresTotal.AddRange(sumaValoresBase);

                                llaveCruce = $"CODF{detalleAgrupado.FirstOrDefault().Substring(6, 10)}";

                                string DescripcionCodigoFactura = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(15).Trim() ?? string.Empty;

                                resultado.Add(Helpers.ValidarPipePipe($"1DDD|{lineaDetalle.Key}|{DescripcionCodigoFactura}|" +
                                $"{Helpers.SumarCampos(sumaValoresBase, "D")}|{Helpers.SumarCampos(sumaValoresIva, "D")}|" +
                                $"{Helpers.SumarCampos(sumaValoresTotal, "D")}|{Helpers.SumarCampos(sumaValoresSubsidio, "D")}| | "));
                            }
                            #endregion
                        }
                        #endregion    
                    }
                    else
                    {
                        //No Aplica Hasta el momento
                    }


                    #region Datos paquete EEE
                    var lineas11CAnexos = from busqueda in lineaDetalle.Value
                                          where busqueda.Length > 4 &&
                                          busqueda.Substring(0, 4).Equals("11C1") ||
                                          busqueda.Substring(0, 4).Equals("11C2") ||
                                          busqueda.Substring(0, 4).Equals("11C3") ||
                                          busqueda.Substring(0, 4).Equals("11C5") ||
                                          busqueda.Substring(0, 4).Equals("11C6") ||
                                          busqueda.Substring(0, 4).Equals("11C9")
                                          where !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                                          select busqueda;

                    var lineas12MAnexos = from busqueda in lineaDetalle.Value
                                          where busqueda.Length > 4 &&
                                          busqueda.Substring(0, 4).Equals("12M1") ||
                                          busqueda.Substring(0, 4).Equals("12M2") ||
                                          busqueda.Substring(0, 4).Equals("12M3") ||
                                          busqueda.Substring(0, 4).Equals("12M5") ||
                                          busqueda.Substring(0, 4).Equals("12M6") ||
                                          busqueda.Substring(0, 4).Equals("12M9")
                                          select busqueda;

                    var linea30004 = from busqueda in datosOriginales
                                     where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("30004")
                                     select busqueda;
                    #endregion

                    if (lineas11CAnexos.Any() && lineas12MAnexos.Any())
                    {
                        llavesDatosEEE = new Dictionary<string, List<string>>();

                        #region Ordenar Informacion
                        foreach (var linea12M in lineas12MAnexos)
                        {
                            string llave = linea12M.Substring(0, 6);

                            if (llavesDatosEEE.ContainsKey(llave))
                            {
                                llavesDatosEEE[llave].Add(linea12M);
                            }
                            else
                            {
                                var buscarEquivalente = from busqueda in lineas11CAnexos
                                                        where busqueda.Substring(0, 6) == $"11C{llave.Substring(3, 3)}"
                                                        select busqueda;

                                llavesDatosEEE.Add(llave, new List<string> { buscarEquivalente.FirstOrDefault(), linea12M });
                            }
                        }
                        #endregion

                        foreach (var lineaDatos in llavesDatosEEE)
                        {
                            #region Datos 1EE1
                            impuesto = "00";
                            restaImpuesto = "-00";

                            if (lineaDatos.Value.ElementAt(0) == null)
                            {
                                //No existe el canal equivalente del 12M 
                                continue;
                            }

                            llaveCruce = lineaDatos.Value.FirstOrDefault().Substring(6, 10); //Se obtine del primer elemento ya que es el 11C

                            if (Variables.Variables.DatosInsumoConfiguracionLLavesDoc1.ContainsKey(llaveCruce))
                            {
                                llaveBusquedaDescripcion = $"CODT{Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoConfiguracionLLavesDoc1, llaveCruce).Split('|').ElementAt(13)}";
                            }
                            else
                            {
                                llaveBusquedaDescripcion = string.Empty;
                            }

                            descripcionTitulo = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveBusquedaDescripcion).FirstOrDefault()?.Substring(11).Trim() ?? "";

                            if (Variables.Variables.DatosInsumoConfiguracionLLavesDoc1.ContainsKey(llaveCruce))
                            {
                                llaveBusquedaNit = $"OPER{Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoConfiguracionLLavesDoc1, llaveCruce).Split('|').ElementAt(8)}";
                            }
                            else
                            {
                                llaveBusquedaNit = string.Empty;
                            }

                            nit = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveBusquedaNit).FirstOrDefault()?.Substring(11).Trim() ?? "";

                            if (!string.IsNullOrEmpty(nit))
                            {
                                lineaNegocio = $"{descripcionTitulo} NIT: {nit}";
                            }
                            else
                            {
                                lineaNegocio = $"{descripcionTitulo}";
                            }

                            if (Variables.Variables.DatosInsumoConfiguracionLLavesDoc1.ContainsKey(llaveCruce))
                            {
                                canalTotales = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoConfiguracionLLavesDoc1, llaveCruce).Split('|').ElementAt(12);
                            }
                            else
                            {
                                canalTotales = string.Empty;
                            }

                            var busquedaCanalTotales = from busqueda in lineaDetalle.Value
                                                       where busqueda.Substring(0, 6) == canalTotales
                                                       select busqueda;

                            //Si se encuentra se toman los datos, si no se calculan de la linea directamente
                            if (busquedaCanalTotales.Any())
                            {
                                @base = busquedaCanalTotales.FirstOrDefault().Substring(6, 14).Trim().TrimStart('0');
                                iva = busquedaCanalTotales.FirstOrDefault().Substring(34, 14).Trim().TrimStart('0');

                                if (!string.IsNullOrEmpty(busquedaCanalTotales.FirstOrDefault().Substring(20, 14).Trim().TrimStart('0')) || busquedaCanalTotales.FirstOrDefault().Substring(3, 1) == "5")//Si tiene recargo se calcula el impueso al consumo
                                {
                                    impuesto = (Convert.ToDecimal(Helpers.FormatearCampos(TiposFormateo.Decimal03, busquedaCanalTotales.FirstOrDefault().Substring(6, 14).Trim().TrimStart('0'))) * (decimal)0.04).ToString("N2").Replace(".", "").Replace(",", "");
                                    restaImpuesto = $"-{impuesto.Replace(".", string.Empty)}";
                                }
                            }
                            else
                            {
                                @base = lineaDatos.Value.FirstOrDefault().Substring(16, 14).Trim().TrimStart('0');
                                iva = lineaDatos.Value.FirstOrDefault().Substring(44, 14).Trim().TrimStart('0');

                                if (lineaDatos.Value.FirstOrDefault().Substring(3, 1) == "1" || lineaDatos.Value.FirstOrDefault().Substring(3, 1) == "6")
                                {
                                    impuesto = (Convert.ToDecimal(Helpers.FormatearCampos(TiposFormateo.Decimal03, lineaDatos.Value.FirstOrDefault().Substring(114, 14).Trim().TrimStart('0'))) * 2).ToString().Replace(".", string.Empty);
                                    restaImpuesto = $"-{impuesto.Replace(".", string.Empty)}";
                                }
                            }

                            resultado.Add(Helpers.ValidarPipePipe($"1EE1|{lineaDetalle.Key}|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, lineaNegocio)}" +
                                $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|{Helpers.SumarCampos(new List<string> { iva, restaImpuesto }, "D")}|" +
                                $"{Helpers.SumarCampos(new List<string> { @base, iva, restaImpuesto, impuesto }, "D")}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, impuesto)}| "));
                            #endregion

                            #region Datos 1EE2
                            if (Variables.Variables.DatosInsumoConfiguracionLLavesDoc1.ContainsKey(llaveCruce))
                            {
                                llaveBusquedaDescripcion = $"CODT{Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoConfiguracionLLavesDoc1, llaveCruce).Split('|').ElementAt(7)}";
                            }
                            else
                            {
                                llaveBusquedaDescripcion = string.Empty;
                            }

                            descripcionSubtitulo = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveBusquedaDescripcion).FirstOrDefault()?.Substring(11).Trim() ?? "";

                            resultado.Add(Helpers.ValidarPipePipe($"1EE2|{lineaDetalle.Key}|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, descripcionSubtitulo)}|LD| "));
                            #endregion

                            #region Datos 1EE3
                            List<string> DatosDetallesEE3 = new List<string>(lineaDatos.Value);
                            DatosDetallesEE3.RemoveAt(0); //Se elimina el primer elemento ya que es el 11C y nos e necesita

                            foreach (var linea in DatosDetallesEE3)
                            {
                                resultado.Add(Helpers.ValidarPipePipe($"1EE3|{lineaDetalle.Key}|{Helpers.FormatearCampos(TiposFormateo.Fecha12, linea.Substring(6, 10))}|" +
                                    $"{Helpers.FormatearCampos(TiposFormateo.HoraMinutoSegundo, linea.Substring(14, 6))}|{linea.Substring(20, 10).Trim()}|" +
                                    $"{linea.Substring(33, 10).Trim()}|{linea.Substring(96, 11).Trim()}|{linea.Substring(66, 2).TrimStart('0')}:00|{Helpers.FormatearCampos(TiposFormateo.Decimal05, linea.Substring(47, 9)).Replace("$", "").Trim()}|" +
                                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, linea.Substring(56, 9)).Replace("$", "").Trim()}| "));
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                #endregion
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
        private IEnumerable<string> FormarPaqueteSERV(List<string> datosOriginales)
        {
            #region FormarPaqueteSERV

            #region Variables
            List<string> resultado = new List<string>();
            List<string> sumaValoresBase = new List<string>();
            List<string> sumaValoresIva = new List<string>();
            List<string> sumaValoresTotal = new List<string>();
            List<string> sumaValoresBase1GGB = new List<string>();
            List<string> sumaValoresIva1GGB = new List<string>();
            List<string> sumaValoresTotal1GGB = new List<string>();
            bool banderaPaquete = false;
            string cuentaConexion = string.Empty;
            Dictionary<string, List<string>> paquetesInformacion = new Dictionary<string, List<string>>();
            Dictionary<string, Dictionary<string, List<string>>> paquetesInformacionFinales = new Dictionary<string, Dictionary<string, List<string>>>();
            string llaveCruce = string.Empty;
            string descripcionProducto = string.Empty;
            string descripcionSubProducto = string.Empty;
            string direccionOrigen = string.Empty;
            string ciudadOrigen = string.Empty;
            string direccionDestino = string.Empty;
            string ciudadDestino = string.Empty;
            string @base = string.Empty;
            string iva = string.Empty;
            #endregion

            var lineas13M = from busqueda in datosOriginales
                            where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("13M")
                            select busqueda;

            var lineas11C = from busqueda in datosOriginales
                            where busqueda.Length > 4 && busqueda.Substring(0, 4).Equals("11C8") &&
                            !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                            select busqueda;

            if (lineas13M.Any() || lineas11C.Any())
            {
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

                foreach (var lineaPaquete in paquetesInformacion)
                {
                    foreach (var detalle in lineaPaquete.Value)
                    {
                        if (detalle.Substring(0, 3) == "13M")
                        {
                            string llave = $"{detalle.Substring(0, 6)}|{detalle.Substring(32, 10).Trim()}";

                            @base = detalle.Substring(42, 14).Trim().TrimStart('0');
                            iva = detalle.Substring(56, 14).Trim().TrimStart('0');

                            if (!string.IsNullOrEmpty(@base) && !string.IsNullOrEmpty(iva))
                            {
                                if (!paquetesInformacionFinales.ContainsKey(llave))
                                {
                                    paquetesInformacionFinales.Add(llave, new Dictionary<string, List<string>> { { lineaPaquete.Key, lineaPaquete.Value } });
                                }
                                else
                                {
                                    paquetesInformacionFinales[llave].Add(lineaPaquete.Key, lineaPaquete.Value);
                                }

                                sumaValoresBase1GGB.Add(detalle.Substring(42, 14).Trim().TrimStart('0'));
                                sumaValoresIva1GGB.Add(detalle.Substring(56, 14).Trim().TrimStart('0'));
                            }

                            break;
                        }
                    }
                }

                #endregion

                if (paquetesInformacionFinales.Any())
                {
                    sumaValoresTotal1GGB.AddRange(sumaValoresBase1GGB);
                    sumaValoresTotal1GGB.AddRange(sumaValoresIva1GGB);

                    foreach (var lineaDetallePaquete in paquetesInformacionFinales)
                    {
                        sumaValoresBase.Clear();
                        sumaValoresIva.Clear();
                        sumaValoresTotal.Clear();

                        #region Armar Canal SERV
                        llaveCruce = $"CODT{lineaDetallePaquete.Key.Split('|').ElementAt(0)}";

                        descripcionProducto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(11).Trim() ?? "";

                        llaveCruce = $"CODF{lineaDetallePaquete.Key.Split('|').ElementAt(1)}";

                        descripcionSubProducto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(14).Trim() ?? "";

                        resultado.Add(Helpers.ValidarPipePipe($"SERV|{descripcionProducto}|{descripcionSubProducto}|{lineaDetallePaquete.Value.Count}| "));

                        #endregion

                        foreach (var lineaDetalle in lineaDetallePaquete.Value)
                        {
                            #region Armar Canal 1GGG
                            lineas13M = from busqueda in lineaDetalle.Value
                                        where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("13M") &&
                                        !string.IsNullOrEmpty(busqueda.Substring(7, 16).Trim())
                                        select busqueda;

                            var linea040001 = from busqueda in lineaDetalle.Value
                                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040001")
                                              select busqueda;

                            if (linea040001.Any())
                            {
                                direccionOrigen = linea040001.FirstOrDefault().Substring(36, 39).Trim();

                                llaveCruce = $"DANC{linea040001.FirstOrDefault().Substring(75, 5).Trim()}";
                                ciudadOrigen = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(10).Trim() ?? "";

                                direccionDestino = linea040001.FirstOrDefault().Substring(81, 39).Trim();

                                llaveCruce = $"DANC{linea040001.FirstOrDefault().Substring(119, 5).Trim()}";
                                ciudadDestino = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(10).Trim() ?? "";
                            }

                            @base = lineas13M.FirstOrDefault().Substring(42, 14).Trim();
                            iva = lineas13M.FirstOrDefault().Substring(56, 14).Trim();

                            resultado.Add(Helpers.ValidarPipePipe($"1GGG|{descripcionProducto}|{descripcionSubProducto}|{lineaDetalle.Key}| |" +
                            $"{lineas13M.FirstOrDefault().Substring(112, 12).Replace(" ", "").Trim()}|{lineas13M.FirstOrDefault().Substring(6, 8).Trim()} - {lineas13M.FirstOrDefault().Substring(14, 8).Trim()}|" +
                            $"{direccionOrigen}|{ciudadOrigen}|{direccionDestino}|{ciudadDestino}| | |" +
                            $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|" +
                            $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, iva)}| "));

                            sumaValoresBase.Add(lineas13M.FirstOrDefault().Substring(42, 14).Trim().TrimStart('0'));
                            sumaValoresIva.Add(lineas13M.FirstOrDefault().Substring(56, 14).Trim().TrimStart('0'));
                            #endregion
                        }

                        #region Armar Canal 1GGA - 1GGB
                        sumaValoresTotal.AddRange(sumaValoresBase);
                        sumaValoresTotal.AddRange(sumaValoresIva);

                        resultado.Add(Helpers.ValidarPipePipe($"1GGA|Total {descripcionSubProducto}|{Helpers.SumarCampos(sumaValoresBase, "D")}|" +
                            $"{Helpers.SumarCampos(sumaValoresIva)}|{Helpers.SumarCampos(sumaValoresTotal)}| "));

                        resultado.Add(Helpers.ValidarPipePipe($"1GGB|Total {descripcionProducto}|{Helpers.SumarCampos(sumaValoresBase1GGB, "D")}|" +
                            $"{Helpers.SumarCampos(sumaValoresIva1GGB, "D")}|{Helpers.SumarCampos(sumaValoresTotal1GGB, "D")}| "));
                        #endregion
                    }
                }
            }

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
            List<string> listaValores = new List<string>();
            string lineaTemp1JJK = string.Empty;

            var result888888 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("888888")
                               select busqueda;


            if (result888888.Any())
            {
                foreach (var registroActual in result888888)
                {
                    listaValores.Add(registroActual.Substring(6, 20).Trim());
                }

                for (int i = 0; i < listaValores.Count; i++)
                {
                    if (i % 3 == 0 && i != 0)
                    {
                        resultado.Add(Helpers.ValidarPipePipe(lineaTemp1JJK));
                        lineaTemp1JJK = String.Empty;
                    }

                    if (i % 3 == 0 || i == 0)
                    {
                        lineaTemp1JJK = "1JJK|";
                    }

                    lineaTemp1JJK += listaValores[i] + "| ";
                }

                if (!string.IsNullOrEmpty(lineaTemp1JJK))
                {
                    resultado.Add(Helpers.ValidarPipePipe(lineaTemp1JJK));
                }
            }


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

            if (MesMora >= 4)
            {
                resultado = "1LLL|Estimado  Cliente: Su servicio se encuentra suspendido debido a que el sistema aún no registra su pago. Para que pueda continuar beneficiándose de nuestros servicios, le invitamos a efectuar el pago a la mayor brevedad únicamente en efectivo o cheque de gerencia. Si cancelá su factura anterior después de la fecha límite de pago y éste no fue aplicado en la presente factura, por favor acérquese a nuestro Centro de Servicios más cercano con el fin de expedirle un recibo con el valor real a pagar.| ";
            }

            return resultado;
            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal CONS
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanalCONS(List<string> datosOriginales)
        {
            #region MapeoCanalCONS
            List<string> LineasCONS = new List<string>();

            string Linea3000401 = string.Empty;
            string Linea3000402 = string.Empty;
            string Linea3000404 = string.Empty;

            var result3000401 = from busqueda in datosOriginales
                                where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000401")
                                select busqueda;

            if (result3000401.Any())
            {
                Linea3000401 = result3000401.FirstOrDefault();
                Int64 impuesto = Convert.ToInt64(Linea3000401.Substring(12, 20).Trim());
                if (impuesto > 0)
                {
                    Linea3000401 = $"CONS|IVA|{Linea3000401.Substring(8, 3).Trim()}|{Linea3000401.Substring(12, 20).Trim()}| ";
                }
                else
                {
                    Linea3000401 = string.Empty;
                }
            }

            var result3000402 = from busqueda in datosOriginales
                                where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000402")
                                select busqueda;

            if (result3000402.Any())
            {
                Linea3000402 = result3000402.FirstOrDefault();
                Int64 impuesto = Convert.ToInt64(Linea3000402.Substring(12, 20).Trim());
                if (impuesto > 0)
                {
                    Linea3000402 = $"CONS|ImpoConsumo|{Linea3000402.Substring(8, 3).Trim()}|{Linea3000402.Substring(12, 20).Trim()}| ";
                }
                else
                {
                    Linea3000402 = string.Empty;
                }
            }

            var result3000404 = from busqueda in datosOriginales
                                where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000404")
                                select busqueda;

            if (result3000404.Any())
            {
                Linea3000404 = result3000404.FirstOrDefault();
                Int64 impuesto = Convert.ToInt64(Linea3000404.Substring(12, 20).Trim());
                if (impuesto > 0)
                {
                    Linea3000404 = $"CONS|ImpoConsumo|{Linea3000404.Substring(8, 3).Trim()}|{Linea3000404.Substring(12, 20).Trim()}| ";
                }
                else
                {
                    Linea3000404 = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(Linea3000402))
            {
                LineasCONS.Add(Linea3000402);
            }

            if (!string.IsNullOrEmpty(Linea3000404))
            {
                LineasCONS.Add(Linea3000404);
            }

            if (!string.IsNullOrEmpty(Linea3000401))
            {
                LineasCONS.Add(Linea3000401);
            }


            return LineasCONS;

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanalCART(List<string> datosOriginales)
        {
            #region FormateoCanalCART
            string lineaTemCART = string.Empty;
            List<string> listaCART = new List<string>();

            if (Variables.Variables.DatosInsumoAsignacionCartas.ContainsKey(Cuenta))
            {
                foreach (var cartaActual in Variables.Variables.DatosInsumoAsignacionCartas[Cuenta])
                {
                    lineaTemCART = cartaActual.Replace(Cuenta + "|", "CART|").Trim();
                    listaCART.Add(Helpers.ValidarPipePipe(lineaTemCART));
                }
            }

            return listaCART;
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
            List<string> lista1CDP = new List<string>();
            List<string> camposClausula = new List<string>();

            List<string> clausulasPermanencia = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoClausulaPermanencia, $"{Cuenta}");

            if (clausulasPermanencia != null && clausulasPermanencia.Count > 0)
            {
                foreach (string clausula in clausulasPermanencia)
                {
                    camposClausula = clausula.Split('|').ToList();
                    camposClausula.RemoveAt(0);
                    lista1CDP.Add($"1CDP|{Helpers.ListaCamposToLinea(camposClausula, '|')}| ");
                }
            }

            return lista1CDP;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private List<string> FormateoCanal1FFF(List<string> datosOriginales)
        {
            #region FormateoCanal1FFF
            List<string> resultado = new List<string>();
            string total13M311 = string.Empty;
            string subtotal13M311 = string.Empty;
            string total113M319 = string.Empty;
            string subtotal113M319 = string.Empty;


            var linea13M311 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("13M311") && string.IsNullOrEmpty(busqueda.Substring(6, 16).Trim())
                              select busqueda;

            if (linea13M311.Any())
            {
                string llaveCruce = $"CODF{linea13M311.FirstOrDefault().Substring(32, 10).Trim()}";
                var concepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault().Substring(14).Trim();

                total13M311 = linea13M311.FirstOrDefault().Substring(42, 14).Trim();
                subtotal13M311 = linea13M311.FirstOrDefault().Substring(56, 14).Trim();

                resultado.Add(Helpers.ValidarPipePipe($"1FFF|{concepto}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, total13M311)}" +
                    $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, subtotal13M311)}|{Helpers.SumarCampos(new List<string> { total13M311, subtotal13M311 }, "D")}| | "));
            }

            var linea113M319 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("13M319") && string.IsNullOrEmpty(busqueda.Substring(6, 16).Trim())
                               select busqueda;

            if (linea113M319.Any())
            {
                string llaveCruce = $"CODF{linea113M319.FirstOrDefault().Substring(32, 10).Trim()}";
                var concepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault().Substring(14).Trim();

                total113M319 = linea113M319.FirstOrDefault().Substring(42, 14).Trim();
                subtotal113M319 = linea113M319.FirstOrDefault().Substring(56, 14).Trim();

                resultado.Add(Helpers.ValidarPipePipe($"1FFF|{concepto}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, total113M319)}" +
                    $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, subtotal113M319)}|{Helpers.SumarCampos(new List<string> { total113M319, subtotal113M319 }, "D")}| | "));
            }

            if (resultado.Count > 0 && resultado.Count % 2 != 0)
            {
                resultado.Add("1FFF| | | | | | ");
            }
            if (resultado.Count > 0)
            {
                resultado.Add(Helpers.ValidarPipePipe($"1FFA|Total|{Helpers.SumarCampos(new List<string> { total13M311, total113M319 }, "D")}" +
                    $"|{Helpers.SumarCampos(new List<string> { subtotal13M311, subtotal113M319 }, "D")}" +
                    $"|{Helpers.SumarCampos(new List<string> { total13M311, total113M319, subtotal13M311, subtotal113M319 }, "D")}| | "));
            }


            return resultado;
            #endregion
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private List<string> FormateoCanal1MMM(List<string> datosOriginales)
        {
            #region FormateoCanal1MMM
            List<string> listaTemp1MMM = new List<string>();
            string lineaTemp1MMM = string.Empty;

            var result02T001 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T001")
                               select busqueda;


            if (result02T001.Any())
            {
                if (Convert.ToDouble(result02T001.FirstOrDefault().Substring(6, 14).Trim()) > 0)
                {
                    lineaTemp1MMM = $"1MMM|tramites|{FechaExpedicion}|{Variables.RxGeneral.TextoTramites}";
                    listaTemp1MMM.Add(Helpers.ValidarPipePipe(lineaTemp1MMM));
                    lineaTemp1MMM = string.Empty;
                }
            }

            if (Variables.Variables.DatosInsumoTramites.ContainsKey(Cuenta))
            {
                foreach (var insumoActual in Variables.Variables.DatosInsumoTramites[Cuenta])
                {
                    lineaTemp1MMM = string.Empty;
                    lineaTemp1MMM = "1MMM|tramites|";
                    lineaTemp1MMM += insumoActual.Split('|')[1] + "|";
                    lineaTemp1MMM += insumoActual.Split('|')[2] + "|";
                    lineaTemp1MMM += insumoActual.Split('|')[3] + "| | ";
                    listaTemp1MMM.Add(Helpers.ValidarPipePipe(lineaTemp1MMM));
                }
            }
            return listaTemp1MMM;
            #endregion
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private List<string> FormateoCanal1III(List<string> datosOriginales)
        {
            #region FormateoCanal1III
            List<string> resultado = new List<string>();

            var linea20C = from busqueda in datosOriginales
                           where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("20C")
                           select busqueda;

            if (linea20C.Any())
            {
                List<string> listalinea20C = linea20C.ToList();

                string descripcion = string.Empty;
                string lineaNegocio = string.Empty;
                string[] camposFinanciacion;
                string[] camposPlanPago;
                Int64 valorFinanciado;
                Int64 saldoRestante;
                string cuota = string.Empty;
                string interes = string.Empty;

                foreach (var item in listalinea20C)
                {

                    descripcion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"CODF{item.Substring(6, 10).Trim()}")?.FirstOrDefault().Substring(14).Trim() ?? string.Empty;

                    camposFinanciacion = item.Substring(136, 100).Split('&');
                    camposPlanPago = camposFinanciacion[1].Split(',');

                    if (camposFinanciacion.Length > 1 && camposFinanciacion[0] == "F" && item.Substring(123, 1).Trim() != "P")
                    {
                        camposPlanPago = camposFinanciacion[4].Split(',');
                        valorFinanciado = Convert.ToInt64(camposFinanciacion[8].Replace(".", string.Empty));
                        saldoRestante = Convert.ToInt64(item.Substring(16, 14).Replace(".", string.Empty));
                        cuota = string.Empty;
                        interes = camposFinanciacion[7];
                    }
                    else if (camposFinanciacion.Length > 1)
                    {
                        valorFinanciado = Convert.ToInt64(camposFinanciacion[6].Replace(".", string.Empty));
                        saldoRestante = Convert.ToInt64(camposFinanciacion[7].Replace(".", string.Empty));
                        cuota = $"{camposFinanciacion[4].Trim()} De {camposFinanciacion[5].Trim()}";
                        interes = camposFinanciacion[2];
                    }
                    else
                    {
                        valorFinanciado = 0;
                        saldoRestante = 0;
                        cuota = "0";
                    }
                    string valorFinanciadoFormat = Helpers.FormatearCampos(TiposFormateo.Decimal05, valorFinanciado.ToString());
                    string saldoRestanteFormat = Helpers.FormatearCampos(TiposFormateo.Decimal05, saldoRestante.ToString());

                    if (valorFinanciado > 0)
                    {
                        resultado.Add($"1III|{descripcion}|{valorFinanciadoFormat}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, item.Substring(16, 14))}|{saldoRestanteFormat}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, item.Substring(44, 14))}|{cuota}|{interes}| ");
                    }

                }
            }

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
            #region Canal 1CUFE
            string LineaCUFE = string.Empty;
            string valor = string.Empty;

            LineaCUFE = "CUFE|";

            if (Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey(Cuenta))
            {
                valor = Variables.Variables.DatosInsumoETBFacturaElectronica[Cuenta];
            }

            if (string.IsNullOrEmpty(valor))
            {
                LineaCUFE = string.Empty;
            }
            else
            {
                LineaCUFE += valor + "| ";
            }

            return Helpers.ValidarPipePipe(LineaCUFE);
            #endregion
        }

        #endregion

        private ClientesEspeciales CargarClientesEspeciales(string pCuenta, string pCiclo)
        {
            ClientesEspeciales cE = new ClientesEspeciales();

            string pLineaInsumo = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoClientesEspecialesDatos, $"{pCuenta}-{pCiclo}") ?? string.Empty;

            if (!string.IsNullOrEmpty(pLineaInsumo.Trim()))
            {
                string[] camposInsumo = pLineaInsumo.Split('|');

                cE.Cuenta = camposInsumo[0].Trim();
                cE.Cliente = camposInsumo[1].Trim();
                cE.Ciclo = camposInsumo[2].Trim();
                cE.Estado = camposInsumo[3].Trim();
                cE.Anticipado = camposInsumo[4].Trim();
                cE.Doc1SaldoGracias = camposInsumo[5].Trim();
                cE.AjusteDecena = camposInsumo[6].Trim();
                cE.NuevoValorPagar = camposInsumo[7].Trim();
                cE.FechaPremuestra = camposInsumo[8].Trim();
                cE.FechaProceso = camposInsumo[9].Trim();
                cE.FechaExpedicion = camposInsumo[10].Trim();
                cE.ConsumoDesde = camposInsumo[11].Trim();
                cE.ConsumoHasta = camposInsumo[12].Trim();
                cE.FechaPago = camposInsumo[13].Trim();
                cE.FechaEmisionFactura = camposInsumo[14].Trim();
                cE.ObservacionesFactura = camposInsumo[15].Trim();
                cE.Estampilla = camposInsumo[16].Trim();
                cE.ReversionPago = camposInsumo[17].Trim();
            }

            return cE;

        }
    }

    /// <summary>
    /// Structura de ClientesEspeciales
    /// </summary>
    public struct ClientesEspeciales
    {
        public string Cuenta;
        public string Cliente;
        public string Ciclo;
        public string Estado;
        public string Anticipado;
        public string Doc1SaldoGracias;
        public string AjusteDecena;
        public string NuevoValorPagar;
        public string FechaPremuestra;
        public string FechaProceso;
        public string FechaExpedicion;
        public string ConsumoDesde;
        public string ConsumoHasta;
        public string FechaPago;
        public string FechaEmisionFactura;
        public string ObservacionesFactura;
        public string Estampilla;
        public string ReversionPago;
    }
}
