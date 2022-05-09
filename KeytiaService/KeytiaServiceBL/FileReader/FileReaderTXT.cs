/*
Nombre:		    Rolando Ramirez
Fecha:		    20110225
Descripción:	Clase para la navegación en archivos de texto
*/

using System;
using System.IO;
using System.Text;

namespace KeytiaServiceBL
{
    public class FileReaderTXT : FileReader
    {
        protected StreamReader poArchivo;

        public override bool Abrir(string lsArchivo)
        {
            bool lbRet = false;

            try
            {
                poArchivo = new StreamReader(lsArchivo);
                lbRet = true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al abrir el archivo de texto '" + lsArchivo + "'", ex);
            }

            return lbRet;
        }

        //RZ.20131213 Se agrega metodo para leer un archivo y especificar el formato de encodificacion de los caracteres
        /// <summary>
        /// Metodo que lee el archivo, en el cual se puede especificar el tipo de encoding que tiene el archivo a leer
        /// </summary>
        /// <param name="lsArchivo">Ruta del archivo a abrir</param>
        /// <param name="tipoEncoding">La encodificacion de caracteres a usar</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indica si se debe buscar marcas de orden de bytes en el principio del archivo</param>
        /// <returns></returns>
        public bool Abrir(string lsArchivo, Encoding tipoEncoding, bool detectEncodingFromByteOrderMarks)
        {
            bool lbRet = false;

            try
            {
                poArchivo = new StreamReader(lsArchivo, tipoEncoding, detectEncodingFromByteOrderMarks);
                lbRet = true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al abrir el archivo de texto '" + lsArchivo + "'", ex);
            }

            return lbRet;
        }

        public override void Cerrar()
        {
	        if (poArchivo != null)
            {
		        poArchivo.Close();
		        poArchivo = null;
	        }
        }

        public override string[] SiguienteRegistro()
        {
	        string[] lsValores = null;

	        if (poArchivo == null)
		        return null;

	        if (!poArchivo.EndOfStream)
            {
                lsValores = new string[1];
		        lsValores[0] = poArchivo.ReadLine();
	        }

	        return lsValores;
        }
    }
}
