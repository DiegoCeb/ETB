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
                string NombreCarpeta = Path.GetFileName(Carpeta).ToUpper();

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

            datosInsumoServicioExclusion.EstructuraSalida = ControlInsumos.Helpers.CargueDinamicoInsumos<Variables.EstructuraExclusionSA>(Variables.Insumos.ExcluirServiciosAdicionales.ToString(),
                datosInsumoServicioExclusion.RutaInsumo, new Variables.EstructuraExclusionSA { Cruce = "163700573" } );

        }

    }
}
