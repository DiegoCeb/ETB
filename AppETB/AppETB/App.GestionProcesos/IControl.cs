using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ControlProcesos
{
    public interface IControl
    {
        void Inicio();
        void Menu();
        void Ejecutar(string pProceso);
    }
}
