using System;
using System.Collections;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;
using System.EnterpriseServices;

namespace KeytiaCOM
{
    [ComVisible(true)]
    [Guid("D4DF646B-30D6-49d3-AD92-00C7801D52DD")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]

    public interface ICargasCOM
    {
        void CargaFacturas(string lhtTablaEnvio, string lsTabla, string lsEntidad, string lsMaestro, int liUsuario);
        void CargaFacturas(string lhtTablaEnvio, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario);

        void CargaCDR(string lhtTablaEnvio, string lsTabla, string lsEntidad, string lsMaestro, int liUsuario);
        void CargaCDR(string lhtTablaEnvio, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario);

        void CargaEmpleado(string lsXmlEmpleado, int liUsuario);
        void CargaEmpleado(string lsXmlEmpleado, string lsXmlHtTablaRetry, int liUsuario);

        void CargaCentroCosto(string lsXmlCenCos, int liUsuario);
        void CargaCentroCosto(string lsXmlCenCos, string lsXmlHtTablaRetry, int liUsuario);

        void CargaResponsable(string lhtTablaEnvio, string lsTabla, string lsEntidad, string lsMaestro, int liUsuario);
        void CargaResponsable(string lhtTablaEnvio, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisUpdate, int liUsuario);

        void Carga(string lhtTablaEnvio, string lsTabla, string lsEntidad, string lsMaestro, int liUsuario);
        void Carga(string lhtTablaEnvio, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario);
        void Carga(string lhtTablaEnvio, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario, bool lbReplicar, bool lbAjustaValores);

        void Replicar(string lsXmlTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario, bool lbAjustarValores,
            string lsXmlVchCodigos, string lsXmlVchCodigosEntidad, string lsXmlVchRelacion, string lsXmlRelacionActual, string lsXmlMaestros, string lsXmlEntidades, string lsXmlAtributosMaestro,
            string lsVchCodigoActualizar, string lsVchDescripcionMaestroActualizar, string[] lsEsquemasReplicados, string[] lsEsquemasPorReplicar, string lsXmlRetry, string lsXmlVigenciasHistoricos);

        void BajaCarga(int iCodCarga, int liUsuario);
        void ActualizarCarga(int iCodCarga, string lsHtCamposActualizar, int liUsuario);

        void EliminarRegistro(string lsTabla, int liCodRegistro, int liUsuario);
        void EliminarRegistro(string lsTabla, int liCodRegistro, string lsFinVigencia, int liUsuario);

        void BajaHistorico(int iCodRegistro, string lsXmlTabla, int liUsuario, bool lbAjustarValores, bool lbReplicar);
        void ReplicarBajaHistorico(string lsEntidad, string lsMaestro, string lsVchDescripcion, string lsVchCodigo, string lsDtFinVigencia);

        void EnviarReporteEstandar(int liCodReporte, string lsHTParam, string lsHTParamDesc, string lsKeytiaWebFPath, string lsStylePath, string lsCorreo, string lsTitulo, string lsExt, int liCodUsuarioDB);

        void ActualizaRestUsuario(string iCodUsuario, string iCodPerfil, string vchCodEntidad, string vchMaeRest, int liUsuario);
        void ActualizaRestPerfil(string iCodPerfil, string vchCodEntidad, string vchMaeRest, int liUsuario);
        void ActualizaRestEmpresa(string iCodEmpresa, string vchCodEntidad, string vchMaeRest, int liUsuario);
        void ActualizaRestCliente(string iCodCliente, string vchCodEntidad, string vchMaeRest, int liUsuario);
        void ActualizaRestriccionesSitio(string iCodCatalogo, int liUsuario);
        void ActualizaJerarquiaRestCenCos(string iCodCatalogo, string iCodPadre, int liUsuario);
        void ActualizaJerarquiaRestEmple(string iCodCatalogo, string iCodPadre, int liUsuario);
    }
}