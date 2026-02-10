using System;
using System.IO;

namespace JavaCommons
{
    public class Dirs
    {
        public static string ProfilePath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(@"\", "/");
        }

        public static string ProfilePath(string name)
        {
            return (ProfilePath() + @"/" + name).Replace(@"\", "/");
            ;
        }

        public static string DocumentsPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace(@"\", "/");
            ;
        }

        public static string DocumentsPath(string name)
        {
            return (DocumentsPath() + @"/" + name).Replace(@"\", "/");
            ;
        }

        public static string SpecialFolderPath(Environment.SpecialFolder folder)
        {
            return System.Environment.GetFolderPath(folder).Replace(@"\", "/");
            ;
        }

        public static string AppDataFolderPath(string orgName, string appName)
        {
            string baseFolder = SpecialFolderPath(Environment.SpecialFolder.ApplicationData);
            return $"{baseFolder}/{orgName}/{appName}".Replace(@"\", "/");
            ;
        }

        public static string AppDataFolderPath(string appName)
        {
            string baseFolder = SpecialFolderPath(Environment.SpecialFolder.ApplicationData);
            return $"{baseFolder}/{appName}".Replace(@"\", "/");
            ;
        }

        public static void Prepare(string dirPath)
        {
            Directory.CreateDirectory(dirPath);
        }

        public static void PrepareForFile(string filePath)
        {
            Prepare(Path.GetDirectoryName(filePath));
        }
    }
}