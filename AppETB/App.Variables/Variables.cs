using System;
using System.Collections.Generic;
using System.IO;

namespace App.Variables
{
    /// <summary>
    /// Clase Variables 
    /// </summary>
    public class Variables
    {
        public string Proceso { get; set; }
        public string RutaPremaestra { get; set; }

        public static Dictionary<string, Dictionary<string, string>> DicGuias = new Dictionary<string, Dictionary<string, string>>();
        //Cuenta  - Formateo
        public static Dictionary<string, List<string>> DiccionarioExtractosFormateados = new Dictionary<string, List<string>>();
        //NombreCarpeta - NombreInsumo(Enumeracion) - DatosInsumos
        public static Dictionary<string, Dictionary<string, DatosInsumos>> DiccionarioInsumos = new Dictionary<string, Dictionary<string, DatosInsumos>>();

        public static StreamReader Lector;
    }

    /// <summary>
    /// Estructura de Datos extracto
    /// </summary>
    public struct DatosExtractos
    {
        public List<string> Extracto;
        public char Separador;
        public Type TipoClase;
    }

    /// <summary>
    /// Estructura Datos Insumo
    /// </summary>
    public struct DatosInsumos
    {
        public string RutaInsumo;
        public dynamic EstructuraSalida;
    }

    public enum Insumos
    {
        [System.ComponentModel.Description("ExcluirServiciosAdicionales")]
        ExcluirServiciosAdicionales = 0,
        [System.ComponentModel.Description("ServiciosAdicionalesTV")]
        ServiciosAdicionalesTV = 1,
        [System.ComponentModel.Description("VelocidadFibra")]
        VelocidadFibra = 2,
        [System.ComponentModel.Description("cuentasExtraer")]
        cuentasExtraer = 3,
        [System.ComponentModel.Description("distribucion_especial")]
        distribucion_especial = 4,
        [System.ComponentModel.Description("doc1_participaciones")]
        doc1_participaciones = 5,
        [System.ComponentModel.Description("CodigosBD")]
        CodigosBD = 6,
        [System.ComponentModel.Description("LOC_BAR")]
        LOC_BAR = 7,
        [System.ComponentModel.Description("CicloCourier")]
        CicloCourier = 8,
        [System.ComponentModel.Description("ConformacionPaquetes")]
        ConformacionPaquetes = 9,
        [System.ComponentModel.Description("ParametrizacionPaquetesFibra")]
        ParametrizacionPaquetesFibra = 10,
        [System.ComponentModel.Description("doc1tsub")]
        doc1tsub = 11,
        [System.ComponentModel.Description("TelefonosPrivadosRevchain")]
        TelefonosPrivadosRevchain = 12,
        [System.ComponentModel.Description("TRAMITES")]
        TRAMITES = 13,
        [System.ComponentModel.Description("BaseTranspromo")]
        BaseTranspromo = 14,
        [System.ComponentModel.Description("FA_DISTRIBUCION_EMAIL_REVCHAIN")]
        FA_DISTRIBUCION_EMAIL_REVCHAIN = 15,
        [System.ComponentModel.Description("ASIGNACION_CARTAS")]
        ASIGNACION_CARTAS = 16,
        [System.ComponentModel.Description("BASE_CUPONES")]
        BASE_CUPONES = 17,
        [System.ComponentModel.Description("NIVEL_RECLAMACION")]
        NIVEL_RECLAMACION = 18,
        [System.ComponentModel.Description("DistribucionDoble_REVCHAIN")]
        DistribucionDoble_REVCHAIN = 19,
        [System.ComponentModel.Description("Fechas_Pago_Fijas")]
        Fechas_Pago_Fijas = 20,
        [System.ComponentModel.Description("ClausulaPermanencia")]
        ClausulaPermanencia = 21,
        [System.ComponentModel.Description("cuentas_Envio_SMS")]
        cuentas_Envio_SMS = 22,
        [System.ComponentModel.Description("Cuentas_LTE")]
        Cuentas_LTE = 23,
        [System.ComponentModel.Description("EXCLUSION_PAGO_INMEDIATO")]
        EXCLUSION_PAGO_INMEDIATO = 24,
        [System.ComponentModel.Description("Codigos_Univer_SVAS")]
        Codigos_Univer_SVAS = 25,
        [System.ComponentModel.Description("Cuentas_SVAS_FueradeBundle")]
        Cuentas_SVAS_FueradeBundle = 26,
        [System.ComponentModel.Description("ETB_Cartas_Mora")]
        ETB_Cartas_Mora = 27,
        [System.ComponentModel.Description("Clientes_Email_Privado")]
        Clientes_Email_Privado = 28,
        [System.ComponentModel.Description("MENSAJE_CLIENTES_SUSPENDIDOS")]
        MENSAJE_CLIENTES_SUSPENDIDOS = 29,
        [System.ComponentModel.Description("ETB_Factura_Electronica")]
        ETB_Factura_Electronica = 30,
        [System.ComponentModel.Description("PROCUNI")]
        PROCUNI = 31,
        [System.ComponentModel.Description("PromocionesLTE")]
        PromocionesLTE = 32,
        [System.ComponentModel.Description("ClientesEspeciales")]
        ClientesEspeciales = 33,
        [System.ComponentModel.Description("ETB_Horas_Exp")]
        ETB_Horas_Exp = 34
    }

}
