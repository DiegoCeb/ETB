﻿using App.ControlInsumos;
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
            FormatearPropiedadesExtracto();

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

            resultadoFormateoLinea = FormateoCanalCONS(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalCART(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
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
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));

                Ciclo = Linea010000.Substring(151, 4).Trim().TrimStart('0'); // Asignamos Ciclo a variable Global
                ListaCanal1AAA.Add(Ciclo);

                listaCortes.Clear();
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
                ListaCanal1AAA.Add((string.IsNullOrEmpty(Linea010000.Substring(218, 20).Trim()) ? " " : Linea010000.Substring(218, 20).Trim()));

                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECP{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);
                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECL{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);
                ListaCanal1AAA.Add(GetFechaExpedicion(Linea010000));

                ListaCanal1AAA.Add(GetNumeroReferencia(Linea010000.Substring(139, 12)));
                ListaCanal1AAA.AddRange(GetCodigosBarras(Linea010000.Substring(139, 12), Linea010000, datosOriginales)); //TODO: Verificar valor a pagar
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
                ListaCanal1AAA.Add(string.Empty); // TODO: Numero de Hojas
                ListaCanal1AAA.Add(GetNivelReclamacion());
                listaCortes.Clear();
                listaCortes.Add(new PosCortes(108, 5));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000)); // Codigo DANE
                CodigoDANE = Linea010000.Substring(108, 5);
                ListaCanal1AAA.Add(GetLogoCivico(datosOriginales));
                ListaCanal1AAA.Add(string.Empty); //TODO: Doble marca Transpromo Verificar
                ListaCanal1AAA.Add(GetMarcaCourier());
                ListaCanal1AAA.Add(GetCuentaVencidaAnticipada()); // TODO: Validar que sea el insumo correcto
                ListaCanal1AAA.Add(string.Empty); //TODO: Tipo Email Prevalidador Verificar
                ListaCanal1AAA.Add(string.Empty); //TODO: Verificar Insumo Parafiscales
                ListaCanal1AAA.AddRange(GetLocBar());
                ListaCanal1AAA.AddRange(GetQR(Linea010000.Substring(6, 50)));
                ListaCanal1AAA.Add(string.Empty); //CampoVacio
                ListaCanal1AAA.Add(string.Empty); // Fecha Corte No Palica para Datos
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

            if (result.Any())
            {
                foreach (var item in result.ToList())
                {

                    if (Cuenta != item.Substring(6, 20).Trim() && item.Substring(6, 20).Trim().Length >= 8)
                    {
                        telefono = item.Substring(6, 20).Trim();
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
            string fechaExpedicionLinea010000 = pLinea010000.Substring(278, 17).Trim();


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
            string numReferencia = GetNumeroReferencia(pNumReferencia, false);
            List<decimal> valoresPago = GetValoresCodeBar(pDatosOriginales);
            string ValorPagarCB1 = valoresPago[0].ToString().Trim().PadLeft(12, '0');
            string ValorPagarCB2 = valoresPago[1].ToString().Trim().PadLeft(12, '0');

            string CodeBar1 = String.Empty;
            if (valoresPago[1] >= 0)
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

        private List<decimal> GetValoresCodeBar(List<string> datosOriginales)
        {
            #region MapeoCanal1BBB
            List<decimal> ValoresCodeBar = new List<decimal>();
            string llave = string.Empty;
            decimal SubTotal1BBB = 0;
            decimal ValorPagarMes = 0;
            decimal calculoAjusteDecena = decimal.Zero;

            #region Notas Crédito
            var result29000 = from busqueda in datosOriginales
                              where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("29000")
                              select busqueda;

            if (result29000.Any())
            {
                string linea29000 = result29000.FirstOrDefault() ?? string.Empty;

                if (!string.IsNullOrEmpty(linea29000))
                {
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
                                 comp4 == "02T0" ||
                                 comp4 == "02S0"
                                 select busqueda;

            if (resultDetalles.Any())
            {
                List<string> listaDetalles = resultDetalles.ToList();

                foreach (string detalle in listaDetalles)
                {
                    llave = detalle.Substring(0, 6).Trim();

                    if (llave == "02S000")
                    {
                        if (!string.IsNullOrEmpty(detalle.Substring(20, 14).Trim()) && Convert.ToInt64(detalle.Substring(20, 14)) != 0)
                        {
                            SubTotal1BBB += Convert.ToInt64(detalle.Substring(20, 14));
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0 /*&& trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X"*/)
                        {
                            if (string.IsNullOrEmpty(linea150001) || (!string.IsNullOrEmpty(linea150001) && linea150001.Substring(6, 1) != "N")) // Regla No sumar saldo anterior
                            {
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14));
                            }
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(34, 14).Trim()) && Convert.ToInt64(detalle.Substring(34, 14)) != 0 /*&& trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X"*/)
                        {
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
                            //if (trim($valores_temp["estampilla"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T016")
                        {
                            //if (trim($valores_temp["revercion_pago"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T002")
                        {
                            //if (trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T003")
                        {
                            //if (trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X") TODO: Validar Regla
                            //    return false;

                            calculoAjusteDecena += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            if (/*valores_temp["EXCLUSION_AJUSTE_DECENA"] && */ IsDatos)
                            {
                                SubTotal1BBB += calculoAjusteDecena;
                                ValorPagarMes += calculoAjusteDecena;
                            }
                            else
                            {
                                SubTotal1BBB += calculoAjusteDecena;
                            }
                        }
                        else
                        {
                            Int64 impuestoConsumo = 0;
                            if (IsDatos)
                            { impuestoConsumo = Convert.ToInt64(detalle.Substring(118, 14)); }

                            decimal sumatoria = Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14)) + impuestoConsumo;

                            if (llave == "02T020" || llave == "02T050")
                            { // Req 11052015 Campo Financiacion Mes
                                continue;
                            }

                            SubTotal1BBB += sumatoria;
                            ValorPagarMes += sumatoria;

                        }
                    }


                }

            }
            #endregion

            ValoresCodeBar.Add(SubTotal1BBB);
            ValoresCodeBar.Add(ValorPagarMes);


            return ValoresCodeBar;
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

        private string GetValorPagarMes(List<string> pDatosOriginales)
        {
            #region GetValorPagarMes
            List<decimal> valoresPago = GetValoresCodeBar(pDatosOriginales);
            string ValorPagar = Helpers.FormatearCampos(TiposFormateo.Decimal01, valoresPago[1].ToString()); 

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
                { tipoEmail = "dual"; }
                else
                { tipoEmail = "solo_email"; }
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
        private IEnumerable<string> FormateoCanal1BBB(List<string> datosOriginales)
        {
            #region MapeoCanal1BBB
            List<string> Lineas1BBB = new List<string>();
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

            #region Notas Crédito
            var result29000 = from busqueda in datosOriginales
                              where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("29000")
                              select busqueda;

            if (result29000.Any())
            {
                string linea29000 = result29000.FirstOrDefault() ?? string.Empty;

                if (!string.IsNullOrEmpty(linea29000))
                {
                    lineaNotasCredito = $"1BBB|Notas Crédito|{Helpers.FormatearCampos(TiposFormateo.Decimal01, linea29000.Substring(29, 20).TrimStart('0'))}| ";
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
                                 comp4 == "02T0" ||
                                 comp4 == "02S0"
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
                        if (!string.IsNullOrEmpty(detalle.Substring(20, 14).Trim()) && Convert.ToInt64(detalle.Substring(20, 14)) != 0)
                        {
                            Lineas1BBB.Add($"1BBB|Traslado de Saldos|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(20, 14).TrimStart('0'))}| ");
                            SubTotal1BBB += Convert.ToInt64(detalle.Substring(20, 14));
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0 /*&& trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X"*/)
                        {
                            if (string.IsNullOrEmpty(linea150001) || (!string.IsNullOrEmpty(linea150001) && linea150001.Substring(6, 1) != "N")) // Regla No sumar saldo anterior
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14));
                            }
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(34, 14).Trim()) && Convert.ToInt64(detalle.Substring(34, 14)) != 0 /*&& trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X"*/)
                        {
                            Lineas1BBB.Add($"1BBB|Ajuste De Pagos|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(34, 14).TrimStart('0'))}| ");
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
                            //if (trim($valores_temp["estampilla"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T016")
                        {
                            //if (trim($valores_temp["revercion_pago"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T002")
                        {
                            //if (trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T003")
                        {
                            //if (trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X") TODO: Validar Regla
                            //    return false;

                            calculoAjusteDecena += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            if (/*valores_temp["EXCLUSION_AJUSTE_DECENA"] && */ IsDatos)
                            {
                                SubTotal1BBB += calculoAjusteDecena;
                                ValorPagarMes += calculoAjusteDecena;
                            }
                            else
                            {
                                lineaAjusteDecena = $"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(6, 14).TrimStart('0'))}| ";
                                SubTotal1BBB += calculoAjusteDecena;
                            }
                        }
                        else
                        {
                            Int64 impuestoConsumo = 0;
                            if (IsDatos)
                            { impuestoConsumo = Convert.ToInt64(detalle.Substring(118, 14)); }

                            decimal sumatoria = Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14)) + impuestoConsumo;

                            if (llave == "02T004")
                            {
                                lineaServiciosETBIVA = $"1BBB|{descripcion}";
                                tempValorTotalIva += sumatoria;

                            }
                            else if (llave == "02T020" || llave == "02T050")
                            { // Req 11052015 Campo Financiacion Mes
                                lineasFinanciacion.Add($"1BBF|$Descripcion|{sumatoria}| ");
                                continue;
                            }

                            if (llave == "02T014")
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{sumatoria.ToString()}| ");
                            }

                            SubTotal1BBB += sumatoria;
                            ValorPagarMes += sumatoria;

                        }
                    }
                }

            }
            #endregion

            #region Servicios ETB IVA, AJuste Decena y Total
            if (!string.IsNullOrEmpty(lineaServiciosETBIVA))
            {
                Lineas1BBB.Add($"{lineaServiciosETBIVA}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, tempValorTotalIva.ToString())}| ");
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
                Lineas1BBB.Add($"1BBA|Total de la Factura ETB|{Helpers.FormatearCampos(TiposFormateo.Decimal01, SubTotal1BBB.ToString())}| ");
            }
            else
            {
                Lineas1BBB.Add($"1BBA|Saldo a Favor|{Helpers.FormatearCampos(TiposFormateo.Decimal01, SubTotal1BBB.ToString())}| ");
            }

            if (lineasFinanciacion.Count > 0)
            {
                Lineas1BBB.AddRange(lineasFinanciacion);
            }

            #endregion

            return Lineas1BBB;
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

        /// Metodo que obtiene las lineas formateadas de Canal CONS
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanalCONS(List<string> datosOriginales)
        {
            #region FormateoCanalCONS
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
                Linea3000401 = $"CONS|IVA|{Linea3000401.Substring(8, 3).Trim()}|{Linea3000401.Substring(12, 20).Trim()}| ";
            }

            var result3000402 = from busqueda in datosOriginales
                                where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000402")
                                select busqueda;

            if (result3000402.Any())
            {
                Linea3000402 = result3000402.FirstOrDefault();
                Linea3000402 = $"CONS|ImpoConsumo|{Linea3000402.Substring(8, 3).Trim()}|{Linea3000402.Substring(12, 20).Trim()}| ";
            }

            var result3000404 = from busqueda in datosOriginales
                                where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000404")
                                select busqueda;

            if (result3000404.Any())
            {
                Linea3000404 = result3000404.FirstOrDefault();
                Linea3000404 = $"CONS|ImpoConsumo|{Linea3000404.Substring(8, 3).Trim()}|{Linea3000404.Substring(12, 20).Trim()}| ";
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
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("13M311") && string.IsNullOrEmpty(busqueda.Substring(6,16).Trim())
                              select busqueda;

            if (linea13M311.Any())
            {
                string llaveCruce = $"CODF{linea13M311.FirstOrDefault().Substring(32, 10).Trim()}";
                var concepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault().Substring(14).Trim();

                total13M311 = linea13M311.FirstOrDefault().Substring(42, 14).Trim();
                subtotal13M311 = linea13M311.FirstOrDefault().Substring(56, 14).Trim();

                resultado.Add(Helpers.ValidarPipePipe($"1FFF|{concepto}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, total13M311)}" +
                    $"|{Helpers.FormatearCampos(TiposFormateo.Decimal01, subtotal13M311)}|{Helpers.SumarCampos(new List<string> { total13M311, subtotal13M311 })}| | "));
                
                
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

                resultado.Add(Helpers.ValidarPipePipe($"1FFF|{concepto}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, total113M319)}" +
                    $"|{Helpers.FormatearCampos(TiposFormateo.Decimal01, subtotal113M319)}|{Helpers.SumarCampos(new List<string> { total113M319, subtotal113M319 })}| | "));


            }

            if (resultado.Count > 0 && resultado.Count % 2 != 0)
            {
                resultado.Add("1FFF| | | | | | ");
            }
            if(resultado.Count > 0)
            {
                resultado.Add(Helpers.ValidarPipePipe($"1FFA|Total|{Helpers.SumarCampos(new List<string> { total13M311, total113M319 })}" +
                    $"|{Helpers.SumarCampos(new List<string> { subtotal13M311, subtotal113M319 })}" +
                    $"|{Helpers.SumarCampos(new List<string> { total13M311, total113M319, subtotal13M311, subtotal113M319 })}| | "));
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
                    string valorFinanciadoFormat = Helpers.FormatearCampos(TiposFormateo.Decimal01, valorFinanciado.ToString());
                    string saldoRestanteFormat = Helpers.FormatearCampos(TiposFormateo.Decimal01, saldoRestante.ToString());

                    if (valorFinanciado > 0)
                    {
                        resultado.Add($"1III|{descripcion}|{valorFinanciadoFormat}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, item.Substring(16, 14))}|{saldoRestanteFormat}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, item.Substring(44, 14))}|{cuota}|{interes}| ");
                    }

                }
            }

            return resultado;
            #endregion
        }

        #endregion
    }
}
