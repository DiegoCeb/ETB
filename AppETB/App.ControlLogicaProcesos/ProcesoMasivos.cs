using App.ControlInsumos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ControlLogicaProcesos
{
    public class ProcesoMasivos : IProcess
    {

        public ProcesoMasivos(string pArchivo)
        {
            #region ProcesoMasivos
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
        public ProcesoMasivos()
        { }

        public void CargueArchivoDiccionario(string pArchivo)
        {
            throw new NotImplementedException();
        }

        public void Ejecutar(string pArchivo)
        {
            CargueArchivoDiccionario(pArchivo);
        }

        public List<string> FormatearArchivo(List<string> datosOriginales)
        {
            //var datosInsumoServicioExclusion = App.Variables.Variables.DiccionarioInsumos[App.Variables.RxGeneral._21_DISTRIBUCION_DUAL][Variables.Insumos.distribucion_especial.ToString()];

            //var datos = Helpers.CargueDinamicoInsumos<App.Variables.EstructuraExclusionSA>(App.Variables.Insumos.ExcluirServiciosAdicionales, datosInsumoServicioExclusion.RutaInsumo, , new object[] { "" ,"" ,"" })
            throw new NotImplementedException();
        }
    }
}
