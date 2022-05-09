using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL
{
    public class EstiloTablaExcel
    {
        private string psEstilo; 
        private bool pbFilaEncabezado = true;
        private bool pbFilaTotales = false;
        private bool pbPrimeraColumna = false;
        private bool pbFilasBandas = true;
        private bool pbAutoFiltro = true;
        private bool pbUltimaColumna = false;
        private bool pbColumnasBandas = false;
        private bool pbAutoAjustarColumnas = false;

        public string Estilo
        {
            get { return psEstilo; }
            set { psEstilo = value; }
        }

        public bool FilaEncabezado
        {
            get { return pbFilaEncabezado; }
            set { pbFilaEncabezado = value; }
        }

        public bool FilaTotales
        {
            get { return pbFilaTotales; }
            set { pbFilaTotales = value; }
        }

        public bool FilasBandas
        {
            get { return pbFilasBandas; }
            set { pbFilasBandas = value; }
        }

        public bool PrimeraColumna
        {
            get { return pbPrimeraColumna; }
            set { pbPrimeraColumna = value; }
        }

        public bool UltimaColumna
        {
            get { return pbUltimaColumna; }
            set { pbUltimaColumna = value; }
        }

        public bool ColumnasBandas
        {
            get { return pbColumnasBandas; }
            set { pbColumnasBandas = value; }
        }

        public bool AutoFiltro
        {
            get { return pbAutoFiltro; }
            set { pbAutoFiltro = value; }
        }

        public bool AutoAjustarColumnas
        {
            get { return pbAutoAjustarColumnas; }
            set { pbAutoAjustarColumnas = value; }
        }

    }
}
