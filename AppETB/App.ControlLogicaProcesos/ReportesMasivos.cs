using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.ControlInsumos;

namespace App.ControlLogicaProcesos
{
    public class ReportesMasivos
    {
        public ReportesMasivos()
        { }

        public ReportesMasivos(Dictionary<string, List<string>> pDatosImprimir)
        {
            #region ProcesoMasivos
            try
            {
                Ejecutar(pDatosImprimir);
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

        public void Ejecutar(Dictionary<string, List<string>> pDatosImprimir)
        {
            ExtraccionReportes(pDatosImprimir);
        }

        private void ExtraccionReportes(Dictionary<string, List<string>> pDatosImprimir)
        {
            List<string> Maestra = GetReporteMaestra(pDatosImprimir);
        }

        private List<string> GetReporteMaestra(Dictionary<string, List<string>> pDatosImprimir)
        {
            #region GetReporteMaestra
            List<string> lineaMaestra = new List<string>();

            foreach (List<string> extracto in pDatosImprimir.Values)
            {
                lineaMaestra.Add(GetLineaMaestra(extracto));
            }

            return lineaMaestra; 
            #endregion

        }

        private string GetLineaMaestra(List<string> pExtracto)
        {
            #region GetLineaMaestra
            string LineaMaestra = string.Empty;
            List<string> camposLinea = new List<string>();
            var result1AAA = from busqueda in pExtracto
                         where busqueda.Length > 5 && busqueda.Substring(0, 5).Equals("1AAA|")
                         select busqueda;

            if (result1AAA.Any())
            {
                string[] campos1AAA = result1AAA.FirstOrDefault().Split('|');

                camposLinea.Add(campos1AAA[16]); // TODO: Telefono
                camposLinea.Add(campos1AAA[7]); // Cuenta
                camposLinea.Add(campos1AAA[2]); // Nombre
                camposLinea.Add(campos1AAA[4]); // Direccion
                camposLinea.Add(campos1AAA[0]); // TODO: Zona
                camposLinea.Add(campos1AAA[1]); // Consecutivo
                camposLinea.Add(campos1AAA[0]); // TODO: Archivos
                camposLinea.Add(campos1AAA[8].TrimStart('0')); // Factura
                camposLinea.Add(campos1AAA[12]); // Mora
                camposLinea.Add(campos1AAA[28]); // Estrato
                camposLinea.Add(campos1AAA[40]); // Dane
                camposLinea.Add(campos1AAA[5]); // Ciudad
                camposLinea.Add(campos1AAA[30]); // Localidad
                camposLinea.Add(campos1AAA[29]); // Barrio
                camposLinea.Add(campos1AAA[10]); // TotalFactura
                camposLinea.Add(campos1AAA[9]); // Ciclo
                camposLinea.Add(campos1AAA[1]); // Lote TODO: Partir
                camposLinea.Add(campos1AAA[19]); // Fecx

                camposLinea.Add(campos1AAA[1]); // Fecp
                camposLinea.Add(campos1AAA[1]); // Fecb
                camposLinea.Add(campos1AAA[1]); // Fecc
                camposLinea.Add(campos1AAA[1]); // Total Iva
                camposLinea.Add(campos1AAA[1]); // Total IVA Otros Operadores
                camposLinea.Add(campos1AAA[1]); // Insertos
                camposLinea.Add(campos1AAA[1]); // Valor Pagar Mes

                camposLinea.Add(campos1AAA[27]); // Actividad

                camposLinea.Add(campos1AAA[1]); // Logo TIC
                camposLinea.Add(campos1AAA[1]); // Valor Subsidiado
                camposLinea.Add(campos1AAA[1]); // TipoEnvioCartaEmail
                camposLinea.Add(campos1AAA[1]); // Email
                camposLinea.Add(campos1AAA[1]); // FECL

                camposLinea.Add(campos1AAA[20].Replace("-",string.Empty)); // ReferenciaPago

                camposLinea.Add(campos1AAA[1]); // ProcedimientoReclamacion
                camposLinea.Add(campos1AAA[1]); // LeyendaCartera

                camposLinea.Add(campos1AAA[3]); // NIT/CED
                camposLinea.Add(campos1AAA[23]); // TipoProducto

                camposLinea.Add(campos1AAA[1]); // PlanPrimarioLTE
                camposLinea.Add(campos1AAA[1]); // PlanActual
                camposLinea.Add(campos1AAA[1]); // ConceptoFinanciacion
                camposLinea.Add(campos1AAA[1]); // SaldoFinanciacion
                camposLinea.Add(campos1AAA[1]); // cuotaFinanciacion
                camposLinea.Add(campos1AAA[1]); // ValorfacturaAnterior
                camposLinea.Add(campos1AAA[1]); // GraciasPorSuPago
                
                camposLinea.Add(campos1AAA[24]); // PorcentajeMora
                camposLinea.Add(campos1AAA[1]); // Retencion
                camposLinea.Add(campos1AAA[1]); // ORDER_COURRIER
                camposLinea.Add(campos1AAA[1]); // CourerAsignado
                camposLinea.Add(campos1AAA[1]); // MarcaRoaming
                camposLinea.Add(campos1AAA[1]); // CodigoBarra
                camposLinea.Add(campos1AAA[1]); // NroLineas12M
                camposLinea.Add(campos1AAA[1]); // MarcaPaqueteHBO
                camposLinea.Add(campos1AAA[1]); // MinutosConsumoMes
                camposLinea.Add(campos1AAA[1]); // CuentaVencidaAnticipada
                camposLinea.Add(campos1AAA[47]); // Precis
                camposLinea.Add(campos1AAA[48]); // ChipCatastral
                camposLinea.Add(campos1AAA[49]); // Cordenadas
                camposLinea.Add(campos1AAA[1]); // CUFE
            }

            return LineaMaestra; 
            #endregion
        }
    }
}
