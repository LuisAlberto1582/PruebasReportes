/*
Nombre:		    Rolando Ramirez
Fecha:		    20110225
Descripción:	Clase base para la navegación en diversos tipos de archivo
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL
{
    public class FileReader
    {
        public virtual bool Abrir(string lsArchivo) { return false; }
        public virtual void Cerrar() { }
        public virtual string[] SiguienteRegistro() { return null; }
    }
}
