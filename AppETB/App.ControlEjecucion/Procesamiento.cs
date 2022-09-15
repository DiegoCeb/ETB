using DLL_Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ControlEjecucion
{
    public class Procesamiento : App.Variables.Variables
    {

        public Procesamiento()
        {
            //Cargue Lista Insumos para despues usar en el formateo
            CargueGeneralInsumos(Utilidades.LeerAppConfig("RutaInsumos"));
        }

        public void EjecutarProcesoMasivo(string pRutaArchivosProcesar)
        {
            //Llamada a App.ControlLogicaProcesos
        }

        public void CargueGeneralInsumos(string Pruta)
        {
            #region CargueGeneralInsumos
            foreach (var Carpeta in Directory.GetDirectories(Pruta))
            {
                string NombreCarpeta = Path.GetFileName(Carpeta);

                DiccionarioInsumos.Add(NombreCarpeta, new Dictionary<string, Variables.DatosInsumos>());
            }

            foreach (var Carpeta in DiccionarioInsumos)
            {
                bool insumoConfiguradoExiste = false;

                foreach (var Archivo in Directory.GetFiles($"{Utilidades.LeerAppConfig("RutaInsumos")}\\{Carpeta.Key}"))
                {
                    string NombreArchivo = Path.GetFileName(Archivo);

                    foreach (var EnumInsumo in Enum.GetValues(typeof(Variables.Insumos)))
                    {
                        //Si el insumo esta configurado
                        if (NombreArchivo.Contains(EnumInsumo.ToString()))
                        {
                            DiccionarioInsumos[Carpeta.Key].Add(EnumInsumo.ToString(), new Variables.DatosInsumos
                            {
                                RutaInsumo = Archivo,
                                EstructuraSalida = null
                            });

                            insumoConfiguradoExiste = true;
                            break;
                        }
                    }

                    if (!insumoConfiguradoExiste)
                    {
                        //TODO: No existe insumo configurado, Mostrar mensaje de error

                    }
                }

            }
            #endregion

            //Ejemplo
            var datosInsumoServicioExclusion = DiccionarioInsumos[App.Variables.RxGeneral._2_SERVICIOS_ADICIONALES_TV][Variables.Insumos.ExcluirServiciosAdicionales.ToString()];
            datosInsumoServicioExclusion.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraExclusionSa>(Variables.Insumos.ExcluirServiciosAdicionales.ToString(),
                datosInsumoServicioExclusion.RutaInsumo, new App.ControlInsumos.EstructuraExclusionSa { Cruce = "163700573" });

            var datosInsumoServicioAdicionalesTV = DiccionarioInsumos[App.Variables.RxGeneral._2_SERVICIOS_ADICIONALES_TV][Variables.Insumos.ServiciosAdicionalesTV.ToString()];
            datosInsumoServicioAdicionalesTV.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraServiciosAdicionalesTv>(Variables.Insumos.ServiciosAdicionalesTV.ToString(),
                datosInsumoServicioAdicionalesTV.RutaInsumo, new App.ControlInsumos.EstructuraServiciosAdicionalesTv { Cruce = "160550620" });

            var datosInsumoVelocidadFibra = DiccionarioInsumos[App.Variables.RxGeneral._2_SERVICIOS_ADICIONALES_TV][Variables.Insumos.VelocidadFibra.ToString()];
            datosInsumoVelocidadFibra.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraVelocidadFibra>(Variables.Insumos.VelocidadFibra.ToString(),
                datosInsumoVelocidadFibra.RutaInsumo, new App.ControlInsumos.EstructuraVelocidadFibra { Cruce = "0160140936" });

            var datosInsumoConformacionPaquetes = DiccionarioInsumos[App.Variables.RxGeneral._10_PAQUETE_FIBRA][Variables.Insumos.ConformacionPaquetes.ToString()];
            datosInsumoConformacionPaquetes.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraConformacionPaquetes>(Variables.Insumos.ConformacionPaquetes.ToString(),
                datosInsumoConformacionPaquetes.RutaInsumo, new App.ControlInsumos.EstructuraConformacionPaquetes { Cruce = "ABC" });

            var datosInsumoParametrizacionPaquetesFibra = DiccionarioInsumos[App.Variables.RxGeneral._10_PAQUETE_FIBRA][Variables.Insumos.ParametrizacionPaquetesFibra.ToString()];
            datosInsumoParametrizacionPaquetesFibra.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraParametrizacionPaquetesFibra>(Variables.Insumos.ParametrizacionPaquetesFibra.ToString(),
                datosInsumoParametrizacionPaquetesFibra.RutaInsumo, new App.ControlInsumos.EstructuraParametrizacionPaquetesFibra { Cruce = "0168080731" });

            var datosInsumoCuentaEnvioSMS = DiccionarioInsumos[App.Variables.RxGeneral._29_DISTRIBUCION_SMS][Variables.Insumos.cuentas_Envio_SMS.ToString()];
            datosInsumoCuentaEnvioSMS.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraCuentasEnvioSms>(Variables.Insumos.cuentas_Envio_SMS.ToString(),
                datosInsumoCuentaEnvioSMS.RutaInsumo, new App.ControlInsumos.EstructuraCuentasEnvioSms { Cruce = "787859" });

            var datosInsumoCodigoUNiverSVAS = DiccionarioInsumos[App.Variables.RxGeneral._32_SVAS_Fuera_Bundle][Variables.Insumos.Codigos_Univer_SVAS.ToString()];
            datosInsumoCodigoUNiverSVAS.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraCodigosUniverSvas>(Variables.Insumos.Codigos_Univer_SVAS.ToString(),
                datosInsumoCodigoUNiverSVAS.RutaInsumo, new App.ControlInsumos.EstructuraCodigosUniverSvas { Cruce = "0163701020" });

            var datosInsumoCuentaSvasFueraBundle = DiccionarioInsumos[App.Variables.RxGeneral._32_SVAS_Fuera_Bundle][Variables.Insumos.Cuentas_SVAS_FueradeBundle.ToString()];
            datosInsumoCuentaSvasFueraBundle.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraCuentasSvasFueraBundle>(Variables.Insumos.Cuentas_SVAS_FueradeBundle.ToString(),
                datosInsumoCuentaSvasFueraBundle.RutaInsumo, new App.ControlInsumos.EstructuraCuentasSvasFueraBundle { Cruce = "708375" });

            //var datosInsumoTablaSustitucion = DiccionarioInsumos[App.Variables.RxGeneral._13_TABLA_SUSTITUCION][Variables.Insumos.doc1tsub.ToString()];
            //datosInsumoTablaSustitucion.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraTablaSutitucion>(Variables.Insumos.doc1tsub.ToString(),
            //    datosInsumoTablaSustitucion.RutaInsumo, new App.ControlInsumos.EstructuraTablaSutitucion { Cruce = "708375" });

            var datosInsumoCuentaExtraer = DiccionarioInsumos[App.Variables.RxGeneral._3_CUENTAS_RETENIDAS][Variables.Insumos.cuentasExtraer.ToString()];

            datosInsumoCuentaExtraer.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.CuentasExtraer>(Variables.Insumos.cuentasExtraer.ToString(),
                datosInsumoCuentaExtraer.RutaInsumo, new App.ControlInsumos.CuentasExtraer { Cruce = "12054417800" });

            var datosInsumoDistrubucionEspecial = DiccionarioInsumos[App.Variables.RxGeneral._4_DISTRIBUCION_ESPECIAL][Variables.Insumos.distribucion_especial.ToString()];

            datosInsumoDistrubucionEspecial.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.DistribucionEspecial>(Variables.Insumos.distribucion_especial.ToString(),
                datosInsumoDistrubucionEspecial.RutaInsumo, new App.ControlInsumos.DistribucionEspecial { Cruce = "3683910" });

            var datosInsumoCicloCourrier = DiccionarioInsumos[App.Variables.RxGeneral._9_OFICINAS_ORDENAMIENTOS][Variables.Insumos.CicloCourier.ToString()];

            datosInsumoCicloCourrier.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.CicloCourier>(Variables.Insumos.CicloCourier.ToString(),
                datosInsumoCicloCourrier.RutaInsumo, new App.ControlInsumos.CicloCourier { Cruce = "90" });

            var datosInsumoClientesEspeciales = DiccionarioInsumos[App.Variables.RxGeneral._12_PARAMETROS_CTAS_NETWORKING][Variables.Insumos.ClientesEspeciales.ToString()];

            datosInsumoClientesEspeciales.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.ClientesEspecialesDatos>(Variables.Insumos.ClientesEspeciales.ToString(),
                datosInsumoClientesEspeciales.RutaInsumo, new App.ControlInsumos.ClientesEspecialesDatos { Cruce = "12053931102" });

            var datosInsumoTranspromo = DiccionarioInsumos[App.Variables.RxGeneral._16_TRASPROMOS][Variables.Insumos.BaseTranspromo.ToString()];

            datosInsumoTranspromo.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.BaseTranspromo>(Variables.Insumos.BaseTranspromo.ToString(),
                datosInsumoTranspromo.RutaInsumo, new App.ControlInsumos.BaseTranspromo { Cruce = "12054311920" });

            var datosInsumoAsignacionCartas = DiccionarioInsumos[App.Variables.RxGeneral._18_CARTAS_ANEXAS][Variables.Insumos.ASIGNACION_CARTAS.ToString()];

            datosInsumoAsignacionCartas.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.AsignacionCartas>(Variables.Insumos.ASIGNACION_CARTAS.ToString(),
                datosInsumoAsignacionCartas.RutaInsumo, new App.ControlInsumos.AsignacionCartas { Cruce = "12054416910" });
            
            var datosInsumoNivelReclamacion = DiccionarioInsumos[App.Variables.RxGeneral._20_NIVEL_RECLAMACION][Variables.Insumos.NIVEL_RECLAMACION.ToString()];

            datosInsumoNivelReclamacion.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.NivelReclamacion>(Variables.Insumos.NIVEL_RECLAMACION.ToString(),
                datosInsumoNivelReclamacion.RutaInsumo, new App.ControlInsumos.NivelReclamacion { Cruce = "12053322738" });

            var datosInsumoFechaPagoFijas = DiccionarioInsumos[App.Variables.RxGeneral._23_FECHAS_PAGO_FIJAS][Variables.Insumos.Fechas_Pago_Fijas.ToString()];

            datosInsumoFechaPagoFijas.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.FechaPagoFijas>(Variables.Insumos.Fechas_Pago_Fijas.ToString(),
                datosInsumoFechaPagoFijas.RutaInsumo, new App.ControlInsumos.FechaPagoFijas { Cruce = "12053161345" });

            var datosInsumoLlanos = DiccionarioInsumos[App.Variables.RxGeneral._26_LLANOS][Variables.Insumos.ETB_Horas_Exp.ToString()];

            datosInsumoLlanos.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.ETBHorasExp>(Variables.Insumos.ETB_Horas_Exp.ToString(),
                datosInsumoLlanos.RutaInsumo, new App.ControlInsumos.ETBHorasExp { Cruce = "C" });

            var datosInsumoPromosionesLTE = DiccionarioInsumos[App.Variables.RxGeneral._27_PROMOSIONES_LTE][Variables.Insumos.PromocionesLTE.ToString()];

            datosInsumoPromosionesLTE.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.PromosionesLTE>(Variables.Insumos.PromocionesLTE.ToString(),
                datosInsumoPromosionesLTE.RutaInsumo, new App.ControlInsumos.PromosionesLTE { Cruce = "PLAN MEGA LIKE CONTROL" });

            var datosInsumoCuentasLTE = DiccionarioInsumos[App.Variables.RxGeneral._30_FACTURACION_VENCIDA_LTE_CORPORATIVO][Variables.Insumos.Cuentas_LTE.ToString()];

            datosInsumoCuentasLTE.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.CuentasLTE>(Variables.Insumos.Cuentas_LTE.ToString(),
                datosInsumoCuentasLTE.RutaInsumo, new App.ControlInsumos.CuentasLTE { Cruce = "12051575579" });

            var datosInsumoClienteEmailPrivado = DiccionarioInsumos[App.Variables.RxGeneral._34_EMAIL_PRIVADO][Variables.Insumos.Clientes_Email_Privado.ToString()];

            datosInsumoClienteEmailPrivado.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.ClientesEmailPrivado>(Variables.Insumos.Clientes_Email_Privado.ToString(),
                datosInsumoClienteEmailPrivado.RutaInsumo, new App.ControlInsumos.ClientesEmailPrivado { Cruce = "1430287" });

            var datosInsumobaseCupones = DiccionarioInsumos[App.Variables.RxGeneral._19_CUPONES_PUBLICITARIOS][Variables.Insumos.BASE_CUPONES.ToString()];

            datosInsumobaseCupones.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.BaseCupones>(Variables.Insumos.BASE_CUPONES.ToString(),
                datosInsumobaseCupones.RutaInsumo, new App.ControlInsumos.BaseCupones { Cruce = "12054349936" });

            var datospruebas = DiccionarioInsumos[App.Variables.RxGeneral._51_PROCUNI][Variables.Insumos.PROCUNI.ToString()];

            datospruebas.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.Procuni>(Variables.Insumos.PROCUNI.ToString(),
                datospruebas.RutaInsumo, new App.ControlInsumos.Procuni { Cruce = "12051743217" });

        }

    }
}
