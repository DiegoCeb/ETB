using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using DLL_Utilidades;
using SharpCompress.Archives;
using SharpCompress.Readers;
using SharpCompress.Common;

namespace App.ControlInsumos
{
    /// <summary>
    /// Clase Helpers
    /// </summary>
    public static class Helpers
    {
        public static string RutaProceso { get; set; }
        public static string RutaOriginales { get; set; }
        public static string RutaInsumos{ get; set; }

        private static dynamic CargueDinamicoInsumos<TEntity>(string pIdentificadorInsumo, string pRutaInsumo, TEntity pObjEntradaRetorno)
        {
            //TODO: Catch Recursivo cuando el archivo esta siendo utilizado en otro proceso con limite de reintentos y 3 segundos de espera
            //TODO: Finally limpiar lista de datos

            var newObject = pObjEntradaRetorno.GetType();

            if (Enum.IsDefined(typeof(Variables.Insumos), pIdentificadorInsumo))
            {
                #region Logica Busqueda Insumos Configurados
                List<string> DatosInsumo = File.ReadAllLines(pRutaInsumo, Encoding.Default).ToList();

                if (pIdentificadorInsumo == Variables.Insumos.ExcluirServiciosAdicionales.ToString())
                {
                    #region Variables.Insumos.ExcluirServiciosAdicionales
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                    #endregion
                }
                else if (pIdentificadorInsumo == Variables.Insumos.ServiciosAdicionalesTV.ToString())
                {
                    #region Variables.Insumos.ServiciosAdicionalesTV
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("DescripcionServicio").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);

                    return newObject.GetProperty("DescripcionServicio").GetValue(pObjEntradaRetorno);
                    #endregion
                }
                else if (pIdentificadorInsumo == Variables.Insumos.VelocidadFibra.ToString())
                {
                    #region Variables.Insumos.VelocidadFibra
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                    #endregion
                }
                else if (pIdentificadorInsumo == Variables.Insumos.ConformacionPaquetes.ToString())
                {
                    #region Variables.Insumos.ConformacionPaquetes
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("DescripcionPaquete").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);

                    return newObject.GetProperty("DescripcionPaquete").GetValue(pObjEntradaRetorno);
                    #endregion
                }
                else if (pIdentificadorInsumo == Variables.Insumos.ParametrizacionPaquetesFibra.ToString())
                {
                    #region Variables.Insumos.ParametrizacionPaquetesFibra
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(1))
                                 select datos;

                    newObject.GetProperty("DescripcionTipo").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);

