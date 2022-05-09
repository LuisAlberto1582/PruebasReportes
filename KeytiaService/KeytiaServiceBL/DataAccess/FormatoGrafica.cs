using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL
{
    public class FormatoGrafica
    {
        protected string psTitulo = "";
        protected bool pbLeyendas = true;
        protected string psXFormat = "";
        protected string psYFormat = "";

        public bool Leyendas
        {
            get { return pbLeyendas; }
            set { pbLeyendas = value; }
        }

        public string Titulo
        {
            get { return psTitulo; }
            set { psTitulo = value; }
        }

        public string XFormat
        {
            get { return psXFormat; }
            set { psXFormat = value; }
        }

        public string YFormat
        {
            get { return psYFormat; }
            set { psYFormat = value; }
        }
    }
}
