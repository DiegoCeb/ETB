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
            //
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
                datosInsumoServicioExclusion.RutaInsumo, new App.ControlInsumos.EstructuraExclusionSa { Cruce = "163700573" } );

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

            var datosInsumoTablaSustitucion = DiccionarioInsumos[App.Variables.RxGeneral._13_TABLA_SUSTITUCION][Variables.Insumos.doc1tsub.ToString()];
            datosInsumoTablaSustitucion.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<App.ControlInsumos.EstructuraTablaSutitucion>(Variables.Insumos.Cuentas_SVAS_FueradeBundle.ToString(),
                datosInsumoTablaSustitucion.RutaInsumo, new App.ControlInsumos.EstructuraTablaSutitucion { Cruce = "708375" });

        }

    }
}