                    return newObject.GetProperty("DescripcionTipo").GetValue(pObjEntradaRetorno);
                    #endregion
                }
                else if (pIdentificadorInsumo == Variables.Insumos.cuentas_Envio_SMS.ToString())
                {
                    #region Variables.Insumos.cuentas_Envio_SMS
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos)
                                 select datos;

                    newObject.GetProperty("CuentaSms").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);

                    return newObject.GetProperty("CuentaSms").GetValue(pObjEntradaRetorno);
                    #endregion
                }
                else if (pIdentificadorInsumo == Variables.Insumos.Codigos_Univer_SVAS.ToString())
                {
                    #region Variables.Insumos.Codigos_Univer_SVAS
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(1))
                                 select datos;

                    newObject.GetProperty("DescripcionCodigo").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);

                    return newObject.GetProperty("DescripcionCodigo").GetValue(pObjEntradaRetorno);
                    #endregion
                }
                else if (pIdentificadorInsumo == Variables.Insumos.Cuentas_SVAS_FueradeBundle.ToString())
                {
                    #region Variables.Insumos.Cuentas_SVAS_FueradeBundle
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("DescripcionCiclo").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);

                    return newObject.GetProperty("DescripcionCiclo").GetValue(pObjEntradaRetorno);
                    #endregion
                }
                else if (pIdentificadorInsumo == Variables.Insumos.doc1tsub.ToString())
                {
                    #region Variables.Insumos.doc1tsub

                    IEnumerable<string> result = null;

                    #region Segmentos Busqueda
                    switch (newObject.GetProperty("IdentificardorBusqueda").GetValue(pObjEntradaRetorno))
                    {
                        case "1": // ACTR ACTIVIDAD REVCHAIN
                        case "2": // TASM TASAS DE MORA POR ACTIVIDAD
                        case "3": // CSERCENTROS DE SERVICIO POR DEPARTAMENTO
                        case "4": // DANCCODIGOS DANE POR MUNICIPIO
                        case "5": // DE216
                            result = from datos in DatosInsumo
                                     where datos.Length > 6
                                     let comp = datos.Substring(0, 6).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "6": // CODLCODIGOS DE LOCALIDAD
                            result = from datos in DatosInsumo
                                     where datos.Length > 7
                                     let comp = datos.Substring(0, 7).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "7": // CODIGO DE BARRASCODBAR32
                        case "8": // TASNTASA DE MORA ANTERIOR
                        case "9": // TSERTIPOS DE SERVICIO
                        case "10": // SSUPSERVICIOS SUPLEMENTARIOS EMPAQUETADOS
                        case "11": // CODSCODIGOS DE SUBOPERADOR
                        case "12": // FACLINLíneaS DE NEGOCIO PARA FACTURA
                        case "13": // ZPOSZONAS POSTALES PARA PUNTOS DE PAGO
                        case "14": // MDEPLPLAN FD MDE
                            result = from datos in DatosInsumo
                                     where datos.Length > 8
                                     let comp = datos.Substring(0, 8).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "15": // DANDCODIGOS DANE DE DEPARTAMENTOS
                        case "16": // CLTFCLASE DE TARIFA DE MULTIMEDICION POR CODIGO DANE
                        case "17": // NRATNUMERO DE ATENCION TELEFONICA POR CODIGO DANE
                        case "18": // PPPUNTO DE PAGO CUNDINAMARCA POR CUENTA
                            result = from datos in DatosInsumo
                                     where datos.Length > 9
                                     let comp = datos.Substring(0, 9).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "19": // FACPRODPRODUCTOS PARA FACTURA
                        case "20": // CZONADIRECCION CENTROS DE SERVICIO POR ZONA
                            result = from datos in DatosInsumo
                                     where datos.Length > 10
                                     let comp = datos.Substring(0, 10).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "21": // CARRDIRECCION CENTROS DE SERVICIO POR LSP
                        case "22": // NCARNOMBRE DE LSPS
                        case "23": // CODTOTALES DE FACTURACION :::: etapa1
                        case "24": // OPERNUMERO DE NIT DE OPERADORES
                        case "25": // CBSUBCONSUMO BASICO DE SUBSISTENCIA
                            result = from datos in DatosInsumo
                                     where datos.Length > 11
                                     let comp = datos.Substring(0, 11).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "26": // FECPFECHA DE PAGO NORMAL
                        case "27": // FECLFECHA ULTIMA DE PAGO EXTEMPORANEO
                        case "28": // FECXFECHA DE EXPEDICION DE LA FACTURA
                        case "29": // PLPLANES REVCHAIN DE LOCAL 
                        case "30": // XC PL BA IC LD DV DELL 
                        case "31": // SVSERVICIOS PLANES REVCHAIN DE LOCAL
                        case "32": // CODXCODIGOS DE BARRIOS
                        case "33": // CJURI1EMPRESAS DE COBRO JURIDICO POR MES
                        case "34": // CJURI2DIRECCION EMPRESAS DE COBRO JURIDICO POR MES
                        case "35": // PERIODOS DE CORTES SERVICIO LTE
                            result = from datos in DatosInsumo
                                     where datos.Length > 12
                                     let comp = datos.Substring(0, 12).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;


                        case "36": // obsoletos segun requerimiento No 142
                            result = from datos in DatosInsumo
                                     where datos.Length > 13
                                     let comp = datos.Substring(0, 13).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "37": // REICOSTOS DE REINSTALACION POR PLAN DE VOZ LOCAL
                        case "38": // CODFCODIGOS DE FACTURACION
                        case "39": // MINCMINUTOS LOCALES INCLUIDOS POR PLAN DE LOCAL   --FABICORA, ACTUALIADO AL 19 DE NOV DE 2012
                            result = from datos in DatosInsumo
                                     where datos.Length > 14
                                     let comp = datos.Substring(0, 14).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "40": // NPRPLANNOMBRE PLANES NPR
                            result = from datos in DatosInsumo
                                     where datos.Length > 15
                                     let comp = datos.Substring(0, 15).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "41": // CODDATCODIGOS DE DATOS
                            result = from datos in DatosInsumo
                                     where datos.Length > 16
                                     let comp = datos.Substring(0, 16).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "42": // NPRVALVALORES PLANES NPR
                            result = from datos in DatosInsumo
                                     where datos.Length > 17
                                     let comp = datos.Substring(0, 17).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "43": // BALOTO DANE LOC BAR    --CADENA PARA INCLUIR EL PUNTO BALOTO MAS CERCANO
                            result = from datos in DatosInsumo
                                     where datos.Length > 29
                                     let comp = datos.Substring(0, 29).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;


                        case "44": // VMIN VALOR DEL MINUTO POR PLAN, ESTRATO Y TIPO DE SELLO EN LA FACTURA
                            result = from datos in DatosInsumo
                                     where datos.Length > 101
                                     let comp = datos.Substring(0, 101).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;

                        case "45": // VFGE VALOR DEL MINUTO ADICIONAL PARA GOBIERNO Y ESPECIALES POR PLAN Y ESTRATO.
                            result = from datos in DatosInsumo
                                     where datos.Length > 104
                                     let comp = datos.Substring(0, 104).Trim()
                                     where comp == newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).ToString()
                                     select datos;
                            break;
                    }
                    #endregion

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                    #endregion
                }
                else if (pIdentificadorInsumo == Variables.Insumos.ExcluirServiciosAdicionales.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    DatosInsumo.Clear();

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.cuentasExtraer.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos)
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.distribucion_especial.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.CicloCourier.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.ClientesEspeciales.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.BaseTranspromo.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.ASIGNACION_CARTAS.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.NIVEL_RECLAMACION.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.Fechas_Pago_Fijas.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.ETB_Horas_Exp.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split(' ').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.PromocionesLTE.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.Cuentas_LTE.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos)
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.Clientes_Email_Privado.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split(' ').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.BASE_CUPONES.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);
                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.doc1_participaciones.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.CodigosBD.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.LOC_BAR.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.TelefonosPrivadosRevchain.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos)
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.TRAMITES.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.FA_DISTRIBUCION_EMAIL_REVCHAIN.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.DistribucionDoble_REVCHAIN.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos)
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.ClausulaPermanencia.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split('|').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.EXCLUSION_PAGO_INMEDIATO.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos)
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.FirstOrDefault(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.ETB_Cartas_Mora.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split(' ').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.MENSAJE_CLIENTES_SUSPENDIDOS.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Split(' ').ElementAt(0))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }
                else if (pIdentificadorInsumo == Variables.Insumos.PROCUNI.ToString())
                {
                    var result = from datos in DatosInsumo
                                 where newObject.GetProperty("Cruce").GetValue(pObjEntradaRetorno).Equals(datos.Substring(0, 20).TrimStart('0'))
                                 select datos;

                    newObject.GetProperty("Resultados").SetValue(pObjEntradaRetorno, result.ToList(), null);

                    return newObject.GetProperty("Resultados").GetValue(pObjEntradaRetorno);
                }

                #endregion
            }
            else
            {
                //TODO: No esta configurado el insumo no se puede cargar
            }

            return pObjEntradaRetorno;
        }

        #region Metodos Uso Insumos

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo ExcluirServiciosAdicionales
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraExclusionSa GetServiciosAdicionales(string pLlaveCruce)
        {
            #region GetServiciosAdicionales
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._2_SERVICIOS_ADICIONALES_TV][Variables.Insumos.ExcluirServiciosAdicionales.ToString()];
            resultado.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraExclusionSa>(Variables.Insumos.ExcluirServiciosAdicionales.ToString(),
                resultado.RutaInsumo, new App.ControlInsumos.EstructuraExclusionSa { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as App.ControlInsumos.EstructuraExclusionSa; 
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo TablaSutitucion
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// /// <param name="pIdentificardorBusqueda"></param>
        /// <returns></returns>
        public static EstructuraTablaSutitucion GetTablaSutitucion(string pLlaveCruce, string pIdentificardorBusqueda)
        {
            #region GetTablaSutitucion
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._13_TABLA_SUSTITUCION][Variables.Insumos.doc1tsub.ToString()];
            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraTablaSutitucion>(Variables.Insumos.doc1tsub.ToString(),
                resultado.RutaInsumo, new EstructuraTablaSutitucion { Cruce = pLlaveCruce, IdentificardorBusqueda = pIdentificardorBusqueda });

            return new EstructuraTablaSutitucion { Resultados = resultado.EstructuraSalida}; 
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo ServiciosAdicionalesTv
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraServiciosAdicionalesTv GetServiciosAdicionalesTv(string pLlaveCruce)
        {
            #region GetServiciosAdicionalesTv
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._2_SERVICIOS_ADICIONALES_TV][Variables.Insumos.ServiciosAdicionalesTV.ToString()];
            resultado.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraServiciosAdicionalesTv>(Variables.Insumos.ServiciosAdicionalesTV.ToString(),
                resultado.RutaInsumo, new App.ControlInsumos.EstructuraServiciosAdicionalesTv { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as App.ControlInsumos.EstructuraServiciosAdicionalesTv; 
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo VelocidadFibra
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraVelocidadFibra GetVelocidadFibra(string pLlaveCruce)
        {
            #region GetVelocidadFibra
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._2_SERVICIOS_ADICIONALES_TV][Variables.Insumos.VelocidadFibra.ToString()];
            resultado.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraVelocidadFibra>(Variables.Insumos.VelocidadFibra.ToString(),
                resultado.RutaInsumo, new App.ControlInsumos.EstructuraVelocidadFibra { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as App.ControlInsumos.EstructuraVelocidadFibra; 
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo ParametrizacionPaquetesFibra
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraParametrizacionPaquetesFibra GetParametrizacionPaquetesFibra(string pLlaveCruce)
        {
            #region GetParametrizacionPaquetesFibra
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._10_PAQUETE_FIBRA][Variables.Insumos.ParametrizacionPaquetesFibra.ToString()];
            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos(Variables.Insumos.ParametrizacionPaquetesFibra.ToString(),
                resultado.RutaInsumo, new EstructuraParametrizacionPaquetesFibra { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as App.ControlInsumos.EstructuraParametrizacionPaquetesFibra;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo CuentasEnvioSms
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraCuentasEnvioSms GetCuentasEnvioSms(string pLlaveCruce)
        {
            #region GetCuentasEnvioSms
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._29_DISTRIBUCION_SMS][Variables.Insumos.cuentas_Envio_SMS.ToString()];
            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraCuentasEnvioSms>(Variables.Insumos.cuentas_Envio_SMS.ToString(),
                resultado.RutaInsumo, new EstructuraCuentasEnvioSms { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraCuentasEnvioSms;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo CodigosUniverSvas
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraCodigosUniverSvas GetCodigosUniverSvas(string pLlaveCruce)
        {
            #region GetCodigosUniverSvas
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._32_SVAS_Fuera_Bundle][Variables.Insumos.Codigos_Univer_SVAS.ToString()];
            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraCodigosUniverSvas>(Variables.Insumos.Codigos_Univer_SVAS.ToString(),
                resultado.RutaInsumo, new EstructuraCodigosUniverSvas { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraCodigosUniverSvas;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo CuentasSvasFueraBundle
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraCuentasSvasFueraBundle GetCuentasSvasFueraBundle(string pLlaveCruce)
        {
            #region GetCuentasSvasFueraBundle
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._32_SVAS_Fuera_Bundle][Variables.Insumos.Cuentas_SVAS_FueradeBundle.ToString()];
            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraCuentasSvasFueraBundle>(Variables.Insumos.Cuentas_SVAS_FueradeBundle.ToString(),
                resultado.RutaInsumo, new EstructuraCuentasSvasFueraBundle { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraCuentasSvasFueraBundle;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo CuentasExtraer
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static CuentasExtraer GetCuentasExtraer(string pLlaveCruce)
        {
            #region GetCuentasExtraer
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._3_CUENTAS_RETENIDAS][Variables.Insumos.cuentasExtraer.ToString()];
            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<CuentasExtraer>(Variables.Insumos.cuentasExtraer.ToString(),
                resultado.RutaInsumo, new CuentasExtraer { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as CuentasExtraer;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo DistribucionEspecial
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static DistribucionEspecial GetDistribucionEspecial(string pLlaveCruce)
        {
            #region GetDistribucionEspecial
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._4_DISTRIBUCION_ESPECIAL][Variables.Insumos.distribucion_especial.ToString()];
            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<DistribucionEspecial>(Variables.Insumos.distribucion_especial.ToString(),
                resultado.RutaInsumo, new DistribucionEspecial { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as DistribucionEspecial;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo CicloCourier
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static CicloCourier GetCicloCourier(string pLlaveCruce)
        {
            #region GetCicloCourier
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._9_OFICINAS_ORDENAMIENTOS][Variables.Insumos.CicloCourier.ToString()];
            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<CicloCourier>(Variables.Insumos.CicloCourier.ToString(),
                resultado.RutaInsumo, new CicloCourier { Cruce = pLlaveCruce });


            return resultado.EstructuraSalida as CicloCourier;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo ClientesEspecialesDatos
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static ClientesEspecialesDatos GetClientesEspecialesDatos(string pLlaveCruce)
        {
            #region GetClientesEspecialesDatos
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._12_PARAMETROS_CTAS_NETWORKING][Variables.Insumos.ClientesEspeciales.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<ClientesEspecialesDatos>(Variables.Insumos.ClientesEspeciales.ToString(),
                resultado.RutaInsumo, new ClientesEspecialesDatos { Cruce = pLlaveCruce });


            return resultado.EstructuraSalida as ClientesEspecialesDatos;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo BaseTranspromo
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static BaseTranspromo GetBaseTranspromo(string pLlaveCruce)
        {
            #region GetBaseTranspromo
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._16_TRASPROMOS][Variables.Insumos.BaseTranspromo.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<BaseTranspromo>(Variables.Insumos.BaseTranspromo.ToString(),
                resultado.RutaInsumo, new BaseTranspromo { Cruce = pLlaveCruce });


            return resultado.EstructuraSalida as BaseTranspromo;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo AsignacionCartas
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static AsignacionCartas GetAsignacionCartas(string pLlaveCruce)
        {
            #region GetAsignacionCartas
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._18_CARTAS_ANEXAS][Variables.Insumos.ASIGNACION_CARTAS.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<AsignacionCartas>(Variables.Insumos.ASIGNACION_CARTAS.ToString(),
                resultado.RutaInsumo, new AsignacionCartas { Cruce = pLlaveCruce });


            return resultado.EstructuraSalida as AsignacionCartas;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo NivelReclamacion
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static NivelReclamacion GetNivelReclamacion(string pLlaveCruce)
        {
            #region GetNivelReclamacion
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._20_NIVEL_RECLAMACION][Variables.Insumos.NIVEL_RECLAMACION.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<NivelReclamacion>(Variables.Insumos.NIVEL_RECLAMACION.ToString(),
                resultado.RutaInsumo, new NivelReclamacion { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as NivelReclamacion;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo FechaPagoFijas
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static FechaPagoFijas GetFechaPagoFijas(string pLlaveCruce)
        {
            #region GetFechaPagoFijas
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._23_FECHAS_PAGO_FIJAS][Variables.Insumos.Fechas_Pago_Fijas.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<FechaPagoFijas>(Variables.Insumos.Fechas_Pago_Fijas.ToString(),
                resultado.RutaInsumo, new FechaPagoFijas { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as FechaPagoFijas;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo ETBHorasExp
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static ETBHorasExp GetETBHorasExp(string pLlaveCruce)
        {
            #region GetETBHorasExp
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._26_LLANOS][Variables.Insumos.ETB_Horas_Exp.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<ETBHorasExp>(Variables.Insumos.ETB_Horas_Exp.ToString(),
                resultado.RutaInsumo, new ETBHorasExp { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as ETBHorasExp;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo PromosionesLTE
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static PromosionesLTE GetPromosionesLTE(string pLlaveCruce)
        {
            #region GetPromosionesLTE
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._27_PROMOSIONES_LTE][Variables.Insumos.PromocionesLTE.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<PromosionesLTE>(Variables.Insumos.PromocionesLTE.ToString(),
                resultado.RutaInsumo, new PromosionesLTE { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as PromosionesLTE;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo CuentasLTE
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static CuentasLTE GetCuentasLTE(string pLlaveCruce)
        {
            #region GetCuentasLTE
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._30_FACTURACION_VENCIDA_LTE_CORPORATIVO][Variables.Insumos.Cuentas_LTE.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<CuentasLTE>(Variables.Insumos.Cuentas_LTE.ToString(),
                resultado.RutaInsumo, new CuentasLTE { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as CuentasLTE;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo ClientesEmailPrivado
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static ClientesEmailPrivado GetClientesEmailPrivado(string pLlaveCruce)
        {
            #region GetClientesEmailPrivado
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._34_EMAIL_PRIVADO][Variables.Insumos.Clientes_Email_Privado.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<ClientesEmailPrivado>(Variables.Insumos.Clientes_Email_Privado.ToString(),
                resultado.RutaInsumo, new ClientesEmailPrivado { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as ClientesEmailPrivado;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo BaseCupones
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static BaseCupones GetBaseCupones(string pLlaveCruce)
        {
            #region GetBaseCupones
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._19_CUPONES_PUBLICITARIOS][Variables.Insumos.BASE_CUPONES.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<BaseCupones>(Variables.Insumos.BASE_CUPONES.ToString(),
                resultado.RutaInsumo, new BaseCupones { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as BaseCupones;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo Procuni
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraProcuni GetProcuni(string pLlaveCruce)
        {
            #region GetProcuni
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._51_PROCUNI][Variables.Insumos.PROCUNI.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraProcuni>(Variables.Insumos.PROCUNI.ToString(),
                resultado.RutaInsumo, new EstructuraProcuni { Cruce = "12051743217" });

            return resultado.EstructuraSalida as EstructuraProcuni;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo Doc1Participaciones
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraDoc1Participaciones GetDoc1Participaciones(string pLlaveCruce)
        {
            #region GetDoc1Participaciones
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._5_DOC1_PARTICIONES][Variables.Insumos.doc1_participaciones.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraDoc1Participaciones>(Variables.Insumos.doc1_participaciones.ToString(),
                resultado.RutaInsumo, new EstructuraDoc1Participaciones { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraDoc1Participaciones;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo CodigosBD
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraCodigosBD GetCodigosBD(string pLlaveCruce)
        {
            #region GetCodigosBD
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._6_LLAVES_CODIGOS_FACTU][Variables.Insumos.CodigosBD.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraCodigosBD>(Variables.Insumos.CodigosBD.ToString(),
                resultado.RutaInsumo, new EstructuraCodigosBD { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraCodigosBD;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo LocBar
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraLocBar GetLocBar(string pLlaveCruce)
        {
            #region GetLocBar
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._7_LOC_BAR][Variables.Insumos.LOC_BAR.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraLocBar>(Variables.Insumos.LOC_BAR.ToString(),
                resultado.RutaInsumo, new EstructuraLocBar { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraLocBar;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo TelefonosPrivadosRevchain
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraTelefonosPrivadosRevchain GetTelefonosPrivadosRevchain(string pLlaveCruce)
        {
            #region GetTelefonosPrivadosRevchain
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._14_TEL_PRIVADOS][Variables.Insumos.TelefonosPrivadosRevchain.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraTelefonosPrivadosRevchain>(Variables.Insumos.TelefonosPrivadosRevchain.ToString(),
                resultado.RutaInsumo, new EstructuraTelefonosPrivadosRevchain { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraTelefonosPrivadosRevchain;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo Tramites
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraTramites GetTramites(string pLlaveCruce)
        {
            #region GetTramites
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._15_TRAMITES][Variables.Insumos.TRAMITES.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraTramites>(Variables.Insumos.TRAMITES.ToString(),
                resultado.RutaInsumo, new EstructuraTramites { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraTramites;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo FADistribucionEmailRevchain
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraFADistribucionEmailRevchain GetFADistribucionEmailRevchain(string pLlaveCruce)
        {
            #region GetFADistribucionEmailRevchain
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._17_DISTRIBUCION_EMAIL][Variables.Insumos.FA_DISTRIBUCION_EMAIL_REVCHAIN.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraFADistribucionEmailRevchain>(Variables.Insumos.FA_DISTRIBUCION_EMAIL_REVCHAIN.ToString(),
                resultado.RutaInsumo, new EstructuraFADistribucionEmailRevchain { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraFADistribucionEmailRevchain;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo DistribucionDobleRevchain
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraDistribucionDobleRevchain GetDistribucionDobleRevchain(string pLlaveCruce)
        {
            #region GetDistribucionDobleRevchain
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._21_DISTRIBUCION_DUAL][Variables.Insumos.DistribucionDoble_REVCHAIN.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraDistribucionDobleRevchain>(Variables.Insumos.DistribucionDoble_REVCHAIN.ToString(),
                resultado.RutaInsumo, new EstructuraDistribucionDobleRevchain { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraDistribucionDobleRevchain;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo ClausulaPermanencia
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraClausulaPermanencia GetClausulaPermanencia(string pLlaveCruce)
        {
            #region GetClausulaPermanencia
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._25_CLAUSULAS_PERMANENCIAS][Variables.Insumos.ClausulaPermanencia.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraClausulaPermanencia>(Variables.Insumos.ClausulaPermanencia.ToString(),
                resultado.RutaInsumo, new EstructuraClausulaPermanencia { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraClausulaPermanencia;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo MensajesClientesSuspendidos
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraMensajesClientesSuspendidos GetMensajesClientesSuspendidos(string pLlaveCruce)
        {
            #region GetMensajesClientesSuspendidos
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._35_MENSAJE_CLIENTES_SUSPENDIDOS][Variables.Insumos.MENSAJE_CLIENTES_SUSPENDIDOS.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraMensajesClientesSuspendidos>(Variables.Insumos.MENSAJE_CLIENTES_SUSPENDIDOS.ToString(),
                resultado.RutaInsumo, new EstructuraMensajesClientesSuspendidos { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraMensajesClientesSuspendidos;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo ETBCartasMora
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraETBCartasMora GetETBCartasMora(string pLlaveCruce)
        {
            #region GetETBCartasMora
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._33_CARTAS_MORA][Variables.Insumos.ETB_Cartas_Mora.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraETBCartasMora>(Variables.Insumos.ETB_Cartas_Mora.ToString(),
                resultado.RutaInsumo, new EstructuraETBCartasMora { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraETBCartasMora;
            #endregion
        }

        /// <summary>
        /// Obtiene el resultado del cruce con el insumo ExclusionPagoInmediato
        /// </summary>
        /// <param name="pLlaveCruce"></param>
        /// <returns></returns>
        public static EstructuraExclusionPagoInmediato GetExclusionPagoInmediato(string pLlaveCruce)
        {
            #region GetExclusionPagoInmediato
            var resultado = Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._31_PAGO_INMEDIATO][Variables.Insumos.EXCLUSION_PAGO_INMEDIATO.ToString()];

            resultado.EstructuraSalida = Helpers.CargueDinamicoInsumos<EstructuraExclusionPagoInmediato>(Variables.Insumos.EXCLUSION_PAGO_INMEDIATO.ToString(),
                resultado.RutaInsumo, new EstructuraExclusionPagoInmediato { Cruce = pLlaveCruce });

            return resultado.EstructuraSalida as EstructuraExclusionPagoInmediato;
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

                case TiposFormateo.Fecha02:
                    return FormatearFecha("02", pCampo); // De ddMMyy a yyyyMM

                case TiposFormateo.Decimal01:
                    return FormatearDecimal("01", pCampo); 
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
            switch (pFormatoFechaTipo)
            {
                case "01":
                    return string.Format("{0}/{1}/{2}", pCampo.Substring(0, 2), pCampo.Substring(2, 2), pCampo.Substring(4, 4));

                case "02":
                    return string.Format("{0}{1}", pCampo.Substring(4, 4), pCampo.Substring(2, 2));

                default:
                    return pCampo;
            }
        }

        /// <summary>
        /// FormateaFecha
        /// </summary>
        /// <param name="pCampo"></param>
        /// <returns></returns>
        private static string FormatearDecimal(string pFormatoDecimalTipo, string pCampo)
        {
            switch (pFormatoDecimalTipo)
            {
                case "01":
                    string transformado = pCampo.Trim().TrimStart('0');
                    transformado = $"{transformado.Substring(0, transformado.Length - 2)}.{transformado.Substring(transformado.Length - 2)}";
                    transformado = Convert.ToDouble(transformado).ToString("N2");
                    return $"$ {transformado.Substring(0, transformado.LastIndexOf('.')).Replace(",", ".")},{transformado.Substring(transformado.LastIndexOf('.') + 1)}"; ;

                default:
                    return pCampo;
            }
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

                resultado += $"{campo.Trim()}";
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
            Console.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Mensaje));
            Utilidades.EscribirLog(Mensaje, Utilidades.LeerAppConfig("RutaLog"));
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
        Decimal01
    }
}
