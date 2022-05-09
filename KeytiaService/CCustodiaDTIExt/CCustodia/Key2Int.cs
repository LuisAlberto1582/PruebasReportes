using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace CCustodiaDTIExt.CCustodia
{
    public class Key2Int
    {
        protected int piValor1;
        protected int piValor2;

        public Key2Int()
        {
            this.piValor1 = 0;
            this.piValor2 = 0;
        }

        public Key2Int(int piValor1, int piValor2)
        {
            this.piValor1 = piValor1;
            this.piValor2 = piValor2;
        }


        public int Valor1
        {
            get { return piValor1; }
            set { piValor1 = value; }
        }

        public int Valor2
        {
            get { return piValor2; }
            set { piValor2 = value; }
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            Key2Int k = (Key2Int)obj;
            return (piValor1 == k.piValor1) && (piValor2 == k.piValor2);
        }

        public override int GetHashCode()
        {
            return piValor1 ^ piValor2;
        }
    }
}
