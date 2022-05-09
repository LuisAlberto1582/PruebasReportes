/*
Nombre:		    Rolando Ramirez
Fecha:		    20110225
Descripción:	Clase para la navegación en archivos XML
*/

using System;
using System.Collections;
using System.Xml;

namespace KeytiaServiceBL
{
    public class FileReaderXML : FileReader
    {
        private XmlDocument poXmlDoc;
        private string psXPath = "/";
        private XmlNodeList pnlNodos;
        private int piNodo;
        private XmlNamespaceManager poXmlNS;

        public XmlNamespaceManager XmlNS
        {
            get { return poXmlNS; }
            set { poXmlNS = value; }
        }

        public XmlNameTable NameTable
        {
            get { return (poXmlDoc != null && poXmlDoc.NameTable != null ? poXmlDoc.NameTable : null); }
        }

        public override bool Abrir(string lsArchivo)
        {
            bool lbRet = false;

            try
            {
	            poXmlDoc = new XmlDocument();
	            poXmlDoc.Load(lsArchivo);
                lbRet = true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al abrir el archivo XML '" + lsArchivo + "'", ex);
            }

	        return lbRet;
        }

        public override void Cerrar()
        {
	        psXPath = "";
		    poXmlDoc = null;
        }

        public override string[] SiguienteRegistro()
        {
	        return SiguienteRegistro(psXPath);
        }

        public string[] SiguienteRegistro(string lsXPath)
        {
	        string[] lsValores = null;
            //int liNodo;
            ArrayList laRet = new ArrayList();

            if (poXmlDoc == null)
                return null;

	        if (lsXPath != psXPath)
            {
		        psXPath = lsXPath;

                if (poXmlNS == null)
		            pnlNodos = poXmlDoc.SelectNodes(psXPath);
                else
                    pnlNodos = poXmlDoc.SelectNodes(psXPath, poXmlNS);

		        piNodo = 0;
            }
	        else
		        piNodo++;

	        if (pnlNodos != null && piNodo < pnlNodos.Count)
            {
                //lsValores = new string[pnlNodos[piNodo].Attributes.Count];
                //liNodo = 0;

                //foreach (XmlAttribute lsAtt in pnlNodos[piNodo].Attributes)
                //{
                //    lsValores[liNodo] = lsAtt.Value;
                //    liNodo++;
                //}

                GetValues(pnlNodos[piNodo], laRet);
                lsValores = (string[])laRet.ToArray(typeof(System.String));
            }

	        return lsValores;
        }

        public void GetValues(XmlNode lxnNodo, ArrayList laRet)
        {
            GetValues("", lxnNodo, laRet);
        }

        public void GetValues(string lsParent, XmlNode lxnNodo, ArrayList laRet)
        {
            string lsParentAct = lsParent.Length != 0 ? lsParent + "_" : "";

            if (lxnNodo.Attributes != null)
                foreach (XmlAttribute lxaAtt in lxnNodo.Attributes)
                    laRet.Add(lsParentAct + lxaAtt.LocalName + "|" + lxaAtt.Value);

            if (lxnNodo.HasChildNodes)
            {
                if (lxnNodo.FirstChild is XmlText)
                    laRet.Add(lsParentAct + lxnNodo.LocalName + "|" + lxnNodo.FirstChild.Value);
                else
                    foreach (XmlNode lxnChild in lxnNodo.ChildNodes)
                        GetValues(lsParentAct + lxnNodo.LocalName, lxnChild, laRet);
            }
            else
                laRet.Add(lsParentAct + lxnNodo.LocalName + "|");
        }
    }
}
