/*
Nombre:		    DMM
Fecha:		    20110525
Descripción:	Clase para escribir archivos de texto
Modificación:	
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
namespace KeytiaServiceBL
{
    public class TxtFileAccess
    {
        protected StreamWriter pFS;

        protected string pFileName;
        public string FileName
        {
            get { return pFileName; }
            set { pFileName = value; }
        }


        public void Abrir()
        {
            if (pFS == null)
            {
                pFS = File.CreateText(pFileName);
            }

        }


        public void Cerrar()
        {
            if ((pFS != null))
            {
                pFS.Close();
            }

            pFS = null;

        }


        public void Escribir(string pTexto)
        {
            if ((pFS != null))
            {
                pFS.WriteLine(pTexto);
            }

        }
    }
}