using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Utils
    { 
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        public static void AddDirectoryFilesToTar(TarArchive tarArchive, string path)
        {
            if (File.Exists(path))
            {
                TarEntry tarEntry = TarEntry.CreateEntryFromFile(path);
                tarArchive.WriteEntry(tarEntry, true);
            }
            else if (Directory.Exists(path))
            {
                Array.ForEach(Directory.GetFiles(path), file => AddDirectoryFilesToTar(tarArchive, file));
            }
            else
            {

            }
        }

        public static void ExtractTGZ(string gzArchiveName, string destFolder)
        {
            Stream inStream = File.OpenRead(gzArchiveName);
            Stream gzipStream = new GZipInputStream(inStream);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
            tarArchive.ExtractContents(destFolder);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
        }
    }
}
