using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ControlLogicaProcesos
{
    public interface IProcess
    {
        void Ejecutar(string pArchivo);
        void CargueFormateoArchivo(string pArchivo);
        List<string> FormatearArchivo(string pLLaveCruce, List<string> datosOriginales);
    }
}
