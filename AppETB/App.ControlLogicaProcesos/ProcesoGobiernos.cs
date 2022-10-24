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
        #region Variables del proceso
        private bool IsGobierno { get; set; }
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

            resultadoFormateoLinea = FormateoCanal1FFF(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
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

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            //resultadoFormateoLinea = FormateoGrupoTPNC(datosOriginales);

            //if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            //{
            //    resultado.AddRange(resultadoFormateoLinea);
            //}

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

            resultadoFormateoLinea = FormateoCanal1MMM(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanalCUFE(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
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
            IsGobierno = false;
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
            #region FormateoCanal1AAA
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
                ListaCanal1AAA.Add(string.IsNullOrEmpty(Linea010000.Substring(218, 20).Trim()) ? " " : Linea010000.Substring(218, 20).Trim());

                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECP{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);
                ListaCanal1AAA.Add(Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"FECL{Helpers.FormatearCampos(TiposFormateo.Fecha02, Linea010000.Substring(168, 8).Trim())}{Linea010000.Substring(151, 3).Trim().TrimStart('0')}").FirstOrDefault()?.Substring(12).Trim() ?? string.Empty);
                ListaCanal1AAA.Add(GetFechaExpedicion(Linea010000));

                ListaCanal1AAA.Add(GetNumeroReferencia(Linea010000.Substring(139, 12)));
                ListaCanal1AAA.AddRange(GetCodigosBarras(Linea010000.Substring(139, 12), Linea010000, datosOriginales)); //TODO: Verificar valor a pagar
                ListaCanal1AAA.Add(GetTipoEtapas(Linea010000.Substring(151, 3))); 
                ListaCanal1AAA.Add(GetTasaInteres(Linea040000));
                listaCortes.Clear();
                listaCortes.Add(new PosCortes(113, 4)); // Zona Postal
                ListaCanal1AAA.Add(Helpers.ExtraccionCamposSpool(listaCortes, Linea010000));
                ListaCanal1AAA.Add(string.Empty); // TODO: Anexos Publicitarios - Verificar regla
                ListaCanal1AAA.Add(GetActividad(Linea040000));
                ListaCanal1AAA.Add(GetEstrato(Linea040000));
                ListaCanal1AAA.AddRange(GetBarrioLocalidad());
                ListaCanal1AAA.Add(string.Empty);
                ListaCanal1AAA.Add(string.Empty);
                ListaCanal1AAA.Add(GetMarcaAnexosPublicidad());
                ListaCanal1AAA.AddRange(GetEmailTipoEmal());
                ListaCanal1AAA.AddRange(GetMarcaCupones());
                ListaCanal1AAA.Add("*"); //10122016 Nubia dice que estas marcas deben ser * para gobierno, para no imprimir los detalles
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
                ListaCanal1AAA.Add(string.Empty); /// Fecha Corte
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
                            SubTotal1BBB += calculoAjusteDecena;
                        }
                        else
                        {
                            decimal impuestoConsumo = 0;
                            if (IsGobierno)
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
                case 18:
                case 19:
                case 37:
                case 64:
                case 67:
                case 69:
                    tipociclo = "GOB";
                    IsGobierno = true;
                    break;

                default:
                    throw new Exception($"Error: El ciclo:{ciclo}, no corresponde al proceso de Gobierno.");


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
            string idActividad = pLinea040000.Substring(124, 2);
            string tasaInteresTablaSustitucion = string.Empty;

            tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM{idActividad}").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
            tasaInteresTablaSustitucion.Replace("TASAS DE MORA POR ACTIVIDAD", string.Empty);

            if (!string.IsNullOrEmpty(tasaInteresTablaSustitucion))
            {
                return tasaInteresTablaSustitucion;
            }
            else
            {
                tasaInteresTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM08").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
                tasaInteresTablaSustitucion += $" {Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"TASM10").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty}";

                return tasaInteresTablaSustitucion;

            }
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
            string idActividad = pLinea040000.Substring(124, 2);

            string actividadTablaSustitucion = string.Empty;

            actividadTablaSustitucion = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, $"ACTR{idActividad}").FirstOrDefault()?.Substring(6).Trim() ?? string.Empty;
            actividadTablaSustitucion = actividadTablaSustitucion.Replace("ACTIVIDAD REVCHAIN", string.Empty);

            if (!string.IsNullOrEmpty(actividadTablaSustitucion) && (IsGobierno))
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
            #region FormateoCanal1BBB
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
                            lineaAjusteDecena = $"1BBB|{descripcion}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, detalle.Substring(6, 14).TrimStart('0'))}| ";
                            SubTotal1BBB += calculoAjusteDecena;
                        }
                        else
                        {
                            long impuestoConsumo = Convert.ToInt64(detalle.Substring(118, 14));

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
            bool validaroficinaPqr = false;

            var result010000 = from busqueda in datosOriginales
                         where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("010000")
                         select busqueda;
            if (result010000.Any())
            {
                string zonaPostal = result010000.FirstOrDefault().Substring(113, 4) ?? String.Empty;

                if (!string.IsNullOrEmpty(zonaPostal.Trim()))
                {
                    oficinaPqr = GetOficinaPQR($"CZONA{zonaPostal}");

                    if (!string.IsNullOrEmpty(oficinaPqr))
                    {
                        oficinaPqr = oficinaPqr.Replace("|", " ");
                        Linea1KKK += Linea1KKK.Trim() == "" ? $"1KKK|{oficinaPqr} " : $"{oficinaPqr} ";
                        validaroficinaPqr = true;
                    }
                }
            }
            
            if (!validaroficinaPqr) // en caso no encontrar la oficina pqr se enlista y trae todas las oficinas de los supercades y puntos de pago importantes
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
                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, linea29000.FirstOrDefault().Substring(29, 20).Trim())}|{linea29000.FirstOrDefault().Substring(49).Trim()}| ");
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
        private string FormateoCanalNTC4(List<string> datosOriginales)
        {
            #region FormateoCanalNTC4
            string resultado = string.Empty;

            var linea30002 = from busqueda in datosOriginales
                             where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("30002")
                             select busqueda;

            if (linea30002.Any())
            {
                resultado = Helpers.ValidarPipePipe($"NTC4|{linea30002.FirstOrDefault().Substring(5, 10).Trim()}|{linea30002.FirstOrDefault().Substring(15, 70).Trim()}|" +
                    $"{linea30002.FirstOrDefault().Substring(85, 20).Trim()}| ");
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

            var result02S000 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("02S000")
                               select busqueda;

            if (result02S000.Any())
            {
                resultado = "1CTF|TOTAL FACTURADO|" + Helpers.FormatearCampos(TiposFormateo.Decimal01, result02S000.FirstOrDefault().Substring(48, 14)) + "| ";
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
                                planMinutos += $" - Valor Minuto Adicional {Helpers.FormatearCampos(TiposFormateo.Decimal01, valorminuto.Substring(0, 10))}";
                            }
                        }

                        #endregion

                        string valor090000 = string.Empty;

                        if (linea090000.Any())
                        {
                            valor090000 = linea090000.FirstOrDefault().Substring(26, 27).Trim();
                        }

                        resultado.Add(Helpers.ValidarPipePipe($"ADN1|{lineaDetalle.Key}|{GetTipo(lineaDetalle.Key)}|{linea040000.FirstOrDefault().Substring(76, 30).Trim()}|" +
                            $"{planMinutos}|{valor090000}| | "));

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
                            if (lineaNegocioInfo.Key == "5" || lineaNegocioInfo.Key == "0")
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
                            else if (lineaNegocioInfo.Key == "7")
                            {
                                canalTotales = "06T710";
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

                                if (lineaNegocioInfo.Key == "4")
                                {
                                    canalTotales = "06T410";

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
                                $"{Helpers.SumarCampos(new List<string> { @base, recargoMora })}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, iva)}|" +
                                $"{Helpers.SumarCampos(new List<string> { @base, iva, recargoMora })}| "));
                            #endregion

                            #region Armar Canal 1DDD

                            IEnumerable<IGrouping<string, string>> lineadetalles = null;

                            if (lineaNegocioInfo.Key == "1" || lineaNegocioInfo.Key == "3" || lineaNegocioInfo.Key == "8" || lineaNegocioInfo.Key == "7")
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
                                $"{Helpers.SumarCampos(sumaValoresBase)}|{Helpers.SumarCampos(sumaValoresIva)}|" +
                                $"{Helpers.SumarCampos(sumaValoresTotal)}|{Helpers.SumarCampos(sumaValoresSubsidio)}| | "));
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
                                $"|{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|{Helpers.SumarCampos(new List<string> { iva, restaImpuesto })}|" +
                                $"{Helpers.SumarCampos(new List<string> { @base, iva, restaImpuesto, impuesto })}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, impuesto)}| "));
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
                                    $"{linea.Substring(33, 10).Trim()}|{linea.Substring(96, 11).Trim()}|{linea.Substring(66, 2).TrimStart('0')}:00|{Helpers.FormatearCampos(TiposFormateo.Decimal01, linea.Substring(47, 9)).Replace("$", "").Trim()}|" +
                                    $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, linea.Substring(56, 9)).Replace("$", "").Trim()}| "));
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

            if (lineas13M.Any())
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
                                        where busqueda.Length > 3 && busqueda.Substring(0, 3).Equals("13M")
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
                            $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, @base)}|" +
                            $"{Helpers.FormatearCampos(TiposFormateo.Decimal01, iva)}| "));

                            sumaValoresBase.Add(lineas13M.FirstOrDefault().Substring(42, 14).Trim().TrimStart('0'));
                            sumaValoresIva.Add(lineas13M.FirstOrDefault().Substring(56, 14).Trim().TrimStart('0'));
                            #endregion
                        }

                        #region Armar Canal 1GGA - 1GGB
                        sumaValoresTotal.AddRange(sumaValoresBase);
                        sumaValoresTotal.AddRange(sumaValoresIva);

                        resultado.Add(Helpers.ValidarPipePipe($"1GGA|Total {descripcionSubProducto}|{Helpers.SumarCampos(sumaValoresBase)}|" +
                            $"{Helpers.SumarCampos(sumaValoresIva)}|{Helpers.SumarCampos(sumaValoresTotal)}| "));

                        resultado.Add(Helpers.ValidarPipePipe($"1GGB|Total {descripcionProducto}|{Helpers.SumarCampos(sumaValoresBase1GGB)}|" +
                            $"{Helpers.SumarCampos(sumaValoresIva1GGB)}|{Helpers.SumarCampos(sumaValoresTotal1GGB)}| "));
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
            string Lineas1LLL = string.Empty;

            if (MesMora > 1 && MesMora < 4)
            {
                Lineas1LLL = $"1LLL|{Variables.RxGeneral.TextoMora_1}| ";
            }
            else if (MesMora >= 4)
            {
                Lineas1LLL = $"1LLL|{Variables.RxGeneral.TextoMora_2}| ";
            }
            else if (MesMora == 2)
            {
                Lineas1LLL = $"1LLL|{Variables.RxGeneral.TextoMora_3}| ";
            }
            else if (MesMora == 3)
            {
                Lineas1LLL = $"1LLL|{Variables.RxGeneral.TextoMora_4}| ";
            }
            else if (MesMora >= 4)
            {
                Lineas1LLL = $"1LLL|{Variables.RxGeneral.TextoMora_5}| ";
            }

            return Lineas1LLL;
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
        private List<string> FormateoCanal1FFF(List<string> datosOriginales)
        {
            #region FormateoCanal1FFF
            List<string> resultado = new List<string>();

            var linea113M311 = from busqueda in datosOriginales
                              where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("13M311")
                              select busqueda;

            if (linea113M311.Any())
            {
                string llaveCruce = $"CODF{linea113M311.FirstOrDefault().Substring(32, 10).Trim()}";
                var concepto = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoTablaSustitucion, llaveCruce).FirstOrDefault().Substring(14).Trim();

                string total = linea113M311.FirstOrDefault().Substring(42, 14).Trim();
                string subtotal = linea113M311.FirstOrDefault().Substring(56, 14).Trim();
                string perido = $"{linea113M311.FirstOrDefault().Substring(6, 8).Trim()} {linea113M311.FirstOrDefault().Substring(14, 8).Trim()}";

                if (perido.Trim() != null)
                {
                    resultado.Add(Helpers.ValidarPipePipe($"1FFF|{concepto}|{Helpers.FormatearCampos(TiposFormateo.Decimal01, total)}" +
                        $"|{Helpers.FormatearCampos(TiposFormateo.Decimal01, subtotal)}|{Helpers.SumarCampos(new List<string> { total, subtotal })}|{perido}| "));

                    resultado.Add("1FFF| | | | | | ");
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
        private string FormateoCanal1FFA(List<string> datosOriginales)
        {
            #region FormateoCanal1FFA
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoGrupoTPNC(List<string> datosOriginales)
        {
            #region FormateoCanalTPNC
            List<string> resultado = new List<string>();
            string lineaFormateada = string.Empty;

            List<PosCortes> listaCortes = new List<PosCortes>();            
            List<string> sumaCampo1 = new List<string>();
            List<string> sumaCampo2 = new List<string>();
            List<string> sumaCampo3 = new List<string>();
            List<string> sumaCampo4 = new List<string>();
            List<string> sumaCampo5 = new List<string>();
            List<string> sumaCampo6 = new List<string>();



            var result888888 = from busqueda in datosOriginales
                               where busqueda.Length > 6 && busqueda.Substring(0, 6).Equals("888888")
                               select busqueda;

            if(result888888.Any())
            {
                #region Formatear TPCN
                lineaFormateada = "TPNC|" + result888888.Count().ToString() + "| ";
                resultado.Add(Helpers.ValidarPipePipe(lineaFormateada));
                #endregion

                #region Formatear 1JJJ

                // Cortes Fijos
                listaCortes.Add(new PosCortes(6, 20));
                listaCortes.Add(new PosCortes(40, 14, TiposFormateo.Decimal04));
                listaCortes.Add(new PosCortes(54, 14, TiposFormateo.Decimal04));
                listaCortes.Add(new PosCortes(68, 14, TiposFormateo.Decimal04));
                listaCortes.Add(new PosCortes(82, 14, TiposFormateo.Decimal04));
                listaCortes.Add(new PosCortes(96, 14, TiposFormateo.Decimal04));
                listaCortes.Add(new PosCortes(110, 14, TiposFormateo.Decimal04));

                foreach (var lineaActual in result888888)
                {                    
                    sumaCampo1.Add(lineaActual.Substring(40,14));
                    sumaCampo2.Add(lineaActual.Substring(54, 14));
                    sumaCampo3.Add(lineaActual.Substring(68, 14));
                    sumaCampo4.Add(lineaActual.Substring(83, 14));
                    sumaCampo5.Add(lineaActual.Substring(96, 14));
                    sumaCampo6.Add(lineaActual.Substring(110, 14));

                    lineaFormateada = "1JJJ|";
                    lineaFormateada += Helpers.ExtraccionCamposSpool(listaCortes,lineaActual) + "|| ";
                    resultado.Add(Helpers.ValidarPipePipe(lineaFormateada));
                }

                if(result888888.Count() % 2 != 0)
                {
                    lineaFormateada = "1JJJ|||||||| ";
                    resultado.Add(Helpers.ValidarPipePipe(lineaFormateada));
                }

                #endregion

                #region Formatear 1JJA

                lineaFormateada = "1JJA|Total|";
                lineaFormateada += Helpers.SumarCampos(sumaCampo1) + "|";
                lineaFormateada += Helpers.SumarCampos(sumaCampo2) + "|";
                lineaFormateada += Helpers.SumarCampos(sumaCampo3) + "|";
                lineaFormateada += Helpers.SumarCampos(sumaCampo4) + "|";
                lineaFormateada += Helpers.SumarCampos(sumaCampo5) + "|";
                lineaFormateada += Helpers.SumarCampos(sumaCampo6) + "| ";
                resultado.Add(Helpers.ValidarPipePipe(lineaFormateada));

                #endregion
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
        private string FormateoCanalCUFE(List<string> datosOriginales)
        {
            #region FormateoCanalCUFE
            string resultado = string.Empty;
            string valor = string.Empty;

            resultado = "CUFE|";

            if (Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey(Cuenta))
            {
                valor = Variables.Variables.DatosInsumoETBFacturaElectronica[Cuenta];
            }

            resultado += valor + "| ";

            return Helpers.ValidarPipePipe(resultado);
            #endregion
        }

        #endregion
    }
}
