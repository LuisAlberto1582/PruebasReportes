using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public static class UnzipFile
    {
        public static bool DescompactarArchivo(FileInfo lfiZipFile, out string lsNombreArchivoDescompactado)
        {
            bool lbResult = false;
            string lsExtensionArchivo = string.Empty;
            string lsoutputFile = string.Empty;
            lsNombreArchivoDescompactado = string.Empty;

            try
            {
                // Get the stream of the source file.
                using (FileStream lfsArch = lfiZipFile.OpenRead())
                {
                    // Get original file extension, for example "doc" from report.doc.gz.
                    string lsRutaArchivo = lfiZipFile.FullName;

                    lsNombreArchivoDescompactado = lsRutaArchivo.ToLower().Replace(".gz", "").Replace(".zip", "");

                    if (!lsNombreArchivoDescompactado.Contains("."))
                        lsNombreArchivoDescompactado = lsNombreArchivoDescompactado + ".txt";

                    lsoutputFile = lsNombreArchivoDescompactado;

                    if (lfiZipFile.Extension.ToLower() == ".gz")
                    {
                        DescompactarGZ(lfsArch, lsNombreArchivoDescompactado);
                    }
                    else if (lfiZipFile.Extension.ToLower() == ".zip")
                    {
                        ZipInputStream inputStream = new ZipInputStream(File.OpenRead(lfiZipFile.FullName));
                        DescompactarZip(inputStream, lfiZipFile.DirectoryName + "\\", out lsNombreArchivoDescompactado);
                    }
                }

                lbResult = true;

            }
            catch (Exception ex)
            {
                Util.LogException("Error al descompactar el archivo '" + lfiZipFile.FullName + "'", ex);

                try
                {
                    if (lsoutputFile != "" && File.Exists(lsoutputFile))
                    {
                        File.Delete(lsoutputFile);
                    }
                }
                catch (Exception ex2)
                {
                    Util.LogException("Error al tratar de eliminar el archivo descompactado '" + lsoutputFile + "'.", ex2);
                }
            }

            return lbResult;
        }

        public static bool DescompactarGZ(FileStream lfsArch, string lsNombreArchivoDescompactado)
        {
            bool lbRet = false;

            try
            {
                //Create the decompressed file from .gz file
                using (FileStream lfsArchSalida = File.Create(lsNombreArchivoDescompactado))
                {
                    using (GZipStream Decompress = 
                                new GZipStream(lfsArch, CompressionMode.Decompress))
                    {
                        //Copy the decompression stream into the output file.
                        byte[] buffer = new byte[4096];
                        int numRead;
                        while ((numRead = Decompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            lfsArchSalida.Write(buffer, 0, numRead);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            

            return lbRet;
        }

        public static void DescompactarZip(ZipInputStream inputStream, string lsRutaArchivo, out string lsNombreArchDescompactado)
        {
            ZipEntry entry;
            lsNombreArchDescompactado = string.Empty;

            try
            {
                while ((entry = inputStream.GetNextEntry()) != null)
                {
                    lsNombreArchDescompactado = String.Format("{0}{1}", lsRutaArchivo, entry.Name.Replace(@"/", @"\"));


                    if (entry.Name.EndsWith("/"))
                    {
                        Directory.CreateDirectory(String.Format("{0}{1}", lsRutaArchivo, entry.Name.Replace(@"/", @"\")));
                    }
                    else
                    {
                        FileStream streamWriter = File.Create(String.Format("{0}{1}", lsRutaArchivo, entry.Name.Replace(@"/", @"\")));
                        //long size = entry.Size;
                        int size = 1024 * 10;
                        Byte[] data = new Byte[size];
                        while (true)
                        {
                            size = inputStream.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, (int)size);
                            }
                            else break;
                        }
                        streamWriter.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
