using System;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using Grout.Base.Logger;

namespace Grout.UMP.Models
{
    public static class ZipManager
    {
        public static void CreateZipFromDirectory(string sourceDirectoryPath, string destinationFilePath)
        {
            sourceDirectoryPath = sourceDirectoryPath.Replace("\\\\", "\\");
            destinationFilePath = destinationFilePath.Replace("\\\\", "\\");

            try
            {
                ZipFile.CreateFromDirectory(sourceDirectoryPath, destinationFilePath, CompressionLevel.Fastest, false);
            }
            catch (Exception exception)
            {
                LogExtension.LogError("Unable to create zip from directory", exception, MethodBase.GetCurrentMethod(), " SourceDirectoryPath - " + sourceDirectoryPath + " DestinationFilePath - " + destinationFilePath);
            }
        }

        public static void ExtractZipToDirectory(string sourceFilePath, string destinationDirectoryPath)
        {
            sourceFilePath = sourceFilePath.Replace("\\\\", "\\");
            destinationDirectoryPath = destinationDirectoryPath.Replace("\\\\", "\\");
            try
            {
                ZipFile.ExtractToDirectory(sourceFilePath, destinationDirectoryPath);
            }
            catch (Exception exception)
            {
                LogExtension.LogError("Unable to extract zip to directory", exception, MethodBase.GetCurrentMethod(), " SourceFilePath - " + sourceFilePath + " DestinationDirectoryPath - " + destinationDirectoryPath);
            }
        }

        public static void CreateZipFromFile(string sourceFilePath, string outputFilePath)
        {
            sourceFilePath = sourceFilePath.Replace("\\\\", "\\");
            outputFilePath = outputFilePath.Replace("\\\\", "\\");

            try
            {
                using (ZipArchive zipArchive = ZipFile.Open(outputFilePath, ZipArchiveMode.Create))
                {
                    zipArchive.CreateEntryFromFile(sourceFilePath, new FileInfo(sourceFilePath).Name,
                        CompressionLevel.Fastest);                    
                }
            }
            catch (Exception exception)
            {
                LogExtension.LogError("Unable to create zip from file", exception, MethodBase.GetCurrentMethod(), " SourceFilePath - " + sourceFilePath + " OutputFilePath - " + outputFilePath);               
            }
        }
    }
}