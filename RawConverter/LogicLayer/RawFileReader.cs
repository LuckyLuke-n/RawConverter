using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawConverter
{
    static class RawFileReader
    {
        // Properties
        public static DataTable dataTableFiles = new();
        public static string FilterString { get { return $"Raw files|*{UserSettings.Default.InputFileType}"; } }

        /// <summary>
        /// Method to read an array of raw files into a list.
        /// </summary>
        /// /// <param name="filesToAdd"></param>
        public static void AddFiles(string[] filesToAdd)
        {
            // intialize columns if necessary
            if (dataTableFiles.Rows.Count == 0)
            {
                dataTableFiles.Columns.Add("Name", typeof(string));
                dataTableFiles.Columns.Add("Size", typeof(string));
                dataTableFiles.Columns.Add("Date created", typeof(DateTime));
            }

            // add rows to data table
            foreach (string path in filesToAdd)
            {
                RawFile rawFile = new(path);
                object[] values = { rawFile.Name, $"{ rawFile.FileSize } MB" , rawFile.CreationTime };
                dataTableFiles.Rows.Add(values);
            }
        }

        /// <summary>
        /// Method to remove files from list.
        /// </summary>
        /// <param name="filesToRemove"></param>
        static void RemoveFiles(string[] filesToRemove)
        {


        }
    }
}
