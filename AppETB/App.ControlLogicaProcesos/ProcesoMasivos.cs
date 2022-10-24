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
        #region Variables del proceso
        private bool IsResidencial { get; set; }
        private bool IsGobierno { get; set; }
        private bool IsFibra { get; set; }
        private bool IsDatos { get; set; }
        private bool IsLte { get; set; }
        private bool IsLteCorporativo { get; set; }
        private bool IsAnexoFibra { get; set; }
        private string Cuenta { get; set; }
        private string Ciclo { get; set; }
        private string Estrato { get; set; }
        private string PeriodoFacturacion { get; set; }
        private string FechaDesde { get; set; }
        private string FechaHasta { get; set; }
        private string FechaExpedicion { get; set; }
        private int? MesMora { get; set; }
        private string CodigoDANE { get; set; }
        private bool Is1OOA { get; set; }
        private bool Is1ODC { get; set; }
        private bool Is1AFI { get; set; }
        private List<string> listaRegi1CFI { get; set; }
        #endregion

        public ProcesoMasivos(string pArchivo, string pPeridoFacturacion)
        {
            #region ProcesoMasivos
            try
            {
                PeriodoFacturacion = pPeridoFacturacion;
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
            bool cuentaErrorLte = false;
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
                            #region Verificar si son de error LTE
                            if (IsLte || IsLteCorporativo)
                            {
                                if (!IsLteCorporativo)
                                {
                                    var linea040011 = from busqueda in datosExtractoFormateo
                                                      where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040011")
                                                      select busqueda;

                                    if (!linea040011.Any())
                                    {
                                        if (!Variables.Variables.DatosInsumoProcuni.ContainsKey(llaveCruce))
                                        {
                                            Variables.Variables.DatosErrorLTE.Add(llaveCruce, FormatearArchivo(llaveCruce, datosExtractoFormateo));
                                            cuentaErrorLte = true;
                                        }
                                    }
                                }
                            }
                            #endregion

                            if (!cuentaErrorLte)
                            {
                                AgregarDiccionario(llaveCruce, FormatearArchivo(llaveCruce, datosExtractoFormateo));

                                datosExtractoFormateo.Clear();
                            }
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

            if (pLLaveCruce == "" ||
                pLLaveCruce == "" ||
                pLLaveCruce == "" ||
                pLLaveCruce == "" ||
                pLLaveCruce == "")
            {

            }

            FormatearPropiedadesExtracto();

            #region Formateo Canales
            resultadoFormateoLinea = MapeoCanal1AAA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1BBB(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoAgrupacion1CCA(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanalADNC(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalADN1(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormarPaqueteEEE(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1OOA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
                Is1OOA = true;
            }

            resultadoFormateoLinea = MapeoCanal1OPL(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1OOB(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1ODC(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
                Is1ODC = true;
            }

            resultadoFormateoLinea = FormateoCanal1ODD(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1HIS(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1HDT(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1OOO(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1OMT(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1OMV(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1PLA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1DET(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1CPA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CMP(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1CCM(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoGrupoAFI(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1CIC(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1PPP(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1FFF(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1FFA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1KKK();

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1LLL();

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1RRT(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1RRR(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1CDP();

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanalCONS(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1MMM(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanalCART(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanalCUFE(datosOriginales);

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

            resultadoFormateoLinea = MapeoCanalNTC2(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanalNTC3(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanalNTC4(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanalNTC5(datosOriginales);

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
            IsResidencial = false;
            IsDatos = false;
            IsFibra = false;
            IsGobierno = false;
            IsLte = false;
            IsLteCorporativo = false;
            Cuenta = string.Empty;
            Ciclo = string.Empty;
            Estrato = string.Empty;
            FechaExpedicion = string.Empty;
            MesMora = null;
            CodigoDANE = string.Empty;
            Is1OOA = false;
            Is1ODC = false;
            Is1AFI = false;
            listaRegi1CFI = new List<string>();
            #endregion
        }

        #region Logica Canales

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

                ListaCanal1AAA.Add(GetTelefono(datosOriginales, Linea010000)); //TODO: Verificar Reglas
                ListaCanal1AAA.Add(IsFibra ? (string.IsNullOrEmpty(Linea010000.Substring(218, 20).Trim()) ? " " : Linea010000.Substring(218, 20).Trim()) : " ");

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
                ListaCanal1AAA.Add(GetEstrato(Linea040000));
                ListaCanal1AAA.AddRange(GetBarrioLocalidad());
                ListaCanal1AAA.Add(string.Empty);
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
                ListaCanal1AAA.Add(GetFechaCorte(Linea010000));
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
        private string GetTelefono(List<string> datosOriginales, string pLinea010000)
        {
            #region GetTelefono
            string telefono = string.Empty;

            if (IsFibra)
            {
                telefono = pLinea010000.Substring(238, 20).Trim();
            }

            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                         select busqueda;

            if (result.Any())
            {
                foreach (var item in result.ToList())
                {
                    if ((IsLte || IsLteCorporativo) && item.Substring(6, 1) == "3")
                    {
                        if (Cuenta != item.Substring(6, 20).Trim() && item.Substring(6, 20).Trim().Length >= 8)
                        {
                            telefono = item.Substring(6, 20).Trim();
                            string numprivado = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoTelefonosPrivadosRevchain, $"{telefono}") ?? string.Empty;
                            //if (string.IsNullOrEmpty(numprivado))
                            //{
                            //    numPrivado = true;
                            //}
                        }
                    }
                    else if (IsResidencial || IsFibra)
                    {
                        if (string.IsNullOrEmpty(telefono) || (telefono.Substring(0, 3) != "601" && telefono.Substring(0, 3) != "608"))
                        {
                            if (Cuenta != item.Substring(6, 20).Trim() &&
                                ((item.Substring(6, 20).Trim().Length == 10 && (item.Substring(6, 3) == "601" && item.Substring(6, 3) == "608")) ||
                                (item.Substring(6, 20).Trim().Length == 8 && (item.Substring(6, 1) == "601"))))
                            {
                                telefono = item.Substring(6, 20).Trim();
                            }
                        }
                    }
                    else
                    {
                        if (Cuenta != item.Substring(6, 20).Trim() && item.Substring(6, 20).Trim().Length >= 8)
                        {
                            telefono = item.Substring(6, 20).Trim();
                        }
                    }
                }

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
                    SubTotal1BBB += Convert.ToInt32(linea29000.Substring(29, 20));
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
                        if (!string.IsNullOrEmpty(detalle.Substring(20, 14).Trim()) && Convert.ToInt32(detalle.Substring(20, 14)) != 0)
                        {
                            SubTotal1BBB += Convert.ToInt32(detalle.Substring(20, 14));
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt32(detalle.Substring(6, 14)) != 0 /*&& trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X"*/)
                        {
                            if (string.IsNullOrEmpty(linea150001) || (!string.IsNullOrEmpty(linea150001) && linea150001.Substring(6, 1) != "N")) // Regla No sumar saldo anterior
                            {
                                SubTotal1BBB += Convert.ToInt32(detalle.Substring(6, 14));
                            }
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(34, 14).Trim()) && Convert.ToInt32(detalle.Substring(34, 14)) != 0 /*&& trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X"*/)
                        {
                            SubTotal1BBB += Convert.ToInt32(detalle.Substring(34, 14));
                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(detalle.Substring(6, 14)) == 0 && Convert.ToInt32(detalle.Substring(20, 14)) == 0 &&
                        Convert.ToInt32(detalle.Substring(34, 14)) == 0 && Convert.ToInt32(detalle.Substring(48, 14)) == 0 &&
                        Convert.ToInt32(detalle.Substring(62, 14)) == 0 && Convert.ToInt32(detalle.Substring(118, 14)) == 0)
                        { continue; }

                        if (llave == "02T019")
                        {
                            //if (trim($valores_temp["estampilla"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt32(detalle.Substring(6, 14)) != 0)
                            {
                                SubTotal1BBB += Convert.ToInt32(detalle.Substring(6, 14)) + Convert.ToInt32(detalle.Substring(20, 14)) + Convert.ToInt32(detalle.Substring(34, 14)) + Convert.ToInt32(detalle.Substring(48, 14)) + Convert.ToInt32(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T016")
                        {
                            //if (trim($valores_temp["revercion_pago"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt32(detalle.Substring(6, 14)) != 0)
                            {
                                SubTotal1BBB += Convert.ToInt32(detalle.Substring(6, 14)) + Convert.ToInt32(detalle.Substring(20, 14)) + Convert.ToInt32(detalle.Substring(34, 14)) + Convert.ToInt32(detalle.Substring(48, 14)) + Convert.ToInt32(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T002")
                        {
                            //if (trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt32(detalle.Substring(6, 14)) != 0)
                            {
                                SubTotal1BBB += Convert.ToInt32(detalle.Substring(6, 14)) + Convert.ToInt32(detalle.Substring(20, 14)) + Convert.ToInt32(detalle.Substring(34, 14)) + Convert.ToInt32(detalle.Substring(48, 14)) + Convert.ToInt32(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T003")
                        {
                            //if (trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X") TODO: Validar Regla
                            //    return false;

                            calculoAjusteDecena += Convert.ToInt32(detalle.Substring(6, 14)) + Convert.ToInt32(detalle.Substring(20, 14)) + Convert.ToInt32(detalle.Substring(34, 14)) + Convert.ToInt32(detalle.Substring(48, 14)) + Convert.ToInt32(detalle.Substring(62, 14));
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
                            int impuestoConsumo = 0;
                            if (IsFibra || IsDatos || IsGobierno || IsLte || IsLteCorporativo)
                            { impuestoConsumo = Convert.ToInt32(detalle.Substring(118, 14)); }

                            decimal sumatoria = Convert.ToInt32(detalle.Substring(6, 14)) + Convert.ToInt32(detalle.Substring(20, 14)) + Convert.ToInt32(detalle.Substring(34, 14)) + Convert.ToInt32(detalle.Substring(48, 14)) + Convert.ToInt32(detalle.Substring(62, 14)) + impuestoConsumo;

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

            #region 11C - Inseparables
            var result02T933 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T933")
                               select busqueda;

            string linea02T933 = string.Empty;
            decimal total02T933 = decimal.Zero;

            if (result02T933.Any())
            {
                linea02T933 = result02T933.FirstOrDefault() ?? string.Empty;
                total02T933 = Convert.ToInt32(linea02T933.Substring(6, 14)) + Convert.ToInt32(linea02T933.Substring(20, 14)) + Convert.ToInt32(linea02T933.Substring(34, 14)) + Convert.ToInt32(linea02T933.Substring(48, 14)) + Convert.ToInt32(linea02T933.Substring(62, 14));
            }

            var result11C = from busqueda in datosOriginales
                            where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C")
                            select busqueda;
            if (result11C.Any())
            {
                List<string> listaDetalles = result11C.ToList();

                foreach (string detalle in listaDetalles)
                {
                    llave = detalle.Substring(6, 10).Trim();

                    if ((llave == "0163710147" || llave == "0163711147") && total02T933 > 0 && (IsLte || IsLteCorporativo))
                    {
                        SubTotal1BBB -= total02T933;

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
                case 15:
                case 16:
                case 87:
                    tipociclo = "FIB";
                    IsFibra = true;
                    break;

                case 17:
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                case 36:
                case 38:
                case 39:
                case 40:
                case 44:
                case 45:
                case 46:
                case 47:
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                case 58:
                case 59:
                case 60:
                case 61:
                case 62:
                case 63:
                case 68:
                case 70:
                case 71:
                    tipociclo = "PAR";
                    IsResidencial = true;
                    break;

                case 18:
                case 19:
                case 37:
                case 64:
                case 67:
                case 69:
                    tipociclo = "GOB";
                    IsGobierno = true;
                    break;

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

                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 90:
                case 91:
                case 92:
                case 93:
                case 94:
                case 95:
                    tipociclo = "LTE";
                    IsLte = true;
                    break;

                case 96:
                case 97:
                    tipociclo = "LTE_CORP";
                    IsLteCorporativo = true;
                    break;
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
                if (IsDatos || IsLteCorporativo)
                {
                    tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM10").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
                    return tasaInteresTablaSustitucion;
                }
                else if (IsLte)
                {
                    tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM08").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
                    return tasaInteresTablaSustitucion;
                }
                else if (IsResidencial || IsFibra || IsGobierno)
                {
                    tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM08").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
                    tasaInteresTablaSustitucion += $" {Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM10").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty}";

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

            if ((IsLte || IsLteCorporativo) && !string.IsNullOrEmpty(actividad))
            {
                actividad = "00";
            }

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

            if (!string.IsNullOrEmpty(actividadTablaSustitucion) && (IsResidencial || IsFibra || IsGobierno))
            {
                return actividadTablaSustitucion;
            }
            else
            {
                return string.Empty;
            }



            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene el Estrato
        /// </summary>
        /// <param name="pLinea040000"></param>
        /// <returns></returns>
        private string GetEstrato(string pLinea040000)
        {
            #region GetEstrato
            string Estrato = pLinea040000.Substring(123, 1);

            if (!string.IsNullOrEmpty(Estrato.Trim()))
            {
                try
                {
                    int estratoInt = Convert.ToInt16(Estrato);
                    estratoInt = estratoInt > 6 ? 6 : estratoInt;
                    Estrato = estratoInt.ToString();
                    return estratoInt.ToString();
                }
                catch
                {
                    return string.Empty;
                }

            }
            else
            {
                return string.Empty;
            }
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

        /// <summary>
        /// Metodo que Obtiene el MarcaAnexosPublicidad
        /// </summary>
        /// <returns></returns>
        private string GetLogoInternet()
        {
            #region GetLogoInternet
            string logoIntener = string.Empty;

            if ((IsFibra || IsResidencial) && Estrato != "" && (Estrato.Trim() == "1" || Estrato.Trim() == "2")) //Regla de validacion del logo de MinTic en facturas fibra y que estrato sea igual a 2
            {
                logoIntener = "LogInt";
            }


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

        // <summary>
        /// Metodo que Obtiene FechaCorte
        /// </summary>
        /// <returns></returns>
        private string GetFechaCorte(string pLinea010000)
        {
            #region FechaCorte
            string fechaFactura = Helpers.FormatearCampos(TiposFormateo.Fecha02, pLinea010000.Substring(168, 8));
            string fechaCorte = string.Empty;

            if (IsLte || IsLteCorporativo)
            {
                string fechaExpedicion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECX{Helpers.FormatearCampos(TiposFormateo.Fecha02, pLinea010000.Substring(168, 8).Trim())}{pLinea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty;
                fechaExpedicion = fechaExpedicion.Substring(0, 10).Trim();
                string fechaCorteFinal = string.Empty;
                if (fechaExpedicion != "")
                {
                    DateTime dt = Convert.ToDateTime(fechaExpedicion);
                    dt.AddMonths(1);
                    string[] array_exp = fechaExpedicion.Split('/');
                    DateTime fecha = new DateTime();

                    if (Ciclo == "90")
                    {
                        fecha = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0);
                        fecha = new DateTime(Convert.ToInt32(array_exp[2]), Convert.ToInt32(array_exp[1]) + 1, 1, 0, 0, 0);
                    }
                    else if (Ciclo == "91")
                    {
                        fecha = new DateTime(Convert.ToInt32(array_exp[2]), Convert.ToInt32(array_exp[1]) + 1, 5, 0, 0, 0);
                    }
                    else if (Ciclo == "92")
                    {
                        fecha = new DateTime(Convert.ToInt32(array_exp[2]), Convert.ToInt32(array_exp[1]) + 1, 10, 0, 0, 0);
                    }
                    else if (Ciclo == "93")
                    {
                        fecha = new DateTime(Convert.ToInt32(array_exp[2]), Convert.ToInt32(array_exp[1]) + 1, 15, 0, 0, 0);
                    }
                    else if (Ciclo == "94")
                    {
                        fecha = new DateTime(Convert.ToInt32(array_exp[2]), Convert.ToInt32(array_exp[1]) + 1, 20, 0, 0, 0);
                    }
                    else if (Ciclo == "95")
                    {
                        fecha = new DateTime(Convert.ToInt32(array_exp[2]), Convert.ToInt32(array_exp[1]) + 1, 25, 0, 0, 0);
                    }

                    fechaCorte = fecha.ToString("MMMM dd de yyyy");
                }
            }

            return fechaCorte;
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

            if (IsFibra || IsResidencial)
            {

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
                            if (Convert.ToInt32(linea11C.Substring(132, 2)) != 0 && Convert.ToInt32(linea11C.Substring(134, 2)) != 0 && Convert.ToInt32(linea11C.Substring(143, 2)) != 0 && Convert.ToInt32(linea11C.Substring(145, 2)) != 0)
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
                    else
                    {
                        fechaInicio = "99999999";
                        fechaFin = "99999999";
                    }

                }
                else
                {
                    fechaInicio = "99999999";
                    fechaFin = "99999999";
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

            }

            periodos.Add(periodoDesdeHasta);
            periodos.Add(periodoDesdeHastaLTE);

            return periodos;
            #endregion
        }


        #endregion Metodos 1AAA

        /// Metodo que obtiene las lineas formateadas de Canal 1BBB
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanal1BBB(List<string> datosOriginales)
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
                    SubTotal1BBB += Convert.ToInt32(linea29000.Substring(29, 20));
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
                    else if (llave == "02T112" && !IsFibra)
                    {
                        descripcion = descripcion.Substring(0, 31);
                    }

                    if (llave == "02T008")
                    { } // se omite el valor de subsidio
                    else if (llave == "02S000")
                    {
                        if (!string.IsNullOrEmpty(detalle.Substring(20, 14).Trim()) && Convert.ToInt32(detalle.Substring(20, 14)) != 0)
                        {
                            Lineas1BBB.Add($"1BBB|Traslado de Saldos|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(20, 14).TrimStart('0'))}| ");
                            SubTotal1BBB += Convert.ToInt32(detalle.Substring(20, 14));
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt32(detalle.Substring(6, 14)) != 0 /*&& trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X"*/)
                        {
                            if (string.IsNullOrEmpty(linea150001) || (!string.IsNullOrEmpty(linea150001) && linea150001.Substring(6, 1) != "N")) // Regla No sumar saldo anterior
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt32(detalle.Substring(6, 14));
                            }
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(34, 14).Trim()) && Convert.ToInt32(detalle.Substring(34, 14)) != 0 /*&& trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X"*/)
                        {
                            Lineas1BBB.Add($"1BBB|Ajuste De Pagos|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(34, 14).TrimStart('0'))}| ");
                            SubTotal1BBB += Convert.ToInt32(detalle.Substring(34, 14));
                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(detalle.Substring(6, 14)) == 0 && Convert.ToInt32(detalle.Substring(20, 14)) == 0 &&
                        Convert.ToInt32(detalle.Substring(34, 14)) == 0 && Convert.ToInt32(detalle.Substring(48, 14)) == 0 &&
                        Convert.ToInt32(detalle.Substring(62, 14)) == 0 && Convert.ToInt32(detalle.Substring(118, 14)) == 0)
                        { continue; }

                        if (llave == "02T019")
                        {
                            //if (trim($valores_temp["estampilla"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt32(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt32(detalle.Substring(6, 14)) + Convert.ToInt32(detalle.Substring(20, 14)) + Convert.ToInt32(detalle.Substring(34, 14)) + Convert.ToInt32(detalle.Substring(48, 14)) + Convert.ToInt32(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T016")
                        {
                            //if (trim($valores_temp["revercion_pago"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt32(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt32(detalle.Substring(6, 14)) + Convert.ToInt32(detalle.Substring(20, 14)) + Convert.ToInt32(detalle.Substring(34, 14)) + Convert.ToInt32(detalle.Substring(48, 14)) + Convert.ToInt32(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T002")
                        {
                            //if (trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X") TODO: Validar Regla
                            //    return false;

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt32(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt32(detalle.Substring(6, 14)) + Convert.ToInt32(detalle.Substring(20, 14)) + Convert.ToInt32(detalle.Substring(34, 14)) + Convert.ToInt32(detalle.Substring(48, 14)) + Convert.ToInt32(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T003")
                        {
                            //if (trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X") TODO: Validar Regla
                            //    return false;

                            calculoAjusteDecena += Convert.ToInt32(detalle.Substring(6, 14)) + Convert.ToInt32(detalle.Substring(20, 14)) + Convert.ToInt32(detalle.Substring(34, 14)) + Convert.ToInt32(detalle.Substring(48, 14)) + Convert.ToInt32(detalle.Substring(62, 14));
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
                            int impuestoConsumo = 0;
                            if (IsFibra || IsDatos || IsGobierno || IsLte || IsLteCorporativo)
                            { impuestoConsumo = Convert.ToInt32(detalle.Substring(118, 14)); }

                            decimal sumatoria = Convert.ToInt32(detalle.Substring(6, 14)) + Convert.ToInt32(detalle.Substring(20, 14)) + Convert.ToInt32(detalle.Substring(34, 14)) + Convert.ToInt32(detalle.Substring(48, 14)) + Convert.ToInt32(detalle.Substring(62, 14)) + impuestoConsumo;

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

                            if (IsFibra || llave == "02T014")
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

            #region 11C - Inseparables
            var result02T933 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T933")
                               select busqueda;

            string linea02T933 = string.Empty;
            decimal total02T933 = decimal.Zero;

            if (result02T933.Any())
            {
                linea02T933 = result02T933.FirstOrDefault() ?? string.Empty;
                total02T933 = Convert.ToInt32(linea02T933.Substring(6, 14)) + Convert.ToInt32(linea02T933.Substring(20, 14)) + Convert.ToInt32(linea02T933.Substring(34, 14)) + Convert.ToInt32(linea02T933.Substring(48, 14)) + Convert.ToInt32(linea02T933.Substring(62, 14));
            }

            var result11C = from busqueda in datosOriginales
                            where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C")
                            select busqueda;
            if (result11C.Any())
            {
                List<string> listaDetalles = result11C.ToList();

                foreach (string detalle in listaDetalles)
                {
                    llave = detalle.Substring(6, 10).Trim();

                    if ((llave == "0163710147" || llave == "0163711147") && total02T933 > 0 && (IsLte || IsLteCorporativo))
                    {

                        if (!Lineas1BBB.Contains("1BBB|Mod. Inseparables con IVA"))
                        {
                            Lineas1BBB.Add($"1BBB|Mod. Inseparables con IVA|{total02T933}| ");
                            tempValorTotalIva -= total02T933;
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

            if (IsFibra)
            {
                Lineas1BBB.Add($"1BBB|Total Facturado en el Mes|{Helpers.FormatearCampos(TiposFormateo.Decimal01, ValorPagarMes.ToString())}| ");
            }

            if (SubTotal1BBB > 0)
            {
                if (IsFibra)
                {
                    Lineas1BBB.Add($"1BBA|Valor total a pagar|{Helpers.FormatearCampos(TiposFormateo.Decimal01, SubTotal1BBB.ToString())}| ");
                }
                else
                {
                    Lineas1BBB.Add($"1BBA|Total de la Factura ETB|{Helpers.FormatearCampos(TiposFormateo.Decimal01, SubTotal1BBB.ToString())}| ");
                }
            }
            else
            {
                if (IsFibra)
                {
                    Lineas1BBB.Add($"1BBA|Valor total a pagar|{Helpers.FormatearCampos(TiposFormateo.Decimal01, SubTotal1BBB.ToString())}| ");
                }
                else
                {
                    Lineas1BBB.Add($"1BBA|Saldo a Favor|{Helpers.FormatearCampos(TiposFormateo.Decimal01, SubTotal1BBB.ToString())}| ");
                }
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
        /// Linea que obtiene canal 1CCM
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1CCM(List<string> datosOriginales)
        {
            #region Canal 1CCM
            string Linea1CCM = string.Empty;
            string cargosCCM = string.Empty;

            if (Ciclo == "70")
            {
                cargosCCM = "Cargos: " + FechaExpedicion;
            }

            if (!IsLte && !IsLteCorporativo)
            {
                Linea1CCM = "1CCM| |" + cargosCCM + " | ";
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
        private IEnumerable<string> MapeoAgrupacion1CCA(List<string> datosOriginales)
        {
            #region MapeoCanal1CCA
            List<string> Linea1CCA = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();
            List<string> camposSumar = new List<string>();
            bool ban02T4 = false;

            string llave = string.Empty;
            string linea1CCATemp = string.Empty;
            string linea1CCCTemp = string.Empty;


            // Campos Pintar 1CCA
            string CCA_LineaNegocio = string.Empty;
            string CCA_Total = string.Empty;

            var resultPaquetes = from busqueda in datosOriginales
                                 where busqueda.Length > 4 && (busqueda.Substring(0, 4).Equals("02T1") || busqueda.Substring(0, 4).Equals("02T2") || busqueda.Substring(0, 4).Equals("02T3")
                                                         || busqueda.Substring(0, 4).Equals("02T4") || busqueda.Substring(0, 4).Equals("02T5") || busqueda.Substring(0, 4).Equals("02T6")
                                                         || busqueda.Substring(0, 4).Equals("02T7") || busqueda.Substring(0, 4).Equals("02T8") || busqueda.Substring(0, 4).Equals("02T9"))
                                 select busqueda;


            if (resultPaquetes.Any())
            {
                foreach (var registroActual in resultPaquetes)
                {

                    if (IsFibra || IsResidencial || IsLte)
                    {
                        llave = "FACLIN" + registroActual.Substring(3, 1);

                        if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llave))
                        {
                            CCA_LineaNegocio = Variables.Variables.DatosInsumoTablaSustitucion[llave][0].Substring(8).Trim();
                        }

                        if (llave == "FACLIN4")
                        { }
                        else if (string.IsNullOrEmpty(CCA_LineaNegocio))
                        {
                            continue;
                        }


                        // Se valida que tenga valor
                        if (Convert.ToDouble(registroActual.Substring(6, 14)) != 0)
                        {
                            linea1CCATemp = string.Empty;
                            // Se toman los totales
                            string llaveCanal = registroActual.Substring(0, 6);

                            switch (llaveCanal)
                            {
                                #region 02T103
                                case "02T103":

                                    linea1CCATemp = "1CCA|" + CCA_LineaNegocio + "|";

                                    camposSumar.Add(registroActual.Substring(6, 14));
                                    camposSumar.Add(registroActual.Substring(34, 14));

                                    CCA_Total = Helpers.SumarCampos(camposSumar);
                                    camposSumar.Clear();
                                    linea1CCATemp += CCA_Total + "| ";

                                    Linea1CCA.Add(Helpers.ValidarPipePipe(linea1CCATemp));

                                    // llena canal 1CCC
                                    Linea1CCA.AddRange(Mapear1CCC(llaveCanal, registroActual));

                                    break;
                                #endregion

                                #region 02T106
                                case "02T106":

                                    linea1CCATemp = "1CCA|" + CCA_LineaNegocio + "|";

                                    camposSumar.Add(registroActual.Substring(6, 14));

                                    CCA_Total = Helpers.SumarCampos(camposSumar);
                                    camposSumar.Clear();
                                    linea1CCATemp += CCA_Total + "| ";

                                    Linea1CCA.Add(Helpers.ValidarPipePipe(linea1CCATemp));

                                    // llena canal 1CCC
                                    Linea1CCA.AddRange(Mapear1CCC(llaveCanal, registroActual));

                                    break;
                                #endregion

                                #region 02T112
                                case "02T112":

                                    linea1CCATemp = "1CCA|" + CCA_LineaNegocio + "|";

                                    var result02T054 = from busqueda in datosOriginales
                                                       where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T054")
                                                       select busqueda;

                                    if (result02T054.Any())
                                    {
                                        camposSumar.Add(result02T054.FirstOrDefault().Substring(6, 14));
                                        camposSumar.Add(result02T054.FirstOrDefault().Substring(34, 14));

                                        CCA_Total = Helpers.SumarCampos(camposSumar);
                                        camposSumar.Clear();
                                        linea1CCATemp += CCA_Total + "| ";

                                        Linea1CCA.Add(Helpers.ValidarPipePipe(linea1CCATemp));

                                        // llena canal 1CCC
                                        Linea1CCA.AddRange(Mapear1CCC("02T054", result02T054.FirstOrDefault()));
                                    }

                                    break;
                                #endregion

                                #region 02T942
                                case "02T942":

                                    linea1CCATemp = "1CCA|" + CCA_LineaNegocio + "|";

                                    var result02T9 = from busqueda in datosOriginales
                                                     where busqueda.Length > 4 && busqueda.Substring(0, 4).Equals("02T9")
                                                     select busqueda;

                                    List<string> listaTemp1CCC = new List<string>();

                                    if (result02T9.Any())
                                    {
                                        foreach (var registrAct02T9 in result02T9)
                                        {
                                            switch (registrAct02T9.Substring(0, 6))
                                            {
                                                case "02T931":
                                                case "02T932":
                                                case "02T933":
                                                    camposSumar.Add(registrAct02T9.Substring(6, 14));
                                                    listaTemp1CCC.AddRange(Mapear1CCC(registrAct02T9.Substring(0, 6), registrAct02T9));
                                                    break;

                                                case "02T942":
                                                    camposSumar.Add(registrAct02T9.Substring(20, 14));
                                                    camposSumar.Add(registrAct02T9.Substring(34, 14));
                                                    camposSumar.Add(registrAct02T9.Substring(118, 14));
                                                    listaTemp1CCC.AddRange(Mapear1CCC(registrAct02T9.Substring(0, 6), registrAct02T9));

                                                    break;
                                                default:
                                                    break;
                                            }
                                        }

                                        // Si llena los campos 
                                        CCA_Total = Helpers.SumarCampos(camposSumar);
                                        camposSumar.Clear();
                                        linea1CCATemp += CCA_Total + "| ";
                                        Linea1CCA.Add(Helpers.ValidarPipePipe(linea1CCATemp));
                                        Linea1CCA.AddRange(listaTemp1CCC);
                                    }
                                    break;
                                #endregion

                                #region 02T409 - 02T410
                                case "02T409":
                                case "02T410":

                                    if (!ban02T4)
                                    {
                                        linea1CCATemp = "1CCA||| ";
                                        Linea1CCA.Add(Helpers.ValidarPipePipe(linea1CCATemp));
                                        ban02T4 = true;
                                    }

                                    // llena canal 1CCC
                                    Linea1CCA.AddRange(Mapear1CCC(llaveCanal, registroActual));

                                    break;
                                #endregion

                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            return Linea1CCA;

            #endregion
        }

        private List<string> Mapear1CCC(string llave, string linea)
        {
            #region Mapear1CCC
            List<string> list = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();
            string linea1CCC = string.Empty;
            string descripcion = string.Empty;
            List<string> camposSumar = new List<string>();

            if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey("CODT" + llave))
            {
                descripcion = Variables.Variables.DatosInsumoTablaSustitucion["CODT" + llave][0].Substring(10).Trim();
            }

            switch (llave)
            {
                case "02T103":
                case "02T106":
                case "02T054":

                    if (Convert.ToDouble(linea.Substring(6, 14)) != 0)
                    {
                        linea1CCC = "1CCC|";
                        linea1CCC += descripcion + "|";
                        linea1CCC += Helpers.FormatearCampos(TiposFormateo.Decimal01, linea.Substring(6, 14)) + "||| ";
                        list.Add(Helpers.ValidarPipePipe(linea1CCC));
                    }
                    linea1CCC = string.Empty;

                    // Se valida que el Iva tengo valor diferente de 0
                    if (Convert.ToDouble(linea.Substring(34, 14)) != 0)
                    {
                        linea1CCC = "1CCC|Iva|";
                        linea1CCC += Helpers.FormatearCampos(TiposFormateo.Decimal01, linea.Substring(34, 14)) + "||| ";

                        list.Add(Helpers.ValidarPipePipe(linea1CCC));
                        linea1CCC = string.Empty;
                    }

                    return list;

                case "02T931":
                case "02T932":
                case "02T933":
                case "02T409":
                case "02T410":

                    if (Convert.ToDouble(linea.Substring(6, 14)) != 0)
                    {
                        linea1CCC = "1CCC|";
                        linea1CCC += descripcion + "|";
                        linea1CCC += Helpers.FormatearCampos(TiposFormateo.Decimal01, linea.Substring(6, 14)) + "||| ";
                        list.Add(Helpers.ValidarPipePipe(linea1CCC));
                    }
                    return list;

                case "02T942":

                    // Recargo Mora
                    if (Convert.ToDouble(linea.Substring(20, 14)) != 0)
                    {
                        linea1CCC = "1CCC|Recargo mora|";
                        linea1CCC += Helpers.FormatearCampos(TiposFormateo.Decimal01, linea.Substring(20, 14)) + "||| ";
                        list.Add(Helpers.ValidarPipePipe(linea1CCC));
                    }

                    // Iva
                    if (Convert.ToDouble(linea.Substring(34, 14)) != 0)
                    {
                        camposSumar.Add(linea.Substring(34, 14));
                        camposSumar.Add("-" + linea.Substring(119, 13));

                        linea1CCC = "1CCC|Iva|";
                        linea1CCC += Helpers.SumarCampos(camposSumar) + "||| ";
                        list.Add(Helpers.ValidarPipePipe(linea1CCC));
                    }

                    return list;


                default:
                    break;
            }

            return list;
            #endregion
        }

        /// <summary>
        /// Linea que obtiene canal 1KKK
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1KKK()
        {
            #region Canal 1KKK
            string Linea1KKK = string.Empty;
            string oficinaPqr = string.Empty;
            bool validaroficinaPqr = false;

            string lineaProcuni = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoProcuni, $"{Cuenta}").FirstOrDefault() ?? string.Empty;

            if (IsGobierno)
            {

                string zonaPostal = CodigoDANE.Substring(5, 4);
                oficinaPqr = GetOficinaPQR($"CZONA{zonaPostal}");

                if (!string.IsNullOrEmpty(oficinaPqr))
                {
                    oficinaPqr = oficinaPqr.Replace("|", " ");
                    Linea1KKK += Linea1KKK.Trim() == "" ? $"1KKK|{oficinaPqr} " : $"{oficinaPqr} ";
                    validaroficinaPqr = true;
                }
            }

            // En caso de datos, tiene una oficina de reclamo por defecto y lo trae de anexos
            if (!validaroficinaPqr && IsDatos)
            {
                oficinaPqr = GetOficinaPQR($"CZONA80000");
                Linea1KKK = $"1KKK|{oficinaPqr} | ";
            }
            else if (!validaroficinaPqr && (IsResidencial || IsGobierno || IsFibra || IsLte || IsLteCorporativo)) // en caso no encontrar la oficina pqr se enlista y trae todas las oficinas de los supercades y puntos de pago importantes
            {
                oficinaPqr = GetOficinaPQR($"CZONA5030");
                Linea1KKK = $"1KKK|{oficinaPqr} ";
                oficinaPqr = GetOficinaPQR($"CZONA2020");
                Linea1KKK += $"{oficinaPqr} ";
                oficinaPqr = GetOficinaPQR($"CZONA3070");
                Linea1KKK += $"{oficinaPqr} ";
                oficinaPqr = GetOficinaPQR($"CZONA2040");
                Linea1KKK += $"{oficinaPqr} ";
                oficinaPqr = GetOficinaPQR($"CZONA37001");
                Linea1KKK += $"{oficinaPqr} ";
                oficinaPqr = GetOficinaPQR($"CZONA2001");
                Linea1KKK += $" {oficinaPqr}| ";
            }
            else if (validaroficinaPqr)
            {
                Linea1KKK += $" | ";
            }

            return Linea1KKK;
            #endregion
        }

        /// <summary>
        /// Obtiene la oficina de PQR a partir de la tabla de sustitucion
        /// </summary>
        /// <param name="pLlave"></param>
        /// <returns></returns>
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

        /// Metodo que obtiene la linea formateada de Canal 1CPA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1CPA(List<string> datosOriginales)
        {
            #region MapeoCanal1CPA
            string Linea1CPA = string.Empty;
            string llave = string.Empty;
            string nombrePlan = string.Empty;

            var result040000 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                               select busqueda;

            if (result040000.Any())
            {
                llave = "PL" + result040000.FirstOrDefault().Substring(133, 10);

                if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llave))
                {
                    nombrePlan = Variables.Variables.DatosInsumoTablaSustitucion[llave][0].Substring(13).Trim();

                    Linea1CPA = "1CPA|Plan Actual|" + nombrePlan + "| ";
                }
            }


            return Helpers.ValidarPipePipe(Linea1CPA);
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
        /// Linea que obtiene canal CUFE
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanalCUFE(List<string> datosOriginales)
        {
            #region Canal 1CUFE
            string LineaCUFE = string.Empty;
            string valor = string.Empty;

            LineaCUFE = "CUFE|";

            if (Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey(Cuenta))
            {
                valor = Variables.Variables.DatosInsumoETBFacturaElectronica[Cuenta];
            }

            LineaCUFE += valor + "| ";

            return Helpers.ValidarPipePipe(LineaCUFE);
            #endregion
        }

        private IEnumerable<string> MapeoGrupoAFI(List<string> datosOriginales)
        {
            #region MapeoGrupoAFI

            #region Variable
            List<string> listaGrupo = new List<string>();
            string resultado = string.Empty;
            Dictionary<string, List<string>> dicOrdenado = new Dictionary<string, List<string>>();
            List<string> lisPeriodoMax = new List<string>();
            string letrasPaquetes = string.Empty;
            string valorBase = string.Empty;
            string valorIva = string.Empty;
            string valorTotal = string.Empty;
            string subsidio = " ";
            string periodoLargo = string.Empty;
            List<string> listaFechas11C304 = new List<string>();
            List<string> listaFechasDesde = new List<string>();
            List<string> listaFechasHasta = new List<string>();
            List<string> LisSumaBase = new List<string>();
            List<string> LisSumaIva = new List<string>();
            List<string> LisSumaTotal = new List<string>();
            #endregion

            #region Busqueda
            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && (busqueda.Substring(0, 6).Equals("11C101") ||
                                                       busqueda.Substring(0, 6).Equals("11C304") ||
                                                       busqueda.Substring(0, 6).Equals("11C401") ||
                                                       busqueda.Substring(0, 6).Equals("11C410") ||
                                                       busqueda.Substring(0, 6).Equals("11C411") ||
                                                       busqueda.Substring(0, 6).Equals("11C412"))
                         select busqueda;
            #endregion

            #region Llenar Diccionario Ordenado
            if (result.Any())
            {
                foreach (var resultActual in result)
                {
                    if (Convert.ToDouble(resultActual.Substring(16, 14)) != 0)
                    {
                        if (resultActual.Substring(128, 19).Trim().Contains("-"))
                        {
                            string periodoTemp = resultActual.Substring(128, 6);
                            lisPeriodoMax.Add(periodoTemp + "01");
                            // Llena Diccionario
                            if (dicOrdenado.ContainsKey(periodoTemp))
                            {
                                dicOrdenado[periodoTemp].Add(resultActual);
                            }
                            else
                            {
                                dicOrdenado.Add(periodoTemp, new List<string>() { resultActual });
                            }
                        }
                    }
                }
            }
            #endregion

            #region Se Arma canal 1AFI - BFI
            string periodoMaximo = "";
            if (lisPeriodoMax.Any())
            {
                periodoMaximo = Helpers.GetFechaMaximaMinima(lisPeriodoMax, 1).Substring(0, 6);
            }

            if (!string.IsNullOrEmpty(periodoMaximo))
            {
                foreach (string key in dicOrdenado.Keys)
                {
                    foreach (var registro in dicOrdenado[key])
                    {
                        if (key == periodoMaximo)
                        {
                            #region Validr exclusiones
                            // Se valida que la linea no este en el las bases de exclusion y reglas para ser 1CFI
                            string llave = string.Empty;
                            string valorSus = string.Empty;

                            // Llave con los 10 digitos 
                            llave = registro.Substring(6, 10);

                            // Buscar en el insumo Codigos_Univer_SASV y Cuentas Svas FueraBundle si se encuentra se omite este registro
                            if (Variables.Variables.DatosInsumoCodigosUniverSvas.ContainsKey(llave) && Variables.Variables.DatosInsumoCuentasSvasFueraBundle.ContainsKey(Cuenta))
                            {
                                listaRegi1CFI.Add(registro);
                                continue;
                            }

                            // Buscar en el insumo ExcluirServiciosAdicionales
                            if (Variables.Variables.DatosInsumoExcluirServiciosAdicionales.ContainsKey(llave.TrimStart('0')))
                            {
                                listaRegi1CFI.Add(registro);
                                continue;
                            }

                            #region Busqueda concepto en tabla Sustitución
                            llave = $"CODF{registro.Substring(6, 10)}";

                            if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llave))
                            {
                                valorSus = Variables.Variables.DatosInsumoTablaSustitucion[llave].FirstOrDefault().Substring(14).Trim();
                            }
                            #endregion

                            // validar lel concepto trae la palabra SVA se omite
                            if (valorSus.Contains("SVA") && Convert.ToDouble(registro.Substring(16, 14)) != 0)
                            {
                                listaRegi1CFI.Add(registro);
                                continue;
                            }
                            #endregion

                            #region Toma de Letras Paquete
                            string llaveUno = $"{registro.Substring(6, 10)}";
                            string llaveDos = $"{registro.Substring(6, 6)}";
                            string valor = string.Empty;

                            if (Variables.Variables.DatosInsumoParametrizacionPaquetesFibra.ContainsKey(llaveUno))
                            {
                                valor = Variables.Variables.DatosInsumoParametrizacionPaquetesFibra[llaveUno];
                            }
                            else if (Variables.Variables.DatosInsumoParametrizacionPaquetesFibra.ContainsKey(llaveDos))
                            {
                                valor = Variables.Variables.DatosInsumoParametrizacionPaquetesFibra[llaveDos];
                            }

                            if (!string.IsNullOrEmpty(valor))
                            {
                                if (!letrasPaquetes.Contains(valor.Split('|')[2]))
                                {
                                    letrasPaquetes += valor.Split('|')[2];
                                }
                            }
                            #endregion

                            #region Llena listas de fechas
                            if (registro.Substring(0, 6) == "11C304")
                            {
                                listaFechas11C304.Add(registro.Substring(128, 19));
                                listaFechasDesde.Add(registro.Substring(128, 8));
                                listaFechasHasta.Add(registro.Substring(139, 8));
                            }
                            #endregion

                            #region Captura Totales
                            if (Convert.ToDouble(registro.Substring(16, 14)) != 0)
                            {
                                LisSumaBase.Add(registro.Substring(16, 14));
                                LisSumaIva.Add(registro.Substring(44, 14));

                                LisSumaTotal.Add(registro.Substring(16, 14));
                                LisSumaTotal.Add(registro.Substring(44, 14));
                            }
                            #endregion
                        }
                        else
                        {
                            listaRegi1CFI.Add(registro);
                        }
                    }
                }

                #region Se captura fecha mas reciente
                string fechaReciente = Helpers.GetFechaMasReciente(listaFechas11C304);

                if (!string.IsNullOrEmpty(fechaReciente))
                {
                    periodoLargo = "(" + Helpers.FormatearCampos(TiposFormateo.Fecha04, fechaReciente.Substring(4, 4)) + " - " + Helpers.FormatearCampos(TiposFormateo.Fecha04, fechaReciente.Substring(13, 4)) + ")";
                }
                else
                {
                    periodoLargo = " ";
                }
                #endregion

                resultado = "1AFI|";
                resultado += periodoMaximo + "|";
                resultado += GetNombrePaquete(letrasPaquetes) + "|";
                resultado += Helpers.SumarCampos(LisSumaBase) + "|";
                resultado += Helpers.SumarCampos(LisSumaIva) + "|";
                resultado += Helpers.SumarCampos(LisSumaTotal) + "|";
                resultado += subsidio + "|";
                resultado += periodoLargo + "||";
                resultado += Helpers.GetFechaMaximaOMinima(listaFechasDesde, 2) + "|";
                resultado += Helpers.GetFechaMaximaOMinima(listaFechasHasta, 1) + "| ";

                // se agrega el canal 1AFI
                listaGrupo.Add(Helpers.ValidarPipePipe(resultado));


                #region Se arma el canal 1BFI
                foreach (string registro1BFI in dicOrdenado[periodoMaximo])
                {
                    string resultadoLogica = Logica1BFI(registro1BFI);

                    if (!string.IsNullOrEmpty(resultadoLogica))
                    {
                        listaGrupo.Add(resultadoLogica);
                    }
                }
                #endregion
            }
            #endregion

            #region Se arma el canal 1CFI
            List<string> lisTemp1CF1 = Logica1CFI(listaRegi1CFI);

            if (lisTemp1CF1.Count > 0)
                listaGrupo.AddRange(lisTemp1CF1);

            string ajusteDecena = logicaAjusteDecena1CFI(datosOriginales);

            if (!string.IsNullOrEmpty(ajusteDecena))
                listaGrupo.Add(ajusteDecena);
            #endregion

            #region Se arma el canal 1TFI
            string canal1TFI = Logica1TFI(datosOriginales);

            if (!string.IsNullOrEmpty(canal1TFI))
                listaGrupo.Add(canal1TFI);
            #endregion

            return listaGrupo;
            #endregion
        }

        private string GetNombrePaquete(string letras)
        {
            #region GetNombrePaquete

            string llave = string.Empty;
            string valor = string.Empty;

            if (letras.Contains("A") && letras.Contains("B") && letras.Contains("C"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["ABC"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("A") && letras.Contains("B"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["AB"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("A") && letras.Contains("C"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["AC"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("B") && letras.Contains("C"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["BC"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("F") && letras.Contains("G"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["FG"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("A") && letras.Contains("G"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["AG"];
                return valor.Split('|')[1];
            }
            else if (letras.Contains("B") && letras.Contains("F"))
            {
                valor = Variables.Variables.DatosInsumoConformacionPaquetes["BF"];
                return valor.Split('|')[1];
            }
            else
            {
                return " ";
            }


            #endregion
        }

        private string Logica1BFI(string lineaProcesar)
        {
            #region Logica1BFI
            string lineaResultado = string.Empty;

            // Se valida que sea diferente de 0 pa continuar
            if (Convert.ToDouble(lineaProcesar.Substring(16, 14).Trim()) > 0)
            {
                string llave = string.Empty;
                string valor = string.Empty;

                // Llave con los 10 digitos 
                llave = lineaProcesar.Substring(6, 10);

                // Buscar en el insumo Codigos_Univer_SASV y Cuentas Svas FueraBundle si se encuentra se omite este registro
                if (Variables.Variables.DatosInsumoCodigosUniverSvas.ContainsKey(llave) && Variables.Variables.DatosInsumoCuentasSvasFueraBundle.ContainsKey(Cuenta))
                {
                    return string.Empty;
                }

                // Buscar en el insumo ExcluirServiciosAdicionales
                if (Variables.Variables.DatosInsumoExcluirServiciosAdicionales.ContainsKey(llave.TrimStart('0')))
                {
                    return string.Empty;
                }

                #region Busqueda concepto en tabla Sustitución
                llave = $"CODF{lineaProcesar.Substring(6, 10)}";

                if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llave))
                {
                    valor = Variables.Variables.DatosInsumoTablaSustitucion[llave].FirstOrDefault().Substring(14).Trim();
                }
                #endregion

                // validar lel concepto trae la palabra SVA se omite
                if (valor.Contains("SVA"))
                {
                    return string.Empty;
                }

                if (Convert.ToDouble(lineaProcesar.Substring(16, 14)) != 0 ||
                        Convert.ToDouble(lineaProcesar.Substring(44, 14)) != 0 ||
                        (Convert.ToDouble(lineaProcesar.Substring(16, 14)) + Convert.ToDouble(lineaProcesar.Substring(44, 14)) != 0))
                {

                    lineaResultado = "1BFI||";

                    if (lineaProcesar.Substring(128, 19).Contains('-'))
                    {
                        lineaResultado += valor + " ( " + Helpers.FormatearCampos(TiposFormateo.Fecha04, lineaProcesar.Substring(132, 4)) + " - " + Helpers.FormatearCampos(TiposFormateo.Fecha04, lineaProcesar.Substring(143, 4)) + " ) |";
                    }
                    else
                    {
                        lineaResultado += valor + "|";
                    }

                    lineaResultado += " |||| ";

                    return Helpers.ValidarPipePipe(lineaResultado);
                }
            }

            return string.Empty;
            #endregion
        }
        private string logicaAjusteDecena1CFI(List<string> datosOriginales)
        {
            #region logicaAjusteDecena1CFI
            string reultadoAjusteDecena = string.Empty;
            List<PosCortes> listaCortes = new List<PosCortes>();


            // Llenar el canal con el ajuste a la decena si lo tiene
            var resultAjusteDecena = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T003")
                                     select busqueda;

            // Validar que tenga Ajuste a la decena
            if (resultAjusteDecena.Any())
            {
                listaCortes.Add(new PosCortes(6, 14, TiposFormateo.Decimal01));

                reultadoAjusteDecena = string.Empty;
                reultadoAjusteDecena = "1CFI||Ajuste Decena|";
                reultadoAjusteDecena += Helpers.ExtraccionCamposSpool(listaCortes, resultAjusteDecena.FirstOrDefault());
                reultadoAjusteDecena += "||";
                reultadoAjusteDecena += Helpers.ExtraccionCamposSpool(listaCortes, resultAjusteDecena.FirstOrDefault());
                reultadoAjusteDecena += "|| ";
            }

            return Helpers.ValidarPipePipe(reultadoAjusteDecena);

            #endregion
        }

        private List<string> Logica1CFI(List<string> listaProcesar)
        {
            #region Logica1CFI
            List<string> resultado1CFI = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();
            List<string> camposSumar = new List<string>();
            string linea1CFI = string.Empty;

            if (listaProcesar.Count > 0)
            {
                foreach (string registro1CFI in listaProcesar)
                {
                    if (Convert.ToInt32(registro1CFI.Substring(16, 14).Trim()) > 0)
                    {
                        string llave = $"CODF{registro1CFI.Substring(6, 10)}";
                        string valor = string.Empty;

                        if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llave))
                        {
                            valor = Variables.Variables.DatosInsumoTablaSustitucion[llave].FirstOrDefault().Substring(15).Trim();
                        }

                        linea1CFI = string.Empty;
                        linea1CFI = "1CFI|";
                        linea1CFI += registro1CFI.Substring(128, 6) + "|";

                        if (registro1CFI.Substring(128, 19).Contains('-'))
                        {
                            linea1CFI += valor + " ( " + Helpers.FormatearCampos(TiposFormateo.Fecha04, registro1CFI.Substring(132, 4)) + " - " + Helpers.FormatearCampos(TiposFormateo.Fecha04, registro1CFI.Substring(143, 4)) + " ) |";
                        }
                        else
                        {
                            linea1CFI += valor + "|";
                        }

                        linea1CFI += Helpers.FormatearCampos(TiposFormateo.Decimal01, registro1CFI.Substring(16, 14)) + "|";
                        linea1CFI += Helpers.FormatearCampos(TiposFormateo.Decimal01, registro1CFI.Substring(44, 14)) + "|";

                        camposSumar.Add(registro1CFI.Substring(16, 14));
                        camposSumar.Add(registro1CFI.Substring(44, 14));
                        linea1CFI += Helpers.SumarCampos(camposSumar) + "|";
                        linea1CFI += "| ";

                        camposSumar.Clear();

                        resultado1CFI.Add(Helpers.ValidarPipePipe(linea1CFI));
                    }
                }
            }

            return resultado1CFI;
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
            string resultado = string.Empty;

            var linea40000 = from busqueda in datosOriginales
                             where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                             select busqueda;

            if (linea40000.Any())
            {
                if (IsLte || IsLteCorporativo && GetTipo(linea40000.FirstOrDefault().Substring(6, 20).Trim()) != "Cuenta")
                {
                    resultado = $"ADNC|{linea40000.FirstOrDefault().Substring(26, 49).Trim()}";
                }
            }

            return resultado;
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
                         where busqueda.Length > 6 && (busqueda.Substring(0, 6).Equals("11C101") || busqueda.Substring(0, 6).Equals("11C304") || busqueda.Substring(0, 6).Equals("11C401"))
                         let x = busqueda
                         group x by busqueda.Substring(6, 10) into busqueda
                         select busqueda;

            if (result != null)
            {
                foreach (var resultadoActual in result.SelectMany(x => x))
                {
                    if (Convert.ToInt32(resultadoActual.Substring(16, 14).Trim()) > 0)
                    {
                        string llave = $"CODF{resultadoActual.Substring(6, 10)}";
                        string valor = string.Empty;

                        if (Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llave))
                        {
                            valor = Variables.Variables.DatosInsumoTablaSustitucion[llave].FirstOrDefault().Trim();
                        }


                        // Tabla Suti = CODF0163730650 SVA Access Point Wifi Plus

                        //    /// 1. Si esta en Codigos_Univer_SVAS pinto, y en elnombre de sustitusion SI este la palabra SVA/                        
                        //    /// 2. Validar en este archivo ExcluirServiciosAdicionales_JUL2022
                        //    // Se valida que es anexo fibra
                        //    if (IsAnexoFibra)
                        //    {

                        //    }

                    }
                }
            }

            #region Ajuste a la decena
            // Llenar el canal con el ajuste a la decena si lo tiene
            var resultAjusteDecena = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T003")
                                     select busqueda;

            // Validar que tenga Ajuste a la decena
            if (resultAjusteDecena.Any())
            {
                listaCortes.Clear();

                listaCortes.Add(new PosCortes(6, 14, TiposFormateo.Decimal01));

                lineaTemp1CFI = string.Empty;
                lineaTemp1CFI = "1CFI||Ajuste Decena|";
                lineaTemp1CFI += Helpers.ExtraccionCamposSpool(listaCortes, resultAjusteDecena.FirstOrDefault());
                lineaTemp1CFI += "||";
                lineaTemp1CFI += Helpers.ExtraccionCamposSpool(listaCortes, resultAjusteDecena.FirstOrDefault());
                lineaTemp1CFI += "|| ";

                resultadoLinea1CFI.Add(Helpers.ValidarPipePipe(lineaTemp1CFI));
            }
            #endregion


            //string SVAS 

            Lineas1CFI = resultadoLinea1CFI;

            return Lineas1CFI;

            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1TFI
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string Logica1TFI(List<string> datosOriginales)
        {
            #region MapeoCanal1TFI
            string Linea1TFI = string.Empty;
            List<PosCortes> listaCortes = new List<PosCortes>();
            string valorTotal = string.Empty;
            string Subsidio = " ";

            if (IsResidencial || IsFibra)
            {
                var result = from busqueda in datosOriginales
                             where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T004")
                             select busqueda;

                if (result.Any())
                {
                    listaCortes.Add(new PosCortes(6, 14, TiposFormateo.Decimal01));
                    listaCortes.Add(new PosCortes(34, 14, TiposFormateo.Decimal01));
                    // Campos para sumar el Total
                    List<string> listCamposSumar = new List<string>();
                    listCamposSumar.Add(result.FirstOrDefault().Substring(6, 14));
                    listCamposSumar.Add(result.FirstOrDefault().Substring(34, 14));
                    valorTotal = Helpers.SumarCampos(listCamposSumar);

                    Linea1TFI = "1TFI| |TOTAL|";
                    Linea1TFI += Helpers.ExtraccionCamposSpool(listaCortes, result.FirstOrDefault()) + "|";
                    Linea1TFI += valorTotal + "|" + Subsidio + "| ";
                }
            }

            return Helpers.ValidarPipePipe(Linea1TFI);

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
        /// Linea que obtiene canal NTC3
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanalNTC3(List<string> datosOriginales)
        {
            #region Canal NTC3
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

        /// Metodo que obtiene la linea formateada de Canal 1CDP
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanal1CDP()
        {
            #region MapeoCanal1CDP
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
        /// Linea que obtiene canal NTC4
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanalNTC4(List<string> datosOriginales)
        {
            #region Canal NTC4
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

        /// Metodo que obtiene las lineas formateadas de Canal CART
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanalCART(List<string> datosOriginales)
        {
            #region MapeoCanalCART
            List<string> listaCART = new List<string>();
            List<string> camposCarta = new List<string>();

            List<string> cartas = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoAsignacionCartas, $"{Cuenta}");

            if (cartas != null && cartas.Count > 0)
            {
                foreach (string carta in cartas)
                {
                    camposCarta = carta.Split('|').ToList();
                    camposCarta.RemoveAt(0);
                    listaCART.Add($"CART|{Helpers.ListaCamposToLinea(camposCarta, '|')}| ");
                }
            }

            return listaCART;

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
            string resultado = string.Empty;
            string numeroConexion = string.Empty;
            string datos = string.Empty;
            string minutos1 = string.Empty;
            string minutos2 = string.Empty;

            if (IsLte || IsLteCorporativo)
            {
                var linea040000 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                  select busqueda;

                var linea040011 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040011")
                                  select busqueda;

                if (linea040011.Any())
                {
                    #region Busqueda Numero Conexion
                    foreach (var lineaDatos in linea040000)
                    {
                        if (lineaDatos.Substring(6, 20).Trim() != Cuenta)
                        {
                            numeroConexion = lineaDatos.Substring(6, 20).Trim();
                            break;
                        }
                    }
                    #endregion

                    string llaveCruce = linea040011.FirstOrDefault().Substring(6, 35).Trim().ToUpper();

                    var datosPromocionesLte = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoPromocionesLte, llaveCruce);

                    if (!string.IsNullOrEmpty(datosPromocionesLte))
                    {
                        datos = $"+Datos {datosPromocionesLte.Split('|').ElementAt(2)}";
                        minutos1 = $"+Minutos Nal {datosPromocionesLte.Split('|').ElementAt(3)}";
                        minutos2 = $"+Minutos WhatsApp {datosPromocionesLte.Split('|').ElementAt(4)}";
                    }

                    resultado = Helpers.ValidarPipePipe($"1OPL|{numeroConexion}|{datos}|{minutos1}|{minutos2}| ");
                }
            }

            return resultado;
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
            List<string> listaTemp1OMV = new List<string>();
            string lineaTem1OMV = string.Empty;
            List<PosCortes> listaCortes = new List<PosCortes>();


            var result14M902 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("14M902")
                               select busqueda;


            if (result14M902.Any())
            {
                foreach (var registroActual in result14M902)
                {

                    if (registroActual.Substring(71, 4).Trim() == "200" ||
                        registroActual.Substring(71, 4).Trim() == "217" ||
                        registroActual.Substring(71, 4).Trim() == "215" ||
                        registroActual.Substring(71, 4).Trim() == "216" ||
                        registroActual.Substring(71, 4).Trim() == "226" ||
                        registroActual.Substring(71, 4).Trim() == "222" ||
                        registroActual.Substring(71, 4).Trim() == "500")
                    {
                        lineaTem1OMV = String.Empty;

                        // Cortes
                        listaCortes.Add(new PosCortes(41, 10));
                        listaCortes.Add(new PosCortes(41, 10));
                        listaCortes.Add(new PosCortes(26, 8, TiposFormateo.Fecha09));
                        listaCortes.Add(new PosCortes(262, 8));
                        listaCortes.Add(new PosCortes(41, 10));
                        listaCortes.Add(new PosCortes(51, 10));
                        listaCortes.Add(new PosCortes(79, 30));

                        lineaTem1OMV = "1OMV|";
                        lineaTem1OMV += Helpers.ExtraccionCamposSpool(listaCortes, registroActual) + "|";

                        listaCortes.Clear();
                        listaCortes.Add(new PosCortes(139, 8));
                        lineaTem1OMV += Helpers.ExtraccionCamposSpool(listaCortes, registroActual) + " seg|";

                        listaCortes.Clear();
                        listaCortes.Add(new PosCortes(179, 14, TiposFormateo.Decimal01));
                        lineaTem1OMV += Helpers.ExtraccionCamposSpool(listaCortes, registroActual) + "| ";

                        listaTemp1OMV.Add(Helpers.ValidarPipePipe(lineaTem1OMV));
                    }
                }
            }

            Lineas1OMV = listaTemp1OMV;
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
            List<string> listaTemp1OOB = new List<string>();
            string lineaTemp1OOB = string.Empty;
            string numeroConexion = string.Empty;

            if (IsLte)
            {
                #region Busqueda Numero Conexion

                var linea040000 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                  select busqueda;

                if (linea040000.Any())
                {
                    foreach (var lineaDatos in linea040000)
                    {
                        if (lineaDatos.Substring(6, 20).Trim() != Cuenta)
                        {
                            numeroConexion = lineaDatos.Substring(6, 20).Trim();
                            break;
                        }
                    }
                }

                #endregion

                var result = from busqueda in datosOriginales
                             where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("04V")
                             select busqueda;

                if (result.Any())
                {
                    foreach (var registroActual in result)
                    {
                        if (registroActual.Substring(0, 6) != "04V000")
                        {
                            switch (registroActual.Substring(3, 1))
                            {
                                case "A":
                                    lineaTemp1OOB = "1OOB|" + numeroConexion + "|Facebook|" + registroActual.Substring(101, 11).Trim() + "| ";
                                    break;
                                case "B":
                                    lineaTemp1OOB = "1OOB|" + numeroConexion + "|Whatsapp|" + registroActual.Substring(101, 11).Trim() + "| ";
                                    break;
                                case "C":
                                    lineaTemp1OOB = "1OOB|" + numeroConexion + "|Email|" + registroActual.Substring(101, 11).Trim() + "| ";
                                    break;
                                case "D":
                                    lineaTemp1OOB = "1OOB|" + numeroConexion + "|Spotify|" + registroActual.Substring(101, 11).Trim() + "| ";
                                    break;
                                case "E":
                                    lineaTemp1OOB = "1OOB|" + numeroConexion + "|" + registroActual.Substring(101, 129).Trim() + "| | ";
                                    break;
                                default:
                                    break;
                            }

                            if (!string.IsNullOrEmpty(lineaTemp1OOB))
                            {
                                listaTemp1OOB.Add(Helpers.ValidarPipePipe(lineaTemp1OOB));
                            }
                        }
                    }
                }
            }

            Linea1OOB = listaTemp1OOB;

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
        /// Linea que obtiene canal 1PLA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        public string MapeoCanal1PLA(List<string> datosOriginales)
        {
            #region Canal 1PLA
            string Linea1PLA = string.Empty;
            List<PosCortes> listaCortes = new List<PosCortes>();

            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040011")
                         select busqueda;

            // Resultado para sacar valor
            var result02S000 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02S000")
                               select busqueda;

            string valorDinero = string.Empty;

            if (result02S000.Any())
            {
                listaCortes.Clear();
                //listaCortes.Add(new PosCortes(48, 14, TiposFormateo.Decimal01));
                valorDinero = Helpers.ExtraccionCamposSpool(listaCortes, result02S000.FirstOrDefault());
            }


            if (result.Any())
            {
                // Si esta en 
                if (Is1OOA)
                {
                    listaCortes.Add(new PosCortes(6, 70));
                    listaCortes.Add(new PosCortes(76, 8));
                    listaCortes.Add(new PosCortes(84, 8));
                    listaCortes.Add(new PosCortes(92, 25));
                    listaCortes.Add(new PosCortes(117, 50));
                    listaCortes.Add(new PosCortes(167, 10));

                    Linea1PLA = "1PLA|";
                    Linea1PLA += Helpers.ExtraccionCamposSpool(listaCortes, result.FirstOrDefault()) + "||";

                    if (result02S000.Any())
                    {
                        Linea1PLA += valorDinero + "| ";
                    }
                    else
                    {
                        Linea1PLA += "|| ";
                    }
                }
                else if (Is1ODC)
                {
                    Linea1PLA = "1PLA|Consumo Voz Adicional| + " + valorDinero + "||||| ";
                }
            }

            return Helpers.ValidarPipePipe(Linea1PLA);
            #endregion
        }
        /// Metodo que obtiene las lineas formateadas de Canal NTC5
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanalNTC5(List<string> datosOriginales)
        {
            #region MapeoCanalNTC5
            List<string> resultado = new List<string>();

            var linea30003 = from busqueda in datosOriginales
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("30003")
                             select busqueda;

            if (linea30003.Any())
            {
                foreach (var lineaDet in linea30003)
                {
                    resultado.Add(Helpers.ValidarPipePipe($"NTC5|{linea30003.FirstOrDefault().Substring(5, 2).Trim()}|{linea30003.FirstOrDefault().Substring(7, 5).Trim()}|" +
                                $"{linea30003.FirstOrDefault().Substring(12, 20).Trim()}| "));
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
                string interesResidencial = string.Empty;

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
                    Int64 valorXPagar = valorFinanciado - saldoRestante;
                    string valorXPagarFormat = Helpers.FormatearCampos(TiposFormateo.Decimal01, valorXPagar.ToString());

                    if (camposFinanciacion.Length > 1 && camposFinanciacion[0].Trim() == "B" && IsResidencial)
                    {
                        resultado.Add($"1IIB|{descripcion}|{camposPlanPago[0]}|{cuota}|{valorFinanciadoFormat}|{saldoRestanteFormat}|{valorXPagarFormat}|****interes_financiacion****|");
                    }
                    else if (camposFinanciacion.Length > 1 && camposFinanciacion[0].Trim() == "C" && IsResidencial)
                    {
                        interesResidencial = interesResidencial == "0" ? interes : interesResidencial;
                    }
                    else if (camposFinanciacion.Length > 1 && camposFinanciacion[0].Trim() == "F" && (IsResidencial || IsLteCorporativo))
                    {
                        resultado.Add($"1IIF|{descripcion}|{camposPlanPago[0]}|{cuota}|{valorFinanciadoFormat}|{saldoRestanteFormat}|{valorXPagarFormat}| ");
                    }
                    else if (IsResidencial)
                    {
                        resultado.Add($"1III|{descripcion}|{camposPlanPago[0]}|{cuota}|{valorFinanciadoFormat}|{saldoRestanteFormat}|{valorXPagarFormat}|{interes}| ");
                    }
                    else if (valorFinanciado > 0)
                    {
                        resultado.Add($"1III|{descripcion}|{valorFinanciadoFormat}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, item.Substring(16, 14))}|{saldoRestanteFormat}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, item.Substring(44, 14))}|{cuota}|{interes}| ");
                    }

                }

                foreach (var item in resultado)
                {
                    if (item.Contains(""))
                    {
                        item.Replace("****interes_financiacion****", interes);
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
        public IEnumerable<string> FormarPaqueteEEE(List<string> datosOriginales)
        {
            #region FormarPaqueteEEE
            List<string> resultado = new List<string>();
            string llaveConcepto = string.Empty;
            string llaveBusquedaDescripcion = string.Empty;
            string descripcionTitulo = string.Empty;
            string llaveBusquedaNit = string.Empty;
            string nit = string.Empty;
            string descripcionSubtitulo = string.Empty;
            string lineaNegocio = string.Empty;
            string @base = string.Empty;
            string iva = string.Empty;
            string impuesto = "00";
            string restaImpuesto = "-00";
            //LLeva paquete ADN1- 1EE1 - 1EE2 - 1EE3

            if (IsResidencial || IsFibra)
            {
                #region Anexos ETB
                var lineasAnexosETB = from busqueda in datosOriginales
                                      where busqueda.Length > 6 &&
                                      busqueda.Substring(0, 6).Equals("11C123") ||
                                      busqueda.Substring(0, 6).Equals("11C218") ||
                                      busqueda.Substring(0, 6).Equals("11C208") ||
                                      busqueda.Substring(0, 6).Equals("11C201") ||
                                      busqueda.Substring(0, 6).Equals("11C116")
                                      where !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                                      select busqueda;

                var lineas12MAnexosETB = from busqueda in datosOriginales
                                         where busqueda.Length > 6 &&
                                         busqueda.Substring(0, 6).Equals("12M123") ||
                                         busqueda.Substring(0, 6).Equals("12M218") ||
                                         busqueda.Substring(0, 6).Equals("12M208") ||
                                         busqueda.Substring(0, 6).Equals("12M201") ||
                                         busqueda.Substring(0, 6).Equals("12M116")
                                         select busqueda;

                if (lineasAnexosETB.Any() && lineas12MAnexosETB.Any())
                {
                    foreach (var linea in lineasAnexosETB)
                    {
                        resultado.Add($"ADN1|-|ANEXO ETB| | | | ");

                        #region Datos 1EE1
                        llaveConcepto = linea.Substring(6, 10);

                        llaveBusquedaDescripcion = $"CODT{Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoConfiguracionLLavesDoc1, llaveConcepto).Split('|').ElementAt(13)}";

                        descripcionTitulo = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveBusquedaDescripcion).FirstOrDefault()?.Substring(11).Trim() ?? "";

                        @base = linea.Substring(16, 14).Trim().TrimStart('0');
                        iva = linea.Substring(44, 14).Trim().TrimStart('0');

                        resultado.Add(Helpers.ValidarPipePipe($"1EE1|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, descripcionTitulo)}" +
                            $"|{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, iva)}|" +
                            $"{Helpers.SumarCampos(new List<string> { @base, iva })}| | | "));
                        #endregion

                        #region Datos 1EE2
                        llaveBusquedaDescripcion = $"CODT{Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoConfiguracionLLavesDoc1, llaveConcepto).Split('|').ElementAt(7)}";

                        descripcionSubtitulo = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveBusquedaDescripcion).FirstOrDefault()?.Substring(11).Trim() ?? "";

                        resultado.Add(Helpers.ValidarPipePipe($"1EE2|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, descripcionSubtitulo)}|LD| "));
                        #endregion

                        #region Datos 1EE3

                        var datosDetalles = from busqueda in lineas12MAnexosETB
                                            where busqueda.Substring(3, 3) == linea.Substring(3, 3)
                                            select busqueda;

                        foreach (var lineaDet in datosDetalles)
                        {
                            resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.Fecha12, lineaDet.Substring(6, 10))}|" +
                                $"{Helpers.FormatearCampos(TiposFormateo.HoraMinutoSegundo, lineaDet.Substring(14, 6))}|{lineaDet.Substring(20, 10).Trim()}|" +
                                $"{lineaDet.Substring(33, 10)}|{lineaDet.Substring(96, 11)}|{lineaDet.Substring(67, 1)}:00|{Helpers.FormatearCampos(TiposFormateo.Decimal01, lineaDet.Substring(47, 9)).Replace("$", "").Trim()}|" +
                                $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, lineaDet.Substring(56, 9)).Replace("$", "").Trim()}| "));
                        }

                        resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, descripcionTitulo)}" +
                            $"|{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, iva)}|" +
                            $"{Helpers.SumarCampos(new List<string> { @base, iva })}| | | "));

                        #endregion
                    }
                }
                #endregion

                #region Anexos Otros Operadores
                var lineasAnexosOtrosOperadores = from busqueda in datosOriginales
                                                  where busqueda.Length > 6 &&
                                                  busqueda.Substring(0, 6).Equals("11C532") ||
                                                  busqueda.Substring(0, 6).Equals("11C533") ||
                                                  busqueda.Substring(0, 6).Equals("11C535") ||
                                                  busqueda.Substring(0, 6).Equals("11C536") ||
                                                  busqueda.Substring(0, 6).Equals("11C538") ||
                                                  busqueda.Substring(0, 6).Equals("11C540") ||
                                                  busqueda.Substring(0, 6).Equals("11C544") ||
                                                  busqueda.Substring(0, 6).Equals("11C545") ||
                                                  busqueda.Substring(0, 6).Equals("11C547") ||
                                                  busqueda.Substring(0, 6).Equals("11C548") ||
                                                  busqueda.Substring(0, 6).Equals("11C552") ||
                                                  busqueda.Substring(0, 6).Equals("11C554") ||
                                                  busqueda.Substring(0, 6).Equals("11C555") ||
                                                  busqueda.Substring(0, 6).Equals("11C650") ||
                                                  busqueda.Substring(0, 6).Equals("11C552") ||
                                                  busqueda.Substring(0, 6).Equals("11C651") ||
                                                  busqueda.Substring(0, 6).Equals("11C652") ||
                                                  busqueda.Substring(0, 6).Equals("11C653") ||
                                                  busqueda.Substring(0, 6).Equals("11C654") ||
                                                  busqueda.Substring(0, 6).Equals("11C700") ||
                                                  busqueda.Substring(0, 6).Equals("11C701") ||
                                                  busqueda.Substring(0, 6).Equals("11C702") ||
                                                  busqueda.Substring(0, 6).Equals("11C550") ||
                                                  busqueda.Substring(0, 6).Equals("11C581")
                                                  where !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                                                  select busqueda;

                var lineas12MAnexosOtrosOperadores = from busqueda in datosOriginales
                                                     where busqueda.Length > 6 &&
                                                     busqueda.Substring(0, 6).Equals("12M532") ||
                                                     busqueda.Substring(0, 6).Equals("12M533") ||
                                                     busqueda.Substring(0, 6).Equals("12M535") ||
                                                     busqueda.Substring(0, 6).Equals("12M536") ||
                                                     busqueda.Substring(0, 6).Equals("12M538") ||
                                                     busqueda.Substring(0, 6).Equals("12M540") ||
                                                     busqueda.Substring(0, 6).Equals("12M544") ||
                                                     busqueda.Substring(0, 6).Equals("12M545") ||
                                                     busqueda.Substring(0, 6).Equals("12M547") ||
                                                     busqueda.Substring(0, 6).Equals("12M548") ||
                                                     busqueda.Substring(0, 6).Equals("12M552") ||
                                                     busqueda.Substring(0, 6).Equals("12M554") ||
                                                     busqueda.Substring(0, 6).Equals("12M555") ||
                                                     busqueda.Substring(0, 6).Equals("12M650") ||
                                                     busqueda.Substring(0, 6).Equals("12M552") ||
                                                     busqueda.Substring(0, 6).Equals("12M651") ||
                                                     busqueda.Substring(0, 6).Equals("12M652") ||
                                                     busqueda.Substring(0, 6).Equals("12M653") ||
                                                     busqueda.Substring(0, 6).Equals("12M654") ||
                                                     busqueda.Substring(0, 6).Equals("12M700") ||
                                                     busqueda.Substring(0, 6).Equals("12M701") ||
                                                     busqueda.Substring(0, 6).Equals("12M702") ||
                                                     busqueda.Substring(0, 6).Equals("12M550") ||
                                                     busqueda.Substring(0, 6).Equals("12M581")
                                                     select busqueda;

                var linea30004 = from busqueda in datosOriginales
                                 where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("30004")
                                 select busqueda;

                if (lineasAnexosOtrosOperadores.Any() && lineas12MAnexosOtrosOperadores.Any())
                {
                    resultado.Add($"ADN1|-|ANEXO OTROS OPERADORES| | | | ");

                    #region Datos 1EE1
                    llaveConcepto = lineasAnexosOtrosOperadores.FirstOrDefault().Substring(6, 10);

                    llaveBusquedaDescripcion = $"CODT{Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoConfiguracionLLavesDoc1, llaveConcepto).Split('|').ElementAt(13)}";

                    descripcionTitulo = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveBusquedaDescripcion).FirstOrDefault()?.Substring(11).Trim() ?? "";

                    llaveBusquedaNit = $"OPER{Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoConfiguracionLLavesDoc1, llaveConcepto).Split('|').ElementAt(8)}";

                    nit = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveBusquedaNit).FirstOrDefault()?.Substring(11).Trim() ?? "";

                    lineaNegocio = $"{descripcionTitulo} NIT: {nit}";

                    @base = lineasAnexosOtrosOperadores.FirstOrDefault().Substring(16, 14).Trim().TrimStart('0');
                    iva = lineasAnexosOtrosOperadores.FirstOrDefault().Substring(44, 14).Trim().TrimStart('0');

                    if (linea30004.Any())
                    {
                        restaImpuesto = $"-{linea30004.FirstOrDefault().Substring(12, 20).Trim().TrimStart('0')}";
                        impuesto = linea30004.FirstOrDefault().Substring(12, 20).Trim().TrimStart('0');

                        if (Convert.ToDecimal(Helpers.FormatearCampos(TiposFormateo.Decimal03, impuesto)) > Convert.ToDecimal(Helpers.FormatearCampos(TiposFormateo.Decimal03, iva)))
                        {
                            restaImpuesto = "-00";
                            impuesto = "00";
                        }
                    }

                    resultado.Add(Helpers.ValidarPipePipe($"1EE1|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, lineaNegocio)}" +
                        $"|{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|{Helpers.SumarCampos(new List<string> { iva, restaImpuesto })}|" +
                        $"{Helpers.SumarCampos(new List<string> { @base, iva, restaImpuesto, impuesto })}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, impuesto)}| | "));
                    #endregion

                    #region Datos 1EE2
                    resultado.Add(Helpers.ValidarPipePipe($"1EE2|-| | "));
                    #endregion

                    #region Datos 1EE3

                    foreach (var linea in lineas12MAnexosOtrosOperadores)
                    {
                        resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.Fecha12, linea.Substring(6, 10))}|" +
                            $"{Helpers.FormatearCampos(TiposFormateo.HoraMinutoSegundo, linea.Substring(14, 6))}|{linea.Substring(20, 10).Trim()}|" +
                            $"{linea.Substring(33, 10).Trim()}|{linea.Substring(96, 11).Trim()}|{linea.Substring(67, 1)}:00|{Helpers.FormatearCampos(TiposFormateo.Decimal01, linea.Substring(47, 9)).Replace("$", "").Trim()}|" +
                            $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, linea.Substring(56, 9)).Replace("$", "").Trim()}| "));
                    }

                    resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, lineaNegocio)}" +
                        $"|{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|{Helpers.SumarCampos(new List<string> { iva, restaImpuesto })}|" +
                        $"{Helpers.SumarCampos(new List<string> { @base, iva, restaImpuesto, impuesto })}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, impuesto)}| | "));

                    #endregion
                }

                #endregion
            }

            return resultado;
            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1OOA
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1OOA(List<string> datosOriginales)
        {
            #region MapeoCanal1OOA
            string resultado = string.Empty;
            string numeroConexion = string.Empty;

            if (IsLte || IsLteCorporativo)
            {
                var linea040000 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                  select busqueda;

                var linea040011 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040011")
                                  select busqueda;

                if (linea040011.Any())
                {
                    #region Busqueda Numero Conexion
                    foreach (var lineaDatos in linea040000)
                    {
                        if (lineaDatos.Substring(6, 20).Trim() != Cuenta)
                        {
                            numeroConexion = lineaDatos.Substring(6, 20).Trim();
                            break;
                        }
                    }
                    #endregion

                    resultado = Helpers.ValidarPipePipe($"1OOA|{numeroConexion}|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea040011.FirstOrDefault().Substring(6, 35).Trim())}|" +
                        $"{linea040011.FirstOrDefault().Substring(76, 8).Trim()}|{linea040011.FirstOrDefault().Substring(84, 8).Trim()}|" +
                        $"{Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea040011.FirstOrDefault().Substring(92, 25).Trim())}|" +
                        $"{Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea040011.FirstOrDefault().Substring(117, 50).Trim())}|" +
                        $"{linea040011.FirstOrDefault().Substring(167, 10).Trim()}| ");
                }
            }

            return resultado;
            #endregion
        }


        /// Metodo que obtiene la linea formateada de Canal 1HIS
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1HIS(List<string> datosOriginales)
        {
            #region MapeoCanal1HIS
            string resultado = string.Empty;
            string numeroConexion = string.Empty;

            var linea10000 = from busqueda in datosOriginales
                             where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                             select busqueda;

            var linea040000 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                              select busqueda;

            var linea070001 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("070001")
                              select busqueda;

            if (IsLte || IsLteCorporativo)
            {
                #region Busqueda Numero Conexion
                foreach (var lineaDatos in linea040000)
                {
                    if (lineaDatos.Substring(6, 20).Trim() != Cuenta)
                    {
                        numeroConexion = lineaDatos.Substring(6, 20).Trim();
                        break;
                    }
                }
                #endregion

                if (linea070001.Any())
                {
                    string mesActual = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, linea070001.FirstOrDefault().Substring(6, 8).Trim())) * 60 / 10).ToString();
                    string promedio = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, linea070001.FirstOrDefault().Substring(62, 8).Trim())) * 60 / 10).ToString();

                    resultado = Helpers.ValidarPipePipe($"1HIS|{numeroConexion}|Mes Actual: {mesActual}|{ArmarMesesHistograma(Helpers.FormatearCampos(TiposFormateo.Fecha01, linea10000.FirstOrDefault().Substring(168, 8)))}|" +
                        $"{ArmarValoresHistograma(linea070001.FirstOrDefault(), "2")}|Promedio: {promedio}| ");
                }
                else
                {
                    resultado = Helpers.ValidarPipePipe($"1HIS|{numeroConexion}|Mes Actual: 0|{ArmarMesesHistograma(Helpers.FormatearCampos(TiposFormateo.Fecha01, linea10000.FirstOrDefault().Substring(168, 8)))}|0|0|0|0|0|0|Promedio: 0| ");
                }
            }

            return resultado;
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
            IEnumerable<string> listalinea1EE3 = null;
            List<string> listaTemp1EE3 = new List<string>();
            string lineaTemp1EE3 = string.Empty;
            string periodo = string.Empty;


            var result11C304 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("11C304")
                               select busqueda;

            var result12M123 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("12M123")
                               select busqueda;

            if (result11C304.Any())
            {

            }

            return listalinea1EE3;
            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1HDT
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1HDT(List<string> datosOriginales)
        {
            #region MapeoCanal1HDT
            string resultado = string.Empty;
            string numeroConexion = string.Empty;

            var linea10000 = from busqueda in datosOriginales
                             where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                             select busqueda;

            var linea040000 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                              select busqueda;

            var linea070002 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("070002")
                              select busqueda;

            if (IsLte || IsLteCorporativo)
            {
                #region Busqueda Numero Conexion
                foreach (var lineaDatos in linea040000)
                {
                    if (lineaDatos.Substring(6, 20).Trim() != Cuenta)
                    {
                        numeroConexion = lineaDatos.Substring(6, 20).Trim();
                        break;
                    }
                }
                #endregion

                if (linea070002.Any())
                {
                    string mesActual = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, linea070002.FirstOrDefault().Substring(6, 6).Trim())) / 10).ToString();
                    string promedio = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, linea070002.FirstOrDefault().Substring(48, 6).Trim())) / 10).ToString();

                    resultado = Helpers.ValidarPipePipe($"1HDT|{numeroConexion}|Mes Actual: {mesActual}|{ArmarMesesHistograma(Helpers.FormatearCampos(TiposFormateo.Fecha01, linea10000.FirstOrDefault().Substring(168, 8)))}|" +
                        $"{ArmarValoresHistograma(linea070002.FirstOrDefault(), "3")}|Promedio: {promedio}| ");
                }
            }


            return resultado;
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
            string resultado = string.Empty;

            var linea13M317 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("13M317")
                              select busqueda;

            if (linea13M317.Any())
            {
                string total = linea13M317.FirstOrDefault().Substring(42, 14).Trim();
                string subtotal = linea13M317.FirstOrDefault().Substring(56, 14).Trim();

                resultado = Helpers.ValidarPipePipe($"1FFA|Total|{Helpers.FormatearCampos(TiposFormateo.Decimal01, total)}" +
                    $"|{Helpers.FormatearCampos(TiposFormateo.Decimal01, subtotal)}|{Helpers.SumarCampos(new List<string> { total, subtotal })}| ");
            }

            return resultado;
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
            string numeroConexion = string.Empty;
            List<string> camposSumar = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();

            if (Is1ODC)
            {
                #region Consultas
                var linea040000 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                  select busqueda;

                var linea888888 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("888888")
                                  select busqueda;

                var linea3000401 = from busqueda in datosOriginales
                                   where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000401")
                                   select busqueda;

                var linea3000404 = from busqueda in datosOriginales
                                   where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000404")
                                   select busqueda;

                var linea11C901 = from busqueda in datosOriginales
                                  where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("11C901")
                                  select busqueda;

                #endregion

                #region Formato
                foreach (var lineaDatos in linea040000)
                {
                    if (lineaDatos.Substring(6, 20).Trim() != Cuenta)
                    {
                        numeroConexion = lineaDatos.Substring(6, 20).Trim();
                        break;
                    }
                }

                if (linea888888.Any())
                {
                    foreach (var registroActual in linea888888)
                    {
                        camposSumar.Add(registroActual.Substring(82, 14));
                    }
                }

                Lineas1DET = "1DET|";
                Lineas1DET += numeroConexion + "|";
                Lineas1DET += "Total|";
                Lineas1DET += Helpers.SumarCampos(camposSumar) + "|";

                if (linea3000401.Any())
                {
                    listaCortes.Add(new PosCortes(12, 20, TiposFormateo.Decimal01));
                    Lineas1DET += Helpers.ExtraccionCamposSpool(listaCortes, linea3000401.FirstOrDefault()) + "|";
                    listaCortes.Clear();

                    camposSumar.Add(linea3000401.FirstOrDefault().Substring(12, 20).Trim());
                }
                else
                {
                    Lineas1DET += "|";
                }

                if (linea3000404.Any())
                {
                    listaCortes.Add(new PosCortes(12, 20, TiposFormateo.Decimal01));
                    Lineas1DET += Helpers.ExtraccionCamposSpool(listaCortes, linea3000404.FirstOrDefault()) + "|";
                    listaCortes.Clear();

                    camposSumar.Add(linea3000404.FirstOrDefault().Substring(12, 20).Trim());
                }
                else
                {
                    Lineas1DET += "|";
                }

                Lineas1DET += Helpers.SumarCampos(camposSumar) + "|";

                if (linea11C901.Any())
                {
                    foreach (var registroACtual11C901 in linea11C901)
                    {
                        if (registroACtual11C901.Substring(128, 19).Contains("-"))
                        {
                            Lineas1DET += registroACtual11C901.Substring(128, 6) + "|";
                            listaCortes.Clear();
                            listaCortes.Add(new PosCortes(128, 8, TiposFormateo.Fecha06));
                            string primerFecha = Helpers.ExtraccionCamposSpool(listaCortes, registroACtual11C901);
                            listaCortes.Clear();

                            listaCortes.Add(new PosCortes(139, 8, TiposFormateo.Fecha06));
                            string segundaFecha = Helpers.ExtraccionCamposSpool(listaCortes, registroACtual11C901);
                            Lineas1DET += primerFecha + " a " + segundaFecha + "| ";
                            break;
                        }
                    }
                }
                else
                {
                    Lineas1DET += "|| ";
                }
                #endregion
            }

            return Helpers.ValidarPipePipe(Lineas1DET);
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas formateadas de Canal 1LLL
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1LLL()
        {
            #region MapeoCanal1LLL
            string Lineas1LLL = string.Empty;

            if (!IsResidencial && !IsFibra && MesMora > 1 && MesMora < 4)
            {
                Lineas1LLL = $"1LLL|{Variables.RxGeneral.TextoMora_1}| ";
            }
            else if (!IsResidencial && !IsFibra && MesMora >= 4)
            {
                Lineas1LLL = $"1LLL|{Variables.RxGeneral.TextoMora_2}| ";
            }
            else if ((IsResidencial || IsFibra) && MesMora == 2)
            {
                Lineas1LLL = $"1LLL|{Variables.RxGeneral.TextoMora_3}| ";
            }
            else if ((IsResidencial || IsFibra) && MesMora == 3)
            {
                Lineas1LLL = $"1LLL|{Variables.RxGeneral.TextoMora_4}| ";
            }
            else if ((IsResidencial || IsFibra) && MesMora >= 4)
            {
                Lineas1LLL = $"1LLL|{Variables.RxGeneral.TextoMora_5}| ";
            }

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

            var result3000404 = from busqueda in datosOriginales
                                where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000404")
                                select busqueda;

            if (result3000404.Any())
            {
                Lineas1CIC = result3000404.FirstOrDefault();
                Lineas1CIC = $"1CIC| |Impuesto Consumo {Helpers.FormatearCampos(TiposFormateo.Decimal01, Lineas1CIC.Substring(12, 20).TrimStart('0').Trim())}| ";
            }

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
            string resultado = string.Empty;

            var linea20M = from busqueda in datosOriginales
                           where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("20M")
                           select busqueda;

            if (linea20M.Any())
            {
                string @base = linea20M.FirstOrDefault().Substring(16, 14).Trim();
                string iva = linea20M.FirstOrDefault().Substring(44, 14).Trim();
                string impuesto = linea20M.FirstOrDefault().Substring(58, 14).Trim();

                string llaveCruceProducto = $"CODT{Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoConfiguracionLLavesDoc1, linea20M.FirstOrDefault().Substring(6, 10).Trim()).Split('|').ElementAt(13).Trim()}";
                string llaveCruceSubProducto = $"CODT{Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoConfiguracionLLavesDoc1, linea20M.FirstOrDefault().Substring(6, 10).Trim()).Split('|').ElementAt(11).Trim()}";

                string producto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruceProducto).FirstOrDefault().Substring(11).Trim();
                string subProducto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruceSubProducto).FirstOrDefault().Substring(11).Trim();

                resultado = Helpers.ValidarPipePipe($"1RRT|Total|{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|" +
                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, iva)}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, impuesto)}|" +
                    $"{Helpers.SumarCampos(new List<string> { @base, iva, impuesto })}|{producto}|{subProducto}| ");
            }

            return resultado;
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
            string resultado = string.Empty;

            var linea20M = from busqueda in datosOriginales
                           where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("20M")
                           select busqueda;

            if (linea20M.Any())
            {
                string llaveCruce = $"CODF{linea20M.FirstOrDefault().Substring(6, 10).Trim()}";
                string concepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault().Substring(14).Trim();
                string @base = linea20M.FirstOrDefault().Substring(16, 14).Trim();
                string iva = linea20M.FirstOrDefault().Substring(44, 14).Trim();
                string impuesto = linea20M.FirstOrDefault().Substring(58, 14).Trim();

                resultado = Helpers.ValidarPipePipe($"1RRR|{concepto}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|" +
                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, iva)}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, impuesto)}|" +
                    $"{Helpers.FormatearCampos(TiposFormateo.Fecha09, linea20M.FirstOrDefault().Substring(86, 8).Trim())}|" +
                    $"{Helpers.FormatearCampos(TiposFormateo.HoraMinuto, linea20M.FirstOrDefault().Substring(94, 4).Trim())}|" +
                    $"{linea20M.FirstOrDefault().Substring(122, 50).Trim()}| ");
            }

            return resultado;
            #endregion
        }

        private string FormateoCanal1CMP(List<string> datosOriginales)
        {
            #region FormateoCanal1CMP
            string resultado = string.Empty;

            var result = from busqueda in datosOriginales
                         where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("060000")
                         select busqueda;

            if (result.Any())
            {
                var valorMinutosConsumo = result.FirstOrDefault().Substring(22, 8).TrimStart('0').Trim();

                if (IsResidencial || IsFibra)
                {
                    var resultCanal = from busqueda in datosOriginales
                                      where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                      select busqueda;

                    string minutosPlan = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"MINC{resultCanal.FirstOrDefault().Substring(133, 10).Trim()}").FirstOrDefault()?.Substring(14).Trim() ?? "";

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

            if (linea60000.Any())
            {
                if (IsResidencial || IsFibra)
                {
                    var linea10000 = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                                     select busqueda;

                    var linea40000 = from busqueda in datosOriginales
                                     where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                     select busqueda;

                    string valor = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"VMIN{linea40000.FirstOrDefault().Substring(133, 10)}").FirstOrDefault()?.Trim() ?? "";

                    var valorPromedio = linea60000.FirstOrDefault().Substring(78, 8).TrimStart('0').Trim() == "" ? "0" : linea60000.FirstOrDefault().Substring(78, 8).TrimStart('0').Trim();

                    resultado += Helpers.ValidarPipePipe($"1PPP|Promedio Historico|{valorPromedio}| |" +
                        $"{ArmarMesesHistograma(Helpers.FormatearCampos(TiposFormateo.Fecha01, linea10000.FirstOrDefault().Substring(168, 8)))}| |" +
                        $"{ArmarValoresHistograma(linea60000.FirstOrDefault(), "1")}| |{GetValorMinutoPlan(linea40000.FirstOrDefault(), valor)}| ");
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

                    meses.Add(Helpers.FormatearCampos(TiposFormateo.LetraCapital, new DateTime(DateTime.Now.Year, i, 1).ToString("MMM", culture).Replace(".", string.Empty)));
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

        private string ArmarValoresHistograma(string pLineaDatos, string pTipoCortes)
        {
            #region ArmarValoresHistograma
            string resultado = string.Empty;
            string valor1 = string.Empty;
            string valor2 = string.Empty;
            string valor3 = string.Empty;
            string valor4 = string.Empty;
            string valor5 = string.Empty;
            string valor6 = string.Empty;

            switch (pTipoCortes)
            {
                case "1": //1PPP
                    valor1 = pLineaDatos.Substring(30, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(30, 8).TrimStart('0');
                    valor2 = pLineaDatos.Substring(38, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(38, 8).TrimStart('0');
                    valor3 = pLineaDatos.Substring(46, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(46, 8).TrimStart('0');
                    valor4 = pLineaDatos.Substring(54, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(54, 8).TrimStart('0');
                    valor5 = pLineaDatos.Substring(62, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(62, 8).TrimStart('0');
                    valor6 = pLineaDatos.Substring(70, 8).TrimStart('0') == "" ? "0" : pLineaDatos.Substring(70, 8).TrimStart('0');
                    break;

                case "2": //1HIS
                    valor1 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(14, 8).Trim())) * 60 / 10).ToString();
                    valor2 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(22, 8).Trim())) * 60 / 10).ToString();
                    valor3 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(30, 8).Trim())) * 60 / 10).ToString();
                    valor4 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(38, 8).Trim())) * 60 / 10).ToString();
                    valor5 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(46, 8).Trim())) * 60 / 10).ToString();
                    valor6 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(54, 8).Trim())) * 60 / 10).ToString();
                    break;

                case "3": //1HDT
                    valor1 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(12, 6).Trim())) / 10).ToString();
                    valor2 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(18, 6).Trim())) / 10).ToString();
                    valor3 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(24, 6).Trim())) / 10).ToString();
                    valor4 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(30, 6).Trim())) / 10).ToString();
                    valor5 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(36, 6).Trim())) / 10).ToString();
                    valor6 = (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, pLineaDatos.Substring(42, 6).Trim())) / 10).ToString();
                    break;
            }

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

        private List<string> FormateoCanalADN1(List<string> datosOriginales)
        {
            #region FormateoCanalADN1
            List<string> resultado = new List<string>();

            var linea40000 = from busqueda in datosOriginales
                             where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                             select busqueda;

            if (IsLte || IsLteCorporativo && GetTipo(linea40000.FirstOrDefault().Substring(6, 20).Trim()) != "Cuenta")
            {
                resultado.Add(Helpers.ValidarPipePipe($"ADN1|{linea40000.FirstOrDefault().Substring(6, 20).Trim()}|{GetTipo(linea40000.FirstOrDefault().Substring(6, 20).Trim())}|" +
                    $"{Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea40000.FirstOrDefault().Substring(76, 39).Trim())}| | | "));
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

        private string FormateoCanal1OMT(List<string> datosOriginales)
        {
            #region FormateoCanal1OMT

            string resultado = string.Empty;
            List<PosCortes> listaCortes = new List<PosCortes>();
            List<string> camposSumar = new List<string>();
            string llave = string.Empty;
            string camposValores = string.Empty;


            var result040000 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                               select busqueda;

            var result11C901 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("11C901")
                               select busqueda;

            var result040011 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040011")
                               select busqueda;

            if (IsLte || IsLteCorporativo)
            {
                // Si trae este canal se pinta el 1OMT
                if (result040011.Any())
                {
                    resultado = "1OMT|";

                    if (result040000.Any())
                    {
                        foreach (var registroActual in result040000)
                        {
                            llave = registroActual.Substring(6, 12).Trim();
                        }
                    }

                    resultado += llave + "|";

                    if (result11C901.Any())
                    {
                        foreach (var registroActual in result11C901)
                        {
                            if (registroActual.Substring(281, 20) == "Pago por Uso Pospago")
                            {
                                // Cortes
                                listaCortes.Add(new PosCortes(16, 14, TiposFormateo.Decimal01));
                                listaCortes.Add(new PosCortes(44, 14, TiposFormateo.Decimal01));
                                listaCortes.Add(new PosCortes(172, 14, TiposFormateo.Decimal01));
                                camposValores = Helpers.ExtraccionCamposSpool(listaCortes, registroActual);
                                camposSumar.Add(registroActual.Substring(16, 14).Trim());
                                camposSumar.Add(registroActual.Substring(44, 14).Trim());
                                camposSumar.Add(registroActual.Substring(172, 14).Trim());
                                break;
                            }
                            else
                            {
                                camposValores = "$ 0,00|$ 0,00|$ 0,00|$ 0,00";
                            }
                        }
                    }

                    resultado += camposValores + "|";
                    resultado += Helpers.SumarCampos(camposSumar) + "|";
                    resultado += "Etb| ";
                }
            }

            return Helpers.ValidarPipePipe(resultado);
            #endregion
        }

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
                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, linea29000.FirstOrDefault().Substring(29, 20).Trim())}|{linea29000.FirstOrDefault().Substring(49).Trim()}| ");
            }

            return resultado;
            #endregion
        }

        private string FormateoCanal1ODC(List<string> datosOriginales)
        {
            #region FormateoCanal1ODC
            string resultado = string.Empty;
            string cuentaConexion = string.Empty;
            List<string> sumaValoresBase = new List<string>();
            List<string> sumaValoresTotal = new List<string>();

            var linea888888 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("888888")
                              select busqueda;

            var linea02T016 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T016")
                              select busqueda;

            if (IsLte || IsLteCorporativo)
            {
                if (linea888888.Any())
                {
                    foreach (var lineaDetalle in linea888888)
                    {
                        if (lineaDetalle.Substring(82, 1) != "-")
                        {
                            sumaValoresBase.Add(lineaDetalle.Substring(82, 14).Trim());
                            sumaValoresTotal.Add(lineaDetalle.Substring(110, 14).Trim());
                            cuentaConexion = lineaDetalle.Substring(6, 10);
                        }
                    }

                    if (linea02T016.Any())
                    {
                        sumaValoresBase.Add($"-{linea02T016.FirstOrDefault().Substring(6, 14).Trim()}");
                        sumaValoresTotal.Add($"-{linea02T016.FirstOrDefault().Substring(6, 14).Trim()}");
                    }

                    var linea3000401 = from busqueda in datosOriginales
                                       where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000401")
                                       select busqueda;

                    string iva = Helpers.FormatearCampos(TiposFormateo.Decimal01, "00");

                    if (linea3000401.Any())
                    {
                        iva = Helpers.FormatearCampos(TiposFormateo.Decimal01, linea3000401.FirstOrDefault().Substring(12, 20).Trim());
                    }

                    var linea3000404 = from busqueda in datosOriginales
                                       where busqueda.Length > 7 && busqueda.Substring(0, 7).Equals("3000404")
                                       select busqueda;

                    string impuesto = Helpers.FormatearCampos(TiposFormateo.Decimal01, "00");

                    if (linea3000404.Any())
                    {
                        impuesto = Helpers.FormatearCampos(TiposFormateo.Decimal01, linea3000404.FirstOrDefault().Substring(12, 20).Trim());
                    }

                    resultado = Helpers.ValidarPipePipe($"1ODC|{cuentaConexion}|Total|{Helpers.SumarCampos(sumaValoresBase)}|" +
                        $"{iva}|{impuesto}|{Helpers.SumarCampos(sumaValoresTotal)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}| ");
                }
            }

            return resultado;
            #endregion
        }

        private IEnumerable<string> FormateoCanal1ODD(List<string> datosOriginales)
        {
            #region FormateoCanal1ODD

            #region Variables
            List<string> resultado = new List<string>();
            Dictionary<string, List<string>> paquetesInformacion = new Dictionary<string, List<string>>();
            List<string> sumaValoresBase = new List<string>();
            List<string> sumaValoresIva = new List<string>();
            List<string> sumaValoresImpuestos = new List<string>();
            List<string> sumaValoresTotal = new List<string>();
            string periodo = string.Empty;
            string llaveCruce = string.Empty;
            string CodigoCanalPrimario = string.Empty;
            string conceptoPrimario = string.Empty;
            string concepto = string.Empty;
            bool banderaPaquete = false;
            bool banderaRecargoMora = false;
            string cuentaConexion = string.Empty;
            #endregion

            if (IsLte || IsLteCorporativo)
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
                #endregion

                #region Periodo

                foreach (var LineaPaquete in paquetesInformacion)
                {
                    #region Busqueda Periodo

                    if (string.IsNullOrEmpty(periodo))
                    {
                        var lineas11Cparaperiodo = from busqueda in LineaPaquete.Value
                                                   where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C")
                                                   select busqueda;

                        //Buscar Periodo
                        var busquedaperiodo = from n in lineas11Cparaperiodo
                                              let comp = n.Substring(128, 19).Trim()
                                              where !string.IsNullOrEmpty(comp) && comp.Contains("-")
                                              select n;

                        if (busquedaperiodo.Any())
                        {
                            periodo = busquedaperiodo.FirstOrDefault() != "" ? busquedaperiodo.FirstOrDefault().Substring(128, 19).Trim() : "";
                        }
                    }

                    #endregion
                }

                if (string.IsNullOrEmpty(periodo))
                {
                    string nuevaFechaDesde = string.Empty;
                    string nuevaFechaHasta = string.Empty;

                    nuevaFechaDesde = $"{FechaDesde.Substring(0, 2)}{(Convert.ToInt16(FechaDesde.Substring(2, 2)) + 1).ToString().PadLeft(2, '0')}{FechaDesde.Substring(4, 4)}";
                    nuevaFechaHasta = $"{FechaHasta.Substring(0, 2)}{(Convert.ToInt16(FechaHasta.Substring(2, 2)) + 1).ToString().PadLeft(2, '0')}{FechaHasta.Substring(4, 4)}";

                    nuevaFechaDesde = Helpers.FormatearCampos(TiposFormateo.Fecha01, nuevaFechaDesde);
                    nuevaFechaHasta = Helpers.FormatearCampos(TiposFormateo.Fecha01, nuevaFechaHasta);

                    periodo = $"{nuevaFechaDesde.Split('/').ElementAt(2)}{nuevaFechaDesde.Split('/').ElementAt(1)}{nuevaFechaDesde.Split('/').ElementAt(0)} - " +
                        $"{nuevaFechaHasta.Split('/').ElementAt(2)}{nuevaFechaHasta.Split('/').ElementAt(1)}{nuevaFechaHasta.Split('/').ElementAt(0)}";
                }
                #endregion

                foreach (var lineaDetallePaquete in paquetesInformacion)
                {
                    #region Logica
                    sumaValoresBase.Clear();
                    sumaValoresIva.Clear();
                    sumaValoresImpuestos.Clear();
                    sumaValoresTotal.Clear();

                    var lineas11C = from busqueda in lineaDetallePaquete.Value
                                    where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C") && !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                                    select busqueda;

                    var linea040000 = from busqueda in lineaDetallePaquete.Value
                                      where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                      select busqueda;

                    #region Busqueda Numero Conexion
                    foreach (var lineaDatos in linea040000)
                    {
                        if (lineaDatos.Substring(6, 20).Trim() != Cuenta)
                        {
                            cuentaConexion = lineaDatos.Substring(6, 20).Trim();
                            break;
                        }
                    }
                    #endregion

                    if (lineas11C.Any())
                    {
                        //Buscar concepto primario
                        var linea040011 = from busqueda in lineaDetallePaquete.Value
                                          where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040011")
                                          select busqueda;

                        if (linea040011.Any())
                        {
                            #region Concepto Primario
                            CodigoCanalPrimario = $"{linea040011.FirstOrDefault().Substring(167, 10).Trim()}{linea040011.FirstOrDefault().Substring(6, 10).Trim()}";

                            //Buscar los del codigo para ver si no estan en 0 
                            var validarConceptoConDatos = from busqueda in lineaDetallePaquete.Value
                                                          where busqueda.Length > 290
                                                          let comp = busqueda.Substring(271, 20).Trim()
                                                          where comp == CodigoCanalPrimario && !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                                                          select busqueda;

                            if (validarConceptoConDatos.Any())
                            {
                                conceptoPrimario = Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea040011.FirstOrDefault().Substring(6, 40).Trim());

                                foreach (var lineaDetalle11C in validarConceptoConDatos)
                                {
                                    sumaValoresBase.Add(lineaDetalle11C.Substring(16, 14).Trim());
                                    sumaValoresIva.Add(lineaDetalle11C.Substring(44, 14).Trim());
                                    sumaValoresImpuestos.Add(lineaDetalle11C.Substring(172, 14).Trim());
                                }

                                sumaValoresTotal.AddRange(sumaValoresBase);
                                sumaValoresTotal.AddRange(sumaValoresIva);
                                sumaValoresTotal.AddRange(sumaValoresImpuestos);

                                resultado.Add(Helpers.ValidarPipePipe($"1ODD|{Helpers.FormatearCampos(TiposFormateo.Fecha06, periodo.Split('-').ElementAt(0).Trim())} a {Helpers.FormatearCampos(TiposFormateo.Fecha06, periodo.Split('-').ElementAt(1).Trim())}|" +
                                    $"{conceptoPrimario} ({Helpers.FormatearCampos(TiposFormateo.Fecha07, periodo.Split('-').ElementAt(0).Trim())} - {Helpers.FormatearCampos(TiposFormateo.Fecha07, periodo.Split('-').ElementAt(1).Trim())})|" +
                                    $"{Helpers.SumarCampos(sumaValoresBase)}|{Helpers.SumarCampos(sumaValoresIva)}|{Helpers.SumarCampos(sumaValoresImpuestos)}|{Helpers.SumarCampos(sumaValoresTotal)}|" +
                                    $"{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|" +
                                    $"{cuentaConexion}| "));
                            }
                            #endregion
                        }

                        var LineasDetalleSinPrimario = from busqueda in lineas11C
                                                       where busqueda.Substring(271, 20).Trim() != CodigoCanalPrimario
                                                       group busqueda by busqueda.Substring(271, 20).Trim() into busqueda
                                                       select busqueda;

                        if (LineasDetalleSinPrimario.Any())
                        {
                            #region Detalles que no son del codigo primario
                            foreach (var lineaDetalleAgrupado in LineasDetalleSinPrimario.Select(x => x))
                            {
                                sumaValoresBase.Clear();
                                sumaValoresIva.Clear();
                                sumaValoresImpuestos.Clear();
                                sumaValoresTotal.Clear();

                                foreach (var lineadetalle in lineaDetalleAgrupado)
                                {
                                    sumaValoresBase.Add(lineadetalle.Substring(16, 14).Trim());
                                    sumaValoresIva.Add(lineadetalle.Substring(44, 14).Trim());
                                    sumaValoresImpuestos.Add(lineadetalle.Substring(172, 14).Trim());
                                }

                                sumaValoresTotal.AddRange(sumaValoresBase);
                                sumaValoresTotal.AddRange(sumaValoresIva);
                                sumaValoresTotal.AddRange(sumaValoresImpuestos);

                                llaveCruce = $"CODF{lineaDetalleAgrupado.FirstOrDefault().Substring(6, 10).Trim()}";

                                concepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(14).Trim() ?? "";

                                if (concepto != "Cobro de Reconexión")
                                {
                                    concepto = Helpers.FormatearCampos(TiposFormateo.LetraCapital, lineaDetalleAgrupado.FirstOrDefault().Substring(281, 55).Trim());

                                    if (string.IsNullOrEmpty(concepto))
                                    {
                                        concepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(14).Trim() ?? "";
                                    }
                                }

                                resultado.Add(Helpers.ValidarPipePipe($"1ODD|{Helpers.FormatearCampos(TiposFormateo.Fecha06, periodo.Split('-').ElementAt(0).Trim())} a {Helpers.FormatearCampos(TiposFormateo.Fecha06, periodo.Split('-').ElementAt(1).Trim())}|" +
                                    $"{concepto} ({Helpers.FormatearCampos(TiposFormateo.Fecha07, periodo.Split('-').ElementAt(0).Trim())} - {Helpers.FormatearCampos(TiposFormateo.Fecha07, periodo.Split('-').ElementAt(1).Trim())})|" +
                                    $"{Helpers.SumarCampos(sumaValoresBase)}|{Helpers.SumarCampos(sumaValoresIva)}|{Helpers.SumarCampos(sumaValoresImpuestos)}|{Helpers.SumarCampos(sumaValoresTotal)}|" +
                                    $"{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|" +
                                    $"{cuentaConexion}| "));
                            }
                            #endregion
                        }

                        #region Validar para recargo de mora
                        var lineas11CRecargoMora = from busqueda in lineaDetallePaquete.Value
                                                   where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C") && !string.IsNullOrEmpty(busqueda.Substring(30, 14).Trim().TrimStart('0'))
                                                   select busqueda;

                        if (lineas11CRecargoMora.Any())
                        {
                            if (!banderaRecargoMora)
                            {
                                resultado.Add(GetRecargoMora(lineas11CRecargoMora.ToList(), periodo, cuentaConexion, ref banderaRecargoMora));
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region Validar para recargo de mora
                        var lineas11CRecargoMora = from busqueda in lineaDetallePaquete.Value
                                                   where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C") && !string.IsNullOrEmpty(busqueda.Substring(30, 14).Trim().TrimStart('0'))
                                                   select busqueda;

                        if (lineas11CRecargoMora.Any())
                        {
                            if (!banderaRecargoMora)
                            {
                                resultado.Add(GetRecargoMora(lineas11CRecargoMora.ToList(), periodo, cuentaConexion, ref banderaRecargoMora));
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }

            return resultado;
            #endregion
        }

        private string GetRecargoMora(List<string> pLineas, string pPeriodo, string pNumeroConexion, ref bool pBanderaRecargoMora)
        {
            #region GetRecargoMora
            string resultado = string.Empty;
            List<string> sumaValoresBase = new List<string>();
            List<string> sumaValoresIva = new List<string>();
            List<string> sumaValoresImpuestos = new List<string>();
            List<string> sumaValoresTotal = new List<string>();

            foreach (var linea in pLineas)
            {
                sumaValoresBase.Add(linea.Substring(30, 14).Trim());
                sumaValoresIva.Add(linea.Substring(44, 14).Trim());
                sumaValoresImpuestos.Add(linea.Substring(172, 14).Trim());
            }

            sumaValoresTotal.AddRange(sumaValoresBase);
            sumaValoresTotal.AddRange(sumaValoresIva);
            sumaValoresTotal.AddRange(sumaValoresImpuestos);

            resultado = $"1ODD|{Helpers.FormatearCampos(TiposFormateo.Fecha06, pPeriodo.Split('-').ElementAt(0).Trim())} a {Helpers.FormatearCampos(TiposFormateo.Fecha06, pPeriodo.Split('-').ElementAt(1).Trim())}|" +
                $"Recargo de Mora ({Helpers.FormatearCampos(TiposFormateo.Fecha07, pPeriodo.Split('-').ElementAt(0).Trim())} - {Helpers.FormatearCampos(TiposFormateo.Fecha07, pPeriodo.Split('-').ElementAt(1).Trim())})|" +
                $"{Helpers.SumarCampos(sumaValoresBase)}|{Helpers.SumarCampos(sumaValoresIva)}|{Helpers.SumarCampos(sumaValoresImpuestos)}|{Helpers.SumarCampos(sumaValoresTotal)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|" +
                $"{pNumeroConexion}| ";

            pBanderaRecargoMora = true;

            return resultado;
            #endregion
        }

        private IEnumerable<string> FormateoCanal1OOO(List<string> datosOriginales)
        {
            #region FormateoCanal1OOO
            List<string> resultado = new List<string>();
            string numeroConexion = string.Empty;

            var linea040000 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                              select busqueda;

            var linea04V000 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("04V000")
                              select busqueda;

            if (linea04V000.Any())
            {
                #region Busqueda Numero Conexion
                foreach (var lineaDatos in linea040000)
                {
                    if (lineaDatos.Substring(6, 20).Trim() != Cuenta)
                    {
                        numeroConexion = lineaDatos.Substring(6, 20).Trim();
                        break;
                    }
                }
                #endregion

                foreach (var linea in linea04V000)
                {
                    resultado.Add(Helpers.ValidarPipePipe($"1OOO|{numeroConexion}|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea.Substring(6, 70).Trim())}|" +
                        $"{Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea.Substring(76, 25).Trim())}|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea.Substring(101, 50).Trim())}| "));
                }
            }

            return resultado;
            #endregion
        }

        private IEnumerable<string> FormateoCanal1FFF(List<string> datosOriginales)
        {
            #region FormateoCanal1FFF
            List<string> resultado = new List<string>();

            var linea13M317 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("13M317")
                              select busqueda;

            if (linea13M317.Any())
            {
                string llaveCruce = $"CODF{linea13M317.FirstOrDefault().Substring(32, 10).Trim()}";
                var concepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault().Substring(14).Trim();

                string total = linea13M317.FirstOrDefault().Substring(42, 14).Trim();
                string subtotal = linea13M317.FirstOrDefault().Substring(56, 14).Trim();

                resultado.Add(Helpers.ValidarPipePipe($"1FFF|{concepto}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, total)}" +
                    $"|{Helpers.FormatearCampos(TiposFormateo.Decimal01, subtotal)}|{Helpers.SumarCampos(new List<string> { total, subtotal })}| | "));

                resultado.Add("1FFF| | | | | | ");
            }

            return resultado;
            #endregion
        }
        #endregion
    }
}
