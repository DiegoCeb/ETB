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
    /// Clase ProcesoLlanos
    /// </summary>
    public class ProcesoLlanos : IProcess
    {
        #region Variables del proceso
        private string Cuenta { get; set; }
        private string Telefono { get; set; }
        private string NombreArchivo { get; set; }
        private string MesConsumo { get; set; }
        #endregion

        /// <summary>
        /// Constructor ProcesoLlanos
        /// </summary>
        /// <param name="pArchivo"></param>
        public ProcesoLlanos(string pArchivo)
        {
            #region ProcesoLlanos
            try
            {
                Ejecutar(pArchivo);
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
        public ProcesoLlanos()
        { }

        /// <summary>
        /// Metodo CargueFormateoArchivo
        /// </summary>
        /// <param name="pArchivo"></param>
        public void CargueFormateoArchivo(string pArchivo)
        {
            if (pArchivo.Contains("desktop.ini"))
            {
                return;
            }

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
                        llaveCruce = datosExtractoFormateo.Find(x => x.Contains("*p2000x190Y")).Substring(12).Trim();

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

                    datosExtractoFormateo.Add(linea.Trim().Replace("|", string.Empty));
                }
                else
                {
                    datosExtractoFormateo.Add(linea.Trim().Replace("|", string.Empty));
                }
            }

            //Ultimo Extracto
            if (datosExtractoFormateo.Count > 1)
            {
                llaveCruce = datosExtractoFormateo.Find(x => x.Contains("*p2000x190Y")).Substring(12).Trim();

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

        /// <summary>
        /// Metodo Ejecutar
        /// </summary>
        /// <param name="pArchivo"></param>
        public void Ejecutar(string pArchivo)
        {
            if (!Path.GetFileName(pArchivo).Contains("desktop.ini"))
            {
                CargarCiudades();
                CargarActividades();
                CargueFormateoArchivo(pArchivo);
            }
        }

        /// <summary>
        /// Metodo FormatearArchivo
        /// </summary>
        /// <param name="pLLaveCruce"></param>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
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

            if (((IEnumerable<string>)resultadoFormateoLinea).Any())
            {
                resultado.AddRange(resultadoFormateoLinea);
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
        /// Metodo de FormateoCanal1AAA
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

        #region Metodos 1AAA
        /// <summary>
        /// Metodo que obtiene CuentaSinLetras
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private string GetCuentaSinLetras(string pCampo)
        {
            #region GetCuentaSinLetras
            string result = pCampo;
            if (!string.IsNullOrEmpty(pCampo))
            {
                result = string.Concat(pCampo.Where(c => Char.IsDigit(c)));

            }

            return result.Trim();
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene ZonaPostal
        /// </summary>
        /// <param name="pZonaPostal"></param>
        /// <param name="separado"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Metodo que obtiene CiudadDepto
        /// </summary>
        /// <param name="pZonaPostal"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Metodo que obtiene Actividad
        /// </summary>
        /// <param name="pActividad"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Metodo que obtiene Email
        /// </summary>
        /// <returns></returns>
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
        #endregion

        /// <summary>
        /// Metodo de 
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
            ListaCanal1BBA.Add(GetVlrMinInt(Helpers.GetCampoLLanos(datosOriginales, 11, "*p1130x435Y", "0.00"), mesConsumo));  // vlr_min_int

            camposllanos.Clear();
            //camposllanos.Add(new CamposLLanos(11, "*p1130x475Y", "0.00")); // vlr_min_adic
            //camposllanos.Add(new CamposLLanos(11, "*p1250x550Y")); // ciclo
            camposllanos.Add(new CamposLLanos(10, "*p920x170Y")); // zona_sector_manzana
            ListaCanal1BBA.Add(Helpers.GetCamposLlanos(datosOriginales, camposllanos));

            resultado = Helpers.ValidarPipePipe(Helpers.ListaCamposToLinea(ListaCanal1BBA, '|'));

            MesConsumo = Helpers.GetCampoLLanos(datosOriginales, 11, "*p1100x290Y");

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene VlrMinInt
        /// </summary>
        /// <param name="pVlrMinInt"></param>
        /// <param name="pMesConsumo"></param>
        /// <returns></returns>
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
        /// Metodo de FormateoCanal1BBB
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
        /// Metodo de FormateoCanal1CCC
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1CCC(List<string> datosOriginales)
        {
            #region FormateoCanal1CCC
            List<string> resultado = new List<string>();

            Dictionary<string, List<string>> diccionario1CCC = new Dictionary<string, List<string>>();
            List<string> lineas1CCC;

            #region Detalles
            lineas1CCC = Helpers.GetListaCampoLLanos(datosOriginales, 5, "*p22x"); // concepto
            LlenarDiccionario1CCC(diccionario1CCC, lineas1CCC);

            lineas1CCC = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p580x"); // descripcion
            LlenarDiccionario1CCC(diccionario1CCC, lineas1CCC);

            string concepto = string.Empty;
            foreach (var detalles in diccionario1CCC.Values)
            {
                if (detalles.Count == 2)
                {
                    concepto = detalles[0].Trim().Replace("]", $" {MesConsumo}]");
                    resultado.Add($"1CCC|{concepto}|{detalles[1].Trim()}");
                }

            }
            #endregion

            #region Detalle IVA
            bool conceptoIva = false;
            string lineaIva = string.Empty;

            lineas1CCC = Helpers.GetListaCampoLLanos(datosOriginales, 10, "*p22x1074Y"); // concepto_iva
            if (lineas1CCC.Count > 0)
            {
                lineaIva += $"1CCC|{lineas1CCC[0].Trim()}|";
                conceptoIva = true;
            }

            lineas1CCC = Helpers.GetListaCampoLLanos(datosOriginales, 11, "*p580x1074Y"); // valor_iva
            if (lineas1CCC.Count > 0 && conceptoIva)
            {
                lineaIva += $"{lineas1CCC[0].Trim()}";
            }
            if (conceptoIva)
            {
                resultado.Add(lineaIva);
            }
            #endregion

            #region Total
            string valorTotal = "0.00";
            lineas1CCC = Helpers.GetListaCampoLLanos(datosOriginales, 11, "*p580x1138Y"); // valor_cargos_etb
            if (lineas1CCC.Count > 0)
            {
                valorTotal = lineas1CCC[0].Trim();
            }

            resultado.Add($"1CCC|VALOR CARGOS ETB|{valorTotal}");
            #endregion

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo de LlenarDiccionario1CCC
        /// </summary>
        /// <param name="pDiccionario1CCC"></param>
        /// <param name="plinea1CCC"></param>
        private void LlenarDiccionario1CCC(Dictionary<string, List<string>> pDiccionario1CCC, List<string> plinea1CCC)
        {
            string llave = string.Empty;
            string datos = string.Empty;
            foreach (string linea in plinea1CCC)
            {
                llave = linea.Substring(0, linea.IndexOf("Y")).Trim();
                datos = linea.Substring(linea.IndexOf("Y") + 1).Trim();

                if (llave != "1074" && llave != "1138")
                {
                    if (pDiccionario1CCC.ContainsKey(llave))
                    {
                        pDiccionario1CCC[llave].Add(datos);
                    }
                    else
                    {
                        pDiccionario1CCC.Add(llave, new List<string>() { datos });
                    }
                }
            }
        }

        /// <summary>
        /// /// Metodo de FormateoCanal1DDD
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1DDD(List<string> datosOriginales)
        {
            #region FormateoCanal1DDD
            List<string> resultado = new List<string>();

            Dictionary<string, List<string>> diccionario1CCC = new Dictionary<string, List<string>>();
            List<string> lineas1DDD;

            #region Detalles
            lineas1DDD = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p832x"); // concepto
            LlenarDiccionario1CCC(diccionario1CCC, lineas1DDD);

            lineas1DDD = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1390x"); // valor
            LlenarDiccionario1CCC(diccionario1CCC, lineas1DDD);

            string concepto = string.Empty;
            foreach (var detalles in diccionario1CCC.Values)
            {
                if (detalles.Count == 2)
                {
                    concepto = detalles[0].Trim();
                    resultado.Add($"1DDD|{concepto}|{detalles[1].Trim()}");
                }

            }
            #endregion

            #region Detalle IVA
            bool conceptoIva = false;
            string lineaIva = string.Empty;

            lineas1DDD = Helpers.GetListaCampoLLanos(datosOriginales, 11, "*p832x1074Y"); // concepto_iva
            if (lineas1DDD.Count > 0)
            {
                lineaIva += $"1DDD|{lineas1DDD[0].Trim()}|";
                conceptoIva = true;
            }

            lineas1DDD = Helpers.GetListaCampoLLanos(datosOriginales, 12, "*p1390x1074Y"); // valor_iva
            if (lineas1DDD.Count > 0 && conceptoIva)
            {

                lineaIva += $"{lineas1DDD[0].Trim()}";
            }
            if (conceptoIva)
            {
                resultado.Add(lineaIva);
            }
            #endregion

            #region Total
            string valorTotal = "0.00";
            lineas1DDD = Helpers.GetListaCampoLLanos(datosOriginales, 12, "*p1390x1138Y"); // valor_ld_movil
            if (lineas1DDD.Count > 0)
            {
                valorTotal = lineas1DDD[0].Trim();
            }

            resultado.Add($"1DDD|PAGAR EMPRESAS LD Y MÓVIL|{valorTotal}");
            #endregion

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo de FormateoCanal1EEE
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1EEE(List<string> datosOriginales)
        {
            #region FormateoCanal1EEE
            List<string> resultado = new List<string>();

            Dictionary<string, List<string>> diccionario1EEE = new Dictionary<string, List<string>>();
            List<string> lineas1EEE;

            #region Detalles
            lineas1EEE = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1592x"); // concepto
            LlenarDiccionario1CCC(diccionario1EEE, lineas1EEE);

            lineas1EEE = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p2150x"); // valor
            LlenarDiccionario1CCC(diccionario1EEE, lineas1EEE);

            diccionario1EEE = diccionario1EEE.Reverse().ToDictionary(x => x.Key, x => x.Value);
            string concepto = string.Empty;
            foreach (var detalles in diccionario1EEE.Values)
            {
                if (detalles.Count == 2)
                {
                    concepto = detalles[0].Trim();
                    resultado.Add($"1EEE|{concepto}|{detalles[1].Trim()}");
                }

            }
            #endregion

            #region Total
            string valorTotal = "0.00";
            lineas1EEE = Helpers.GetListaCampoLLanos(datosOriginales, 12, "*p2150x1138Y"); // valor_ld_movil
            if (lineas1EEE.Count > 0)
            {
                valorTotal = lineas1EEE[0].Trim();
            }

            resultado.Add($"1EEE|TOTAL FACTURAS VENCIDAS|{valorTotal}");
            #endregion

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo de FormateoCanal1FFF
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormateoCanal1FFF(List<string> datosOriginales)
        {
            #region FormateoCanal1FFF
            List<string> resultado = new List<string>();

            Dictionary<string, List<string>> diccionario1FFF = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> diccionario1FFF2 = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> diccionario1FFF3 = new Dictionary<string, List<string>>();
            List<string> lineas1FFF;


            #region Fecha
            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 5, "*p30x"); // fecha
            LlenarDiccionario1CCC(diccionario1FFF, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1227x"); // fecha
            LlenarDiccionario1CCC(diccionario1FFF2, lineas1FFF);


            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1240x"); // fecha
            LlenarDiccionario1CCC(diccionario1FFF3, lineas1FFF);

            #endregion

            #region Hora
            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p140x"); // Hora
            LlenarDiccionario1CCC(diccionario1FFF, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1337x"); // Hora
            LlenarDiccionario1CCC(diccionario1FFF2, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1350x"); // Hora
            LlenarDiccionario1CCC(diccionario1FFF3, lineas1FFF);
            #endregion

            #region CL
            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p250x"); // CL
            LlenarDiccionario1CCC(diccionario1FFF, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1447x"); // CL
            LlenarDiccionario1CCC(diccionario1FFF2, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1460x"); // CL
            LlenarDiccionario1CCC(diccionario1FFF3, lineas1FFF);
            #endregion

            #region Destino
            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p310x"); // Destino
            LlenarDiccionario1CCC(diccionario1FFF, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1507x"); // Destino
            LlenarDiccionario1CCC(diccionario1FFF2, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1520x"); // Destino
            LlenarDiccionario1CCC(diccionario1FFF3, lineas1FFF);
            #endregion

            #region Telefono
            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p490x"); // Telefono
            LlenarDiccionario1CCC(diccionario1FFF, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1687x"); // Telefono
            LlenarDiccionario1CCC(diccionario1FFF2, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1700x"); // Telefono
            LlenarDiccionario1CCC(diccionario1FFF3, lineas1FFF);
            #endregion

            #region Valor Minutos
            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p720x"); // Minutos
            LlenarDiccionario1CCC(diccionario1FFF, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1917x"); // Minutos
            LlenarDiccionario1CCC(diccionario1FFF2, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1930x"); // Minutos
            LlenarDiccionario1CCC(diccionario1FFF3, lineas1FFF);
            #endregion

            #region Valor Minuto
            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 6, "*p840x"); // Valor Minuto
            LlenarDiccionario1CCC(diccionario1FFF, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p2037x"); // Valor Minuto
            LlenarDiccionario1CCC(diccionario1FFF2, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p2050x"); // Valor Minuto
            LlenarDiccionario1CCC(diccionario1FFF3, lineas1FFF);
            #endregion

            #region Valor Total
            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p1020x"); // Valor Total
            LlenarDiccionario1CCC(diccionario1FFF, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p2217x"); // Valor Total
            LlenarDiccionario1CCC(diccionario1FFF2, lineas1FFF);

            lineas1FFF = Helpers.GetListaCampoLLanos(datosOriginales, 7, "*p2230x"); // Valor Total
            LlenarDiccionario1CCC(diccionario1FFF3, lineas1FFF);
            #endregion

            #region Recorrer Diccionarios y Ordenar Detalles
            string lineaDetalle = string.Empty;
            SortedDictionary<DateTime, List<string>> detallesOrdenados = new SortedDictionary<DateTime, List<string>>();
            DateTime dateTime;
            foreach (var detalles in diccionario1FFF.Values)
            {
                if (detalles.Count == 8)
                {
                    dateTime = Convert.ToDateTime($"{DateTime.Now.Year.ToString()}/{detalles[0]} {detalles[1]}", new CultureInfo("es-CO"));
                    lineaDetalle = String.Empty;

                    foreach (var campo in detalles)
                    {
                        if (string.IsNullOrEmpty(lineaDetalle.Trim()))
                        {
                            lineaDetalle += $"{campo.Trim()}";
                        }
                        else
                        {
                            lineaDetalle += $"|{campo.Trim()}";
                        }
                    }

                    if (!detallesOrdenados.ContainsKey(dateTime))
                    {
                        detallesOrdenados.Add(dateTime, new List<string>() { lineaDetalle });
                    }
                    else
                    {
                        detallesOrdenados[dateTime].Add(lineaDetalle);
                    }


                }

            }

            foreach (var detalles in diccionario1FFF2.Values)
            {
                if (detalles.Count == 8)
                {
                    dateTime = Convert.ToDateTime($"{DateTime.Now.Year.ToString()}/{detalles[0]} {detalles[1]}", new CultureInfo("es-CO"));
                    lineaDetalle = String.Empty;

                    foreach (var campo in detalles)
                    {
                        if (string.IsNullOrEmpty(lineaDetalle.Trim()))
                        {
                            lineaDetalle += $"{campo.Trim()}";
                        }
                        else
                        {
                            lineaDetalle += $"|{campo.Trim()}";
                        }
                    }

                    if (!detallesOrdenados.ContainsKey(dateTime))
                    {
                        detallesOrdenados.Add(dateTime, new List<string>() { lineaDetalle });
                    }
                    else
                    {
                        detallesOrdenados[dateTime].Add(lineaDetalle);
                    }
                }

            }

            foreach (var detalles in diccionario1FFF3.Values)
            {
                if (detalles.Count == 8)
                {
                    dateTime = Convert.ToDateTime($"{DateTime.Now.Year.ToString()}/{detalles[0]} {detalles[1]}", new CultureInfo("es-CO"));
                    lineaDetalle = String.Empty;

                    foreach (var campo in detalles)
                    {
                        if (string.IsNullOrEmpty(lineaDetalle.Trim()))
                        {
                            lineaDetalle += $"{campo.Trim()}";
                        }
                        else
                        {
                            lineaDetalle += $"|{campo.Trim()}";
                        }
                    }

                    if (!detallesOrdenados.ContainsKey(dateTime))
                    {
                        detallesOrdenados.Add(dateTime, new List<string>() { lineaDetalle });
                    }
                    else
                    {
                        detallesOrdenados[dateTime].Add(lineaDetalle);
                    }
                }
            }

            int contadorDetalles = 0;
            foreach (List<string> listaDetalles in detallesOrdenados.Values)
            {
                foreach (string detalle in listaDetalles)
                {
                    contadorDetalles++;
                    if (contadorDetalles < 37)
                    {
                        resultado.Add($"1FFF|{detalle}");
                    }
                    else if (contadorDetalles < 74)
                    {
                        resultado.Add($"1FFA|{detalle}");
                    }
                    else
                    {
                        resultado.Add($"1FFB|{detalle}");
                    }

                }
            }
            #endregion

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo de FormateoCanal1FFA
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
        /// Metodo de FormateoCanal1FFB
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
        /// Metodo de FormateoCanal1HHH
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private string FormateoCanal1HHH(List<string> datosOriginales)
        {
            #region FormateoCanal1HHH
            string resultado = string.Empty;

            string actividad = Helpers.GetCampoLLanos(datosOriginales, 10, "*p230x330Y").Trim(); // actividad

            List<string> lineas1CCC = Helpers.GetListaCampoLLanos(datosOriginales, 5, "*p22x"); // concepto
            string datos;
            bool isXDSL = false;

            foreach (string linea in lineas1CCC)
            {
                datos = linea.Substring(linea.IndexOf("Y") + 1).Trim();

                if (datos.Contains("XDSL"))
                {
                    isXDSL = true;
                    break;
                }
            }

            if (isXDSL && actividad.Trim() == "3")
            {
                resultado = $"1HHH|logo internet|true";
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// /// Metodo de FormarPaqueteCPC
        /// </summary>
        /// <param name="datosOriginales"></param>
        /// <returns></returns>
        private IEnumerable<string> FormarPaqueteCPC(List<string> datosOriginales)
        {
            #region FormarPaqueteCPC
            List<string> resultado = new List<string>();

            string titulo = Helpers.GetCampoLLanos(datosOriginales, 11, "*p250x2500Y"); // TituloClausulaPermanencia

            if (!string.IsNullOrEmpty(titulo))
            {
                resultado.Add($"1CPC|{titulo}");

                List<string> linea1CPC;
                Dictionary<string, List<string>> diccionario1CPC = new Dictionary<string, List<string>>();

                linea1CPC = Helpers.GetListaCampoLLanos(datosOriginales, 5, "*p35x"); // Servicio
                LlenarDiccionario1CPC(diccionario1CPC, linea1CPC);

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

        /// <summary>
        /// Metodo de LlenarDiccionario1CPC
        /// </summary>
        /// <param name="pDiccionario1CPC"></param>
        /// <param name="plinea1CPC"></param>
        private void LlenarDiccionario1CPC(Dictionary<string, List<string>> pDiccionario1CPC, List<string> plinea1CPC)
        {
            #region LlenarDiccionario1CPC
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
            #endregion
        }

        /// <summary>
        /// Metodo que Obtiene Valor1CPC
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
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
        /// Metodo de FormateoCanal1MEN
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
        /// Metodo de FormateoCanal1LMP
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
        /// Metodo de FormateoCanal1LSP
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

        /// <summary>
        /// Metodo Cargar Ciudades
        /// </summary>
        private void CargarCiudades()
        {
            if (!Variables.Variables.DicCiudadesLlanos.ContainsKey("01"))
            {
                Variables.Variables.DicCiudadesLlanos.Add("01", new Variables.CiudadesLLanos("VILLAVICENCIO", "META"));
            }
            if (!Variables.Variables.DicCiudadesLlanos.ContainsKey("1"))
            {
                Variables.Variables.DicCiudadesLlanos.Add("1", new Variables.CiudadesLLanos("VILLAVICENCIO", "META"));
            }
            if (!Variables.Variables.DicCiudadesLlanos.ContainsKey("50"))
            {
                Variables.Variables.DicCiudadesLlanos.Add("50", new Variables.CiudadesLLanos("ACACIAS", "META"));
            }
            if (!Variables.Variables.DicCiudadesLlanos.ContainsKey("51"))
            {
                Variables.Variables.DicCiudadesLlanos.Add("51", new Variables.CiudadesLLanos("GRANADA", "META"));
            }
            if (!Variables.Variables.DicCiudadesLlanos.ContainsKey("52"))
            {
                Variables.Variables.DicCiudadesLlanos.Add("52", new Variables.CiudadesLLanos("SAN MARTIN", "META"));
            }
            if (!Variables.Variables.DicCiudadesLlanos.ContainsKey("53"))
            {
                Variables.Variables.DicCiudadesLlanos.Add("53", new Variables.CiudadesLLanos("GUAMAL", "META"));
            }
            if (!Variables.Variables.DicCiudadesLlanos.ContainsKey("54"))
            {
                Variables.Variables.DicCiudadesLlanos.Add("54", new Variables.CiudadesLLanos("CASTILLA LA NUEVA", "META"));
            }
            if (!Variables.Variables.DicCiudadesLlanos.ContainsKey("55"))
            {
                Variables.Variables.DicCiudadesLlanos.Add("55", new Variables.CiudadesLLanos("CUMARAL", "META"));
            }
            if (!Variables.Variables.DicCiudadesLlanos.ContainsKey("56"))
            {
                Variables.Variables.DicCiudadesLlanos.Add("56", new Variables.CiudadesLLanos("PUERTO LOPEZ", "META"));
            }
            if (!Variables.Variables.DicCiudadesLlanos.ContainsKey("31"))
            {
                Variables.Variables.DicCiudadesLlanos.Add("31", new Variables.CiudadesLLanos("BOGOTA", "C / MARCA"));
            }
        }

        /// <summary>
        /// Metodo Cargar Actividades
        /// </summary>
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
