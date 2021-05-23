using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawConverter
{
    static class RawFileReader
    {
        // Properties
        public static string InputFileType { get; set; }
        public static string[] FilesRead { get; }
        public static List<RawFile> fileList = new();

        /// <summary>
        /// Method to read an array of raw files into a list.
        /// </summary>
        /// /// <param name="filesToAdd"></param>
        static void AddFiles(string[] filesToAdd)
        {
            foreach (string path in filesToAdd)
            {
                RawFile file = new(path);
                fileList.Add(file);
            }
        }

        /// <summary>
        /// Method to remove files from list.
        /// </summary>
        /// <param name="filesToRemove"></param>
        static void RemoveFiles(string[] filesToRemove)
        {
            foreach (string file in filesToRemove)
            {
                fileList.Remove(new RawFile(file));
            }
        }
    }
}
