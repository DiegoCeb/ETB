using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using DLL_Utilidades;
using SharpCompress.Archives;
using SharpCompress.Readers;
using SharpCompress.Common;
using Excel;

namespace App.ControlInsumos
{
    /// <summary>
    /// Clase Helpers
    /// </summary>
    public static class Helpers
    {
        public static string RutaProceso { get; set; }
        public static string RutaOriginales { get; set; }
        public static string RutaInsumos { get; set; }

        #region Metodos Cargue Insumos

        /// <summary>
        ///
        /// </summary>
        /// <param name="pDiccionario"></param>
        /// <param name="pLlave"></param>
        /// <returns></returns>
        public static List<string> GetValueInsumoLista(Dictionary<string, List<string>> pDiccionario, string pLlave)
        {
            #region GetValueInsumoLista
            List<string> resultado = new List<string>();



            if (pDiccionario.ContainsKey(pLlave))
            {
                resultado = pDiccionario[pLlave];
            }



            return resultado;
            #endregion
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pDiccionario"></param>
        /// <param name="pLlave"></param>
        /// <returns></returns>
        public static string GetValueInsumoCadena(Dictionary<string, string> pDiccionario, string pLlave)
        {
            #region GetValueInsumoCadena
            string resultado = string.Empty;



            if (pDiccionario.ContainsKey(pLlave))
            {
                resultado = pDiccionario[pLlave];
            }



            return resultado;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetServiciosAdicionales(List<string> pDatosInsumo)
        {
            #region GetServiciosAdicionales

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoExcluirServiciosAdicionales.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoExcluirServiciosAdicionales[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoExcluirServiciosAdicionales.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetTablaSutitucion(List<string> pDatosInsumo)
        {
            #region GetTablaSutitucion

            #region Substring 6
            // ACTR ACTIVIDAD REVCHAIN
            // TASM TASAS DE MORA POR ACTIVIDAD
            // CSERCENTROS DE SERVICIO POR DEPARTAMENTO
            // DANCCODIGOS DANE POR MUNICIPIO
            var result6 = from datos in pDatosInsumo
                          where datos.Length > 6
                          let comp = datos.Substring(0, 4).Trim()
                          where comp == "ACTR" ||
                           comp == "TASM" ||
                           comp == "CSER" ||
                           comp == "DAND"
                          select datos;

            foreach (var dato in result6)
            {
                string llaveCruce = dato.Substring(0, 6).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 7
            // CODLCODIGOS DE LOCALIDAD
            var result7 = from datos in pDatosInsumo
                          where datos.Length > 7
                          let comp = datos.Substring(0, 4).Trim()
                          where comp == "CODL"
                          select datos;

            foreach (var dato in result7)
            {
                string llaveCruce = dato.Substring(0, 7).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 8
            // CODIGO DE BARRASCODBAR32
            // TASNTASA DE MORA ANTERIOR
            // TSERTIPOS DE SERVICIO
            // SSUPSERVICIOS SUPLEMENTARIOS EMPAQUETADOS
            // CODSCODIGOS DE SUBOPERADOR
            // FACLINLíneaS DE NEGOCIO PARA FACTURA
            // ZPOSZONAS POSTALES PARA PUNTOS DE PAGO
            // MDEPLPLAN FD MDE

            var result8 = from datos in pDatosInsumo
                          where datos.Length > 8
                          let comp4 = datos.Substring(0, 4).Trim()
                          let comp5 = datos.Substring(0, 5).Trim()
                          let comp6 = datos.Substring(0, 6).Trim()
                          where
                          comp6 == "CODBAR" ||
                          comp4 == "TASN" ||
                          comp4 == "TSER" ||
                          comp4 == "SSUP" ||
                          comp4 == "CODS" ||
                          comp6 == "FACLIN" ||
                          comp4 == "ZPOS" ||
                          comp5 == "MDEPL"
                          select datos;

            foreach (var dato in result8)
            {
                string llaveCruce = dato.Substring(0, 8).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 9
            // DANDCODIGOS DANE DE DEPARTAMENTOS
            // CLTFCLASE DE TARIFA DE MULTIMEDICION POR CODIGO DANE
            // NRATNUMERO DE ATENCION TELEFONICA POR CODIGO DANE
            // PPPUNTO DE PAGO CUNDINAMARCA POR CUENTA

            var result9 = from datos in pDatosInsumo
                          where datos.Length > 9
                          let comp2 = datos.Substring(0, 2).Trim()
                          let comp4 = datos.Substring(0, 4).Trim()
                          let comp6 = datos.Substring(0, 6).Trim()
                          where
                          comp4 == "DANC" ||
                          comp4 == "CLTF" ||
                          comp4 == "NRAT" ||
                          comp2 == "PP"
                          select datos;

            foreach (var dato in result9)
            {
                string llaveCruce = dato.Substring(0, 9).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 10
            // FACPRODPRODUCTOS PARA FACTURA
            // CZONADIRECCION CENTROS DE SERVICIO POR ZONA

            var result10 = from datos in pDatosInsumo
                           where datos.Length > 10
                           let comp5 = datos.Substring(0, 5).Trim()
                           let comp7 = datos.Substring(0, 7).Trim()
                           where
                           comp7 == "FACPROD" ||
                           comp5 == "CZONA"
                           select datos;

            foreach (var dato in result10)
            {
                string llaveCruce = dato.Substring(0, 10).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 11
            // CARRDIRECCION CENTROS DE SERVICIO POR LSP
            // NCARNOMBRE DE LSPS
            // CODTOTALES DE FACTURACION :::: etapa1
            // OPERNUMERO DE NIT DE OPERADORES
            // CBSUBCONSUMO BASICO DE SUBSISTENCIA

            var result11 = from datos in pDatosInsumo
                           where datos.Length > 11
                           let comp4 = datos.Substring(0, 4).Trim()
                           let comp5 = datos.Substring(0, 5).Trim()
                           where
                           comp4 == "CARR" ||
                           comp4 == "NCAR" ||
                           comp4 == "CODT" ||
                           comp4 == "OPER" ||
                           comp5 == "CBSUB"
                           select datos;

            foreach (var dato in result11)
            {
                string llaveCruce = dato.Substring(0, 11).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 12
            // FECPFECHA DE PAGO NORMAL
            // FECLFECHA ULTIMA DE PAGO EXTEMPORANEO
            // FECXFECHA DE EXPEDICION DE LA FACTURA
            // PLPLANES REVCHAIN DE LOCAL 
            // XC PL BA IC LD DV DELL 
            // SVSERVICIOS PLANES REVCHAIN DE LOCAL
            // CODXCODIGOS DE BARRIOS
            // CJURI1EMPRESAS DE COBRO JURIDICO POR MES
            // CJURI2DIRECCION EMPRESAS DE COBRO JURIDICO POR MES
            // PERIODOS DE CORTES SERVICIO LTE

            var result12 = from datos in pDatosInsumo
                           where datos.Length > 12
                           let comp2 = datos.Substring(0, 2).Trim()
                           let comp4 = datos.Substring(0, 4).Trim()
                           let comp6 = datos.Substring(0, 6).Trim()
                           where
                           comp4 == "FECP" ||
                           comp4 == "FECL" ||
                           comp4 == "FECX" ||
                           comp2 == "PL" ||
                           comp2 == "XC" ||
                           comp2 == "SV" ||
                           comp4 == "CODX" ||
                           comp6 == "CJURI1" ||
                           comp6 == "CJURI2" ||
                           comp6 == "PERCOR"
                           select datos;

            foreach (var dato in result12)
            {
                string llaveCruce = dato.Substring(0, 12).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 14
            // REICOSTOS DE REINSTALACION POR PLAN DE VOZ LOCAL
            // CODFCODIGOS DE FACTURACION
            // MINCMINUTOS LOCALES INCLUIDOS POR PLAN DE LOCAL   --FABICORA, ACTUALIADO AL 19 DE NOV DE 2012
            // VMIN VALOR DEL MINUTO POR PLAN, ESTRATO Y TIPO DE SELLO EN LA FACTURA
            // VFGE VALOR DEL MINUTO ADICIONAL PARA GOBIERNO Y ESPECIALES POR PLAN Y ESTRATO.

            var result14 = from datos in pDatosInsumo
                           where datos.Length > 14
                           let comp3 = datos.Substring(0, 3).Trim()
                           let comp4 = datos.Substring(0, 4).Trim()
                           where
                           comp3 == "REI" ||
                           comp4 == "CODF" ||
                           comp4 == "MINC" ||
                           comp4 == "VMIN" ||
                           comp4 == "VFGE"
                           select datos;

            foreach (var dato in result14)
            {
                string llaveCruce = dato.Substring(0, 14).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 15
            // NPRPLANNOMBRE PLANES NPR

            var result15 = from datos in pDatosInsumo
                           where datos.Length > 15
                           let comp5 = datos.Substring(0, 5).Trim()
                           where
                           comp5 == "NPRPL"
                           select datos;

            foreach (var dato in result15)
            {
                string llaveCruce = dato.Substring(0, 15).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 16
            // CODDATCODIGOS DE DATOS

            var result16 = from datos in pDatosInsumo
                           where datos.Length > 16
                           let comp6 = datos.Substring(0, 6).Trim()
                           where
                           comp6 == "CODDAT"
                           select datos;

            foreach (var dato in result16)
            {
                string llaveCruce = dato.Substring(0, 16).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 17
            // NPRVALVALORES PLANES NPR

            var result17 = from datos in pDatosInsumo
                           where datos.Length > 17
                           let comp6 = datos.Substring(0, 6).Trim()
                           where
                           comp6 == "NPRVAL"
                           select datos;

            foreach (var dato in result17)
            {
                string llaveCruce = dato.Substring(0, 17).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #region Substring 29
            // BALOTO DANE LOC BAR    --CADENA PARA INCLUIR EL PUNTO BALOTO MAS CERCANO

            var result29 = from datos in pDatosInsumo
                           where datos.Length > 29
                           let comp16 = datos.Substring(0, 16).Trim()
                           where
                           comp16 == "BALOTODANELOCBAR"
                           select datos;

            foreach (var dato in result29)
            {
                string llaveCruce = dato.Substring(0, 29).Trim();

                if (!Variables.Variables.DatosInsumoTablaSustitucion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTablaSustitucion.Add(llaveCruce, new List<string> { dato });
                }
            }
            #endregion

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetServiciosAdicionalesTv(List<string> pDatosInsumo)
        {
            #region GetServiciosAdicionalesTv

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (!Variables.Variables.DatosInsumoServiciosAdicionalesTv.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoServiciosAdicionalesTv.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetVelocidadFibra(List<string> pDatosInsumo)
        {
            #region GetVelocidadFibra

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoVelocidadFibra.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoVelocidadFibra[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoVelocidadFibra.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetParametrizacionPaquetesFibra(List<string> pDatosInsumo)
        {
            #region GetParametrizacionPaquetesFibra

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(1);

                if (!Variables.Variables.DatosInsumoParametrizacionPaquetesFibra.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoParametrizacionPaquetesFibra.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetCuentasEnvioSms(List<string> pDatosInsumo)
        {
            #region GetCuentasEnvioSms

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea;

                if (!Variables.Variables.DatosInsumoCuentasEnvioSms.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoCuentasEnvioSms.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetCodigosUniverSvas(List<string> pDatosInsumo)
        {
            #region GetCodigosUniverSvas

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(1);

                if (!Variables.Variables.DatosInsumoCodigosUniverSvas.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoCodigosUniverSvas.Add(llaveCruce, datoLinea);
                }
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetCuentasSvasFueraBundle(List<string> pDatosInsumo)
        {
            #region GetCuentasSvasFueraBundle

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (!Variables.Variables.DatosInsumoCuentasSvasFueraBundle.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoCuentasSvasFueraBundle.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetCuentasExtraer(List<string> pDatosInsumo)
        {
            #region GetCuentasExtraer

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea;

                if (!Variables.Variables.DatosInsumoCuentasExtraer.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoCuentasExtraer.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetDistribucionEspecial(List<string> pDatosInsumo)
        {
            #region GetDistribucionEspecial

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoDistribucionEspecial.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoDistribucionEspecial[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoDistribucionEspecial.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetCicloCourier(List<string> pDatosInsumo)
        {
            #region GetCicloCourier

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (!Variables.Variables.DatosInsumoCicloCourier.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoCicloCourier.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetClientesEspecialesDatos(List<string> pDatosInsumo)
        {
            #region GetClientesEspecialesDatos

            foreach (var datoLinea in pDatosInsumo)
            {
                if (!string.IsNullOrEmpty(datoLinea.Trim()))
                {
                    string llaveCruce = $"{datoLinea.Split('|').ElementAt(0)}-{datoLinea.Split('|').ElementAt(2)}";

                    if (!Variables.Variables.DatosInsumoClientesEspecialesDatos.ContainsKey(llaveCruce))
                    {
                        Variables.Variables.DatosInsumoClientesEspecialesDatos.Add(llaveCruce, datoLinea);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetBaseTranspromo(List<string> pDatosInsumo)
        {
            #region GetBaseTranspromo

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoBaseTranspromo.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoBaseTranspromo[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoBaseTranspromo.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetAsignacionCartas(List<string> pDatosInsumo)
        {
            #region GetAsignacionCartas

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoAsignacionCartas.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoAsignacionCartas[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoAsignacionCartas.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetNivelReclamacion(List<string> pDatosInsumo)
        {
            #region GetNivelReclamacion

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoNivelReclamacion.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoNivelReclamacion[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoNivelReclamacion.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetFechaPagoFijas(List<string> pDatosInsumo)
        {
            #region GetFechaPagoFijas

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (!Variables.Variables.DatosInsumoFechaPagoFijas.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoFechaPagoFijas.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetETBHorasExp(List<string> pDatosInsumo)
        {
            #region GetETBHorasExp

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (!Variables.Variables.DatosInsumoEtbHorasExp.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoEtbHorasExp.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetPromocionesLTE(List<string> pDatosInsumo)
        {
            #region GetPromocionesLTE

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (!Variables.Variables.DatosInsumoPromocionesLte.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoPromocionesLte.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetCuentasLTE(List<string> pDatosInsumo)
        {
            #region GetCuentasLTE

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea;

                if (!Variables.Variables.DatosInsumoCuentasLte.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoCuentasLte.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetClientesEmailPrivado(List<string> pDatosInsumo)
        {
            #region GetClientesEmailPrivado

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split(' ').ElementAt(0);

                if (!Variables.Variables.DatosInsumoClientesEmailPrivado.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoClientesEmailPrivado.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetBaseCupones(List<string> pDatosInsumo)
        {
            #region GetBaseCupones

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoBaseCupones.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoBaseCupones[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoBaseCupones.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetProcuni(List<string> pDatosInsumo)
        {
            #region GetProcuni

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Substring(0, 20).TrimStart('0');

                if (Variables.Variables.DatosInsumoProcuni.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoProcuni[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoProcuni.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetDoc1Participaciones(List<string> pDatosInsumo)
        {
            #region GetDoc1Participaciones

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoDoc1Participaciones.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoDoc1Participaciones[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoDoc1Participaciones.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetCodigosBD(List<string> pDatosInsumo)
        {
            #region GetCodigosBD

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoCodigosBd.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoCodigosBd[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoCodigosBd.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetLocBar(List<string> pDatosInsumo)
        {
            #region GetLocBar

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoLocBar.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoLocBar[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoLocBar.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetTelefonosPrivadosRevchain(List<string> pDatosInsumo)
        {
            #region GetTelefonosPrivadosRevchain

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea;

                if (!Variables.Variables.DatosInsumoTelefonosPrivadosRevchain.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTelefonosPrivadosRevchain.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetTramites(List<string> pDatosInsumo)
        {
            #region GetTramites

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoTramites.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoTramites[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoTramites.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetFADistribucionEmailRevchain(List<string> pDatosInsumo)
        {
            #region GetFADistribucionEmailRevchain

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoDistribucionEmailRevchain.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoDistribucionEmailRevchain[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoDistribucionEmailRevchain.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetDistribucionDobleRevchain(List<string> pDatosInsumo)
        {
            #region GetDistribucionDobleRevchain

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea;

                if (!Variables.Variables.DatosInsumoDistribucionDobleRevchain.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoDistribucionDobleRevchain.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetClausulaPermanencia(List<string> pDatosInsumo)
        {
            #region GetClausulaPermanencia

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (Variables.Variables.DatosInsumoClausulaPermanencia.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoClausulaPermanencia[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoClausulaPermanencia.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetMensajesClientesSuspendidos(List<string> pDatosInsumo)
        {
            #region GetMensajesClientesSuspendidos

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split(' ').ElementAt(0);

                if (Variables.Variables.DatosInsumoMensajesClientesSuspendidos.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoMensajesClientesSuspendidos[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoMensajesClientesSuspendidos.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetETBCartasMora(List<string> pDatosInsumo)
        {
            #region GetETBCartasMora

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split(' ').ElementAt(0);

                if (Variables.Variables.DatosInsumoEtbCartasMora.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoEtbCartasMora[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoEtbCartasMora.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetExclusionPagoInmediato(List<string> pDatosInsumo)
        {
            #region GetExclusionPagoInmediato

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea;

                if (!Variables.Variables.DatosInsumoExclusionPagoInmediato.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoExclusionPagoInmediato.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetConformacionPaquetes(List<string> pDatosInsumo)
        {
            #region GetConformacionPaquetes

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (!Variables.Variables.DatosInsumoConformacionPaquetes.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoConformacionPaquetes.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetConfiguracionLLavesDoc1(List<string> pDatosInsumo)
        {
            #region GetConfiguracionLLavesDoc1

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(4);

                if (!Variables.Variables.DatosInsumoConfiguracionLLavesDoc1.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoConfiguracionLLavesDoc1.Add(llaveCruce, datoLinea);
                }
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetETBFacturaElectronica(List<string> pDatosInsumo)
        {
            #region GetETBFacturaElectronica

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split(' ').ElementAt(0).Trim();

                if (!Variables.Variables.DatosInsumoETBFacturaElectronica.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoETBFacturaElectronica.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetDualLlanos(List<string> pDatosInsumo)
        {
            #region GetDualLlanos

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea;

                if (!Variables.Variables.DatosInsumoDualLlanos.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoDualLlanos.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetDistribucionEmailLlanos(List<string> pDatosInsumo)
        {
            #region GetDistribucionEmailLlanos

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea.Split('|').ElementAt(0);

                if (!Variables.Variables.DatosInsumoDistribucionEmailLlanos.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoDistribucionEmailLlanos.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetLlanosEnvioSMS(List<string> pDatosInsumo)
        {
            #region GetLlanosEnvioSMS

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea;

                if (!Variables.Variables.DatosInsumoLlanosEnvioSMS.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoLlanosEnvioSMS.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetExtraerLlanos(List<string> pDatosInsumo)
        {
            #region GetExtraerLlanos

            foreach (var datoLinea in pDatosInsumo)
            {
                string llaveCruce = datoLinea;

                if (!Variables.Variables.DatosInsumoExtraerLlanos.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoExtraerLlanos.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetLlanosMinPlan(List<string> pDatosInsumo)
        {
            #region GetLlanosMinPlan

            foreach (var datoLinea in pDatosInsumo)
            {

                string llaveCruce = $"{datoLinea.Split('|').ElementAt(0)}{datoLinea.Split('|').ElementAt(1)}";

                if (!Variables.Variables.DatosInsumoLlanosMinPlan.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoLlanosMinPlan.Add(llaveCruce, datoLinea);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetLlanosSuple(List<string> pDatosInsumo)
        {
            #region GetLlanosSuple

            foreach (var datoLinea in pDatosInsumo)
            {

                string llaveCruce = $"{datoLinea.Split('|').ElementAt(0)}{datoLinea.Split('|').ElementAt(1)}";

                if (Variables.Variables.DatosInsumoLlanosSuple.ContainsKey(llaveCruce))
                {
                    Variables.Variables.DatosInsumoLlanosSuple[llaveCruce].Add(datoLinea);
                }
                else
                {
                    Variables.Variables.DatosInsumoLlanosSuple.Add(llaveCruce, new List<string> { datoLinea });
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetAuxAnexosVerdes(List<string> pArchivos)
        {
            #region GetAuxAnexosVerdes

            foreach (var archivo in pArchivos)
            {
                List<string> datosInsumo = File.ReadAllLines(archivo, Encoding.Default).ToList();

                foreach (var datoLinea in datosInsumo)
                {

                    string llaveCruce = $"{datoLinea.Substring(0, 7)}";

                    if (Variables.Variables.DatosAuxAnexosVerdes.ContainsKey(llaveCruce))
                    {
                        Variables.Variables.DatosAuxAnexosVerdes[llaveCruce] = (datoLinea);
                    }
                    else
                    {
                        Variables.Variables.DatosAuxAnexosVerdes.Add(llaveCruce, datoLinea);
                    }
                }
            }



            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDatosInsumo"></param>
        public static void GetCartasHipotecario(List<string> pArchivos)
        {
            #region GetAuxAnexosVerdes
            string[] campos; 
            foreach (var archivo in pArchivos)
            {
                List<string> datosInsumo = Helpers.ConvertirExcel(archivo);

                foreach (var datoLinea in datosInsumo)
                {
                    campos = datoLinea.Split('|');
                    if (campos.Length > 1)
                    {
                        string llaveCruce = campos[0];

                        if (Variables.Variables.DatosCartasHipotecario.ContainsKey(llaveCruce))
                        {
                            Variables.Variables.DatosCartasHipotecario[llaveCruce] = (datoLinea);
                        }
                        else
                        {
                            Variables.Variables.DatosCartasHipotecario.Add(llaveCruce, datoLinea);
                        }
                    }
                }
            }



            #endregion
        }

        #endregion

        /// <summary>
        /// Obtiene el tamaño del archivo
        /// </summary>
        /// <param name="pRutaArchivo">Ruta del Archivo</param>
        /// <returns>Tamaño</returns>
        public static Int64 GetTamañoArchivo(string pRutaArchivo)
        {
            #region GetTamañoArchivo
            Int64 tamañoArchivo = 0;

            FileInfo fileInfo = new FileInfo(pRutaArchivo);

            if (fileInfo.Exists)
            {
                tamañoArchivo = fileInfo.Length;
            }

            return tamañoArchivo;
            #endregion
        }

        /// <summary>
        /// Obtiene el Tamaño del ultimo corte por posicion
        /// </summary>
        /// <param name="tamañoUltimoCorte">Lista de campos UltimoCorte</param>
        /// <param name="pPosicion">Posicion</param>
        /// <returns>Tamaño</returns>
        public static Int64 GetTamañoHistorico(List<string> tamañoUltimoCorte, int pPosicion)
        {
            #region GetTamañoHistorico
            Int64 tamañoArchivo = 0;

            try
            {
                if (tamañoUltimoCorte.Count >= pPosicion)
                {
                    string pValor = tamañoUltimoCorte[pPosicion];

                    if (!string.IsNullOrEmpty(pValor))
                    {
                        tamañoArchivo = Convert.ToInt64(pValor);
                    }
                }

            }
            catch
            {
                tamañoArchivo = 0;
            }

            return tamañoArchivo;
            #endregion
        }

        /// <summary>
        /// Obtiene el Tamaño del ultimo corte por posicion
        /// </summary>
        /// <param name="tamañoUltimoCorte">Lista de campos UltimoCorte</param>
        /// <param name="pPosicion">Posicion</param>
        /// <returns>Tamaño</returns>
        public static Int32 GetTamañoHistoricoInt(List<string> tamañoUltimoCorte, int pPosicion)
        {
            #region GetTamañoHistoricoInt

            Int32 tamañoArchivo = 0;

            try
            {
                if (tamañoUltimoCorte.Count >= pPosicion)
                {
                    string pValor = tamañoUltimoCorte[pPosicion];

                    if (!string.IsNullOrEmpty(pValor))
                    {
                        tamañoArchivo = Convert.ToInt32(pValor);
                    }
                }

            }
            catch
            {
                tamañoArchivo = 0;
            }

            return tamañoArchivo;
            #endregion
        }

        /// <summary>
        /// Metodo para crear carpeta 
        /// </summary>
        /// <param name="ruta"></param>
        /// <returns>Ruta de la carpeta creada</returns>
        /// <exception cref="Exception"></exception>
        public static string CrearCarpeta(string ruta)
        {
            #region CrearCarpeta
            try
            {
                string carpeta = ruta;

                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                }

                return carpeta;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
            #endregion
        }

        /// <summary>
        /// Metodo que valida si hay Pipe seguidos o si se debe ageregar balnco al final de la cadena
        /// </summary>
        /// <param name="linea">String a verificar</param>
        /// <returns>linea con el blanco adicionado</returns>
        public static string ValidarPipePipe(string linea)
        {
            #region ValidarPipePipe

            linea = linea.Replace("||", "| |").Replace("||", "| |");

            if (linea != "")
            {
                if (linea.Last() == '|')
                {
                    return $"{linea} ";
                }
                else
                {
                    return linea;
                }
            }
            else
            {
                return linea;
            }

            #endregion
        }

        /// <summary>
        /// Metodo que remplaza un carcater en la linea enviada.
        /// </summary>
        /// <param name="caracterRemplazar">Caracter que se quiere remplazar.</param>
        /// <param name="caracterNuevo">Carcater con el que se quiere remplazar.</param>
        /// <param name="linea">Linea donde se va a hacer el remplazo.</param>
        /// <returns>Linea con carcater remplazado</returns>
        public static string RemplazarCaracteres(char caracterRemplazar, char caracterNuevo, string linea)
        {
            #region RemplazarCaracteres
            linea = linea.Replace(caracterRemplazar, caracterNuevo);
            return linea;
            #endregion

        }

        /// <summary>
        /// Metodo que hace trim a los campos de una linea separa por caracter especial.
        /// </summary>
        /// <param name="separador">caracter de separador</param>
        /// <param name="linea">linea de datos</param>
        /// <returns>linea con trim de campos</returns>
        public static string TrimCamposLinea(char separador, string linea)
        {
            #region TrimCamposLinea
            string resultado = string.Empty;
            List<string> lines = linea.Split(separador).ToList();

            foreach (string campo in lines)
            {
                if (!string.IsNullOrEmpty(resultado))
                { resultado += "|"; }

                resultado += $"{campo.Trim()}";
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo que extrae los campos de una linea de posiciones fijas y Formatea de acuerdo a Tipo
        /// </summary>
        /// <param name="posCortes">LIsta de Cortes</param>
        /// <param name="linea">Linea</param>
        /// /// <param name="pFormateoTipo">FormateoTipo</param>
        /// <returns></returns>
        public static string ExtraccionCamposSpool(List<PosCortes> posCortes, string linea)
        {
            #region ExtraccionCamposSpool
            string lineaResultado = string.Empty;
            string campo = string.Empty;
            foreach (PosCortes campos in posCortes)
            {
                if (!string.IsNullOrEmpty(lineaResultado))
                { lineaResultado += "|"; }

                if (campos.PosInicial != null && campos.Cantidad != null)
                {
                    try
                    {
                        if (campos.Cantidad > 0)
                        {
                            campo = linea.Substring(campos.PosInicial.Value, campos.Cantidad.Value).Trim();
                        }
                        else
                        {
                            campo = linea.Substring(campos.PosInicial.Value).Trim();
                        }

                        if (campos.FormateoTipo != null)
                        {
                            campo = FormatearCampos(campos.FormateoTipo, campo);
                        }
                    }
                    catch
                    {
                        campo = " ";
                    }
                }
                else
                {
                    campo = " ";
                }

                if (string.IsNullOrEmpty(campo))
                { campo = " "; }

                lineaResultado += campo;
            }

            return lineaResultado;
            #endregion

        }

        /// <summary>
        /// Metodo intermedio que indica como formatear Campo
        /// </summary>
        /// <param name="pFormateoTipo"></param>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        public static string FormatearCampos(TiposFormateo? pFormateoTipo, string pCampo)
        {
            switch (pFormateoTipo)
            {
                case TiposFormateo.Fecha01:
                    return FormatearFecha("01", pCampo); // De ddMMyy a dd/MM/yyyy

                case TiposFormateo.LetraCapital:
                    return FormatearLetraCapital(pCampo);

                case TiposFormateo.Fecha02:
                    return FormatearFecha("02", pCampo); // De ddMMyy a yyyyMM

                case TiposFormateo.Fecha03:
                    return FormatearFecha("03", pCampo); // De ddMMyy a yyyyMM

                case TiposFormateo.Fecha04:
                    return FormatearFecha("04", pCampo); // De MMdd a dd MMM (17 Jul)

                case TiposFormateo.Fecha05:
                    return FormatearFecha("05", pCampo); // De MMyyyy a yyyyMM

                case TiposFormateo.Fecha06:
                    return FormatearFecha("06", pCampo); // De yyyyMMdd a MMMM dd

                case TiposFormateo.Fecha07:
                    return FormatearFecha("07", pCampo); // De yyyyMMdd a MMM dd

                case TiposFormateo.Fecha08:
                    return FormatearFecha("08", pCampo); // De dd/MM/yyyy a yyyyMMdd

                case TiposFormateo.Fecha09:
                    return FormatearFecha("09", pCampo); // De dd/MM/yyyy a yyyyMMdd

                case TiposFormateo.Fecha10:
                    return FormatearFecha("10", pCampo); // De yyyyMMdd a yyyy/MM/dd                

                case TiposFormateo.Fecha11:
                    return FormatearFecha("11", pCampo); // De yyyyMMdd a yyyy/MM/dd

                case TiposFormateo.Fecha12:
                    return FormatearFecha("12", pCampo); // De yyyyMMdd a ddMMMyyyy

                case TiposFormateo.Fecha13:
                    return FormatearFecha("13", pCampo); // De yyyyMM a MMMM de yyyy

                case TiposFormateo.Fecha14:
                    return FormatearFecha("14", pCampo); // De yyyyMMdd a MMMM dd

                case TiposFormateo.Fecha15:
                    return FormatearFecha("15", pCampo); // De ddMMyyyy a ddMMMMyyyy

                case TiposFormateo.Fecha16:
                    return FormatearFecha("16", pCampo); // De ddMMyyyy a ddMMMMyyyy

                case TiposFormateo.Fecha17:
                    return FormatearFecha("17", pCampo); // De dd/MM/yyyy a yyyyMMdd - con padleft en mes y año

                case TiposFormateo.Decimal01:
                    return FormatearDecimal("01", pCampo);

                case TiposFormateo.Decimal02:
                    return FormatearDecimal("02", pCampo);

                case TiposFormateo.Decimal03:
                    return FormatearDecimal("03", pCampo);

                case TiposFormateo.Decimal04:
                    return FormatearDecimal("04", pCampo);

                case TiposFormateo.Decimal05:
                    return FormatearDecimal("05", pCampo);

                case TiposFormateo.HoraMinuto:
                    return FormatearHoraMinuto(pCampo);

                case TiposFormateo.HoraMinutoSegundo:
                    return FormatearHoraMinutoSegundo(pCampo);

                case TiposFormateo.Cadena01:
                    return FormatearCadena("01", pCampo);

                case TiposFormateo.Redondear:
                    return FormatearRedondeo("01", pCampo);
                default:
                    return pCampo;
            }
        }

        /// <summary>
        /// FormateaFecha
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private static string FormatearFecha(string pFormatoFechaTipo, string pCampo)
        {
            #region FormatearFecha
            string fechaRetorno = string.Empty;

            switch (pFormatoFechaTipo)
            {
                case "01":
                    return string.Format("{0}/{1}/{2}", pCampo.Substring(0, 2), pCampo.Substring(2, 2), pCampo.Substring(4, 4));

                case "02":
                    return string.Format("{0}{1}", pCampo.Substring(4, 4), pCampo.Substring(2, 2));

                case "03":
                    return string.Format("{0}{1}{2}", pCampo.Substring(4, 4), pCampo.Substring(2, 2), pCampo.Substring(0, 2));
                case "04":

                    Dictionary<string, string> dicMeses = new Dictionary<string, string>();

                    if (string.IsNullOrEmpty(pCampo.Trim()))
                    {
                        return string.Empty;
                    }

                    if (pCampo == "9999")
                    {
                        return "99";
                    }

                    dicMeses.Add("01", "Ene");
                    dicMeses.Add("02", "Feb");
                    dicMeses.Add("03", "Mar");
                    dicMeses.Add("04", "Abr");
                    dicMeses.Add("05", "May");
                    dicMeses.Add("06", "Jun");
                    dicMeses.Add("07", "Jul");
                    dicMeses.Add("08", "Ago");
                    dicMeses.Add("09", "Sep");
                    dicMeses.Add("10", "Oct");
                    dicMeses.Add("11", "Nov");
                    dicMeses.Add("12", "Dic");

                    string dia, Mes = string.Empty;
                    dia = pCampo.Substring(2, 2);
                    Mes = dicMeses[pCampo.Substring(0, 2)];

                    return string.Format("{0} {1}", dia, Mes);

                case "05":
                    return string.Format("{0}{1}", pCampo.Substring(2, 4), pCampo.Substring(0, 2));

                case "06":
                    fechaRetorno = Convert.ToDateTime($"{pCampo.Substring(0, 4)}/{pCampo.Substring(4, 2)}/{pCampo.Substring(6, 2)}").ToString("MMMM dd");

                    return FormatearCampos(TiposFormateo.LetraCapital, fechaRetorno);

                case "07":
                    fechaRetorno = Convert.ToDateTime($"{pCampo.Substring(0, 4)}/{pCampo.Substring(4, 2)}/{pCampo.Substring(6, 2)}").ToString("MMM dd").Replace(".", "");

                    return FormatearCampos(TiposFormateo.LetraCapital, fechaRetorno);

                case "08":
                    string[] camposFecha = pCampo.Split('/');
                    if (camposFecha.Length > 2)
                    {
                        return $"{camposFecha[2]}{camposFecha[1]}{camposFecha[0]}";
                    }
                    else
                    {
                        return string.Empty;
                    }
                case "10":

                    return $"{pCampo.Substring(0, 4)}/{pCampo.Substring(4, 2)}/{pCampo.Substring(6, 2)}";


                case "09":
                    return string.Format("{0}/{1}/{2}", pCampo.Substring(0, 4), pCampo.Substring(4, 2), pCampo.Substring(6, 2));

                case "11":
                    string[] campos = pCampo.Split(new string[] { "de" }, StringSplitOptions.None);
                    if (campos.Length > 2)
                    {
                        return $"{campos[0]}/{campos[1].ToLower()}/{campos[2]}";
                    }
                    else
                    {
                        return string.Empty;
                    }

                case "12":
                    fechaRetorno = Convert.ToDateTime($"{pCampo.Substring(0, 4)}/{pCampo.Substring(4, 2)}/{pCampo.Substring(6, 2)}").ToString("ddMMMyyyy").Replace(".", "");

                    return FormatearCampos(TiposFormateo.LetraCapital, fechaRetorno);

                case "13":

                    Dictionary<string, string> dicMes = new Dictionary<string, string>();

                    dicMes.Add("01", "Enero");
                    dicMes.Add("02", "Febrero");
                    dicMes.Add("03", "Marzo");
                    dicMes.Add("04", "Abril");
                    dicMes.Add("05", "Mayo");
                    dicMes.Add("06", "Junio");
                    dicMes.Add("07", "Julio");
                    dicMes.Add("08", "Agosto");
                    dicMes.Add("09", "Septembre");
                    dicMes.Add("10", "Octubre");
                    dicMes.Add("11", "Noviembre");
                    dicMes.Add("12", "Diciembre");

                    string año, mes = string.Empty;
                    año = pCampo.Substring(0, 4);
                    mes = dicMes[pCampo.Substring(4, 2)];

                    return string.Format("{0} de {1}", mes, año);

                case "14":

                    dicMes = new Dictionary<string, string>();

                    dicMes.Add("01", "Enero");
                    dicMes.Add("02", "Febrero");
                    dicMes.Add("03", "Marzo");
                    dicMes.Add("04", "Abril");
                    dicMes.Add("05", "Mayo");
                    dicMes.Add("06", "Junio");
                    dicMes.Add("07", "Julio");
                    dicMes.Add("08", "Agosto");
                    dicMes.Add("09", "Septembre");
                    dicMes.Add("10", "Octubre");
                    dicMes.Add("11", "Noviembre");
                    dicMes.Add("12", "Diciembre");

                    dia = string.Empty;
                    mes = string.Empty;
                    dia = pCampo.Substring(6, 2);
                    mes = dicMes[pCampo.Substring(4, 2)];

                    return string.Format("{0} {1}", mes, dia);

                case "15":

                    dicMes = new Dictionary<string, string>();

                    dicMes.Add("01", "Enero");
                    dicMes.Add("02", "Febrero");
                    dicMes.Add("03", "Marzo");
                    dicMes.Add("04", "Abril");
                    dicMes.Add("05", "Mayo");
                    dicMes.Add("06", "Junio");
                    dicMes.Add("07", "Julio");
                    dicMes.Add("08", "Agosto");
                    dicMes.Add("09", "Septembre");
                    dicMes.Add("10", "Octubre");
                    dicMes.Add("11", "Noviembre");
                    dicMes.Add("12", "Diciembre");

                    dia = string.Empty;
                    mes = string.Empty;
                    año = string.Empty;
                    dia = pCampo.Substring(0, 2);
                    mes = dicMes[pCampo.Substring(2, 2)];
                    año = pCampo.Substring(4, 4);
                    return string.Format("{0}{1}{2}", dia, mes, año);

                case "16":

                    dicMes = new Dictionary<string, string>();

                    dicMes.Add("01", "Enero");
                    dicMes.Add("02", "Febrero");
                    dicMes.Add("03", "Marzo");
                    dicMes.Add("04", "Abril");
                    dicMes.Add("05", "Mayo");
                    dicMes.Add("06", "Junio");
                    dicMes.Add("07", "Julio");
                    dicMes.Add("08", "Agosto");
                    dicMes.Add("09", "Septembre");
                    dicMes.Add("10", "Octubre");
                    dicMes.Add("11", "Noviembre");
                    dicMes.Add("12", "Diciembre");

                    dia = string.Empty;
                    mes = string.Empty;
                    año = string.Empty;
                    dia = pCampo.Substring(0, 2);
                    mes = dicMes[pCampo.Substring(2, 2).PadLeft(2, '0')];
                    año = pCampo.Substring(4, 4);
                    return string.Format("{0} {1} de {2}", mes, dia, año);

                case "17":
                    camposFecha = pCampo.Split('/');
                    if (camposFecha.Length > 2)
                    {
                        return $"{camposFecha[2]}{camposFecha[1].PadLeft(2, '0')}{camposFecha[0].PadLeft(2, '0')}";
                    }
                    else
                    {
                        return string.Empty;
                    }

                default:
                    return pCampo;
            }
            #endregion
        }

        /// <summary>
        /// FormateaFecha
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private static string FormatearDecimal(string pFormatoDecimalTipo, string pCampo)
        {
            string transformado = string.Empty;
            double temTransformado = 0;

            switch (pFormatoDecimalTipo)
            {
                case "01":
                    transformado = pCampo.Trim().TrimStart('0');

                    if (string.IsNullOrEmpty(transformado))
                    {
                        transformado = "00";
                    }

                    if (transformado.Contains("-"))
                    {
                        if (transformado.Replace("-", "").Length < 3)
                        {
                            transformado = transformado.Replace("-", "");
                            transformado = $"-{transformado.PadLeft(3, '0')}";
                        }
                    }
                    else
                    {
                        if (transformado.Length < 3)
                        {
                            transformado = $"{transformado.PadLeft(3, '0')}";
                        }
                    }

                    if (transformado.Length == 1)
                    {
                        transformado = transformado.PadLeft(2, '0');
                    }
                    transformado = $"{transformado.Substring(0, transformado.Length - 2)}.{transformado.Substring(transformado.Length - 2)}";
                    //var temTransformado = Convert.ToDouble(transformado);
                    temTransformado = Convert.ToDouble(transformado);

                    if (temTransformado < 0)
                    {
                        temTransformado = temTransformado * -1;
                        transformado = temTransformado.ToString("N2");
                        return $"-$ {transformado.Substring(0, transformado.LastIndexOf('.')).Replace(",", ".")},{transformado.Substring(transformado.LastIndexOf('.') + 1)}";
                    }
                    else
                    {
                        transformado = temTransformado.ToString("N2");
                        return $"$ {transformado.Substring(0, transformado.LastIndexOf('.')).Replace(",", ".")},{transformado.Substring(transformado.LastIndexOf('.') + 1)}"; ;
                    }

                case "02":
                    transformado = pCampo.Trim().TrimStart('0');

                    if (string.IsNullOrEmpty(transformado))
                    {
                        transformado = "00";
                    }

                    if (transformado.Length == 1)
                    {
                        transformado = transformado.PadLeft(2, '0');
                    }

                    transformado = $"{transformado.Substring(0, transformado.Length - 2)}.{transformado.Substring(transformado.Length - 2)}";

                    return transformado;

                case "03":
                    transformado = pCampo.Trim().TrimStart('0').Replace("$", string.Empty).Replace(",", string.Empty).Replace(".", string.Empty);

                    if (string.IsNullOrEmpty(transformado))
                    {
                        transformado = "00";
                    }

                    if (transformado.Length == 1)
                    {
                        transformado = transformado.PadLeft(2, '0');
                    }

                    transformado = $"{transformado.Substring(0, transformado.Length - 2)}.{transformado.Substring(transformado.Length - 2)}";

                    return transformado;

                case "04":

                    transformado = pCampo.Trim().TrimStart('0');

                    if (string.IsNullOrEmpty(transformado))
                    {
                        transformado = "00";
                    }

                    if (transformado.Length == 1)
                    {
                        transformado = transformado.PadLeft(2, '0');
                    }

                    transformado = $"{transformado.Substring(0, transformado.Length - 2)}.{transformado.Substring(transformado.Length - 2)}";
                    //var temTransformado = Convert.ToDouble(transformado);
                    temTransformado = Convert.ToDouble(transformado);

                    if (temTransformado < 0)
                    {
                        temTransformado = temTransformado * -1;
                        transformado = temTransformado.ToString("N2");
                        return $"- {transformado.Substring(0, transformado.LastIndexOf('.')).Replace(",", ".")},{transformado.Substring(transformado.LastIndexOf('.') + 1)}";
                    }
                    else
                    {
                        transformado = temTransformado.ToString("N2");
                        return $"{transformado.Substring(0, transformado.LastIndexOf('.')).Replace(",", ".")},{transformado.Substring(transformado.LastIndexOf('.') + 1)}"; ;
                    }

                case "05":

                    transformado = pCampo.Trim().TrimStart('0');

                    if (string.IsNullOrEmpty(transformado) || transformado.Contains("#"))
                    {
                        transformado = "00";
                    }

                    if (transformado.Contains("-"))
                    {
                        if (transformado.Replace("-", "").Length < 3)
                        {
                            transformado = transformado.Replace("-", "");
                            transformado = $"-{transformado.PadLeft(3, '0')}";
                        }
                    }
                    else
                    {
                        if (transformado.Length < 3)
                        {
                            transformado = $"{transformado.PadLeft(3, '0')}";
                        }
                    }

                    if (transformado.Length == 1)
                    {
                        transformado = transformado.PadLeft(2, '0');
                    }
                    transformado = $"{transformado.Substring(0, transformado.Length - 2)}.{transformado.Substring(transformado.Length - 2)}";
                    //var temTransformado = Convert.ToDouble(transformado);
                    temTransformado = Convert.ToDouble(transformado);

                    if (temTransformado < 0)
                    {
                        temTransformado = temTransformado * -1;
                        transformado = temTransformado.ToString("N2");
                        return $"-$ {transformado}";                        
                    }
                    else
                    {
                        transformado = temTransformado.ToString("N2");
                        return $"$ {transformado}";                        
                    }

                default:
                    return pCampo;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private static string FormatearLetraCapital(string pCampo)
        {
            #region FormatearLetraCapital
            string lower = string.Empty;
            string letraCapital = string.Empty;
            TextInfo myTextInfo = new CultureInfo("es-CO", false).TextInfo;

            lower = myTextInfo.ToLower(pCampo);
            letraCapital = myTextInfo.ToTitleCase(lower);

            return letraCapital;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private static string FormatearRedondeo(string pFormatoRedondeoTipo, string pCampo)
        {
            #region FormatearRedondeo
            string resultado = string.Empty;

            switch (pFormatoRedondeoTipo)
            {
                case "01":

                    if (pCampo.Substring(0, 1) == "-")
                    {
                        return string.Empty;
                    }

                    string temp = pCampo.Replace("$", string.Empty).Trim().Replace(".", string.Empty).Replace(",", ".");

                    decimal transformado = Convert.ToDecimal(temp) + 0.01m;

                    resultado = $"$ {Math.Round(transformado).ToString("N0")}".Replace(",", ".");
                    resultado += ",00";

                    break;
            }

            return resultado;
            #endregion
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private static string FormatearHoraMinuto(string pCampo)
        {
            #region FormatearHoraMinuto
            return String.Format("{0}:{1}", pCampo.Substring(0, 2), pCampo.Substring(2, 2));
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private static string FormatearCadena(string pFormatoCadenaTipo, string pCampo)
        {
            #region FormatearCadena
            switch (pFormatoCadenaTipo)
            {
                case "01":
                    #region FormatearCadena Tipo 01
                    string campo = pCampo;
                    while (campo.Contains("  "))
                    {
                        campo = campo.Replace("  ", " ");
                    }
                    return campo;
                #endregion

                default:
                    return string.Empty;


            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private static string FormatearHoraMinutoSegundo(string pCampo)
        {
            #region FormatearHoraMinuto
            return String.Format("{0}:{1}:{2}", pCampo.Substring(0, 2), pCampo.Substring(2, 2), pCampo.Substring(4, 2));
            #endregion
        }

        /// <summary>
        /// Metodo para tomar la fecha mas reciente
        /// </summary>
        /// <param name="ListaFechas"></param>
        /// <returns></returns>
        public static string GetFechaMasReciente(List<string> ListaFechas)
        {
            #region GetFechaMasReciente

            DateTime fechaUno, fechaDos;
            DateTime fechaUnoTem = new DateTime(), fechaDosTem = new DateTime();
            bool PrimeraVez = false;

            string fechaReciente = string.Empty;

            foreach (string valorActual in ListaFechas)
            {
                if (!string.IsNullOrEmpty(valorActual.Trim()) && valorActual.Contains("-"))
                {
                    fechaUno = Convert.ToDateTime(valorActual.Substring(0, 4) + "/" + valorActual.Substring(4, 2) + "/" + valorActual.Substring(6, 2));
                    fechaDos = Convert.ToDateTime(valorActual.Substring(11, 4) + "/" + valorActual.Substring(15, 2) + "/" + valorActual.Substring(17, 2));

                    if (PrimeraVez == false)
                    {
                        fechaUnoTem = fechaUno;
                        fechaDosTem = fechaDos;
                        fechaReciente = fechaUno.ToString("yyyyMMdd") + "-" + fechaDos.ToString("yyyyMMdd");
                        PrimeraVez = true;
                    }

                    if (DateTime.Compare(fechaUno, fechaUnoTem) >= 0 && DateTime.Compare(fechaDos, fechaDosTem) >= 0)
                    {
                        fechaReciente = fechaUno.ToString("yyyyMMdd") + "-" + fechaDos.ToString("yyyyMMdd");
                        fechaUnoTem = fechaUno;
                        fechaDosTem = fechaDos;
                    }
                }

            }

            return fechaReciente;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listaFechas"></param>
        /// <param name="accion">1 = Maxima -- 2 = Minima</param>
        /// <returns></returns>
        public static string GetFechaMaximaOMinima(List<string> listaFechas, int accion)
        {
            DateTime fecha;
            DateTime fechaTem = new DateTime();
            bool PrimeraVez = false;
            string fechaResultado = string.Empty;

            foreach (string registroActual in listaFechas)
            {
                if (!string.IsNullOrEmpty(registroActual.Trim()))
                {
                    fecha = Convert.ToDateTime(registroActual.Substring(0, 4) + "/" + registroActual.Substring(4, 2) + "/" + registroActual.Substring(6, 2));

                    if (PrimeraVez == false)
                    {
                        fechaTem = fecha;
                        PrimeraVez = true;
                    }

                    switch (accion)
                    {
                        // Fecha Maxima
                        case 1:
                            if (DateTime.Compare(fecha, fechaTem) >= 0)
                            {
                                fechaResultado = fecha.ToString("yyyyMMdd");
                                fechaTem = fecha;
                            }

                            break;

                        // Fecha Minima
                        case 2:
                            if (DateTime.Compare(fecha, fechaTem) <= 0)
                            {
                                var d = DateTime.Compare(fecha, fechaTem);
                                fechaResultado = fecha.ToString("yyyyMMdd");
                                fechaTem = fecha;
                            }

                            break;
                        default:
                            break;
                    }
                }

            }
            return fechaResultado;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listaFechas"></param>
        /// <param name="accion">1 = Maxima -- 2 = Minima</param>
        /// <returns></returns>
        public static string GetFechaMaximaMinima(List<string> listaFechas, int accion)
        {
            DateTime fecha;
            DateTime fechaTem = new DateTime();
            bool PrimeraVez = false;
            string fechaResultado = string.Empty;

            foreach (string registroActual in listaFechas)
            {
                if (!string.IsNullOrEmpty(registroActual.Trim()) && registroActual.Length == 8)
                {
                    fecha = Convert.ToDateTime(registroActual.Substring(0, 4) + "/" + registroActual.Substring(4, 2) + "/" + registroActual.Substring(6, 2));

                    if (PrimeraVez == false)
                    {
                        fechaTem = fecha;
                        PrimeraVez = true;
                    }

                    switch (accion)
                    {
                        // Fecha Maxima
                        case 1:
                            if (DateTime.Compare(fecha, fechaTem) >= 0)
                            {
                                fechaResultado = fecha.ToString("yyyyMMdd");
                                fechaTem = fecha;
                            }

                            break;

                        // Fecha Minima
                        case 2:
                            if (DateTime.Compare(fecha, fechaTem) <= 0)
                            {
                                var d = DateTime.Compare(fecha, fechaTem);
                                fechaResultado = fecha.ToString("yyyyMMdd");
                                fechaTem = fecha;
                            }

                            break;
                        default:
                            break;
                    }
                }

            }
            return fechaResultado;
        }

        public static bool GetContieneLetras(string pCampo)
        {
            if (!pCampo.Contains('-'))
            { return true; }
            else if (pCampo.Substring(9, 1) != "-")
            { return true; }
            else
            { return false; }
        }

        /// <summary>
        /// Completa una estructura con campos blancos
        /// </summary>
        /// <param name="linea">Liea</param>
        /// <param name="cantPosiciones"> Posiciones que se necesitan</param>
        /// <returns></returns>
        public static string CompletarEspaciosLinea(string linea, int cantPosiciones)
        {
            #region CompletarEspaciosLinea
            string lineaResultado = linea;
            for (int i = linea.Length; i < cantPosiciones; i++)
            {
                lineaResultado += " ";
            }


            return lineaResultado;
            #endregion

        }

        /// <summary>
        /// Da formato de monea a campo
        /// </summary>
        /// <param name="pCampo">Campo</param>
        /// <param name="pPosDecimales">Cantidad Decimales</param>
        /// <returns>Campo Formateado</returns>
        public static string StringToMoneda(string pCampo, int pPosDecimales)
        {
            #region StringToMoneda
            string resultado = pCampo;
            try
            {
                if (string.IsNullOrEmpty(pCampo))
                {
                    pCampo = "0";
                }
                NumberFormatInfo nfi = new CultureInfo(CultureInfo.InvariantCulture.Name, false).NumberFormat;
                nfi.NumberDecimalSeparator = ".";
                nfi.NumberDecimalDigits = pPosDecimales;
                nfi.CurrencyDecimalDigits = pPosDecimales;
                nfi.CurrencySymbol = String.Empty;

                decimal Valor = Convert.ToDecimal(pCampo);
                resultado = Valor.ToString("C", nfi);

            }
            catch (Exception)
            {
                resultado = pCampo;
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Retorna una lidea a partir de una lista de campos
        /// </summary>
        /// <param name="campos">Lista Campos</param>
        /// <param name="separador">Separador</param>
        /// <returns>Linea</returns>
        public static string ListaCamposToLinea(List<string> campos, char separador)
        {
            #region ListaCamposToLinea
            string resultado = string.Empty;

            foreach (string campo in campos)
            {
                if (!string.IsNullOrEmpty(resultado))
                { resultado += separador; }

                if (string.IsNullOrEmpty(campo.Trim()))
                {
                    resultado += $" ";
                }
                else
                {
                    resultado += $"{campo.Trim()}";
                }
            }

            return resultado;
            #endregion
        }

        /// <summary>
        /// Metodo para copiar archivo filtrando por extensión
        /// </summary>
        /// <param name="RutaEntrada">Ruta carpeta original</param>
        /// <param name="Extension">Extension a filtrar</param>
        /// <param name="RutaSalida">Ruta a copiar los archivos</param>
        public static void MoverArchivosExtension(string RutaEntrada, string Extension, string RutaSalida)
        {
            #region Mover Archivos
            try
            {
                foreach (var _Archivo in Directory.GetFiles(RutaEntrada, Extension))
                {
                    File.Copy(_Archivo, RutaSalida + "\\" + Path.GetFileName(_Archivo));
                }
            }
            catch (Exception ex)
            {
                Utilidades.EscribirLog(ex.Message, Utilidades.LeerAppConfig("RutaLog"));
                throw;
            }
            #endregion
        }


        /// <summary>
        /// Metodo para Mover archivos de una carpeta a otra filtrada por extensión
        /// </summary>
        /// <param name="RutaEntrada">Ruta carpeta original</param>
        /// <param name="Extension">Extension a filtrar</param>
        /// <param name="RutaSalida">Ruta a mover los archivos</param>
        public static void CortarMoverArchivosExtension(string RutaEntrada, string Extension, string RutaSalida)
        {
            #region Mover Archivos
            try
            {
                foreach (var _Archivo in Directory.GetFiles(RutaEntrada, Extension))
                {
                    File.Move(_Archivo, RutaSalida + "\\" + Path.GetFileName(_Archivo));
                }
            }
            catch (Exception ex)
            {
                Utilidades.EscribirLog(ex.Message, Utilidades.LeerAppConfig("RutaLog"));
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Metodo para mover archivos de una carpeta a otra bajo condicinados
        /// </summary>
        /// <param name="RutaEntrada">Ruta carpeta original</param>
        /// <param name="Extension">Extension a filtrar</param>
        /// <param name="RutaSalida">Ruta a mover los archivos</param>
        /// <param name="CondicionNomre">Condicion nombre</param>
        /// <param name="pNombreFinal">Nombre Final Archivo</param>
        public static void MoverArchivosCondicionados(string RutaEntrada, string Extension, string RutaSalida, string CondicionNomre, string pNombreFinal)
        {
            #region Mover Archivos
            try
            {
                foreach (var _Archivo in Directory.GetFiles(RutaEntrada, Extension))
                {
                    if (Path.GetFileNameWithoutExtension(_Archivo).Contains(CondicionNomre))
                    {
                        File.Move(_Archivo, RutaSalida + "\\" + Path.GetFileName(_Archivo));
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Utilidades.EscribirLog(ex.Message, Utilidades.LeerAppConfig("RutaLog"));
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Metodo para mover archivos de una carpeta a otra
        /// </summary>
        /// <param name="RutaEntrada">Ruta carpeta original</param>        
        /// <param name="RutaSalida">Ruta a mover los archivos</param>
        public static void MoverArchivos(string RutaEntrada, string RutaSalida)
        {
            #region Mover Archivos
            try
            {
                foreach (var _Archivo in Directory.GetFiles(RutaEntrada))
                {
#if DEBUG == false
                    File.Copy(_Archivo, RutaSalida + "\\" + Path.GetFileName(_Archivo));
#endif
                    File.Copy(_Archivo, RutaSalida + "\\" + Path.GetFileName(_Archivo));
                }
            }
            catch (Exception ex)
            {
                Utilidades.EscribirLog(ex.Message, Utilidades.LeerAppConfig("RutaLog"));
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Metodo para escribir en archivo plano
        /// </summary>
        /// <param name="ruta">Ruta del archivo</param>
        /// <param name="listado">Lista string a escribir</param>
        public static void EscribirEnArchivo(string ruta, List<string> listado)
        {
            #region EscribirEnArchivo
            if (File.Exists(ruta))
            {
                using (StreamWriter streamWriter = new StreamWriter(ruta, true, Encoding.Default))
                {
                    foreach (var item in listado)
                    {
                        streamWriter.WriteLine(item);
                    }
                }
            }
            else
            {
                FileStream escritor = File.Create(ruta);

                using (StreamWriter streamWriter = new StreamWriter(escritor, Encoding.Default))
                {
                    foreach (var item in listado)
                    {
                        streamWriter.WriteLine(item);
                    }
                }

                escritor.Close();
            }
            #endregion
        }

        /// <summary>
        /// Metodo para escribir en el Log y la ventan ade ejecucion // Dependiendo el error se cierra la aplicacion
        /// </summary>
        /// <param name="strucDatosError">Estructura de Datos error</param>
        /// <param name="finalizaProceso">Bandera para finalizar proceso // True = Cierra - False = Continua </param>
        public static void EscribirLogVentana(DatosError strucDatosError, bool finalizaProceso = false)
        {
            #region EscribirLogVentana
            string Error = $"Clase: {strucDatosError.Clase} -|- Metodo:{strucDatosError.Metodo} -|- linea Error: {strucDatosError.LineaError} -|- Mensaje: {strucDatosError.Error}";

            if (!string.IsNullOrEmpty(strucDatosError.Error))
            {
                Console.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Error));
                Utilidades.EscribirLog(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Error), Utilidades.LeerAppConfig("RutaLog"));
            }

            if (finalizaProceso)
            {
                Console.WriteLine("Existe un problema en la ejecucion revise el log y de ser necesario comuniquelo al ingeniero a cargo");
                Console.WriteLine("Presione una tecla para cerrar...");
                Console.ReadKey();
                Environment.Exit(1);
            }
            #endregion
        }

        /// <summary>
        /// Metodo creado para escribir en el Log y le Ventana de ejecucion
        /// </summary>
        /// <param name="Mensaje">Mensaje a escribir</param>
        public static void EscribirVentanaLog(string Mensaje)
        {
            #region EscribirVentanaLog
            Console.ResetColor();
            Console.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Mensaje));
            Utilidades.EscribirLog(Mensaje, Utilidades.LeerAppConfig("RutaLog"));
            #endregion
        }

        /// <summary>
        /// Metodo creado para escribir en la Ventana de ejecucion
        /// </summary>
        /// <param name="Mensaje">Mensaje a escribir</param>
        public static void EscribirVentanaMismaLinea(string Mensaje, bool fecha = true)
        {
            #region EscribirVentanaLog
            if (fecha)
            {
                Console.ResetColor();
                Console.Write(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Mensaje));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(Mensaje);
            }

            #endregion
        }

        /// <summary>
        /// Metodo para escribir el usuario en el Log
        /// </summary>
        /// <param name="usuario">Usuario que ejecuta la aplicación</param>
        public static void EscribirLogUsuario(string usuario)
        {
            Utilidades.EscribirLog("*** Nuevo proceso ejecutado por: " + usuario, Utilidades.LeerAppConfig("RutaLog"));
        }

        /// <summary>
        /// Metodo encargado de descomprimir archivos, en este caso guias.
        /// </summary>
        /// <param name="archivos">Archivos a descomprimir</param>
        public static void DescomprimirGuias(string[] archivos)
        {
            #region Descomprimir Archivos
            foreach (string archivo in archivos)
            {
                string extension = Path.GetExtension(archivo);
                string nombre = Path.GetFileNameWithoutExtension(archivo);

                if (nombre == null || (extension == null) ||
                                       (extension.ToLower() != ".rar"))
                    continue;
                string ruta = archivo;

                IArchive iArchivo = ArchiveFactory.Open(ruta);

                ExtractionOptions opcionesDeExtraccion = new ExtractionOptions { Overwrite = true };

                foreach (IArchiveEntry item in iArchivo.Entries)
                {
                    if (!item.IsDirectory)
                    {
                        item.WriteToFile(Path.GetDirectoryName(archivo) + "\\" + nombre.Replace(".rar", ""), opcionesDeExtraccion);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Metodo para cargar las Guias
        /// </summary>
        /// <param name="rutaGuias"></param>
        /// <param name="poscicion"></param>
        /// <param name="canal"></param>
        public static void CargarGuias(string[] rutaGuias, int poscicion, string canal)
        {
            #region CargarGuias
            foreach (string archivo in rutaGuias)
            {
                string nombreArchivo = Path.GetFileNameWithoutExtension(archivo);
                string extension = Path.GetExtension(archivo);

                if (nombreArchivo != null && (extension != null && (extension.ToLower() == ".sal" && nombreArchivo.Contains("guias"))))
                {
                    Variables.Variables.Lector = new StreamReader(archivo, Encoding.Default);
                    Dictionary<string, string> dicGuiasTemp = new Dictionary<string, string>();
                    string Linea = string.Empty;
                    string[] Separador = null;

                    while ((Linea = Variables.Variables.Lector.ReadLine()) != null)
                    {
                        if (Linea.Substring(0, 4) == canal)
                        {
                            Separador = Linea.Split('|');

                            if (!dicGuiasTemp.ContainsKey(Separador[poscicion].Trim()))
                            {
                                if (Separador[poscicion].Trim() == "")
                                {
                                    if (!dicGuiasTemp.ContainsKey(Separador[poscicion].Trim()))
                                    {
                                        dicGuiasTemp.Add(Separador[poscicion].Trim(), Separador[1]);
                                    }
                                }
                                else
                                {
                                    dicGuiasTemp.Add(Separador[poscicion].Trim(), Separador[1]);
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (!Variables.Variables.DicGuias.ContainsKey(nombreArchivo))
                    {
                        Variables.Variables.DicGuias.Add(nombreArchivo, dicGuiasTemp);
                    }

                    Variables.Variables.Lector.Close();
                }
            }
            #endregion
        }

        /// <summary>
        /// Metodo encargado de mover un archivo a una carpeta especifica
        /// </summary>
        /// <param name="rutaInsumoActual">Ruta archivo a mover</param>
        /// <param name="nuevaRutaDirectorioInsumo">Ruta cerpeta donde se va a mover el archivo</param>
        /// <param name="nombreInsumo">Nombre del archivo</param>
        public static void MoverArchivoaCarpeta(string rutaInsumoActual, string nuevaRutaDirectorioInsumo, string nombreInsumo)
        {
            #region MoverArchivoaCarpeta
            if (!Directory.Exists(nuevaRutaDirectorioInsumo))
            {
                Directory.CreateDirectory(nuevaRutaDirectorioInsumo);
            }

            File.Move(rutaInsumoActual, $@"{nuevaRutaDirectorioInsumo}\{nombreInsumo}");
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDirectorioOrigen"></param>
        /// <param name="pDirectorioDestino"></param>
        /// <param name="pRecursive"></param>
        public static void CopiarCarpetaCompleta(string pDirectorioOrigen, string pDirectorioDestino, bool pRecursive)
        {
            #region MoverArchivoaCarpeta
            // Get information about the source directory
            var dir = new DirectoryInfo(pDirectorioOrigen);

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(pDirectorioDestino);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(pDirectorioDestino, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (pRecursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(pDirectorioDestino, subDir.Name);
                    CopiarCarpetaCompleta(subDir.FullName, newDestinationDir, true);
                }
            }
            #endregion
        }

        /// <summary>
        /// Metodo para sumar varios numeros
        /// </summary>
        /// <param name="pCamposSumar">Lista de campos a Sumar</param>
        /// <returns>string formateado con la suma d elos valores</returns>
        public static string SumarCampos(List<string> pCamposSumar, string proceso = "M")
        {
            #region SumarCampos
            double totalSuma = 0;
            string numeroSuma = string.Empty;

            if (pCamposSumar.Count > 0)
            {
                foreach (string numeroActual in pCamposSumar)
                {
                    numeroSuma = numeroActual;

                    if (string.IsNullOrEmpty(numeroSuma))
                    {
                        numeroSuma = "0";
                    }

                    string transformado = numeroSuma.Replace("$", "").Replace(" ", "");
                    var temTransformado = Convert.ToDouble(transformado);
                    totalSuma += temTransformado;
                }
            }
            string ValorSumadoFormateado = string.Empty;

            switch (proceso)
            {
                case "M": // Masivo
                    ValorSumadoFormateado = FormatearCampos(TiposFormateo.Decimal01, totalSuma.ToString());
                    break;

                case "D": // Datos
                case "G": // Gobierno
                    ValorSumadoFormateado = FormatearCampos(TiposFormateo.Decimal05, totalSuma.ToString());
                    break;

                default:
                    break;
            }
            
            return ValorSumadoFormateado;
            #endregion
        }

        /// <summary>
        /// Metodo para sumar varios numeros
        /// </summary>
        /// <param name="pCamposSumar">Lista de campos a Sumar</param>
        /// <returns>string formateado con la suma d elos valores</returns>
        public static string SumarCamposLinea(string pLineaSumar, char delimitador)
        {
            #region SumarCampos
            string resultado = string.Empty;
            double totalSuma = 0;
            string numeroSuma = string.Empty;
            List<string> camposSumar = new List<string>();

            if (!string.IsNullOrEmpty(pLineaSumar))
            {
                camposSumar = pLineaSumar.Split(delimitador).ToList();

                if (camposSumar.Count > 0)
                {
                    foreach (string numeroActual in camposSumar)
                    {
                        numeroSuma = numeroActual;

                        if (string.IsNullOrEmpty(numeroSuma))
                        {
                            numeroSuma = "0";
                        }

                        string transformado = numeroSuma.Replace("$", "").Replace(" ", "");
                        var temTransformado = Convert.ToDouble(transformado);
                        totalSuma += temTransformado;
                    }
                }

            }

            resultado = FormatearCampos(TiposFormateo.Decimal01, totalSuma.ToString());

            return resultado;
            #endregion
        }

        /// Obtienen los camposenviados en la lista de identificadores
        /// </summary>
        /// <param name="datosOriginales">Lista de datos del extracto</param>
        /// <param name="pIdentificadores">Diccionario Int => caracteres del indentificador, String texto identificador</param>
        public static string GetCamposLlanos(List<string> pDatosOriginales, List<CamposLLanos> pIdentificadores)
        {
            string resultado = string.Empty;
            string campo = string.Empty;
            foreach (CamposLLanos item in pIdentificadores)
            {
                campo = GetCampoLLanos(pDatosOriginales, item.CantCaracteres, item.Identificador, item.ValorDefault).Trim();

                if (item.FormateoTipo != null)
                {
                    campo = FormatearCampos(item.FormateoTipo, campo);
                }

                if (string.IsNullOrEmpty(resultado))
                {
                    resultado += campo;
                }
                else
                {

                    resultado += $"|{campo}";
                }
            }


            return ValidarPipePipe(resultado);
        }

        public static string GetCampoLLanos(List<string> datosOriginales, int pCantidadCaracteres, string pIdentificador, string pValorDefault = null)
        {
            string resultado = string.Empty;
            var campo = from busqueda in datosOriginales
                        where busqueda.Length > pCantidadCaracteres && busqueda.Substring(0, pCantidadCaracteres).Equals(pIdentificador)
                        select busqueda;

            if (campo.Any())
            {
                resultado = campo.FirstOrDefault()?.Substring(pCantidadCaracteres) ?? string.Empty;
            }

            if (string.IsNullOrEmpty(resultado.Trim()) && (!string.IsNullOrEmpty(pValorDefault)))
            {
                resultado = pValorDefault;
            }

            return resultado;
        }

        public static List<string> GetListaCampoLLanos(List<string> datosOriginales, int pCantidadCaracteres, string pIdentificador)
        {
            List<string> resultado = new List<string>();
            var campos = from busqueda in datosOriginales
                         where busqueda.Length > pCantidadCaracteres && busqueda.Substring(0, pCantidadCaracteres).Equals(pIdentificador)
                         select busqueda;

            if (campos.Any())
            {
                foreach (var item in campos.ToList())
                {
                    resultado.Add(item.Substring(pCantidadCaracteres));
                }
            }

            return resultado;
        }

        /// <summary>
        /// Metodo para convertir Excel (.xlsx - .xls) en archivo plano
        /// </summary>
        /// <param name="archivo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<string> ConvertirExcel(string archivo)
        {
            #region ConvertirExcel
            DataSet result = new DataSet();
            try
            {
                #region Leer Excel
                if (archivo.EndsWith(".xlsx"))
                {
                    //Reading from a binary Excel file(format; .xlsx)
                    FileStream stream = File.Open(archivo, FileMode.Open, FileAccess.Read);
                    IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    result = excelReader.AsDataSet();
                    excelReader.Close();
                }

                if (archivo.EndsWith(".xls"))
                {
                    //Reading from a binary Excel file('97-2003 format; .xls)
                    FileStream stream = File.Open(archivo, FileMode.Open, FileAccess.Read);
                    IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                    result = excelReader.AsDataSet();
                    excelReader.Close();
                }

                #endregion

                #region Pasar Excel a Plano
                List<string> datosExcel = new List<string>();
                int numHoja = Convert.ToInt32(0);
                string a = "";

                for (int j = 0; j < result.Tables[numHoja].Rows.Count; j++)
                {
                    for (int i = 0; i < result.Tables[numHoja].Columns.Count; i++)
                    {
                        if (result.Tables[numHoja].Rows[j][i].ToString() != "")
                        {
                            a += result.Tables[numHoja].Rows[j][i].ToString().Replace("\n", " ");
                        }
                        else
                        {
                            a += " ";
                        }

                        if (i < (result.Tables[numHoja].Columns.Count + 1))
                        {
                            a += "|";
                        }
                    }

                    datosExcel.Add(a);
                    a = "";

                }
                #endregion

                return datosExcel;
            }
            catch (Exception mens)
            {
                throw new Exception("Error: " + mens.Message);
            }
            #endregion
        }

        /// <summary>
        /// Metodo que comprueba si una fecha es festiva o Dominical
        /// </summary>
        /// <param name="pFestivos"></param>
        /// <param name="pFechaComporbar"></param>
        /// <param name="pComprarDomingo"></param>
        /// <returns>True es Festiva, false no es festiva</returns>
        public static bool EsFestivo(List<DateTime> pFestivos, DateTime pFechaComporbar, bool pComprarDomingo = true)
        {
            #region EsFestivo
            if (pFechaComporbar != null)
            {
                if (pFechaComporbar.DayOfWeek == DayOfWeek.Sunday)
                {
                    return true;
                }
                else
                {

                    if (pFestivos.Contains(pFechaComporbar))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            } 
            #endregion
        }

        /// <summary>
        /// Metodo que obtiene el siguiente dia habil
        /// </summary>
        /// <param name="pFechaComporbar"></param>
        /// <returns>proximo dia habil</returns>
        public static DateTime GetSiguienteDiaHabil(DateTime pFechaComporbar)
        {
            #region GetSiguienteDiaHabil
            DateTime fechaResult = pFechaComporbar;
            
            List<DateTime> festivos = ControlInsumos.FestivosColombia.DiasFestivos(pFechaComporbar.Year);

            while (EsFestivo(festivos, pFechaComporbar))
            {
                fechaResult = fechaResult.AddDays(1);
                pFechaComporbar = fechaResult;
            }

            return fechaResult; 
            #endregion

        }
    }

    public struct PosCortes
    {
        public Int32? PosInicial;
        public Int32? Cantidad;
        public TiposFormateo? FormateoTipo;

        public PosCortes(Int32 posInicial, Int32 cantidad)
        {
            this.PosInicial = posInicial;
            this.Cantidad = cantidad;
            this.FormateoTipo = null;
        }

        public PosCortes(Int32? posInicial, Int32? cantidad)
        {
            this.PosInicial = posInicial;
            this.Cantidad = cantidad;
            this.FormateoTipo = null;
        }

        public PosCortes(Int32 posInicial, Int32 cantidad, TiposFormateo pFormateoTipo)
        {
            this.PosInicial = posInicial;
            this.Cantidad = cantidad;
            this.FormateoTipo = pFormateoTipo;
        }

        public PosCortes(Int32? posInicial, Int32? cantidad, TiposFormateo pFormateoTipo)
        {
            this.PosInicial = posInicial;
            this.Cantidad = cantidad;
            this.FormateoTipo = pFormateoTipo;
        }
    }

    public struct CamposLLanos
    {
        public Int32 CantCaracteres;
        public string Identificador;
        public string ValorDefault;
        public TiposFormateo? FormateoTipo;

        public CamposLLanos(Int32 pCantCaracteres, string pIdentificador)
        {
            this.CantCaracteres = pCantCaracteres;
            this.Identificador = pIdentificador;
            this.FormateoTipo = null;
            this.ValorDefault = null;
        }

        public CamposLLanos(Int32 pCantCaracteres, string pIdentificador, TiposFormateo pFormateoTipo)
        {
            this.CantCaracteres = pCantCaracteres;
            this.Identificador = pIdentificador;
            this.FormateoTipo = pFormateoTipo;
            this.ValorDefault = null;
        }

        public CamposLLanos(Int32 pCantCaracteres, string pIdentificador, string pValorDefault)
        {
            this.CantCaracteres = pCantCaracteres;
            this.Identificador = pIdentificador;
            this.FormateoTipo = null;
            this.ValorDefault = pValorDefault;
        }
    }

    /// <summary>
    /// Structura de datos Error para las Exception
    /// </summary>
    public struct DatosError
    {
        public string Metodo;
        public string Clase;
        public string Error;
        public int? LineaError;
    }

    public enum TiposFormateo
    {
        Fecha01,
        Fecha02,
        Fecha03,
        Fecha04,
        Fecha05,
        Fecha06,
        Fecha07,
        Fecha08,
        Fecha09,
        Fecha10,
        Fecha11,
        Fecha12,
        Fecha13,
        Fecha14,
        Fecha15,
        Fecha16,
        Fecha17,
        LetraCapital,
        Decimal01,
        Decimal02,
        Decimal03,
        Decimal04,
        Decimal05,
        HoraMinuto,
        HoraMinutoSegundo,
        Cadena01,
        Redondear
    }
}
