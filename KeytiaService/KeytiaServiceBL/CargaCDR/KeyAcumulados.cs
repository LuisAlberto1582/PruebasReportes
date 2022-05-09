using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR
{
    class KeyAcumulados
    {
        protected  int piCodContrato;
        protected int piCodUniCon;

          public KeyAcumulados() {
              this.piCodContrato = 0;
              this.piCodUniCon = 0;
   }

          public KeyAcumulados(int piCodContrato, int piCodTarifa)
          {
              this.piCodContrato = piCodContrato;
              this.piCodUniCon = piCodContrato;
   }
 

        public int CodContrato
        {
            get { return piCodContrato; }
            set { piCodContrato = value; }
        }

        public int CodUniCon
        {
            get { return piCodUniCon; }
            set { piCodUniCon = value; }
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            KeyAcumulados k = (KeyAcumulados)obj;
            return (piCodContrato == k.piCodContrato) && (piCodUniCon == k.piCodUniCon);
        }

        public override int GetHashCode()
        {
            return piCodContrato ^ piCodUniCon;
        }

    }
}
