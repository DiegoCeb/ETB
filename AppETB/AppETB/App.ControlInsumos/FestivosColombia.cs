using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ControlInsumos
{
    /// <summary>
    /// Clase FestivosColombia
    /// </summary>
    public class FestivosColombia
    {
        /// <summary>
        /// Metodo tomar dias festivos
        /// </summary>
        /// <param name="Anio">El año a calcular lo días festivos</param>
        /// <returns></returns>
        public static List<DateTime> DiasFestivos(int Anio)
        {
            #region DiasFestivos
            DateTime Pascua = calcularPascua(Anio);

            List<DateTime> diasFestivos = new List<DateTime>();

            IncluirFecha(ref diasFestivos, new DateTime(Anio, 1, 1)); //Primero de Enero
            IncluirFecha(ref diasFestivos, new DateTime(Anio, 5, 1)); //Dia del Trabajo 1 de Mayo
            IncluirFecha(ref diasFestivos, new DateTime(Anio, 7, 20)); // Independencia 20 de Julio
            IncluirFecha(ref diasFestivos, new DateTime(Anio, 8, 7)); // Batalla de Boyacá 7 de Agosto
            IncluirFecha(ref diasFestivos, new DateTime(Anio, 12, 8)); // Inmaculada Concepción
            IncluirFecha(ref diasFestivos, new DateTime(Anio, 12, 25)); // Navidad

            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Monday, new DateTime(Anio, 1, 6))); //Reyes magos
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Monday, new DateTime(Anio, 3, 19))); //San Jose
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Monday, Pascua).AddDays(42)); //Ascensión de Jesús
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Monday, Pascua).AddDays(63)); //Corpus Christi
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Monday, Pascua).AddDays(70)); //Sagrado Corazón
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Monday, new DateTime(Anio, 6, 29))); //san Pedro y san Pablo
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Monday, new DateTime(Anio, 8, 15))); //Asuncion de la virgen
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Monday, new DateTime(Anio, 10, 12))); //Día de la Raza
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Monday, new DateTime(Anio, 11, 1))); //Todos los Santos
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Monday, new DateTime(Anio, 11, 11))); //Independencia de Cartagena

            //IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Sunday, Pascua, true, false)); //Domingo de Ramos
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Thursday, Pascua, true)); //Jueves Santo
            IncluirFecha(ref diasFestivos, SiguienteDiaSemana(DayOfWeek.Friday, Pascua, true)); //Viernes Santo

            //IncluirFecha(ref diasFestivos, Pascua); //Pascua

            return diasFestivos; 
            #endregion
        }

        /// <summary>
        /// Metodo para incluir los días festivos en lista
        /// </summary>
        /// <param name="ListaDias"></param>
        /// <param name="fecha"></param>
        private static void IncluirFecha(ref List<DateTime> ListaDias, DateTime fecha)
        {
            #region IncluirFecha
            if (ListaDias.Contains(fecha) == false)
                ListaDias.Add(fecha); 
            #endregion
        }

        /// <summary>
        /// Metodo para calcular el Siguiente Dia Semana
        /// </summary>
        /// <param name="DiaSemana">Dia de la semana</param>
        /// <param name="fecha">Fecha</param>
        /// <param name="haciaAtras">Bandera para sumar un dia o restar</param>
        /// <param name="inclusive">Bandera para ver si es Inclusive</param>
        /// <returns></returns>
        private static DateTime SiguienteDiaSemana(DayOfWeek DiaSemana, DateTime fecha, bool haciaAtras = false, bool inclusive = true)
        {
            #region SiguienteDiaSemana
            if (inclusive)
            {
                if (fecha.DayOfWeek == DiaSemana)
                {
                    return fecha;
                }
            }
            else
            {
                if (haciaAtras)
                    fecha = fecha.AddDays(-1);
                else
                    fecha = fecha.AddDays(1);
            }

            while (fecha.DayOfWeek != DiaSemana)
                if (haciaAtras)
                    fecha = fecha.AddDays(-1);
                else
                    fecha = fecha.AddDays(1);

            return fecha; 
            #endregion
        }

        /// <summary>
        /// Metodo para calcular el dia de pascua
        /// </summary>
        /// <param name="Anio">Año a calcular</param>
        /// <returns></returns>
        private static DateTime calcularPascua(int Anio)
        {
            #region calcularPascua

            int a, b, c, d, e;
            int m = 24, n = 5;


            if (Anio >= 1583 && Anio <= 1699)
            {
                m = 22;
                n = 2;
            }
            else if (Anio >= 1700 && Anio <= 1799)
            {
                m = 23;
                n = 3;
            }
            else if (Anio >= 1800 && Anio <= 1899)
            {
                m = 23;
                n = 4;
            }
            else if (Anio >= 1900 && Anio <= 2099)
            {
                m = 24;
                n = 5;
            }
            else if (Anio >= 2100 && Anio <= 2199)
            {
                m = 24;
                n = 6;
            }
            else if (Anio >= 2200 && Anio <= 2299)
            {
                m = 25;
                n = 0;
            }

            a = Anio % 19;
            b = Anio % 4;
            c = Anio % 7;
            d = ((a * 19) + m) % 30;
            e = ((2 * b) + (4 * c) + (6 * d) + n) % 7;


            int dia = d + e;


            if (dia < 10) //Marzo
                return new DateTime(Anio, 3, dia + 22);
            else //Abril
            {

                if (dia == 26)
                    dia = 19;
                else if (dia == 25 && d == 28 && e == 6 && a > 10)
                    dia = 18;
                else
                    dia -= 9;

                return new DateTime(Anio, 4, dia);
            } 
            #endregion
        }
    }
}
