/*
Nombre:		    Rolando Ramirez
Fecha:		    20110225
Descripción:	Clase para la navegación en archivos de texto separado por comas
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL
{
    public class FileReaderCSV : FileReaderTXT
    {
        public override string[] SiguienteRegistro()
        {
	        string[] lsValores = null;
            string lsLine = "";

	        if (poArchivo == null)
		        return null;

            if (!poArchivo.EndOfStream)
            {
                //lsValores = poArchivo.ReadLine().Split(',');
                lsLine = poArchivo.ReadLine();

                //Regex regex = new Regex("(\"(?:[^\"]|\"\")*\"|[^,]*)(?:,(\"(?:[^\"]|\"\")*\"|[^,]*))*");

                //(((?<x>(?=[,\r\n]+))|"(?<x>([^"]|"")+)"|(?<x>[^,\r\n]+)),?)
                MatchCollection mcValores = Regex.Matches(
                    lsLine,
                    @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
                    //@"\s?((?<x>(?=[,]+))|""(?<x>([^""]|"""")+)""|""(?<x>)""|(?<x>[^,]+)),?",
                    RegexOptions.ExplicitCapture);

                if (mcValores != null && mcValores.Count > 0)
                {
                    lsValores = new string[mcValores.Count + (lsLine.EndsWith(",") ? 1 : 0)];

                    for (int i = 0; i < lsValores.Length; i++)
                        lsValores[i] = "";

                    for (int i = 0; i < mcValores.Count; i++)
                    {
                        lsValores[i] = mcValores[i].Value;

                        if (lsValores[i].EndsWith(","))
                            lsValores[i] = lsValores[i].Substring(0, lsValores[i].Length - 1);

                        if (lsValores[i].StartsWith("\"") && lsValores[i].EndsWith("\""))
                            lsValores[i] = lsValores[i].Substring(1, lsValores[i].Length - 2);
                    }
                }
            }

            return lsValores;
        }


        /// <summary>
        /// Se usa este metodo cuando se requiere separar una linea divida por caracteres diferentes
        /// al signo coma
        /// </summary>
        /// <param name="Separador">Caracter que separa los campos</param>
        /// <param name="EsCaracterEspecial">Indica si el caracter es un caracter de control o no</param>
        /// <returns>Arreglo de strings con los valores obtenidos de la linea</returns>
        public string[] SiguienteRegistro(char Separador, bool EsCaracterEspecial)
        {
            string[] lsValores = null;
            string lsLine = string.Empty;
            string lsSeparadorEspecial = Separador.ToString();

            if (EsCaracterEspecial)
            {
                lsSeparadorEspecial = @"\" + lsSeparadorEspecial;
            }

            StringBuilder patron = new StringBuilder();

            patron.Append(@"(((?<x>(?=[");
            patron.Append(@lsSeparadorEspecial);
            patron.Append(@"\r\n]+))|""""(?<x>([^""""]|"""""""")+)""""|(?<x>[^");
            patron.Append(@lsSeparadorEspecial);
            patron.Append(@"\r\n]+))");
            patron.Append(@lsSeparadorEspecial);
            patron.Append(@"?)");


            if (poArchivo == null)
                return null;

            if (!poArchivo.EndOfStream)
            {
                lsLine = poArchivo.ReadLine();

                MatchCollection mcValores = Regex.Matches(
                    lsLine,
                    @patron.ToString(),
                    RegexOptions.ExplicitCapture);

                if (mcValores != null && mcValores.Count > 0)
                {
                    lsValores = new string[mcValores.Count + (lsLine.EndsWith(@Separador.ToString()) ? 1 : 0)];

                    for (int i = 0; i < lsValores.Length; i++)
                        lsValores[i] = "";

                    for (int i = 0; i < mcValores.Count; i++)
                    {
                        lsValores[i] = mcValores[i].Value;

                        if (lsValores[i].EndsWith(@Separador.ToString()))
                            lsValores[i] = lsValores[i].Substring(0, lsValores[i].Length - 1);

                        if (lsValores[i].StartsWith("\"") && lsValores[i].EndsWith("\""))
                            lsValores[i] = lsValores[i].Substring(1, lsValores[i].Length - 2);
                    }
                }
            }

            return lsValores;
        }
    }
}
