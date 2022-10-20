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
    public class ProcesoLlanos : IProcess
    {
        #region Variables del proceso
        private string Cuenta { get; set; }
        private string Telefono { get; set; }
        private string NombreArchivo { get; set; }
        #endregion

        public ProcesoLlanos(string pArchivo)
        {
            #region ProcesoLlanos
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
        public ProcesoLlanos()
        { }

        public void CargueFormateoArchivo(string pArchivo)
        {
            #region CargueFormateoArchivo
            List<string> DatosArchivo = File.ReadAllLines(pArchivo, Encoding.Default).ToList();
            List<string> datosExtractoFormateo = new List<string>();
            List<string> datosLectura = new List<string>();
            string llaveCruce = string.Empty;
            bool extractoCompleto = false;
            NombreArchivo = Path.GetFileName(pArchivo);

            foreach (var linea in DatosArchivo)
            {
                if (!string.IsNullOrEmpty(linea.Trim()))
                {
                    datosLectura.AddRange(linea.Trim().Split(''));
                }
            }

            foreach (var linea in datosLectura)
            {
                if (string.IsNullOrEmpty(linea.Trim()))
                    continue;

                if (linea.Trim() == "&l0030Z") //Inicio Extracto
                {
                    extractoCompleto = false;

                    if (datosExtractoFormateo.Count > 1)
                    {
                        extractoCompleto = true;
                    }

                    if (extractoCompleto)
                    {
                        llaveCruce = datosExtractoFormateo.Find(x => x.Contains("*p30x170Y")).Substring(17).Trim();

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

                    datosExtractoFormateo.Add(linea.Trim());
                }
                else
                {
                    datosExtractoFormateo.Add(linea.Trim());
                }
            }

            //Ultimo Extracto
            if (datosExtractoFormateo.Count > 1)
            {
                llaveCruce = datosExtractoFormateo.Find(x => x.Contains("*p30x170Y")).Substring(17).Trim();

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
            CargarCiudades();
            CargarActividades();
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

            resultadoFormateoLinea = FormateoCanal1BBA(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1BBB(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1CCC(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1DDD(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1EEE(datosOriginales);

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

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1FFB(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1HHH(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormarPaqueteCPC(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1MEN(datosOriginales);

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1LMP(datosOriginales);

            if (!string.IsNullOrEmpty(resultadoFormateoLinea))
            {
                resultado.Add(resultadoFormateoLinea);
            }

            resultadoFormateoLinea = FormateoCanal1LSP(datosOriginales);

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
            List<string> ListaCanal1AAA = new List<string>();
            List<CamposLLanos> camposllanos = new List<CamposLLanos>();


            ListaCanal1AAA.Add("1AAA");

            camposllanos.Add(new CamposLLanos(11, "*p2000x190Y"));                                                  // numero_factura
            Cuenta = GetCuentaSinLetras(Helpers.GetCampoLLanos(datosOriginales, 11, "*p2000x190Y"));
            camposllanos.Add(new CamposLLanos(8, "*p30x80Y"));                                                      // nombre
            camposllanos.Add(new CamposLLanos(9, "*p30x110Y", TiposFormateo.Cadena01));                             // direccion1
            camposllanos.Add(new CamposLLanos(9, "*p30x140Y", TiposFormateo.Cadena01));                             // direccion2
            camposllanos.Add(new CamposLLanos(9, "*p30x170Y"));                                                     // cedula
            camposllanos.Add(new CamposLLanos(10, "*p530x360Y"));                                                   // telefono
            Telefono = Helpers.GetCampoLLanos(datosOriginales, 10, "*p530x360Y").Trim();
            camposllanos.Add(new CamposLLanos(10, "*p650x580Y"));                                                   // numero antencion cliente
            camposllanos.Add(new CamposLLanos(11, "*p2000x330Y"));                                                  // total_pago
            camposllanos.Add(new CamposLLanos(11, "*p1890x440Y"));                                                  // periodo_facturado
            camposllanos.Add(new CamposLLanos(11, "*p2000x580Y"));                                                  // plazo_pago
            ListaCanal1AAA.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));

            ListaCanal1AAA.Add(GetZonaPostal(Helpers.GetCampoLLanos(datosOriginales, 10, "*p230x250Y"), true));     // zona postal

            camposllanos.Clear();
            camposllanos.Add(new CamposLLanos(10, "*p230x300Y"));                                                   // plan facturacion
            ListaCanal1AAA.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));

            ListaCanal1AAA.Add(GetActividad(Helpers.GetCampoLLanos(datosOriginales, 10, "*p230x330Y")));            // actividad  

            camposllanos.Clear();
            camposllanos.Add(new CamposLLanos(10, "*p230x360Y"));                                                   // estrato
            camposllanos.Add(new CamposLLanos(10, "*p230x400Y"));                                                   // mes deuda
            camposllanos.Add(new CamposLLanos(10, "*p230x430Y"));                                                   // fecha exped
            camposllanos.Add(new CamposLLanos(10, "*p230x465Y"));                                                   // interes mora
            camposllanos.Add(new CamposLLanos(10, "*p230x495Y"));                                                   // fecha servicio
            camposllanos.Add(new CamposLLanos(10, "*p230x530Y"));                                                   // ultimo_pago
            camposllanos.Add(new CamposLLanos(10, "*p230x560Y"));                                                   // fact_ultimo_pago
            camposllanos.Add(new CamposLLanos(10, "*p230x595Y"));                                                   // fecha_ultimo_pago
            camposllanos.Add(new CamposLLanos(12, "*p1150x2890Y"));                                                 // cupon
            ListaCanal1AAA.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));

            ListaCanal1AAA.Add("Cons");                                                                             // consecutivo                                         
            ListaCanal1AAA.Add(Helpers.GetCampoLLanos(datosOriginales, 11, "*p848x3070Y"));                         // codigo_barras

            camposllanos.Clear();
            ListaCanal1AAA.AddRange(GetCiudadDepto(Helpers.GetCampoLLanos(datosOriginales, 10, "*p230x250Y")));     // CIudad / Departamento
            ListaCanal1AAA.Add("KitXXXXX");                                                                         // Kit
            ListaCanal1AAA.Add(GetZonaPostal(Helpers.GetCampoLLanos(datosOriginales, 10, "*p230x250Y"), false));    // Zona Postal
            ListaCanal1AAA.Add(NombreArchivo);                                                                      // Nombre Archivo
            ListaCanal1AAA.AddRange(GetEmail());                                                                    // EMaill, Dual, Prevalidador
            ListaCanal1AAA.Add("****tipo_salida****");                                                              // Tipo Salida

            resultado = Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1AAA, '|'));
            return resultado;
            #endregion
        }

        private string GetCuentaSinLetras(string pCampo)
        {
            string result = pCampo;
            if (!string.IsNullOrEmpty(pCampo))
            {
                result = string.Concat(pCampo.Where(c => Char.IsDigit(c)));

            }

            return result.Trim();
        }

        private string GetZonaPostal(string pZonaPostal, bool separado)
        {
            #region GetZonaPostal
            string resultado = string.Empty;
            if (!string.IsNullOrEmpty(pZonaPostal))
            {
                string[] campos = pZonaPostal.Split('-');

                if (campos.Length > 1)
                {
                    if (separado)
                    {
                        resultado = campos[1].Trim();
                    }
                    else
                    {
                        resultado = $"{campos[0].Trim()}-{campos[1].Trim()}";
                    }
                }

            }

            return resultado; 
            #endregion
        }

        private List<string> GetCiudadDepto(string pZonaPostal)
        {
            #region GetCiudadDepto
            List<string> lisCiudadDepto = new List<string>();
            string resultado = string.Empty;
            if (!string.IsNullOrEmpty(pZonaPostal))
            {
                string[] campos = pZonaPostal.Split('-');

                if (campos.Length > 1)
                {
                    resultado = campos[0].Trim();

                    if (Variables.Variables.DicCiudadesLlanos.ContainsKey(resultado))
                    {
                        lisCiudadDepto.Add(Variables.Variables.DicCiudadesLlanos[resultado].Ciudad);
                        lisCiudadDepto.Add(Variables.Variables.DicCiudadesLlanos[resultado].Departamento);
                    }
                    else
                    {
                        lisCiudadDepto.Add(" ");
                        lisCiudadDepto.Add(" ");
                    }
                }

            }

            return lisCiudadDepto; 
            #endregion
        }

        private string GetActividad(string pActividad)
        {
            #region GetActividad
            string resultado = string.Empty;
            if (!string.IsNullOrEmpty(pActividad))
            {
                if (Variables.Variables.DicActividadesLlanos.ContainsKey(pActividad.Trim()))
                {
                    resultado = $"{pActividad.Trim()} {Variables.Variables.DicActividadesLlanos[pActividad.Trim()]}";
                }
                else
                {
                    resultado = pActividad.Trim();
                }

            }

            return resultado; 
            #endregion
        }

        private List<string> GetEmail()
        {
            #region GetActividad
            List<string> resultado = new List<string>();
            bool IsDual = false;
            string insumoDual = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoDualLlanos, $"{Telefono}");
            string insumoEmail = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoDistribucionEmailLlanos, $"{Telefono}");

            string tipoEmail = string.Empty;
            string email = string.Empty;
            string prevalidador = string.Empty;


            if (!string.IsNullOrEmpty(insumoDual))
            {
                IsDual = true;
                tipoEmail = "dual";
            }

            if (IsDual)
            {
                if (!string.IsNullOrEmpty(email))
                {
                    string[] campos = email.Split('|');
                    if (campos.Length > 1)
                    {
                        email = $"{campos[1]}";
                    }
                }
            }

            resultado.Add(email);
            resultado.Add(tipoEmail);
            resultado.Add(prevalidador);


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
            List<string> ListaCanal1BBA = new List<string>();
            List<CamposLLanos> camposllanos = new List<CamposLLanos>();

            ListaCanal1BBA.Add("1BBA");
            camposllanos.Add(new CamposLLanos(11, "*p1250x210Y")); // unidad_consumo
            camposllanos.Add(new CamposLLanos(11, "*p1150x250Y", "0")); // lectura_inicial
            camposllanos.Add(new CamposLLanos(11, "*p1150x290Y", "0")); // lectura
            camposllanos.Add(new CamposLLanos(11, "*p1150x325Y", "0")); // consumo_mes
            camposllanos.Add(new CamposLLanos(11, "*p1150x365Y", "0")); // consumo_int
            camposllanos.Add(new CamposLLanos(11, "*p1130x400Y", "0.00")); // vlr_min_voz
            ListaCanal1BBA.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));

            string mesConsumo = Helpers.GetCampoLLanos(datosOriginales, 11, "*p1100x290Y");
            ListaCanal1BBA.Add(GetVlrMinInt(Helpers.GetCampoLLanos(datosOriginales,11, "*p1130x435Y", "0.00"),mesConsumo));  // vlr_min_int
            
            camposllanos.Clear();
            //camposllanos.Add(new CamposLLanos(11, "*p1130x475Y", "0.00")); // vlr_min_adic
            //camposllanos.Add(new CamposLLanos(11, "*p1250x550Y")); // ciclo
            camposllanos.Add(new CamposLLanos(10, "*p920x170Y")); // zona_sector_manzana
            //camposllanos.Add(new CamposLLanos(10, "*p1100x290Y")); // mes_consumo
            ListaCanal1BBA.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));

            resultado = Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1BBA, '|'));
            return resultado;
            #endregion
        }

        private string GetVlrMinInt(string pVlrMinInt, string pMesConsumo)
        {
            #region GetVlrMinInt
            string resultado = string.Empty;
            if (!string.IsNullOrEmpty(pMesConsumo))
            {
                resultado = $"{pVlrMinInt}|0.00|0";
            }
            else
            {
                resultado = pVlrMinInt;
            }

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
            List<string> ListaCanal1BBB = new List<string>();
            List<CamposLLanos> camposllanos = new List<CamposLLanos>();

            ListaCanal1BBB.Add("1BBB");
            camposllanos.Add(new CamposLLanos(11, "*p1400x290Y", "0")); // ultimo_consumo_mes_1
            camposllanos.Add(new CamposLLanos(11, "*p1550x290Y", "0")); // ultimo_consumo_voz_1
            camposllanos.Add(new CamposLLanos(11, "*p1690x290Y", "0")); // ultimo_consumo_int_1
            ListaCanal1BBB.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));
            resultado.Add(Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1BBB, '|')));

            camposllanos.Clear();
            ListaCanal1BBB.Clear();
            ListaCanal1BBB.Add("1BBB");
            camposllanos.Add(new CamposLLanos(11, "*p1400x350Y", "0")); // ultimo_consumo_mes_2
            camposllanos.Add(new CamposLLanos(11, "*p1550x350Y", "0")); // ultimo_consumo_voz_2
            camposllanos.Add(new CamposLLanos(11, "*p1690x350Y", "0")); // ultimo_consumo_int_2
            ListaCanal1BBB.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));
            resultado.Add(Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1BBB, '|')));

            camposllanos.Clear();
            ListaCanal1BBB.Clear();
            ListaCanal1BBB.Add("1BBB");
            camposllanos.Add(new CamposLLanos(11, "*p1400x410Y", "0")); // ultimo_consumo_mes_3
            camposllanos.Add(new CamposLLanos(11, "*p1550x410Y", "0")); // ultimo_consumo_voz_3
            camposllanos.Add(new CamposLLanos(11, "*p1690x410Y", "0")); // ultimo_consumo_int_3
            ListaCanal1BBB.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));
            resultado.Add(Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1BBB, '|')));

            camposllanos.Clear();
            ListaCanal1BBB.Clear();
            ListaCanal1BBB.Add("1BBB");
            camposllanos.Add(new CamposLLanos(11, "*p1400x470Y", "0")); // ultimo_consumo_mes_4
            camposllanos.Add(new CamposLLanos(11, "*p1550x470Y", "0")); // ultimo_consumo_voz_4
            camposllanos.Add(new CamposLLanos(11, "*p1690x470Y", "0")); // ultimo_consumo_int_4
            ListaCanal1BBB.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));
            resultado.Add(Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1BBB, '|')));

            camposllanos.Clear();
            ListaCanal1BBB.Clear();
            ListaCanal1BBB.Add("1BBB");
            camposllanos.Add(new CamposLLanos(11, "*p1400x530Y", "0")); // ultimo_consumo_mes_5
            camposllanos.Add(new CamposLLanos(11, "*p1550x530Y", "0")); // ultimo_consumo_voz_5
            camposllanos.Add(new CamposLLanos(11, "*p1690x530Y", "0")); // ultimo_consumo_int_5
            ListaCanal1BBB.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));
            resultado.Add(Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1BBB, '|')));

            camposllanos.Clear();
            ListaCanal1BBB.Clear();
            ListaCanal1BBB.Add("1BBB");
            camposllanos.Add(new CamposLLanos(11, "*p1400x590Y", "0")); // ultimo_promedio
            camposllanos.Add(new CamposLLanos(11, "*p1550x590Y", "0")); // ultimo_promedio_voz
            camposllanos.Add(new CamposLLanos(11, "*p1690x590Y", "0")); // ultimo_promedio_int
            ListaCanal1BBB.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));
            resultado.Add(Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1BBB, '|')));

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1CCC(List<string> datosOriginales)
        {
            #region FormateoCanal1CCC
            List<string> resultado = new List<string>();

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1DDD(List<string> datosOriginales)
        {
            #region FormateoCanal1DDD
            string resultado = string.Empty;

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1EEE(List<string> datosOriginales)
        {
            #region FormateoCanal1EEE
            List<string> resultado = new List<string>();

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1FFF(List<string> datosOriginales)
        {
            #region FormateoCanal1FFF
            List<string> resultado = new List<string>();

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1FFA(List<string> datosOriginales)
        {
            #region FormateoCanal1FFA
            List<string> resultado = new List<string>();

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1FFB(List<string> datosOriginales)
        {
            #region FormateoCanal1FFB
            List<string> resultado = new List<string>();

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1HHH(List<string> datosOriginales)
        {
            #region FormateoCanal1HHH
            string resultado = string.Empty;

            string actividad = Helpers.GetCampoLLanos(datosOriginales, 10, "*p230x330Y").Trim(); // actividad

            if (/*stristr($extracto["1CCC"], "XDSL") &&*/ actividad.Trim() == "3")
            {
                resultado = $"1HHH|logo internet|true";
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormarPaqueteCPC(List<string> datosOriginales)
        {
            #region FormarPaqueteCPC
            List<string> resultado = new List<string>();

            string titulo = Helpers.GetCampoLLanos(datosOriginales,11, "*p250x2500Y"); // TituloClausulaPermanencia

            if (!string.IsNullOrEmpty(titulo))
            {
                resultado.Add($"1CPC|{titulo}");
                
                List<string> linea1CPC;
                Dictionary<string, List<string>> diccionario1CPC = new Dictionary<string, List<string>>();

                linea1CPC = Helpers.GetListaCampoLLanos(datosOriginales, 5, "*p35x"); // Servicio
                LlenarDiccionario1CPC(diccionario1CPC,linea1CPC);

                linea1CPC = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p155x"); // FechaInicio
                LlenarDiccionario1CPC(diccionario1CPC, linea1CPC);

                linea1CPC = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p335x"); // FechaFin
                LlenarDiccionario1CPC(diccionario1CPC, linea1CPC);

                linea1CPC = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p475x"); // CargoConexión
                LlenarDiccionario1CPC(diccionario1CPC, linea1CPC);

                linea1CPC = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p665x"); // DescuentoNoRetiro
                LlenarDiccionario1CPC(diccionario1CPC, linea1CPC);

                linea1CPC = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p925x"); // ValorPagarRetiro
                LlenarDiccionario1CPC(diccionario1CPC, linea1CPC);

                foreach (string item in diccionario1CPC.Keys)
                {
                    if (item.Trim() != "2660Y")
                    {
                        resultado.Add($"1CPD|{Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(diccionario1CPC[item], '|'))}");
                    }
                }

                resultado.Add(Helpers.ValidarPipePipe($"1CPM|{Helpers.GetCampoLLanos(datosOriginales, 10, "*p35x2660Y")}")); // MensajeClausulaPermanencia
            }
            

            return resultado;
            #endregion
        }

        private void LlenarDiccionario1CPC(Dictionary<string, List<string>> pDiccionario1CPC, List<string> plinea1CPC)
        {
            string llave = string.Empty;

            foreach (string linea in plinea1CPC)
            {
                llave = linea.Substring(0, 5).Trim();
                if (pDiccionario1CPC.ContainsKey(llave))
                {
                    pDiccionario1CPC[llave].Add(GetCampoValor1CPC(linea.Substring(5).Trim()));
                }
                else
                {
                    pDiccionario1CPC.Add(llave, new List<string>() { GetCampoValor1CPC(linea.Substring(5).Trim()) });
                }

            }
        }

        private string GetCampoValor1CPC(string pCampo)
        {
            string resultado = pCampo;
            if (pCampo.Contains("."))
            {
                string[] campos = pCampo.Split('.');

                if (campos.Length == 2 && campos[1].Length == 2)
                {
                    resultado = campos[0];
                }
            }

            return resultado;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1MEN(List<string> datosOriginales)
        {
            #region FormateoCanal1MEN
            List<string> resultado = new List<string>();
            List<string> ListaCanal1MEN = new List<string>();
            List<CamposLLanos> camposllanos = new List<CamposLLanos>();

            //ListaCanal1MEN.Add("1MEN");
            ListaCanal1MEN.Add(Helpers.GetCampoLLanos(datosOriginales, 10, "*p35x2500Y")); // MensajeInformativo
            ListaCanal1MEN.Add(Helpers.GetCampoLLanos(datosOriginales, 10, "*p35x2680Y")); // MensajeInformativo
            ListaCanal1MEN.Add(Helpers.GetCampoLLanos(datosOriginales, 10, "*p36x2541Y")); // MensajeInformativo
            ListaCanal1MEN.Add(Helpers.GetCampoLLanos(datosOriginales, 10, "*p35x2575Y")); // MensajeInformativo
            ListaCanal1MEN.Add(Helpers.GetCampoLLanos(datosOriginales, 10, "*p35x2610Y")); // MensajeInformativo
            ListaCanal1MEN.Add(Helpers.GetCampoLLanos(datosOriginales, 10, "*p35x2645Y")); // MensajeInformativo
            ListaCanal1MEN.Add(Helpers.GetCampoLLanos(datosOriginales, 10, "*p490x470Y")); // MensajeInformativo
            ListaCanal1MEN.Add(Helpers.GetCampoLLanos(datosOriginales, 10, "*p490x500Y")); // MensajeInformativo

            foreach (var item in ListaCanal1MEN)
            {
                if (!string.IsNullOrEmpty(item.Trim()))
                {
                    resultado.Add($"1MEN|{item.Trim()}");
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
        private string FormateoCanal1LMP(List<string> datosOriginales)
        {
            #region FormateoCanal1LMP
            string resultado = string.Empty;

            string linea1LMP = Helpers.GetValueInsumoCadena(Variables.Variables.DatosInsumoLlanosMinPlan, $"{Cuenta}{Telefono}");

            if (!string.IsNullOrEmpty(linea1LMP))
            {
                string[] campos = linea1LMP.Split('|');
                if (campos.Length > 4)
                {
                    resultado = $"1LMP|{campos[2]}|{campos[3]}|{campos[4]}| ";
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
        private IEnumerable<string> FormateoCanal1LSP(List<string> datosOriginales)
        {
            #region FormateoCanal1LSP
            List<string> resultado = new List<string>();

            List<string> linea1LSP = Helpers.GetValueInsumoLista(Variables.Variables.DatosInsumoLlanosSuple, $"{Cuenta}{Telefono}");

            string[] campos;
            
            foreach (var item in linea1LSP)
            {
                campos = item.Split('|');
                if (campos.Length > 3)
                {
                    resultado.Add($"1LSP|{campos[2]}|{campos[3]} ");
                }
            }

            return resultado;
            #endregion
        }

        #endregion

        private void CargarCiudades()
        {
            Variables.Variables.DicCiudadesLlanos.Add("01", new Variables.CiudadesLLanos("VILLAVICENCIO", "META"));
            Variables.Variables.DicCiudadesLlanos.Add("1", new Variables.CiudadesLLanos("VILLAVICENCIO", "META"));
            Variables.Variables.DicCiudadesLlanos.Add("50", new Variables.CiudadesLLanos("ACACIAS", "META"));
            Variables.Variables.DicCiudadesLlanos.Add("51", new Variables.CiudadesLLanos("GRANADA", "META"));
            Variables.Variables.DicCiudadesLlanos.Add("52", new Variables.CiudadesLLanos("SAN MARTIN", "META"));
            Variables.Variables.DicCiudadesLlanos.Add("53", new Variables.CiudadesLLanos("GUAMAL", "META"));
            Variables.Variables.DicCiudadesLlanos.Add("54", new Variables.CiudadesLLanos("CASTILLA LA NUEVA", "META"));
            Variables.Variables.DicCiudadesLlanos.Add("55", new Variables.CiudadesLLanos("CUMARAL", "META"));
            Variables.Variables.DicCiudadesLlanos.Add("56", new Variables.CiudadesLLanos("PUERTO LOPEZ", "META"));
            Variables.Variables.DicCiudadesLlanos.Add("31", new Variables.CiudadesLLanos("BOGOTA", "C / MARCA"));
        }

        private void CargarActividades()
        {
            Variables.Variables.DicActividadesLlanos.Add("-1", "");
            Variables.Variables.DicActividadesLlanos.Add("2", "NO RESIDENCIAL");
            Variables.Variables.DicActividadesLlanos.Add("3", "RESIDENCIAL");
            Variables.Variables.DicActividadesLlanos.Add("6", "TELEFONOS EMPRESA");
            Variables.Variables.DicActividadesLlanos.Add("8", "PBX");
            Variables.Variables.DicActividadesLlanos.Add("1", "OFICIAL");
            Variables.Variables.DicActividadesLlanos.Add("7", "PARES AISLADOS");
            Variables.Variables.DicActividadesLlanos.Add("10", "INALAMBRICO");
            Variables.Variables.DicActividadesLlanos.Add("4", "CAMBIO DE LINEA");
            Variables.Variables.DicActividadesLlanos.Add("11", "INALAMBRICA PROVISIONAL");
            Variables.Variables.DicActividadesLlanos.Add("12", "PLAN PORFIA");
            Variables.Variables.DicActividadesLlanos.Add("0", "INDEFINIDO");
            Variables.Variables.DicActividadesLlanos.Add("16", "ACOMETIDA EXTERNA");
            Variables.Variables.DicActividadesLlanos.Add("17", "E1");
            Variables.Variables.DicActividadesLlanos.Add("20", "ACCESO INTERNET");
            Variables.Variables.DicActividadesLlanos.Add("30", "NO DOMICILIARIO");
            Variables.Variables.DicActividadesLlanos.Add("18", "TELECENTROS");
        }
    }
}
