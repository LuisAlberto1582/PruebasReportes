using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using System.IO;

namespace KeytiaServiceBL
{
    public class ZipFileAccess
    {
        private string pFilePath;
        private ZipOutputStream pZipOutputStream;

        public string FilePath
        {
            get { return pFilePath; }
            set { pFilePath = value; }
        }

        public void Abrir()
        {
            Abrir(FileMode.Append);
        }

        public void Abrir(FileMode mode)
        {
            FileStream strmZipFile = null;

            if (string.IsNullOrEmpty(pFilePath))
            {
                return;
            }

            //si no existe el directorio del archivo zip entonces lo creo
            if (!Directory.Exists(Path.GetDirectoryName(pFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(pFilePath));
            }

            //si el archivo existe entonces que se borre y se vuelva a crear
            strmZipFile = File.Open(pFilePath, mode);

            pZipOutputStream = new ZipOutputStream(strmZipFile);
            pZipOutputStream.SetLevel((int)CompressionMethod.Deflated);

        }


        public void Cerrar()
        {
            if (pZipOutputStream == null)
            {
                return;
            }
            pZipOutputStream.Finish();
            pZipOutputStream.Close();

        }

        public bool Agregar(string pArchivo)
        {
            return Agregar(pArchivo, false);
        }

        public bool Agregar(string pArchivo, bool pPathCompleto)
        {
            bool functionReturnValue = false;
            FileStream strmFile = null;
            Crc32 objCrc32 = new Crc32();
            ZipEntry objEntry = default(ZipEntry);
            FileInfo fi = null;

            if (pZipOutputStream == null || !File.Exists(pArchivo))
            {
                return functionReturnValue;
            }

            try
            {
                strmFile = File.OpenRead(pArchivo);
                byte[] abyBuffer = new byte[Convert.ToInt32(strmFile.Length - 1) + 1];
                strmFile.Read(abyBuffer, 0, abyBuffer.Length);

                //Obtengo informacion del archivo
                fi = new FileInfo(pArchivo);

                // guardar el nombre que va a tener el archivo en el zip
                if (pPathCompleto)
                {
                    objEntry = new ZipEntry(pArchivo);
                }
                else
                {
                    objEntry = new ZipEntry(fi.Name);
                }

                // guardar la fecha y hora de la última modificación
                objEntry.DateTime = fi.LastWriteTime;
                //objEntry.DateTime = DateTime.Now

                objEntry.Size = strmFile.Length;
                strmFile.Close();
                objCrc32.Reset();
                objCrc32.Update(abyBuffer);
                objEntry.Crc = objCrc32.Value;
                pZipOutputStream.PutNextEntry(objEntry);
                pZipOutputStream.Write(abyBuffer, 0, abyBuffer.Length);
            }
            finally
            {
                functionReturnValue = true;
            }
            return functionReturnValue;
        }
    }
}