/*
Nombre:		    Rolando Ramirez
Fecha:		    20110225
Descripción:	Clase para la navegación en archivos de excel
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL
{
    public class FileReaderXLS : FileReader
    {
        private ExcelAccess poExcel;

        private string psHoja = "";
        private int piRenglon = -1;
        private int piMaxCol = -1;
        private int piMaxRow = -1;
        private string psArchivo = "";

        //NZ 20170921
        private int piIndexHoja = -1;

        public override bool Abrir(string lsArchivo)
        {
            bool lbRet = false;

            try
            {
                poExcel = new ExcelAccess();
                poExcel.FilePath = lsArchivo;
                poExcel.Abrir(true);
                psArchivo = lsArchivo;
                lbRet = true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al abrir el archivo de Excel '" + lsArchivo + "'", ex);
            }

            return lbRet;
        }

        public override void Cerrar()
        {
            psHoja = "";

            if (poExcel != null)
            {
                poExcel.Cerrar(true);
                poExcel.Dispose();
                poExcel = null;
            }
        }

        public override string[] SiguienteRegistro()
        {
            return SiguienteRegistro(psHoja);
        }

        public string[] SiguienteRegistro(string lsHoja)
        {
	        string[] lsValores = null;
            try
            {
                if (poExcel == null)
                    return null;

                if (lsHoja == "")
                    lsHoja = poExcel.NombreHoja0();

                if (psHoja != lsHoja)
                {
                    psHoja = lsHoja;
                    piRenglon = 1;
                    piMaxCol = poExcel.MaxCol(psHoja);
                    piMaxRow = poExcel.MaxRow(psHoja);
                }
                else
                    piRenglon++;

                if (piRenglon <= piMaxRow)
                {
                    object[,] loValores = poExcel.Consultar(psHoja, piRenglon, 1, piRenglon, piMaxCol);
                    lsValores = new string[piMaxCol];

                    for (int c = 1; c <= piMaxCol; c++)
                        lsValores[c - 1] = (loValores[1, c] != null ? (loValores[1, c] is DateTime ? ((DateTime)loValores[1, c]).ToString("yyyy-MM-dd HH:mm:ss") : loValores[1, c].ToString()) : "");
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error leyendo el siguiente registro del archivo '" + psArchivo + "'.", ex);
            }

	        return lsValores;
        }


        //NZ 20170921
        public int GetTotalHojas()
        {
            return poExcel.GetTotalHojas();
        }

        public bool CambiarHoja(int indexHoja)
        {
            try
            {
                poExcel.SetXlSheet(indexHoja);
                piIndexHoja = indexHoja;
                psHoja = poExcel.NombreHoja(piIndexHoja);

                piRenglon = 0;
                piMaxCol = poExcel.MaxCol(psHoja);
                piMaxRow = poExcel.MaxRow(psHoja);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CambiarHoja(string pHoja)
        {
            try
            {
                poExcel.SetXlSheet(pHoja);
                piIndexHoja = poExcel.IndexHoja(pHoja);
                psHoja = pHoja;

                piRenglon = 0;
                piMaxCol = poExcel.MaxCol(psHoja);
                piMaxRow = poExcel.MaxRow(psHoja);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
