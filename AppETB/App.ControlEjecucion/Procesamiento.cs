using DLL_Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.ControlLogicaProcesos;
using App.ControlInsumos;

namespace App.ControlEjecucion
{
    public class Procesamiento : App.Variables.Variables
    {
        public Procesamiento()
        { }

        public void AdecuarTrabajoApp(string pIdentificadorProceso)
        {
            //Creacion Carpeta Salida
            Helpers.RutaProceso = Directory.CreateDirectory($"{Utilidades.LeerAppConfig("RutaSalida")}\\{pIdentificadorProceso}{DateTime.Now:yyyyMMddhhmmss}").FullName;

            //Mover de entrada a originales y trabajar con estos archivos
            Helpers.RutaOriginales = Directory.CreateDirectory($"{Utilidades.LeerAppConfig("RutaOriginales")}\\{pIdentificadorProceso}Originales{DateTime.Now:yyyyMMddhhmmss}").FullName;
            Helpers.MoverArchivos(Utilidades.LeerAppConfig("RutaEntrada"), Helpers.RutaOriginales);

            //Mover archivos de entrada a original y la carpeta completa de insumos
            Helpers.RutaInsumos = $"{Helpers.RutaOriginales}\\Insumos";
            Helpers.CopiarCarpetaCompleta(Utilidades.LeerAppConfig("RutaInsumos"), Helpers.RutaInsumos, true);

            //Cargue Lista Insumos para despues usar en el formateo desde originales
            CargueGeneralInsumos(Helpers.RutaInsumos);
        }

        public void EjecutarProcesoMasivo(string pRutaArchivosProcesar)
        {
            //Llamada a App.ControlLogicaProcesos
            //TODO: si es muy lento esto se puede lanzar en hilos ya que se llena el diccionario de formateo y al finalizar con un ContinueWith del hilo se encadena la escritura
            foreach (var archivo in Directory.GetFiles(pRutaArchivosProcesar))
            {
                string nombreArchivo = Path.GetFileNameWithoutExtension(archivo);

                if (nombreArchivo.Contains("CONTEO") || nombreArchivo.Contains("IDENTIFICACION")) //TODO: se omiten estos archivos peor ahi que revisar para que sirven
                    continue;

                if (nombreArchivo.ToLower().Contains("premaestra"))
                {
                    RutaPremaestra = archivo;
                    continue;
                }

                _ = new ProcesoMasivos(archivo);
            }

            //TODO: Escribir Diccionario Formateados llamando a un metodo de cracion de salidas donde se realice la segmentacion
            App.ControlInsumos.Helpers.EscribirEnArchivo($"{App.ControlInsumos.Helpers.RutaProceso}\\MasivoCompletoPrueba.sal", DiccionarioExtractosFormateados.SelectMany(d => d.Value).ToList());
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

                foreach (var Archivo in Directory.GetFiles($"{Helpers.RutaInsumos}\\{Carpeta.Key}"))
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

        }

    }
}
