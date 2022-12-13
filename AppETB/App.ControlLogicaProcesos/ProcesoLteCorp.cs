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
    public class ProcesoLteCorp : IProcess
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
        private bool Is1PLA { get; set; }

        #endregion

        public ProcesoLteCorp(string pArchivo, string pPeridoFacturacion)
        {
            #region ProcesoLteCorp
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
        public ProcesoLteCorp()
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
                                            //Variables.Variables.DatosErrorLTE.Add(llaveCruce, FormatearArchivo(llaveCruce, datosExtractoFormateo));
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

                    datosExtractoFormateo.Add(linea.Replace("|", " "));
                }
                else
                {
                    datosExtractoFormateo.Add(linea.Replace("|", " "));
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

            if (pLLaveCruce == "")
            {

            }

            FormatearPropiedadesExtracto();

            #region Formateo Canales
            resultadoFormateoLinea = MapeoCanal1AAA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            List<string> datosSinformatearTemp = new List<string>();
            datosSinformatearTemp.AddRange(datosOriginales);

            // Se llena el diccionario sin formatear
            if (Variables.Variables.DicDatosSinFormatear.ContainsKey(Ciclo))
            {
                Variables.Variables.DicDatosSinFormatear[Ciclo].AddRange(datosSinformatearTemp);
            }
            else
            {
                Variables.Variables.DicDatosSinFormatear.Add(Ciclo, datosSinformatearTemp);
            }

            resultadoFormateoLinea = MapeoCanal1BBB(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            List<string> temp = new List<string>();

            var resultLineasHIS = from busqueda in datosOriginales
                                  where busqueda.Substring(0, 6).Equals("010000") ||
                                  busqueda.Substring(0, 6).Equals("070002")
                                  select busqueda;

            temp.AddRange(resultLineasHIS.ToList());

            resultadoFormateoLinea = MapeoCanal1PPD(temp);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            temp = new List<string>();

            var resultLineasHDT = from busqueda in datosOriginales
                                  where busqueda.Substring(0, 6).Equals("010000") ||
                                  busqueda.Substring(0, 6).Equals("070001")
                                  select busqueda;

            temp.AddRange(resultLineasHDT.ToList());

            resultadoFormateoLinea = MapeoCanal1PPV(temp);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoGrupo1DAA(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoGrupo1JAA(datosOriginales);

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

            resultadoFormateoLinea = MapeoPaqueteADN1(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormarPaqueteEEE(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = MapeoCanal1PLA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
                Is1PLA = true;
            }

            resultadoFormateoLinea = MapeoCanal1DET(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
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

            resultadoFormateoLinea = FormateoPaqueteNTC(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1III(datosOriginales);

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
            Is1PLA = false;
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

                // Se consulta antes de todo porque se requiere para validaciones
                string etapas = GetTipoEtapas(Linea010000.Substring(151, 3));
                Cuenta = Linea010000.Substring(117, 20).Trim();// Cuenta
                Ciclo = Linea010000.Substring(151, 3).Trim().TrimStart('0'); // Asignamos Ciclo a variable Global

                if (Cuenta == "7798926091")
                {

                }

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

                ListaCanal1AAA.AddRange(GetFechaDesdeHasta(Linea010000));

                ListaCanal1AAA.Add(GetTelefono(datosOriginales, Linea010000));
                ListaCanal1AAA.Add(IsFibra ? (string.IsNullOrEmpty(Linea010000.Substring(218, 20).Trim()) ? " " : Linea010000.Substring(218, 20).Trim()) : " ");

                ListaCanal1AAA.AddRange(GetFechaPagoLimiteExpedicion(Linea010000));

                ListaCanal1AAA.Add(GetNumeroReferencia(Linea010000.Substring(139, 12)));
                ListaCanal1AAA.AddRange(GetCodigosBarras(Linea010000.Substring(139, 12), Linea010000, datosOriginales));
                ListaCanal1AAA.Add(etapas);
                ListaCanal1AAA.Add(GetTasaInteres(Linea040000));
                listaCortes.Clear();
                listaCortes.Add(new PosCortes(108, 5));
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));
                ListaCanal1AAA.Add(string.Empty); // TODO: Anexos Publicitarios - Verificar regla
                ListaCanal1AAA.Add(GetActividad(Linea040000));
                ListaCanal1AAA.Add(GetEstrato(Linea040000));
                ListaCanal1AAA.AddRange(GetBarrioLocalidad(datosOriginales));
                ListaCanal1AAA.Add(GetValorPagarMes(datosOriginales)); // Valor Pagar Vacio
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
                ListaCanal1AAA.Add(GetCuentaVencidaAnticipada()); // TODO: Validar que sea el insumo correcto
                ListaCanal1AAA.Add(GetMarcaParafiscales()); //Parafiscales
                ListaCanal1AAA.Add(string.Empty); //TODO: Tipo Email Prevalidador Verificar
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
            //string totalPagar = valoresPago[0].ToString().Trim().PadLeft(12, '0');

            Int64 total = Convert.ToInt64(valoresPago[0].ToString().Trim());

            string totalPagar = String.Empty;
            if (total > 0)
            {
                totalPagar = valoresPago[0].ToString().Trim();
            }
            else
            {
                totalPagar = "0";
            }

            resultado = Helpers.FormatearCampos(TiposFormateo.Decimal05, totalPagar);
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

            Int64 total = Convert.ToInt64(valoresPago[1].ToString().Trim());

            #region Requerimiento 360
            var result29000 = from busqueda in pDatosOriginales
                              where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("29000")
                              select busqueda;

            var result02T003 = from busqueda in pDatosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T003")
                               select busqueda;

            if (result29000.Any())
            {
                //Nota Credito
                foreach (var lineaNotaCredito in result29000)
                {
                    total += Convert.ToInt64(lineaNotaCredito.Substring(29, 20).Trim());
                }
            }

            if (result02T003.Any())
            {
                //Ajuste Decena
                foreach (var lineaAjusteDecena in result02T003)
                {
                    total += Convert.ToInt64(lineaAjusteDecena.Substring(6, 14)) +
                       Convert.ToInt64(lineaAjusteDecena.Substring(20, 14)) +
                       Convert.ToInt64(lineaAjusteDecena.Substring(34, 14)) +
                       Convert.ToInt64(lineaAjusteDecena.Substring(48, 14)) +
                       Convert.ToInt64(lineaAjusteDecena.Substring(62, 14));
                }
            }
            #endregion

            string totalPagar = String.Empty;
            if (total > 0)
            {
                totalPagar = total.ToString();
            }
            else
            {
                totalPagar = "0";
            }

            string ValorPagar = Helpers.FormatearCampos(TiposFormateo.Decimal05, totalPagar);

            return ValorPagar ?? string.Empty;
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
        /// Regla FechaDesdeHasta
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private List<string> GetFechaDesdeHasta(string pLinea010000)
        {
            #region FechaDesdeHasta
            List<string> FechaDesdeHasta = new List<string>();

            if (IsLte || IsLteCorporativo)
            {
                string fechaFactura = Helpers.FormatearCampos(TiposFormateo.Fecha02, pLinea010000.Substring(168, 8));

                int mes = Convert.ToInt32(fechaFactura.Substring(4, 2));
                int año = Convert.ToInt32(fechaFactura.Substring(0, 4));
                DateTime fecha = new DateTime(año, mes, 1);
                DateTime fecha1 = new DateTime(año, mes, 1);

                string mes1 = string.Empty;
                string año1 = string.Empty;

                if (Ciclo != "90" && Ciclo != "3")
                {
                    fecha1 = fecha1.AddMonths(1);
                }

                string fechaInsumo = FechaCorteLTE(fecha.Month.ToString(), fecha.Year);

                if (!string.IsNullOrEmpty(fechaInsumo))
                {
                    string[] camposFechaInsumo = fechaInsumo.Split(' ');

                    DateTime fechaDesde = new DateTime(fecha.Year, fecha.Month, Convert.ToInt32(camposFechaInsumo[0]));
                    DateTime fechaHasta = new DateTime(fecha1.Year, fecha1.Month, Convert.ToInt32(camposFechaInsumo[2]));

                    FechaDesdeHasta.Add(fechaDesde.ToString("dd/MM/yyyy"));
                    FechaDesdeHasta.Add(fechaHasta.ToString("dd/MM/yyyy"));
                }

            }

            if (FechaDesdeHasta.Count == 0)
            {
                List<PosCortes> listaCortes = new List<PosCortes>();
                listaCortes.Add(new PosCortes(178, 8, TiposFormateo.Fecha01));
                listaCortes.Add(new PosCortes(186, 8, TiposFormateo.Fecha01));
                FechaDesdeHasta.Add(Helpers.ExtraccionCamposSpool(listaCortes, pLinea010000));
            }

            FechaDesde = pLinea010000.Substring(178, 8).Trim();
            FechaHasta = pLinea010000.Substring(186, 8).Trim();

            #endregion

            return FechaDesdeHasta;
        }

        /// <summary>
        /// Obtiene la Fecha de Corte para LTE y LTE CORP
        /// </summary>
        /// <param name="mes"></param>
        /// <param name="año"></param>
        /// <returns></returns>
        private string FechaCorteLTE(string mes, int año)
        {
            //global $valores_temp;

            mes = mes.PadLeft(2, '0');
            string mesText = string.Empty;

            switch (mes)
            {
                case "01":
                case "03":
                case "05":
                case "07":
                case "08":
                case "10":
                case "12":
                    mesText = "M1";
                    break;
                case "04":
                case "06":
                case "09":
                case "11":
                    mesText = "M2";
                    break;
                case "02":
                    mesText = esBisiesto(año) ? "M4" : "M3";
                    break;
            }

            if (Ciclo != "90" && Ciclo != "3")
            {
                mesText = "M1";
            }

            string resultado = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"PERCORLTE{Ciclo}{mesText}").FirstOrDefault() ?? string.Empty;
            resultado = resultado.Replace($"PERCORLTE{Ciclo}{mesText}", "").Trim();
            return resultado;
        }

        /// <summary>
        /// Verifica si un año es biciesto
        /// </summary>
        /// <param name="pAño"></param>
        /// <returns>true o False</returns>
        private bool esBisiesto(int pAño)
        {
            if (DateTime.IsLeapYear(pAño))
            {
                return true;
            }
            else
            {
                return false;
            }
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


            string campoTelefono;
            bool IsTelefono = false;
            if (result.Any())
            {
                foreach (var item in result.ToList())
                {
                    campoTelefono = item.Substring(6, 20).Trim();

                    if (GetTipo(item.Substring(6, 20).Trim()) == "Conexion")
                    {
                        if ((IsLte || IsLteCorporativo) && item.Substring(6, 1) == "3" && item.Substring(6, 20).Trim().Length >= 8)
                        {
                            IsTelefono = true;
                        }
                        else if (IsResidencial &&
                                    ((item.Substring(6, 20).Trim().Length == 10 && (item.Substring(6, 3) == "601" || item.Substring(6, 3) == "608")) ||
                                     (item.Substring(6, 20).Trim().Length == 8 && item.Substring(6, 3) == "601")))
                        {
                            IsTelefono = true;
                        }
                        else if (IsFibra && string.IsNullOrEmpty(telefono) && (!string.IsNullOrEmpty(item.Substring(124, 2).Trim())))
                        {
                            IsTelefono = true;
                        }
                    }

                    if (IsTelefono)
                    {
                        telefono = item.Substring(6, 20).Trim();
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(telefono))
            {
                if (IsResidencial || IsFibra)
                {
                    if ((telefono.Length != 8 && telefono.Length != 10) || telefono.Substring(0, 1) == "1")
                    {
                        telefono = string.Empty;
                    }
                }

            }

            return telefono;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene Fecha de Pago de Limite de pago y de Expedicion
        /// </summary>
        /// <param name="pLinea010000"></param>
        /// <returns>Lista de Fechas</returns>
        private List<string> GetFechaPagoLimiteExpedicion(string pLinea010000)
        {
            #region GetFechaPagoLimiteExpedicion
            List<string> listaFechas = new List<string>();
            string fechaPago = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECP{Helpers.FormatearCampos(TiposFormateo.Fecha02, pLinea010000.Substring(168, 8).Trim())}{pLinea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty;
            string fechaLimitePago = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECL{Helpers.FormatearCampos(TiposFormateo.Fecha02, pLinea010000.Substring(168, 8).Trim())}{pLinea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty;
            string fechaExpedicionInsumo = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECX{Helpers.FormatearCampos(TiposFormateo.Fecha02, pLinea010000.Substring(168, 8).Trim())}{pLinea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty;
            string fechaExpedicion = GetFechaExpedicion(pLinea010000);

            string fechaPagoFijo = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoFechaPagoFijas, $"{Cuenta}") ?? string.Empty;

            if (!string.IsNullOrEmpty(fechaPagoFijo) && !string.IsNullOrEmpty(fechaExpedicionInsumo))
            {
                string[] campos = fechaPagoFijo.Split('|');
                int diasCorte = Convert.ToInt32(campos[1]);

                DateTime fechaReferencia = Convert.ToDateTime(fechaExpedicionInsumo.Substring(0, 10));

                int año = fechaReferencia.Year;
                int mes = fechaReferencia.Month;
                int dia = fechaReferencia.Day;

                DateTime fechaCorte = Helpers.GetSiguienteDiaHabil(fechaReferencia);
                fechaCorte = fechaCorte.AddDays(diasCorte);
                fechaCorte = Helpers.GetSiguienteDiaHabil(fechaCorte);

                DateTime fechaCorteLimite = fechaCorte.AddDays(1);
                fechaCorteLimite = Helpers.GetSiguienteDiaHabil(fechaCorteLimite);

                fechaPago = fechaCorte.ToString("dd/MM/yyyy");
                fechaLimitePago = fechaCorteLimite.ToString("dd/MM/yyyy");

            }



            listaFechas.Add(fechaPago);
            listaFechas.Add(fechaLimitePago);
            listaFechas.Add(fechaExpedicion);
            return listaFechas;
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
            List<string> valoresPago = ObtenerDatosCanal1BBB(pDatosOriginales, true).ToList();
            string ValorPagarCB1 = valoresPago[0].ToString().Trim().PadLeft(12, '0');
            string ValorPagarCB2 = valoresPago[1].ToString().Trim().PadLeft(12, '0');
            string CodeBar1 = String.Empty;
            string CodeBar2 = String.Empty;

            if (Convert.ToInt64(valoresPago[1]) >= 0)
            {
                CodeBar1 = $"(415){numeroETB}(8020){numReferencia}(3900){ValorPagarCB1.Substring(0, 10)}(96){fechaPago}";
            }
            else
            {
                CodeBar1 = $"(415){numeroETB}(8020){numReferencia}(3900)0000000000(96){fechaPago}";
            }

            result.Add(CodeBar1);

            #region Requerimiento 360
            Int64 total = Convert.ToInt64(ValorPagarCB2);

            var result29000 = from busqueda in pDatosOriginales
                              where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("29000")
                              select busqueda;

            var result02T003 = from busqueda in pDatosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T003")
                               select busqueda;

            if (result29000.Any())
            {
                //Nota Credito
                foreach (var lineaNotaCredito in result29000)
                {
                    total += Convert.ToInt64(lineaNotaCredito.Substring(29, 20).Trim());
                }
            }

            if (result02T003.Any())
            {
                //Ajuste Decena
                foreach (var lineaAjusteDecena in result02T003)
                {
                    total += Convert.ToInt64(lineaAjusteDecena.Substring(6, 14)) +
                       Convert.ToInt64(lineaAjusteDecena.Substring(20, 14)) +
                       Convert.ToInt64(lineaAjusteDecena.Substring(34, 14)) +
                       Convert.ToInt64(lineaAjusteDecena.Substring(48, 14)) +
                       Convert.ToInt64(lineaAjusteDecena.Substring(62, 14));
                }
            }

            if (total < 0)
            {
                total = 0;
            }
            #endregion

            if (total == 0)
            {
                CodeBar2 = $"(415){numeroETB}(8020){numReferencia}(3900){total.ToString().PadLeft(10, '0')}(96){fechaPago}";
            }
            else
            {
                if (total.ToString().Length < 3)
                {
                    CodeBar2 = $"(415){numeroETB}(8020){numReferencia}(3900){total.ToString().PadLeft(10, '0')}(96){fechaPago}";
                }
                else
                {
                    CodeBar2 = $"(415){numeroETB}(8020){numReferencia}(3900){total.ToString().Substring(0, total.ToString().Length - 2).PadLeft(10, '0')}(96){fechaPago}";
                }
            }

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

            string tasaInteresTablaSustitucion = string.Empty;

            tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM10").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(tasaInteresTablaSustitucion))
            {
                string idActividad = GetIdActividad(pLinea040000.Substring(124, 2));
                tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM{idActividad}").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
            }

            return tasaInteresTablaSustitucion;

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
        private List<string> GetBarrioLocalidad(List<string> pDatosOriginales)
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


            var result010002 = from busqueda in pDatosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010002")
                               select busqueda;

            if (result010002.Any())
            {
                string linea010002 = result010002.FirstOrDefault();
                if (IsLte || IsLteCorporativo)
                {
                    if (string.IsNullOrEmpty(barrio.Trim()))
                    {
                        barrio = linea010002.Substring(6, 30).Trim();
                    }

                    if (string.IsNullOrEmpty(localidad.Trim()))
                    {
                        localidad = linea010002.Substring(36, 50).Trim();
                    }
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

            if (IsGobierno)
            {
                if (isDual)
                {
                    MarcaNumHojas = "*";
                }
                else if (isSoloMail)
                {
                    MarcaNumHojas = "*";
                }
            }
            else
            {
                if (isDual)
                {
                    MarcaNumHojas = "DISTRIBUCION_DUAL";
                }
                else if (isSoloMail)
                {
                    MarcaNumHojas = "FACTURA_SOLO_EMAIL";
                }
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
            if (result11C.Any())
            {
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

                    fechaCorte = fecha.ToString("ddMMyyyy");
                    fechaCorte = Helpers.FormatearCampos(TiposFormateo.Fecha16, fechaCorte);
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
        private IEnumerable<string> MapeoCanal1BBB(List<string> pDatosOriginales)
        {
            #region MapeoCanal1BBB
            IEnumerable<string> Lineas1BBB = ObtenerDatosCanal1BBB(pDatosOriginales, false);

            return Lineas1BBB;
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene las lineas del Canal 1BBB
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <param name="sumatorias"></param>
        /// <returns></returns>
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
            string lineaImpuestosDistriTales = string.Empty;
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
                    else if (llave == "02T112" && !IsFibra)
                    {
                        descripcion = descripcion.Substring(0, 31);
                    }

                    if (llave == "02T008")
                    { } // se omite el valor de subsidio
                    else if (llave == "02S000")
                    {
                        if (!string.IsNullOrEmpty(detalle.Substring(20, 14).Trim()) && Convert.ToInt64(detalle.Substring(20, 14)) != 0)
                        {
                            Lineas1BBB.Add($"1BBB|Traslado de Saldos|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(20, 14).TrimStart('0'))}| ");
                            SubTotal1BBB += Convert.ToInt64(detalle.Substring(20, 14));
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0 /*&& trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X"*/)
                        {
                            if (string.IsNullOrEmpty(linea150001) || (!string.IsNullOrEmpty(linea150001) && linea150001.Substring(6, 1) != "N")) // Regla No sumar saldo anterior
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14));
                            }
                        }

                        if (!string.IsNullOrEmpty(detalle.Substring(34, 14).Trim()) && Convert.ToInt64(detalle.Substring(34, 14)) != 0 /*&& trim($valores_temp["DOC1_SALDO_GRACIAS"]) == "X"*/)
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
                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T016")
                        {

                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T002")
                        {
                            if (!string.IsNullOrEmpty(detalle.Substring(6, 14).Trim()) && Convert.ToInt64(detalle.Substring(6, 14)) != 0)
                            {
                                Lineas1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(6, 14).TrimStart('0'))}| ");
                                SubTotal1BBB += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));
                            }
                        }
                        else if (llave == "02T003")
                        {
                            calculoAjusteDecena += Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14));

                            lineaAjusteDecena = $"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, detalle.Substring(6, 14).TrimStart('0'))}| ";
                            SubTotal1BBB += calculoAjusteDecena;

                        }
                        else
                        {
                            int impuestoConsumo = 0;
                            if (IsFibra || IsDatos || IsGobierno || IsLte || IsLteCorporativo)
                            { impuestoConsumo = Convert.ToInt32(detalle.Substring(118, 14)); }

                            decimal sumatoria = Convert.ToInt64(detalle.Substring(6, 14)) + Convert.ToInt64(detalle.Substring(20, 14)) + Convert.ToInt64(detalle.Substring(34, 14)) + Convert.ToInt64(detalle.Substring(48, 14)) + Convert.ToInt64(detalle.Substring(62, 14)) + impuestoConsumo;

                            if (llave == "02T004")
                            {
                                lineaServiciosETBIVA = $"1BBB|{descripcion}|***| ";

                                tempValorTotalIva += sumatoria;

                                if (!IsFibra)
                                {
                                    lineasFinales1BBB.Add(lineaServiciosETBIVA);
                                }

                            }
                            else if (llave == "02T020" || llave == "02T050")
                            { // Req 11052015 Campo Financiacion Mes
                                lineasFinanciacion.Add($"1BBF|$Descripcion|{Helpers.FormatearCampos(TiposFormateo.Decimal05, sumatoria.ToString())}| ");
                                continue;
                            }

                            if ((!IsFibra || llave == "02T014") && llave != "02T004")
                            {
                                lineasFinales1BBB.Add($"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, sumatoria.ToString())}| ");
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
                total02T933 = Convert.ToInt64(linea02T933.Substring(6, 14)) + Convert.ToInt64(linea02T933.Substring(20, 14)) + Convert.ToInt64(linea02T933.Substring(34, 14)) + Convert.ToInt64(linea02T933.Substring(48, 14)) + Convert.ToInt64(linea02T933.Substring(62, 14));
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
                            Lineas1BBB.Add($"1BBB|Mod. Inseparables con IVA|{Helpers.FormatearCampos(TiposFormateo.Decimal05, total02T933.ToString())}| ");
                            tempValorTotalIva -= total02T933;
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

                    if (campos[2] == "***")
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

            if (IsFibra)
            {
                Lineas1BBB.Add($"1BBB|Total Facturado en el Mes|{Helpers.FormatearCampos(TiposFormateo.Decimal05, ValorPagarMes.ToString())}| ");
            }

            if (SubTotal1BBB > 0)
            {
                if (IsFibra)
                {
                    Lineas1BBB.Add($"1BBA|Valor total a pagar|{Helpers.FormatearCampos(TiposFormateo.Decimal05, SubTotal1BBB.ToString())}| ");
                }
                else
                {
                    Lineas1BBB.Add($"1BBA|Total de la Factura ETB|{Helpers.FormatearCampos(TiposFormateo.Decimal05, SubTotal1BBB.ToString())}| ");
                }
            }
            else
            {
                if (IsFibra)
                {
                    Lineas1BBB.Add($"1BBA|Valor total a pagar|{Helpers.FormatearCampos(TiposFormateo.Decimal05, SubTotal1BBB.ToString())}| ");
                }
                else
                {
                    Lineas1BBB.Add($"1BBA|Saldo a Favor|{Helpers.FormatearCampos(TiposFormateo.Decimal05, SubTotal1BBB.ToString())}| ");
                }
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

                    if (IsFibra || IsResidencial || IsLte || IsLteCorporativo)
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
                        linea1CCC += Helpers.FormatearCampos(TiposFormateo.Decimal05, linea.Substring(6, 14)) + "||| ";
                        list.Add(Helpers.ValidarPipePipe(linea1CCC));
                    }
                    linea1CCC = string.Empty;

                    // Se valida que el Iva tengo valor diferente de 0
                    if (Convert.ToDouble(linea.Substring(34, 14)) != 0)
                    {
                        linea1CCC = "1CCC|Iva|";
                        linea1CCC += Helpers.FormatearCampos(TiposFormateo.Decimal05, linea.Substring(34, 14)) + "||| ";

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
                        linea1CCC += Helpers.FormatearCampos(TiposFormateo.Decimal05, linea.Substring(6, 14)) + "||| ";
                        list.Add(Helpers.ValidarPipePipe(linea1CCC));
                    }
                    return list;

                case "02T942":

                    // Recargo Mora
                    if (Convert.ToDouble(linea.Substring(20, 14)) != 0)
                    {
                        linea1CCC = "1CCC|Recargo mora|";
                        linea1CCC += Helpers.FormatearCampos(TiposFormateo.Decimal05, linea.Substring(20, 14)) + "||| ";
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
                return string.Empty;
            }


            #endregion
        }


        private IEnumerable<string> MapeoGrupoAFI(List<string> datosOriginales)
        {
            #region MapeoGrupoAFI

            #region Variables
            List<string> resultado = new List<string>();
            Dictionary<int, List<string>> ordenSalidaInformacion = new Dictionary<int, List<string>>();
            List<string> datosBFI = new List<string>();
            List<string> datosCFI = new List<string>();
            List<string> totales = new List<string>();
            List<string> totalesAfi = new List<string>();
            List<string> sumaValoresBase = new List<string>();
            List<string> sumaValoresIva = new List<string>();
            List<string> sumaValoresImpuestos = new List<string>();
            List<string> sumaValoresTotal = new List<string>();
            string letrasPaquetes = string.Empty;
            string llaveCruce = string.Empty;
            string descripcionConcepto = string.Empty;
            string ajusteDecena = "000";
            string baseRecargoMora = "000";
            string ivaRecargoMora = "000";
            string periodo = string.Empty;
            string nombrePaquete = string.Empty;
            bool banderaRecargoMora = false;
            bool banderaAjusteDecena = false;
            bool banderaExclusion = true;
            bool banderaAFI = false;
            bool banderaMarcarOfertaValor = false;
            #endregion

            if (IsResidencial || IsFibra)
            {
                var lineas11CCompletos = from busqueda in datosOriginales
                                         where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C")
                                         group busqueda by busqueda.Substring(128, 5) into busqueda
                                         select busqueda;

                var lineas11C = from busqueda in datosOriginales
                                where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C")
                                select busqueda;

                if (lineas11CCompletos.Any())
                {
                    foreach (var datosPaqueteAgrupado in lineas11CCompletos.Select(x => x))
                    {
                        #region Logica

                        #region Limpieza Variables
                        letrasPaquetes = string.Empty;
                        nombrePaquete = string.Empty;
                        banderaExclusion = true;
                        banderaAFI = false;
                        datosBFI.Clear();
                        datosCFI.Clear();
                        totalesAfi.Clear();
                        ordenSalidaInformacion.Clear();
                        #endregion

                        foreach (var lineaDetalle in datosPaqueteAgrupado)
                        {
                            #region OrganizacionInformacion 1BFI - 1CFI
                            if (!string.IsNullOrEmpty(lineaDetalle.Substring(16, 14).Trim().TrimStart('0')))
                            {
                                string llaveUno = $"{lineaDetalle.Substring(6, 10)}";
                                string llaveDos = $"{lineaDetalle.Substring(6, 6)}";
                                string valorLetra = string.Empty;

                                valorLetra = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoParametrizacionPaquetesFibra, llaveUno);

                                if (string.IsNullOrEmpty(valorLetra))
                                {
                                    valorLetra = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoParametrizacionPaquetesFibra, llaveDos);
                                }

                                if (string.IsNullOrEmpty(valorLetra))
                                {
                                    if (descripcionConcepto.ToLower().Contains("soporte pc") && lineaDetalle.Substring(137, 1).Contains("-"))
                                    {
                                        datosBFI.Add(lineaDetalle);
                                        continue;
                                    }
                                    //No da informacion para el paquete por tanto se excluye 1CFI
                                    datosCFI.Add(lineaDetalle);
                                }
                                else
                                {
                                    //Informacion para el paquete 1BFI

                                    #region Reglas Exclusion del paquete

                                    llaveCruce = lineaDetalle.Substring(6, 10);

                                    descripcionConcepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"CODF{lineaDetalle.Substring(6, 10)}").FirstOrDefault()?.Substring(14).Trim() ?? string.Empty;

                                    if (descripcionConcepto.ToLower().Contains("empaquetado"))
                                    {
                                        banderaExclusion = false;
                                    }

                                    if (banderaExclusion)
                                    {
                                        // Buscar en el insumo Codigos_Univer_SASV y Cuentas Svas FueraBundle si se encuentra se omite este registro
                                        if (Variables.Variables.DatosInsumoCodigosUniverSvas.ContainsKey(llaveCruce) && Variables.Variables.DatosInsumoCuentasSvasFueraBundle.ContainsKey(Cuenta))
                                        {
                                            //CFI
                                            if (!string.IsNullOrEmpty(valorLetra.Trim()))
                                            {
                                                letrasPaquetes += valorLetra.Split('|')[2];
                                            }
                                            datosCFI.Add(lineaDetalle);
                                            continue;
                                        }

                                        // Buscar en el insumo ExcluirServiciosAdicionales
                                        if (Variables.Variables.DatosInsumoExcluirServiciosAdicionales.ContainsKey(llaveCruce.TrimStart('0')))
                                        {
                                            if (descripcionConcepto.ToLower().Contains("soporte pc") && !lineaDetalle.Substring(137, 1).Contains("-"))
                                            {
                                                datosCFI.Add(lineaDetalle);
                                                continue;
                                            }

                                            if (!string.IsNullOrEmpty(valorLetra.Trim()))
                                            {
                                                letrasPaquetes += valorLetra.Split('|')[2];
                                            }

                                            datosBFI.Add(lineaDetalle);
                                            continue;
                                        }

                                        if (!string.IsNullOrEmpty(descripcionConcepto))
                                        {
                                            // validar el concepto trae la palabra SVA se omite
                                            if (descripcionConcepto.Contains("SVA"))
                                            {
                                                //CFI
                                                if (!string.IsNullOrEmpty(valorLetra.Trim()))
                                                {
                                                    letrasPaquetes += valorLetra.Split('|')[2];
                                                }
                                                datosCFI.Add(lineaDetalle);
                                                continue;
                                            }
                                        }
                                    }

                                    #endregion

                                    if (!letrasPaquetes.Contains(valorLetra.Split('|')[2]))
                                    {
                                        letrasPaquetes += valorLetra.Split('|')[2];
                                    }

                                    datosBFI.Add(lineaDetalle);
                                }
                            }
                            #endregion
                        }

                        nombrePaquete = GetNombrePaquete(letrasPaquetes);

                        if (string.IsNullOrEmpty(nombrePaquete) && datosBFI.Any())
                        {
                            datosCFI.AddRange(datosBFI);
                            datosBFI.Clear();
                        }

                        var buscarAFI = from busqueda in resultado
                                        where busqueda.Substring(0, 4) == "1AFI"
                                        select busqueda;

                        if (!string.IsNullOrEmpty(nombrePaquete) && buscarAFI.Any())
                        {
                            if (datosCFI.Any() && !datosBFI.Any())
                            {
                                datosBFI.AddRange(datosCFI);
                                datosCFI.Clear();
                            }
                        }

                        if (datosBFI.Any())
                        {
                            #region Logica BFI
                            var lineasDetallePaqueteAgrupadoPeriodo = from busqueda in datosBFI
                                                                      group busqueda by busqueda.Substring(128, 19).Trim() into busqueda
                                                                      select busqueda;


                            string mesAnterior = string.Empty;
                            bool banderaSIguEnPaquete = true;

                            foreach (var periodos in lineasDetallePaqueteAgrupadoPeriodo.Select(x => x))
                            {
                                if (periodos.Key.Contains("-"))
                                {
                                    string mesPeriodo = periodos.Key.Split('-').ElementAt(0).Substring(4, 2);

                                    if (string.IsNullOrEmpty(mesAnterior))
                                    {
                                        mesAnterior = mesPeriodo;
                                    }

                                    if (mesPeriodo != mesAnterior)
                                    {
                                        mesAnterior = mesPeriodo;
                                        banderaSIguEnPaquete = false;
                                    }
                                }
                            }

                            if (!banderaSIguEnPaquete)
                            {
                                //Si el periodo no es el mismo se excluye
                                #region Validacion Exclusion Periodo
                                for (int i = 1; i < lineasDetallePaqueteAgrupadoPeriodo.Count(); i++)
                                {
                                    var dato = lineasDetallePaqueteAgrupadoPeriodo.ElementAt(i);

                                    foreach (var linea in dato)
                                    {
                                        datosCFI.Add(linea);

                                        if (datosBFI.Contains(linea))
                                        {
                                            datosBFI.Remove(linea);
                                        }
                                    }
                                }
                                #endregion
                            }

                            lineasDetallePaqueteAgrupadoPeriodo = from busqueda in datosBFI
                                                                  group busqueda by busqueda.Substring(128, 19).Trim() into busqueda
                                                                  select busqueda;

                            foreach (var lineaDetalleAgrupPeriodo in lineasDetallePaqueteAgrupadoPeriodo.Select(x => x))
                            {
                                var lineasDetallePaqueteAgrupado = from busqueda in lineaDetalleAgrupPeriodo
                                                                   group busqueda by busqueda.Substring(274, 15).Trim() into busqueda
                                                                   select busqueda;

                                foreach (var lineasDetalleFinal in lineasDetallePaqueteAgrupado.SelectMany(lineasDetalleFinal => lineasDetalleFinal))
                                {
                                    llaveCruce = $"CODF{lineasDetalleFinal.Substring(6, 10)}";

                                    descripcionConcepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(14).Trim() ?? string.Empty;

                                    if (descripcionConcepto == "Soporte pc ETB")
                                    {
                                        descripcionConcepto = "Soporte pc";
                                    }

                                    string lineaBfi = string.Empty;

                                    if (string.IsNullOrEmpty(lineasDetalleFinal.Substring(132, 4).Trim()) || !lineasDetalleFinal.Substring(128, 19).Trim().Contains("-"))
                                    {
                                        lineaBfi = Helpers.ValidarPipePipe($"1BFI| |{descripcionConcepto}| | | | | ");
                                    }
                                    else
                                    {
                                        lineaBfi = Helpers.ValidarPipePipe($"1BFI| |{descripcionConcepto} ({Helpers.FormatearCampos(TiposFormateo.Fecha04, lineasDetalleFinal.Substring(132, 4))} - {Helpers.FormatearCampos(TiposFormateo.Fecha04, lineasDetalleFinal.Substring(143, 4))})| | | | | ");
                                    }

                                    if (ordenSalidaInformacion.ContainsKey(2))
                                    {
                                        if (!ordenSalidaInformacion[2].Contains(lineaBfi))
                                        {
                                            ordenSalidaInformacion[2].Add(lineaBfi);
                                        }
                                    }
                                    else
                                    {
                                        ordenSalidaInformacion.Add(2, new List<string> { lineaBfi });
                                    }
                                }
                            }
                            #endregion
                        }

                        if (datosCFI.Any())
                        {
                            #region Logica CFI
                            var lineasDetallePaqueteAgrupadoPeriodo = from busqueda in datosCFI
                                                                      group busqueda by busqueda.Substring(128, 5).Trim() into busqueda
                                                                      select busqueda;

                            foreach (var lineaDetalleAgrupPeriodo in lineasDetallePaqueteAgrupadoPeriodo.Select(x => x))
                            {
                                var lineasDetallePaqueteAgrupado = from busqueda in lineaDetalleAgrupPeriodo
                                                                   group busqueda by busqueda.Substring(274, 15).Trim() into busqueda
                                                                   select busqueda;

                                foreach (var lineaDetalleAgrupado in lineasDetallePaqueteAgrupado.Select(lineaDetalleAgrupado => lineaDetalleAgrupado))
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

                                    llaveCruce = $"CODF{lineaDetalleAgrupado.FirstOrDefault().Substring(6, 10)}";

                                    descripcionConcepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault()?.Substring(14).Trim() ?? string.Empty;

                                    if (descripcionConcepto == "Cargo fijo llamada en espera" && lineaDetalleAgrupado.FirstOrDefault().Substring(3, 1) == "0" && !banderaMarcarOfertaValor)
                                    {
                                        descripcionConcepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FACLIN0").FirstOrDefault()?.Substring(8).Trim() ?? string.Empty;
                                        banderaMarcarOfertaValor = true;
                                    }

                                    if (descripcionConcepto == "Soporte pc ETB")
                                    {
                                        descripcionConcepto = "Soporte pc";
                                    }

                                    string lineaCfi = string.Empty;

                                    if (string.IsNullOrEmpty(lineaDetalleAgrupado.FirstOrDefault().Substring(132, 4).Trim()) || !lineaDetalleAgrupado.FirstOrDefault().Substring(128, 19).Trim().Contains("-") || !lineaDetalleAgrupado.FirstOrDefault().Substring(137, 1).Trim().Contains("-"))
                                    {
                                        lineaCfi = Helpers.ValidarPipePipe($"1CFI| |{descripcionConcepto}|{Helpers.SumarCampos(sumaValoresBase)}|{Helpers.SumarCampos(sumaValoresIva)}|" +
                                            $"{Helpers.SumarCampos(sumaValoresTotal)}|{Helpers.SumarCampos(sumaValoresImpuestos)}| ");
                                    }
                                    else
                                    {
                                        lineaCfi = Helpers.ValidarPipePipe($"1CFI|{lineaDetalleAgrupado.FirstOrDefault().Substring(128, 6)}|{descripcionConcepto} ({Helpers.FormatearCampos(TiposFormateo.Fecha04, lineaDetalleAgrupado.FirstOrDefault().Substring(132, 4))} - {Helpers.FormatearCampos(TiposFormateo.Fecha04, lineaDetalleAgrupado.FirstOrDefault().Substring(143, 4))})" +
                                        $"|{Helpers.SumarCampos(sumaValoresBase)}|{Helpers.SumarCampos(sumaValoresIva)}|{Helpers.SumarCampos(sumaValoresTotal)}|{Helpers.SumarCampos(sumaValoresImpuestos)}| ");
                                    }

                                    if (ordenSalidaInformacion.ContainsKey(3))
                                    {
                                        ordenSalidaInformacion[3].Add(lineaCfi);
                                    }
                                    else
                                    {
                                        ordenSalidaInformacion.Add(3, new List<string> { lineaCfi });
                                    }
                                }
                            }
                            #endregion
                        }

                        #region Validar para recargo de mora
                        var lineas11CRecargoMora = from busqueda in lineas11C
                                                   where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C") && !string.IsNullOrEmpty(busqueda.Substring(30, 14).Trim().TrimStart('0'))
                                                   select busqueda;

                        if (lineas11CRecargoMora.Any())
                        {
                            if (!banderaRecargoMora)
                            {
                                string lineaRecargo = GetRecargoMora(lineas11CRecargoMora.ToList(), string.Empty, string.Empty, ref banderaRecargoMora, "2");

                                baseRecargoMora = Helpers.FormatearCampos(TiposFormateo.Decimal03, lineaRecargo.Split('|').ElementAt(3)).Trim().Replace(".", string.Empty);
                                ivaRecargoMora = Helpers.FormatearCampos(TiposFormateo.Decimal03, lineaRecargo.Split('|').ElementAt(4)).Trim().Replace(".", string.Empty);

                                if (ordenSalidaInformacion.ContainsKey(3))
                                {
                                    ordenSalidaInformacion[3].Add(lineaRecargo);
                                }
                                else
                                {
                                    ordenSalidaInformacion.Add(3, new List<string> { lineaRecargo });
                                }
                            }
                        }
                        #endregion

                        #region Validar para Ajuste Decena
                        var lineaAjusteDecena = from busqueda in datosOriginales
                                                where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T003") && !string.IsNullOrEmpty(busqueda.Substring(6, 14).Trim().TrimStart('0'))
                                                select busqueda;

                        if (lineaAjusteDecena.Any())
                        {
                            if (!banderaAjusteDecena)
                            {
                                string lineaRecargo = GetAjusteDecena(lineaAjusteDecena.FirstOrDefault(), ref banderaAjusteDecena);

                                ajusteDecena = lineaAjusteDecena.FirstOrDefault().Substring(6, 14);

                                if (ordenSalidaInformacion.ContainsKey(3))
                                {
                                    ordenSalidaInformacion[3].Add(lineaRecargo);
                                }
                                else
                                {
                                    ordenSalidaInformacion.Add(3, new List<string> { lineaRecargo });
                                }
                            }
                        }
                        #endregion

                        totales.AddRange(datosBFI);
                        totales.AddRange(datosCFI);
                        totalesAfi.AddRange(datosBFI);

                        if (!string.IsNullOrEmpty(nombrePaquete))
                        {
                            if (buscarAFI.Any())
                            {
                                if (buscarAFI.FirstOrDefault().Split('|').ElementAt(1).Substring(0, 5) != datosPaqueteAgrupado.Key.Substring(0, 5))
                                {
                                    banderaAFI = true;
                                }
                            }
                            else
                            {
                                banderaAFI = true;
                            }

                            if (banderaAFI)
                            {
                                #region Logica AFI

                                sumaValoresBase.Clear();
                                sumaValoresIva.Clear();
                                sumaValoresImpuestos.Clear();
                                sumaValoresTotal.Clear();

                                foreach (var lineadetalle in totalesAfi)
                                {
                                    sumaValoresBase.Add(lineadetalle.Substring(16, 14).Trim());
                                    sumaValoresIva.Add(lineadetalle.Substring(44, 14).Trim());
                                }

                                sumaValoresTotal.AddRange(sumaValoresBase);
                                sumaValoresTotal.AddRange(sumaValoresIva);

                                #region Fechas y Periodos
                                var fechas = from busqueda in datosPaqueteAgrupado
                                             select busqueda.Substring(128, 19);

                                periodo = Helpers.GetFechaMasReciente(fechas.ToList());

                                if (string.IsNullOrEmpty(periodo))
                                {
                                    periodo = "99999999-99999999";
                                }

                                string Corte = string.IsNullOrEmpty(datosPaqueteAgrupado.Key.Trim()) ? "       " : datosPaqueteAgrupado.FirstOrDefault().Substring(128, 6).Trim();

                                var fechasDesde = from busqueda in lineas11C
                                                  where busqueda.Substring(137, 1).Trim().Contains("-") &&
                                                  !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                                                  select busqueda.Substring(128, 8);

                                var fechasHasta = from busqueda in lineas11C
                                                  where busqueda.Substring(137, 1).Trim().Contains("-") &&
                                                  !string.IsNullOrEmpty(busqueda.Substring(16, 14).Trim().TrimStart('0'))
                                                  select busqueda.Substring(139, 8);

                                string FechaDesdeAntigua = Helpers.GetFechaMaximaMinima(fechasDesde.ToList(), 2);
                                string FechaHastaReciente = Helpers.GetFechaMaximaMinima(fechasHasta.ToList(), 1);
                                #endregion

                                string lineaAfi = Helpers.ValidarPipePipe($"1AFI|{Corte.Substring(0, 6).Trim()}|{nombrePaquete}|{Helpers.SumarCampos(sumaValoresBase)}|" +
                                    $"{Helpers.SumarCampos(sumaValoresIva)}|{Helpers.SumarCampos(sumaValoresTotal)}| |" +
                                    $"({Helpers.FormatearCampos(TiposFormateo.Fecha04, periodo.Split('-').ElementAt(0).Trim().Substring(4, 4))} - {Helpers.FormatearCampos(TiposFormateo.Fecha04, periodo.Split('-').ElementAt(1).Trim().Substring(4, 4))})" +
                                    $"| |{FechaDesdeAntigua}|{FechaHastaReciente}| ");

                                if (ordenSalidaInformacion.ContainsKey(1))
                                {
                                    ordenSalidaInformacion[1].Add(lineaAfi);
                                }
                                else
                                {
                                    ordenSalidaInformacion.Add(1, new List<string> { lineaAfi });
                                }
                                #endregion
                            }
                        }

                        var resultadosOrdenados = from busqueda in ordenSalidaInformacion
                                                  orderby busqueda.Key ascending
                                                  select busqueda;

                        foreach (var lineaFinal in resultadosOrdenados.SelectMany(x => x.Value))
                        {
                            resultado.Add(lineaFinal);
                        }
                        #endregion
                    }

                    #region Logica TFI

                    sumaValoresBase.Clear();
                    sumaValoresIva.Clear();
                    sumaValoresImpuestos.Clear();
                    sumaValoresTotal.Clear();

                    sumaValoresBase.Add(baseRecargoMora);
                    sumaValoresIva.Add(ivaRecargoMora);
                    sumaValoresBase.Add(ajusteDecena);

                    foreach (var lineadetalle in totales)
                    {
                        sumaValoresBase.Add(lineadetalle.Substring(16, 14).Trim());
                        sumaValoresIva.Add(lineadetalle.Substring(44, 14).Trim());
                    }

                    sumaValoresTotal.AddRange(sumaValoresBase);
                    sumaValoresTotal.AddRange(sumaValoresIva);

                    if (sumaValoresTotal.Any())
                    {
                        string lineaTfi = Helpers.ValidarPipePipe($"1TFI| |TOTAL|{Helpers.SumarCampos(sumaValoresBase)}|" +
                            $"{Helpers.SumarCampos(sumaValoresIva)}|{Helpers.FormatearCampos(TiposFormateo.Redondear, Helpers.SumarCampos(sumaValoresTotal))}| | ");

                        resultado.Add(lineaTfi);
                    }

                    #endregion
                }
            }

            var resultadoOrdenado = from busqueda in resultado
                                    orderby busqueda.Split('|').ElementAt(0) ascending
                                    select busqueda;

            if (resultadoOrdenado.Count() == 2)
            {
                if (resultadoOrdenado.FirstOrDefault().Contains("Ajuste Decena"))
                {
                    return new List<string>();
                }
            }

            if (resultadoOrdenado.Where(x => x.Contains("1CFI")).Any() &&
                resultadoOrdenado.Where(x => x.Contains("1TFI")).Any() &&
                !resultadoOrdenado.Where(x => x.Contains("1BFI")).Any())
            {
                #region Logica Desagrupados
                var lineasCFI = from busqueda in resultadoOrdenado
                                where busqueda.Substring(0, 4).Equals("1CFI")
                                select busqueda;

                var lineaTFI = from busqueda in resultadoOrdenado
                               where busqueda.Substring(0, 4).Equals("1TFI")
                               select busqueda;

                Dictionary<string, List<string>> validarDuplicados = new Dictionary<string, List<string>>();
                List<string> resultadoFinalOrdenado = new List<string>();
                bool banderaDuplicados = false;

                foreach (var linea in lineasCFI)
                {
                    string llave = linea.Split('|').ElementAt(2);

                    if (validarDuplicados.ContainsKey(llave))
                    {
                        banderaDuplicados = true;
                        validarDuplicados[llave].Add(linea);
                    }
                    else
                    {
                        validarDuplicados.Add(llave, new List<string> { linea });
                    }
                }

                if (banderaDuplicados)
                {
                    #region Desagrupados
                    foreach (var detallesDuplicados in validarDuplicados)
                    {
                        if (detallesDuplicados.Value.Count > 1)
                        {
                            sumaValoresBase.Clear();
                            sumaValoresIva.Clear();
                            sumaValoresImpuestos.Clear();
                            sumaValoresTotal.Clear();

                            foreach (var lineadetalle in detallesDuplicados.Value)
                            {
                                sumaValoresBase.Add(Helpers.FormatearCampos(TiposFormateo.Decimal03, lineadetalle.Split('|').ElementAt(3)).Replace(".", string.Empty));
                                sumaValoresIva.Add(Helpers.FormatearCampos(TiposFormateo.Decimal03, lineadetalle.Split('|').ElementAt(4)).Replace(".", string.Empty));
                                sumaValoresImpuestos.Add(Helpers.FormatearCampos(TiposFormateo.Decimal03, lineadetalle.Split('|').ElementAt(6)).Replace(".", string.Empty));
                            }

                            sumaValoresTotal.AddRange(sumaValoresBase);
                            sumaValoresTotal.AddRange(sumaValoresIva);
                            sumaValoresTotal.AddRange(sumaValoresImpuestos);

                            resultadoFinalOrdenado.Add(Helpers.ValidarPipePipe($"{detallesDuplicados.Value.FirstOrDefault().Split('|').ElementAt(0)}|" +
                                $"{detallesDuplicados.Value.FirstOrDefault().Split('|').ElementAt(1)}|" +
                                $"{detallesDuplicados.Value.FirstOrDefault().Split('|').ElementAt(2)}|" +
                                $"{Helpers.SumarCampos(sumaValoresBase)}|" +
                                $"{Helpers.SumarCampos(sumaValoresIva)}|" +
                                $"{Helpers.SumarCampos(sumaValoresTotal)}|" +
                                $"{Helpers.SumarCampos(sumaValoresImpuestos)}| "));
                        }
                        else
                        {
                            resultadoFinalOrdenado.Add(detallesDuplicados.Value.FirstOrDefault());
                        }
                    }

                    resultadoFinalOrdenado.Add(lineaTFI.FirstOrDefault());

                    return resultadoFinalOrdenado;
                    #endregion
                }

                #endregion
            }

            return resultadoOrdenado.ToList();
            #endregion
        }

        private string GetAjusteDecena(string pDato, ref bool pBandera)
        {
            #region GetAjusteDecena
            string resultado = string.Empty;

            resultado = $"1CFI| |Ajuste Decena|{Helpers.FormatearCampos(TiposFormateo.Decimal05, pDato.Substring(6, 14))}| |{Helpers.FormatearCampos(TiposFormateo.Decimal05, pDato.Substring(6, 14))}| | ";

            pBandera = true;

            return resultado;

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
                foreach (var item in linea40000)
                {
                    if (IsLte || IsLteCorporativo && GetTipo(item.Substring(6, 20).Trim()) != "Cuenta")
                    {
                        resultado = $"ADNC|{linea40000.FirstOrDefault().Substring(26, 49).Trim()}";

                        if (!string.IsNullOrEmpty(resultado))
                        {
                            break;
                        }
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
        private IEnumerable<string> MapeoPaqueteADN1(List<string> datosOriginales)
        {
            #region MapeoPaqueteADN1

            #region Variables
            List<string> resultadoTemporal = new List<string>();
            List<string> resultadoFinal = new List<string>();
            List<string> temp = new List<string>();
            List<string> sumaValoresBase = new List<string>();
            List<string> sumaValoresIva = new List<string>();
            List<string> sumaValoresImpuestos = new List<string>();
            List<string> sumaValoresTotal = new List<string>();
            dynamic resultadoFormateo = null;
            Dictionary<string, List<string>> paquetesInformacion = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> paquetesInformacionFinal = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> paquetesInformacion888888 = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> paquetesInformacion888888Final = new Dictionary<string, List<string>>();
            string cuentaConexion = string.Empty;
            bool banderaPaquete = false;
            bool banderaRecargoMora = false;
            #endregion

            //Se arma paquete con los canales ADN1 - 1OOA - 1OPL - 1OOB - 1ODC - 1ODD - 1HIS - 1HDT - 1OOO - 1OMT - 1OMV

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

            #region Organizacion Paquetes de Informacion 888888
            foreach (var linea in datosOriginales)
            {
                if (linea.Substring(0, 6).Equals("888888"))
                {
                    cuentaConexion = linea.Substring(6, 20).Trim();
                    var baseComparacion = linea.Substring(96, 14).Trim().TrimStart('0');

                    if (!paquetesInformacion888888.ContainsKey(cuentaConexion) && !string.IsNullOrEmpty(baseComparacion))
                    {
                        paquetesInformacion888888.Add(cuentaConexion, new List<string> { linea });
                    }
                }
            }
            #endregion

            //if (paquetesInformacion888888.Count == 2)
            //{
            //    #region Reordenamiento
            //    //Se reordena 
            //    foreach (var item in paquetesInformacion)
            //    {
            //        if (paquetesInformacionFinal.ContainsKey("1"))
            //        {
            //            paquetesInformacionFinal["1"].AddRange(item.Value);
            //        }
            //        else
            //        {
            //            paquetesInformacionFinal.Add("1", item.Value);
            //        }
            //    }

            //    foreach (var item in paquetesInformacion888888)
            //    {
            //        if (paquetesInformacion888888Final.ContainsKey("1"))
            //        {
            //            paquetesInformacion888888Final["1"].AddRange(item.Value);
            //        }
            //        else
            //        {
            //            paquetesInformacion888888Final.Add("1", item.Value);
            //        }
            //    }
            //    #endregion
            //}
            //else
            //{
                paquetesInformacionFinal = paquetesInformacion;
                paquetesInformacion888888Final = paquetesInformacion888888;
            //}

            foreach (var llaveCuentaConexion in paquetesInformacion888888)
            {
                if (paquetesInformacion.ContainsKey(llaveCuentaConexion.Key))
                {
                    #region Limpieza Variables
                    sumaValoresBase.Clear();
                    sumaValoresIva.Clear();
                    sumaValoresImpuestos.Clear();
                    sumaValoresTotal.Clear();
                    Is1OOA = false;
                    Is1ODC = false;
                    resultadoTemporal = new List<string>();
                    #endregion

                    #region Mapeo Canales por linea 
                    resultadoFormateo = FormateoCanalADN1(paquetesInformacion[llaveCuentaConexion.Key]);

                    if (((IEnumerable<string>)resultadoFormateo).Any())
                    {
                        resultadoTemporal.AddRange(resultadoFormateo);
                    }

                    resultadoFormateo = MapeoCanal1OOA(paquetesInformacion[llaveCuentaConexion.Key]);

                    if (!string.IsNullOrEmpty(resultadoFormateo))
                    {
                        resultadoTemporal.Add(resultadoFormateo);
                        Is1OOA = true;
                    }

                    resultadoFormateo = MapeoCanal1OPL(paquetesInformacion[llaveCuentaConexion.Key]);

                    if (!string.IsNullOrEmpty(resultadoFormateo))
                    {
                        resultadoTemporal.Add(resultadoFormateo);
                    }

                    resultadoFormateo = MapeoCanal1OOB(paquetesInformacion[llaveCuentaConexion.Key]);

                    if (((IEnumerable<string>)resultadoFormateo).Any())
                    {
                        resultadoTemporal.AddRange(resultadoFormateo);
                    }

                    temp = new List<string>();

                    temp.AddRange(paquetesInformacion[llaveCuentaConexion.Key]);
                    temp.AddRange(llaveCuentaConexion.Value);

                    resultadoFormateo = FormateoCanal1ODC(temp);

                    if (!string.IsNullOrEmpty(resultadoFormateo))
                    {
                        resultadoTemporal.Add(resultadoFormateo);
                        Is1ODC = true;
                    }

                    resultadoFormateo = FormateoCanal1ODD(paquetesInformacion[llaveCuentaConexion.Key], datosOriginales, ref banderaRecargoMora);

                    if (((IEnumerable<string>)resultadoFormateo).Any())
                    {
                        var Ordenado = from busqueda in ((IEnumerable<string>)resultadoFormateo)
                                       orderby busqueda.Split('|').ElementAt(2).Substring(0, 1) ascending
                                       select busqueda;

                        resultadoTemporal.AddRange(Ordenado.ToList());

                        if (Is1ODC)
                        {
                            #region Datos Finales 1ODC
                            foreach (var lineaDetalle in ((IEnumerable<string>)resultadoFormateo))
                            {
                                sumaValoresBase.Add(Helpers.FormatearCampos(TiposFormateo.Decimal03, lineaDetalle.Split('|').ElementAt(3)).Replace(".", String.Empty));
                                sumaValoresIva.Add(Helpers.FormatearCampos(TiposFormateo.Decimal03, lineaDetalle.Split('|').ElementAt(4)).Replace(".", String.Empty));
                                sumaValoresImpuestos.Add(Helpers.FormatearCampos(TiposFormateo.Decimal03, lineaDetalle.Split('|').ElementAt(5)).Replace(".", String.Empty));
                                sumaValoresTotal.Add(Helpers.FormatearCampos(TiposFormateo.Decimal03, lineaDetalle.Split('|').ElementAt(6)).Replace(".", String.Empty));
                            }

                            string lineaODC = resultadoTemporal.Find(x => x.Substring(0, 4).Equals("1ODC"));
                            int lineaODCIndice = resultadoTemporal.FindIndex(x => x.Substring(0, 4).Equals("1ODC"));

                            lineaODC = lineaODC.Replace("***BASE***", Helpers.SumarCampos(sumaValoresBase));
                            lineaODC = lineaODC.Replace("***IVA***", Helpers.SumarCampos(sumaValoresIva));
                            lineaODC = lineaODC.Replace("***IMPUESTOS***", Helpers.SumarCampos(sumaValoresImpuestos));
                            lineaODC = lineaODC.Replace("***TOTAL***", Helpers.SumarCampos(sumaValoresTotal));

                            resultadoTemporal[lineaODCIndice] = lineaODC;
                            #endregion
                        }
                    }

                    resultadoFormateo = FormateoCanal1OOO(paquetesInformacion[llaveCuentaConexion.Key]);

                    if (((IEnumerable<string>)resultadoFormateo).Any())
                    {
                        resultadoTemporal.AddRange(resultadoFormateo);
                    }

                    resultadoFormateo = FormateoCanal1OMT(paquetesInformacion[llaveCuentaConexion.Key]);

                    if (!string.IsNullOrEmpty(resultadoFormateo))
                    {
                        resultadoTemporal.Add(resultadoFormateo);
                    }

                    resultadoFormateo = MapeoCanal1OMV(paquetesInformacion[llaveCuentaConexion.Key]);

                    if (((IEnumerable<string>)resultadoFormateo).Any())
                    {
                        resultadoTemporal.AddRange(resultadoFormateo);
                    }
                    #endregion

                    resultadoFinal.AddRange(resultadoTemporal);
                }
            }

            return resultadoFinal;
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
                        listaCortes.Add(new PosCortes(179, 14, TiposFormateo.Decimal05));
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
                //listaCortes.Add(new PosCortes(48, 14, TiposFormateo.Decimal05));
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
                int totalDispositivosFinanciados = 0;
                Int64 totalValorFinanciado = 0;
                Int64 totalValorPorPagar = 0;
                Int64 totalSaldoRestante = 0;

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
                    Int64 valorXPagar = valorFinanciado - saldoRestante;
                    string valorXPagarFormat = Helpers.FormatearCampos(TiposFormateo.Decimal05, valorXPagar.ToString());

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
                        totalDispositivosFinanciados++;
                        totalValorFinanciado += valorFinanciado;
                        totalSaldoRestante += saldoRestante;
                        totalValorPorPagar += valorXPagar;
                    }
                    else if (IsResidencial)
                    {
                        resultado.Add($"1III|{descripcion}|{camposPlanPago[0]}|{cuota}|{valorFinanciadoFormat}|{saldoRestanteFormat}|{valorXPagarFormat}|{interes}| ");
                    }
                    else if (valorFinanciado > 0)
                    {
                        resultado.Add($"1III|{descripcion}|{valorFinanciadoFormat}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, item.Substring(16, 14))}|{saldoRestanteFormat}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, item.Substring(44, 14))}|{cuota}|{interes}| ");
                    }

                }

                foreach (var item in resultado)
                {
                    if (item.Contains(""))
                    {
                        item.Replace("****interes_financiacion****", interes);
                    }
                }

                if (IsLteCorporativo && totalDispositivosFinanciados > 0)
                {
                    resultado.Add($"1IRT|{totalDispositivosFinanciados.ToString()}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, totalValorFinanciado.ToString())}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, totalSaldoRestante.ToString())}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, totalValorPorPagar.ToString())}| ");
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
            bool banderaCreacionPaquete = false;
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

                        resultado.Add(Helpers.ValidarPipePipe($"1EE1|-|{descripcionTitulo}" +
                            $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, iva)}|" +
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
                                $"{lineaDet.Substring(33, 10)}|{lineaDet.Substring(96, 11)}|{lineaDet.Substring(66, 2).TrimStart('0')}:00|{Helpers.FormatearCampos(TiposFormateo.Decimal05, lineaDet.Substring(47, 9)).Replace("$", "").Trim()}|" +
                                $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, lineaDet.Substring(56, 9)).Replace("$", "").Trim()}| "));
                        }

                        resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, descripcionTitulo)}" +
                            $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, iva)}|" +
                            $"{Helpers.SumarCampos(new List<string> { @base, iva })}| | | "));

                        #endregion
                    }
                    banderaCreacionPaquete = true;
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
                        $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|{Helpers.SumarCampos(new List<string> { iva, restaImpuesto })}|" +
                        $"{Helpers.SumarCampos(new List<string> { @base, iva, restaImpuesto, impuesto })}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, impuesto)}| | "));
                    #endregion

                    #region Datos 1EE2
                    resultado.Add(Helpers.ValidarPipePipe($"1EE2|-| | "));
                    #endregion

                    #region Datos 1EE3

                    foreach (var linea in lineas12MAnexosOtrosOperadores)
                    {
                        resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.Fecha12, linea.Substring(6, 10))}|" +
                            $"{Helpers.FormatearCampos(TiposFormateo.HoraMinutoSegundo, linea.Substring(14, 6))}|{linea.Substring(20, 10).Trim()}|" +
                            $"{linea.Substring(33, 10).Trim()}|{linea.Substring(96, 11).Trim()}|{linea.Substring(66, 2).TrimStart('0')}:00|{Helpers.FormatearCampos(TiposFormateo.Decimal05, linea.Substring(47, 9)).Replace("$", "").Trim()}|" +
                            $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, linea.Substring(56, 9)).Replace("$", "").Trim()}| "));
                    }

                    resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, lineaNegocio)}" +
                        $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|{Helpers.SumarCampos(new List<string> { iva, restaImpuesto })}|" +
                        $"{Helpers.SumarCampos(new List<string> { @base, iva, restaImpuesto, impuesto })}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, impuesto)}| | "));

                    #endregion

                    banderaCreacionPaquete = true;
                }

                #endregion

                if (!banderaCreacionPaquete && lineas12MAnexosETB.Any())
                {
                    resultado.Add($"ADN1|-|ANEXO ETB| | | | ");

                    #region Datos 1EE1
                    llaveConcepto = $"CODT{lineas12MAnexosETB.FirstOrDefault().Substring(0, 6)}";

                    descripcionTitulo = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveConcepto).FirstOrDefault()?.Substring(19).Trim() ?? "";

                    @base = "000";
                    iva = "000";

                    resultado.Add(Helpers.ValidarPipePipe($"1EE1|-|{descripcionTitulo}" +
                        $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, iva)}|" +
                        $"{Helpers.SumarCampos(new List<string> { @base, iva })}| | | "));
                    #endregion

                    #region Datos 1EE2
                    resultado.Add(Helpers.ValidarPipePipe($"1EE2|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, descripcionTitulo)}|LD| "));
                    #endregion

                    #region Datos 1EE3

                    foreach (var lineaDet in lineas12MAnexosETB)
                    {
                        resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.Fecha12, lineaDet.Substring(6, 10))}|" +
                            $"{Helpers.FormatearCampos(TiposFormateo.HoraMinutoSegundo, lineaDet.Substring(14, 6))}|{lineaDet.Substring(20, 10).Trim()}|" +
                            $"{lineaDet.Substring(33, 10)}|{lineaDet.Substring(96, 11)}|{lineaDet.Substring(66, 2).TrimStart('0')}:00|{Helpers.FormatearCampos(TiposFormateo.Decimal05, lineaDet.Substring(47, 9)).Replace("$", "").Trim()}|" +
                            $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, lineaDet.Substring(56, 9)).Replace("$", "").Trim()}| "));
                    }

                    resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, descripcionTitulo)}" +
                        $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, iva)}|" +
                        $"{Helpers.SumarCampos(new List<string> { @base, iva })}| | | "));

                    #endregion
                }

                if (!banderaCreacionPaquete && lineas12MAnexosOtrosOperadores.Any())
                {
                    resultado.Add($"ADN1|-|ANEXO OTROS OPERADORES| | | | ");

                    #region Datos 1EE1
                    llaveConcepto = $"CODT{lineas12MAnexosOtrosOperadores.FirstOrDefault().Substring(0, 6)}";

                    descripcionTitulo = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveConcepto).FirstOrDefault()?.Substring(11).Trim() ?? "";

                    descripcionTitulo = descripcionTitulo.Substring(0, descripcionTitulo.IndexOf("LD")).Trim();

                    llaveBusquedaNit = $"OPER06T{lineas12MAnexosOtrosOperadores.FirstOrDefault().Substring(3, 3)}";

                    nit = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveBusquedaNit).FirstOrDefault()?.Substring(11).Trim() ?? "";

                    lineaNegocio = $"{descripcionTitulo} NIT: {nit}";

                    @base = "000";
                    iva = "000";
                    restaImpuesto = "-00";
                    impuesto = "00";

                    resultado.Add(Helpers.ValidarPipePipe($"1EE1|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, lineaNegocio)}" +
                        $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|{Helpers.SumarCampos(new List<string> { iva, restaImpuesto })}|" +
                        $"{Helpers.SumarCampos(new List<string> { @base, iva, restaImpuesto, impuesto })}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, impuesto)}| | "));
                    #endregion

                    #region Datos 1EE2
                    resultado.Add(Helpers.ValidarPipePipe($"1EE2|-| | "));
                    #endregion

                    #region Datos 1EE3

                    foreach (var linea in lineas12MAnexosOtrosOperadores)
                    {
                        resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.Fecha12, linea.Substring(6, 10))}|" +
                            $"{Helpers.FormatearCampos(TiposFormateo.HoraMinutoSegundo, linea.Substring(14, 6))}|{linea.Substring(20, 10).Trim()}|" +
                            $"{linea.Substring(33, 10).Trim()}|{linea.Substring(96, 11).Trim()}|{linea.Substring(65, 3).TrimStart('0')}:00|{Helpers.FormatearCampos(TiposFormateo.Decimal05, linea.Substring(47, 9)).Replace("$", "").Trim()}|" +
                            $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, linea.Substring(56, 9)).Replace("$", "").Trim()}| "));
                    }

                    resultado.Add(Helpers.ValidarPipePipe($"1EE3|-|{Helpers.FormatearCampos(TiposFormateo.LetraCapital, lineaNegocio)}" +
                        $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|{Helpers.SumarCampos(new List<string> { iva, restaImpuesto })}|" +
                        $"{Helpers.SumarCampos(new List<string> { @base, iva, restaImpuesto, impuesto })}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, impuesto)}| | "));

                    #endregion
                }
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


        /// Metodo que obtiene la linea formateada de Canal 1PPV
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1PPV(List<string> datosOriginales)
        {
            #region MapeoCanal1PPV
            string resultado = string.Empty;
            string numeroConexion = string.Empty;
            double mesActual = 0;
            double promedio = 0;
            double valormes1 = 0;
            double valormes2 = 0;
            double valormes3 = 0;
            double valormes4 = 0;
            double valormes5 = 0;
            double valormes6 = 0;

            var linea10000 = from busqueda in datosOriginales
                             where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                             select busqueda;

            var linea070001 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("070001")
                              select busqueda;

            if (IsLte || IsLteCorporativo)
            {
                if (linea070001.Any())
                {
                    foreach (var item in linea070001)
                    {
                        mesActual += (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, item.Substring(6, 8).Trim())) * 60 / 10);
                        promedio += (Convert.ToDouble(Helpers.FormatearCampos(TiposFormateo.Decimal02, item.Substring(62, 8).Trim())) * 60 / 10);
                        valormes1 += Convert.ToDouble(item.Substring(14, 8).Trim());
                        valormes2 += Convert.ToDouble(item.Substring(22, 8).Trim());
                        valormes3 += Convert.ToDouble(item.Substring(30, 8).Trim());
                        valormes4 += Convert.ToDouble(item.Substring(38, 8).Trim());
                        valormes5 += Convert.ToDouble(item.Substring(46, 8).Trim());
                        valormes6 += Convert.ToDouble(item.Substring(54, 8).Trim());
                    }

                    string lineaMeses = $"XXXXXXXXXXXXXX{valormes1.ToString().PadLeft(8, '0')}" +
                        $"{valormes2.ToString().PadLeft(8, '0')}" +
                        $"{valormes3.ToString().PadLeft(8, '0')}" +
                        $"{valormes4.ToString().PadLeft(8, '0')}" +
                        $"{valormes5.ToString().PadLeft(8, '0')}" +
                        $"{valormes6.ToString().PadLeft(8, '0')}";

                    resultado = Helpers.ValidarPipePipe($"1PPV|Mes Actual: {mesActual}|{ArmarMesesHistograma(Helpers.FormatearCampos(TiposFormateo.Fecha01, linea10000.FirstOrDefault().Substring(168, 8)))}|" +
                        $"{ArmarValoresHistograma(lineaMeses, "2")}|Promedio: {promedio}| ");
                }
            }

            return resultado;
            #endregion
        }

        /// Metodo que obtiene la linea formateada de Canal 1PPD
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string MapeoCanal1PPD(List<string> datosOriginales)
        {
            #region MapeoCanal1PPD
            string resultado = string.Empty;
            string numeroConexion = string.Empty;
            double mesActual = 0;
            double promedio = 0;
            double valormes1 = 0;
            double valormes2 = 0;
            double valormes3 = 0;
            double valormes4 = 0;
            double valormes5 = 0;
            double valormes6 = 0;

            var linea10000 = from busqueda in datosOriginales
                             where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                             select busqueda;

            var linea070002 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("070002")
                              select busqueda;

            if (IsLte || IsLteCorporativo)
            {
                if (linea070002.Any())
                {
                    foreach (var item in linea070002)
                    {
                        mesActual += (Convert.ToDouble(item.Substring(6, 6).Trim()) / 10);
                        promedio += (Convert.ToDouble(item.Substring(48, 6).Trim()) / 10);
                        valormes1 += Convert.ToDouble(item.Substring(12, 6).Trim());
                        valormes2 += Convert.ToDouble(item.Substring(18, 6).Trim());
                        valormes3 += Convert.ToDouble(item.Substring(24, 6).Trim());
                        valormes4 += Convert.ToDouble(item.Substring(30, 6).Trim());
                        valormes5 += Convert.ToDouble(item.Substring(36, 6).Trim());
                        valormes6 += Convert.ToDouble(item.Substring(42, 6).Trim());
                    }

                    string lineaMeses = $"XXXXXXXXXXXX{valormes1.ToString().PadLeft(6, '0')}" +
                        $"{valormes2.ToString().PadLeft(6, '0')}" +
                        $"{valormes3.ToString().PadLeft(6, '0')}" +
                        $"{valormes4.ToString().PadLeft(6, '0')}" +
                        $"{valormes5.ToString().PadLeft(6, '0')}" +
                        $"{valormes6.ToString().PadLeft(6, '0')}";

                    resultado = Helpers.ValidarPipePipe($"1PPD|Mes Actual: {mesActual.ToString().Replace(".", string.Empty)}|{ArmarMesesHistograma(Helpers.FormatearCampos(TiposFormateo.Fecha01, linea10000.FirstOrDefault().Substring(168, 8)))}|" +
                        $"{ArmarValoresHistograma(lineaMeses, "3")}|Promedio: {promedio.ToString().Replace(".", string.Empty)}| ");
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

                resultado = Helpers.ValidarPipePipe($"1FFA|Total|{Helpers.FormatearCampos(TiposFormateo.Decimal05, total)}" +
                    $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, subtotal)}|{Helpers.SumarCampos(new List<string> { total, subtotal })}| ");
            }

            return resultado;
            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal 1DET
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoCanal1DET(List<string> datosOriginales)
        {
            #region MapeoCanal1DET
            List<string> list1DET = new List<string>();
            string Lineas1DET = string.Empty;
            string linea1PLA = string.Empty;
            string numeroConexion = string.Empty;
            List<string> camposSumar = new List<string>();
            List<PosCortes> listaCortes = new List<PosCortes>();

            Dictionary<string, List<string>> dicAgruNumConexion = new Dictionary<string, List<string>>();
            List<string> paqueteActual = new List<string>();
            string llaveCruve = string.Empty;

            List<string> valor1 = new List<string>();
            List<string> valor2 = new List<string>();
            List<string> valor3 = new List<string>();
            List<string> total = new List<string>();
            string periodo = string.Empty;
            string primerFecha = string.Empty;
            string segundaFecha = string.Empty;
            bool primerReg1DET = true;

            if (Is1ODC)
            {
                #region armar paquetes
                foreach (var lineaActual in datosOriginales)
                {
                    if (lineaActual.Substring(0, 6) == "040000") // inicio Paquete
                    {
                        llaveCruve = lineaActual.Substring(0, 25).Trim();
                    }

                    if (!string.IsNullOrEmpty(llaveCruve))
                    {
                        // Se llena Diccionario a procesar

                        if (dicAgruNumConexion.ContainsKey(llaveCruve))
                        {
                            dicAgruNumConexion[llaveCruve].Add(lineaActual);
                        }
                        else
                        {
                            dicAgruNumConexion.Add(llaveCruve, new List<string> { lineaActual });
                        }
                    }
                }
                #endregion

                #region Procesamiento

                foreach (var dicAgruNumConexionActual in dicAgruNumConexion)
                {
                    #region Busquedas
                    var lineas040000 = from busqueda in dicAgruNumConexionActual.Value
                                       where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                       select busqueda;

                    var lineas11C901 = from busqueda in dicAgruNumConexionActual.Value
                                       where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("11C901")
                                       select busqueda;
                    #endregion

                    #region Mapeo

                    if (lineas040000.Any())
                    {
                        if (lineas040000.FirstOrDefault().Substring(6, 20).Trim() != Cuenta)
                        {
                            numeroConexion = lineas040000.FirstOrDefault().Substring(6, 20).Trim();

                            // Se toman los valores
                            if (lineas11C901.Any())
                            {
                                foreach (var linea11CActual in lineas11C901)
                                {
                                    valor1.Add(linea11CActual.Substring(16, 14));
                                    valor2.Add(linea11CActual.Substring(44, 14));
                                    valor3.Add(linea11CActual.Substring(172, 14));

                                    if (linea11CActual.Substring(128, 19).Contains("-"))
                                    {
                                        periodo = linea11CActual.Substring(128, 6);
                                        listaCortes.Clear();
                                        listaCortes.Add(new PosCortes(128, 8, TiposFormateo.Fecha06));
                                        primerFecha = Helpers.ExtraccionCamposSpool(listaCortes, linea11CActual);
                                        listaCortes.Clear();

                                        listaCortes.Add(new PosCortes(139, 8, TiposFormateo.Fecha06));
                                        segundaFecha = Helpers.ExtraccionCamposSpool(listaCortes, linea11CActual);
                                    }
                                }
                            }

                            // sumar el total
                            total.AddRange(valor1);
                            total.AddRange(valor2);
                            total.AddRange(valor3);

                            Lineas1DET = "1DET|";
                            Lineas1DET += numeroConexion + "|";
                            Lineas1DET += "Total|";
                            Lineas1DET += Helpers.SumarCampos(valor1) + "|";
                            Lineas1DET += Helpers.SumarCampos(valor2) + "|";
                            Lineas1DET += Helpers.SumarCampos(valor3) + "|";
                            Lineas1DET += Helpers.SumarCampos(total) + "|";
                            Lineas1DET += periodo + "| ";
                            Lineas1DET += primerFecha + " a " + segundaFecha + "| ";

                            valor1.Clear();
                            valor2.Clear();
                            valor3.Clear();
                            total.Clear();

                            if (!string.IsNullOrEmpty(Lineas1DET))
                            {
                                if (Is1PLA == false && primerReg1DET)
                                {
                                    linea1PLA = "1PLA||||||||| ";
                                    list1DET.Add(Helpers.ValidarPipePipe(linea1PLA));
                                    primerReg1DET = false;
                                }

                                list1DET.Add(Helpers.ValidarPipePipe(Lineas1DET));
                            }
                        }
                    }

                    #endregion

                }

                #endregion
            }

            return list1DET;
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
                Lineas1CIC = $"1CIC| |Impuesto Consumo {Helpers.FormatearCampos(TiposFormateo.Decimal05, Lineas1CIC.Substring(12, 20).TrimStart('0').Trim())}| ";
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

                resultado = Helpers.ValidarPipePipe($"1RRT|Total|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|" +
                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, iva)}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, impuesto)}|" +
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

                resultado = Helpers.ValidarPipePipe($"1RRR|{concepto}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, @base)}|" +
                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, iva)}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, impuesto)}|" +
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

                    if (string.IsNullOrEmpty(valorMinutosConsumo))
                    {
                        valorMinutosConsumo = "0";
                    }

                    resultado += $"1CMP|Minutos del Plan {minutosPlan}|Minutos Consumidos {valorMinutosConsumo}| ";
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
            List<string> mesesFinal = new List<string>();
            CultureInfo culture = new CultureInfo("es-CO");

            if (!string.IsNullOrEmpty(pFechaReferencia))
            {
                byte mesFacturacion = Convert.ToByte(pFechaReferencia.Split('/').ElementAt(1));
                mesFacturacion--;

                for (int i = mesFacturacion; i <= mesFacturacion; i--)
                {
                    if (i == 0 && meses.LastOrDefault() == "Ene")
                    {
                        meses.Add(Helpers.FormatearCampos(TiposFormateo.LetraCapital, new DateTime(DateTime.Now.Year, 12, 1).ToString("MMM", culture).Replace(".", string.Empty)));
                        break;
                    }

                    meses.Add(Helpers.FormatearCampos(TiposFormateo.LetraCapital, new DateTime(DateTime.Now.Year, i, 1).ToString("MMM", culture).Replace(".", string.Empty)));
                }
                meses.RemoveAt(0);
                meses.RemoveAt(meses.Count - 1);

                for (int i = 0; i < meses.Count; i++)
                {
                    if (i == 6)
                    {
                        break;
                    }
                    mesesFinal.Add(meses[i]);
                }

                mesesFinal.Reverse();

                resultado = $"{mesesFinal.ElementAt(0).Substring(0, 3)}|{mesesFinal.ElementAt(1).Substring(0, 3)}|{mesesFinal.ElementAt(2).Substring(0, 3)}|{mesesFinal.ElementAt(3).Substring(0, 3)}|{mesesFinal.ElementAt(4).Substring(0, 3)}|{mesesFinal.ElementAt(5).Substring(0, 3)}";
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
                    valor1 = (Convert.ToDouble(pLineaDatos.Substring(12, 6).Trim())).ToString().Replace(".", string.Empty);
                    valor2 = (Convert.ToDouble(pLineaDatos.Substring(18, 6).Trim())).ToString().Replace(".", string.Empty);
                    valor3 = (Convert.ToDouble(pLineaDatos.Substring(24, 6).Trim())).ToString().Replace(".", string.Empty);
                    valor4 = (Convert.ToDouble(pLineaDatos.Substring(30, 6).Trim())).ToString().Replace(".", string.Empty);
                    valor5 = (Convert.ToDouble(pLineaDatos.Substring(36, 6).Trim())).ToString().Replace(".", string.Empty);
                    valor6 = (Convert.ToDouble(pLineaDatos.Substring(42, 6).Trim())).ToString().Replace(".", string.Empty);
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
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal05, pLineaTablaSustitucion.Substring(14, 10));
                        break;

                    case "2":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal05, pLineaTablaSustitucion.Substring(24, 10));
                        break;

                    case "3":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal05, pLineaTablaSustitucion.Substring(34, 10));
                        break;

                    case "4":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal05, pLineaTablaSustitucion.Substring(44, 10));
                        break;

                    case "5":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal05, pLineaTablaSustitucion.Substring(54, 10));
                        break;

                    case "6":
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal05, pLineaTablaSustitucion.Substring(64, 10));
                        break;

                    default:
                        //Por defecto va con estrato 6
                        resultado = Helpers.FormatearCampos(TiposFormateo.Decimal05, pLineaTablaSustitucion.Substring(64, 10));
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

            if (linea40000.Any())
            {
                if (IsLte || IsLteCorporativo)
                {
                    resultado.Add(Helpers.ValidarPipePipe($"ADN1|{linea40000.FirstOrDefault().Substring(6, 20).Trim()}|{GetTipo(linea40000.FirstOrDefault().Substring(6, 20).Trim())}|" +
                        $"{Helpers.FormatearCampos(TiposFormateo.LetraCapital, linea40000.FirstOrDefault().Substring(76, 39).Trim())}| | | "));
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
                                listaCortes.Add(new PosCortes(16, 14, TiposFormateo.Decimal05));
                                listaCortes.Add(new PosCortes(44, 14, TiposFormateo.Decimal05));
                                listaCortes.Add(new PosCortes(172, 14, TiposFormateo.Decimal05));
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoPaqueteNTC(List<string> datosOriginales)
        {
            #region FormateoPaqueteNTC
            List<string> resultado = new List<string>();

            int indiceBusquedaInicio = datosOriginales.FindIndex(x => x.Substring(0, 5).Equals("28000"));

            for (int i = indiceBusquedaInicio; i < datosOriginales.Count; i++)
            {
                string linea = datosOriginales.ElementAt(i);
                string canal = datosOriginales.ElementAt(i).Substring(0, 5);

                switch (canal)
                {
                    case "28000":
                        resultado.Add(FormateoCanalNTC0(new List<string> { linea }));
                        break;

                    case "29000":
                        resultado.Add(FormateoCanalNTC1(new List<string> { linea }));
                        break;

                    case "30000":
                        resultado.Add(MapeoCanalNTC2(new List<string> { linea }));
                        break;

                    case "30001":
                        resultado.Add(MapeoCanalNTC3(new List<string> { linea }));
                        break;

                    case "30002":
                        resultado.AddRange(MapeoCanalNTC4(new List<string> { linea }));
                        break;

                    case "30003":
                        resultado.AddRange(MapeoCanalNTC5(new List<string> { linea }));
                        break;
                }
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
                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal05, linea29000.FirstOrDefault().Substring(29, 20).Trim())}|{linea29000.FirstOrDefault().Substring(49).Trim()}| ");
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
            List<string> sumaValoresIva = new List<string>();
            List<string> sumaValoresImpuestos = new List<string>();
            List<string> sumaValoresTotal = new List<string>();

            var linea888888 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("888888")
                              select busqueda;

            if (IsLte || IsLteCorporativo)
            {
                var linea040000 = from busqueda in datosOriginales
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

                if (linea888888.Any())
                {
                    resultado = Helpers.ValidarPipePipe($"1ODC|{cuentaConexion}|Total|***BASE***|" +
                        $"***IVA***|***IMPUESTOS***|***TOTAL***|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}| ");
                }
                else
                {
                    if (!string.IsNullOrEmpty(cuentaConexion))
                    {
                        if (GetTipo(cuentaConexion) != "Cuenta")
                        {
                            resultado = Helpers.ValidarPipePipe($"1ODC|{cuentaConexion}|Total|$ 0,00|$ 0,00|$ 0,00|$ 0,00| | ");
                        }
                    }
                }
            }

            return resultado;
            #endregion
        }

        private IEnumerable<string> FormateoCanal1ODD(List<string> datosOriginales, List<string> pDatosCompletos, ref bool pBanderaRecargoMora)
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
            bool banderaRecargoMora = pBanderaRecargoMora;
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
                        var lineas11CRecargoMora = from busqueda in pDatosCompletos
                                                   where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C") && !string.IsNullOrEmpty(busqueda.Substring(30, 14).Trim().TrimStart('0'))
                                                   select busqueda;

                        if (lineas11CRecargoMora.Any())
                        {
                            if (!banderaRecargoMora)
                            {
                                resultado.Add(GetRecargoMora(lineas11CRecargoMora.ToList(), periodo, cuentaConexion, ref banderaRecargoMora, "1"));
                                pBanderaRecargoMora = true;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region Validar para recargo de mora
                        var lineas11CRecargoMora = from busqueda in pDatosCompletos
                                                   where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("11C") && !string.IsNullOrEmpty(busqueda.Substring(30, 14).Trim().TrimStart('0'))
                                                   select busqueda;

                        if (lineas11CRecargoMora.Any())
                        {
                            if (!banderaRecargoMora)
                            {
                                resultado.Add(GetRecargoMora(lineas11CRecargoMora.ToList(), periodo, cuentaConexion, ref banderaRecargoMora, "1"));
                                pBanderaRecargoMora = true;
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

        private string GetRecargoMora(List<string> pLineas, string pPeriodo, string pNumeroConexion, ref bool pBanderaRecargoMora, string pTipo)
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

            switch (pTipo)
            {
                case "1":
                    resultado = $"1ODD|{Helpers.FormatearCampos(TiposFormateo.Fecha06, pPeriodo.Split('-').ElementAt(0).Trim())} a {Helpers.FormatearCampos(TiposFormateo.Fecha06, pPeriodo.Split('-').ElementAt(1).Trim())}|" +
                        $"Recargo de Mora ({Helpers.FormatearCampos(TiposFormateo.Fecha07, pPeriodo.Split('-').ElementAt(0).Trim())} - {Helpers.FormatearCampos(TiposFormateo.Fecha07, pPeriodo.Split('-').ElementAt(1).Trim())})|" +
                        $"{Helpers.SumarCampos(sumaValoresBase)}|{Helpers.SumarCampos(sumaValoresIva)}|{Helpers.SumarCampos(sumaValoresImpuestos)}|{Helpers.SumarCampos(sumaValoresTotal)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|{Helpers.FormatearCampos(TiposFormateo.Fecha05, PeriodoFacturacion)}|" +
                        $"{pNumeroConexion}| ";
                    break;

                case "2":
                    resultado = $"1CFI| |Recargo Mora|{Helpers.SumarCampos(sumaValoresBase)}|{Helpers.SumarCampos(sumaValoresIva)}|{Helpers.SumarCampos(sumaValoresTotal)}| | ";
                    break;
            }

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

                resultado.Add(Helpers.ValidarPipePipe($"1FFF|{concepto}|{Helpers.FormatearCampos(TiposFormateo.Decimal05, total)}" +
                    $"|{Helpers.FormatearCampos(TiposFormateo.Decimal05, subtotal)}|{Helpers.SumarCampos(new List<string> { total, subtotal })}| | "));

                resultado.Add("1FFF| | | | | | ");
            }

            return resultado;
            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal 1DAA y 1DBB
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoGrupo1DAA(List<string> datosOriginales)
        {
            #region MapeoGrupo1DAA

            List<string> listResultado = new List<string>();
            string LineaTemp = string.Empty;
            string numeroConexion = string.Empty;
            Dictionary<string, List<string>> dicAgruNumConexion = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> dicAgruPorCargo = new Dictionary<string, List<string>>();
            Dictionary<string, int> dicCountPorCargo = new Dictionary<string, int>();
            List<string> paqueteActual = new List<string>();
            string llaveCruve = string.Empty;

            string concepto1DBB = string.Empty;
            List<string> valor1_1DBB = new List<string>();
            List<string> valor2_1DBB = new List<string>();
            List<string> valor3_1DBB = new List<string>();
            List<string> valor4_1DBB = new List<string>();

            List<string> valor1_1DAA = new List<string>();
            List<string> valor2_1DAA = new List<string>();
            List<string> valor3_1DAA = new List<string>();
            List<string> valor4_1DAA = new List<string>();
            string valor02T004 = string.Empty;

            #region Busquea 02T004

            var lineas02T004 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02T004")
                               select busqueda;

            if (lineas02T004.Any())
            {
                valor02T004 = lineas02T004.FirstOrDefault().Substring(104, 14).Trim().TrimStart('0');
            }

            #endregion

            #region armar paquetes por 040000
            foreach (var lineaActual in datosOriginales)
            {
                if (lineaActual.Substring(0, 6) == "040000") // inicio Paquete
                {
                    llaveCruve = lineaActual.Substring(0, 25).Trim();
                }

                if (!string.IsNullOrEmpty(llaveCruve))
                {
                    // Se llena Diccionario a procesar

                    if (dicAgruNumConexion.ContainsKey(llaveCruve))
                    {
                        dicAgruNumConexion[llaveCruve].Add(lineaActual);
                    }
                    else
                    {
                        dicAgruNumConexion.Add(llaveCruve, new List<string> { lineaActual });
                    }
                }
            }
            #endregion

            #region Arma grupo por concepto 1DBB

            foreach (var dicAgruNumConexionActual in dicAgruNumConexion)
            {
                #region Busquedas               

                var lineas11C = from busqueda in dicAgruNumConexionActual.Value
                                where busqueda.Length > 6 && busqueda.Substring(0, 3).Equals("11C")
                                select busqueda;
                #endregion

                #region agrupo

                if (lineas11C.Any())
                {
                    foreach (var linea11CActual in lineas11C)
                    {

                        concepto1DBB = linea11CActual.Substring(281).Trim();

                        if (Convert.ToDouble(linea11CActual.Substring(30, 14)) != 0)
                        {
                            concepto1DBB = "Recargo de mora";
                        }

                        if (dicAgruPorCargo.ContainsKey(concepto1DBB))
                        {
                            dicAgruPorCargo[concepto1DBB].Add(linea11CActual);
                        }
                        else
                        {
                            dicAgruPorCargo.Add(concepto1DBB, new List<string> { linea11CActual });
                        }
                    }
                }

                #endregion

            }

            #endregion

            #region Formatear

            foreach (var dicAgruPorCargoActual in dicAgruPorCargo)
            {
                concepto1DBB = dicAgruPorCargoActual.Key;

                foreach (var lineaProcesar in dicAgruPorCargoActual.Value)
                {

                    if (concepto1DBB == "Recargo de mora")
                    {
                        valor1_1DBB.Add(lineaProcesar.Substring(30, 14));

                        valor2_1DBB.Add(lineaProcesar.Substring(30, 14));
                        valor2_1DBB.Add(lineaProcesar.Substring(44, 14));

                        valor3_1DBB.Add(lineaProcesar.Substring(44, 14));

                        valor4_1DBB.Add("0");
                    }
                    else
                    {

                        valor1_1DBB.Add(lineaProcesar.Substring(16, 14));

                        valor2_1DBB.Add(lineaProcesar.Substring(16, 14));
                        valor2_1DBB.Add(lineaProcesar.Substring(44, 14));
                        valor2_1DBB.Add(lineaProcesar.Substring(172, 14));

                        valor3_1DBB.Add(lineaProcesar.Substring(44, 14));

                        valor4_1DBB.Add(lineaProcesar.Substring(172, 14));

                    }
                }

                // Agregar Campos a Totalizar
                valor1_1DAA.AddRange(valor1_1DBB);
                valor2_1DAA.AddRange(valor2_1DBB);
                valor3_1DAA.AddRange(valor3_1DBB);
                valor4_1DAA.AddRange(valor4_1DBB);


                LineaTemp = "1DBB|";
                LineaTemp += concepto1DBB + "|";
                LineaTemp += Helpers.SumarCampos(valor1_1DBB, "G") + "|";
                LineaTemp += Helpers.SumarCampos(valor2_1DBB, "G") + "|";
                LineaTemp += " |";
                LineaTemp += Helpers.SumarCampos(valor3_1DBB, "G") + "|";
                LineaTemp += Helpers.SumarCampos(valor4_1DBB, "G") + "|";

                if (concepto1DBB == "Recargo de mora")
                {
                    LineaTemp += "0| ";
                }
                else
                {
                    LineaTemp += valor02T004 + "| ";
                }


                valor1_1DBB.Clear();
                valor2_1DBB.Clear();
                valor3_1DBB.Clear();
                valor4_1DBB.Clear();

                listResultado.Add(Helpers.ValidarPipePipe(LineaTemp));
            }

            #endregion

            // Se crea el canal 1DAA
            LineaTemp = "1DAA|";
            LineaTemp += "Total cargos|";
            LineaTemp += Helpers.SumarCampos(valor1_1DAA, "G") + "|";
            LineaTemp += Helpers.SumarCampos(valor2_1DAA, "G") + "|";
            LineaTemp += Helpers.SumarCampos(valor3_1DAA, "G") + "|";
            LineaTemp += Helpers.SumarCampos(valor4_1DAA, "G") + "| ";

            listResultado.Insert(0, Helpers.ValidarPipePipe(LineaTemp));

            return listResultado;
            #endregion
        }

        /// Metodo que obtiene las lineas formateadas de Canal 1JAA y 1JBB
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> MapeoGrupo1JAA(List<string> datosOriginales)
        {
            #region MapeoGrupo1JAA

            List<string> listResultado = new List<string>();
            string LineaTemp = string.Empty;
            string numeroConexion = string.Empty;
            Dictionary<string, List<string>> dicAgruNumConexion = new Dictionary<string, List<string>>();
            List<string> paqueteActual = new List<string>();
            string llaveCruve = string.Empty;

            List<string> valor1_1JBB = new List<string>();
            List<string> valor2_1JBB = new List<string>();

            List<string> valor1_1JAA = new List<string>();
            List<string> valor2_1JAA = new List<string>();


            #region armar paquetes
            foreach (var lineaActual in datosOriginales)
            {
                if (lineaActual.Substring(0, 6) == "040000") // inicio Paquete
                {
                    llaveCruve = lineaActual.Substring(0, 25).Trim();
                }

                if (!string.IsNullOrEmpty(llaveCruve))
                {
                    // Se llena Diccionario a procesar

                    if (dicAgruNumConexion.ContainsKey(llaveCruve))
                    {
                        dicAgruNumConexion[llaveCruve].Add(lineaActual);
                    }
                    else
                    {
                        dicAgruNumConexion.Add(llaveCruve, new List<string> { lineaActual });
                    }
                }
            }
            #endregion

            #region Procesamiento

            foreach (var dicAgruNumConexionActual in dicAgruNumConexion)
            {
                #region Busquedas
                var lineas040000 = from busqueda in dicAgruNumConexionActual.Value
                                   where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("040000")
                                   select busqueda;

                var lineas11C = from busqueda in dicAgruNumConexionActual.Value
                                where busqueda.Length > 6 && busqueda.Substring(0, 3).Equals("11C")
                                select busqueda;
                #endregion

                #region Mapeo

                if (lineas040000.Any())
                {
                    numeroConexion = lineas040000.FirstOrDefault().Substring(6, 20).Trim();

                    // Se toman los valores
                    if (lineas11C.Any())
                    {
                        foreach (var linea11CActual in lineas11C)
                        {
                            if (numeroConexion == Cuenta)
                            {
                                valor1_1JBB.Add(linea11CActual.Substring(30, 14));

                                valor2_1JBB.Add(linea11CActual.Substring(30, 14));
                                valor2_1JBB.Add(linea11CActual.Substring(44, 14));
                            }
                            else
                            {
                                valor1_1JBB.Add(linea11CActual.Substring(16, 14));
                                valor1_1JBB.Add(linea11CActual.Substring(30, 14));

                                valor2_1JBB.Add(linea11CActual.Substring(16, 14));
                                valor2_1JBB.Add(linea11CActual.Substring(30, 14));
                                valor2_1JBB.Add(linea11CActual.Substring(44, 14));
                                valor2_1JBB.Add(linea11CActual.Substring(172, 14));
                            }

                        }

                        // Agregar Campos a Totalizar
                        valor1_1JAA.AddRange(valor1_1JBB);
                        valor2_1JAA.AddRange(valor2_1JBB);
                    }

                    LineaTemp = "1JBB|";
                    LineaTemp += numeroConexion + "|";
                    LineaTemp += Helpers.SumarCampos(valor1_1JBB, "G") + "|";
                    LineaTemp += Helpers.SumarCampos(valor2_1JBB, "G") + "| ";

                    valor1_1JBB.Clear();
                    valor2_1JBB.Clear();

                    listResultado.Add(Helpers.ValidarPipePipe(LineaTemp));

                }

                #endregion
            }

            // Se crea el canal 1JAA
            LineaTemp = "1JAA|";
            LineaTemp += "Total por linea|";
            LineaTemp += Helpers.SumarCampos(valor1_1JAA, "G") + "|";
            LineaTemp += Helpers.SumarCampos(valor2_1JAA, "G") + "| ";

            listResultado.Insert(0, Helpers.ValidarPipePipe(LineaTemp));

            #endregion


            return listResultado;
            #endregion
        }
        #endregion
    }
}
