using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ImageMagick;

namespace RawConverter
{
    static class RawFileProcessor
    {
        // ATTRIBUTES
        private static readonly List<string> listInputFileTypes = Enum.GetNames(typeof(InputFileTypes)).ToList();
        private static readonly List<string> listOutputFileTypes = Enum.GetNames(typeof(OutputFileTypes)).ToList();
        public static DataTable dataTableFiles = new();
        /// <summary>
        /// Used in ButtonConvert_Clicked event. Necessary to loop through a list of objects of type RawFile in order to call the Convert() method.
        /// </summary>
        public static List<RawFile> rawFiles = new();


        // PROPERTIES
        public static string FilterString { get { return $"Raw files|*{UserSettings.Default.InputFileType}"; } } // for selected file type in browser file window
        public static string OutputFolder { get; set; }
        /// <summary>
        /// Gets the output file type for converted images.
        /// </summary>
        public static OutputFileTypes OutputFileType { get; private set; } = (OutputFileTypes)listOutputFileTypes.IndexOf(UserSettings.Default.OutputFileType);
        /// <summary>
        /// Gets the input file type for raw files.
        /// </summary>
        public static InputFileTypes InputFileType { get; private set; } = (InputFileTypes)listInputFileTypes.IndexOf(UserSettings.Default.InputFileType);
        /// <summary>
        /// Get a list of raw file names including the file extension.
        /// </summary>
        public static List<string> RawFilesNames => GetRawFileNames();
        /// <summary>
        /// Property to set a flag if the converting process is running.
        /// </summary>
        public static bool ProcessIsRunning { get; set; } = false;


        /// <summary>
        /// Class for the raw file item.
        /// </summary>
        public class RawFile
        {
            // PROPERTIES
            /// <summary>
            /// Gets the file name. This does not include the file extension.
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// Gets the value for the file extension including the dot (.).
            /// </summary>
            public string Extension { get; }
            public DateTime CreationTime { get; }
            public double FileSize { get; }
            public InputFileTypes FileType {get; set;}

            // variables
            public readonly string path;
            //private readonly List<string> listFileTypes = Enum.GetNames(typeof(InputFileTypes)).ToList();
            private readonly FileInfo fileInfo;


            /// <summary>
            /// Creates a new RawFile object only reading the FileInfo.
            /// </summary>
            public RawFile(string path)
            {
                this.path = path;
                fileInfo = new(path);
                Name = fileInfo.Name.Split(".")[0]; // everything before first dot
                Extension = fileInfo.Extension;
                CreationTime = fileInfo.CreationTime;
                FileSize = Math.Round((float)fileInfo.Length / 1000000, 3, MidpointRounding.AwayFromZero); // B to MB
                FileType = (InputFileTypes)listInputFileTypes.IndexOf(UserSettings.Default.InputFileType);
            }

            /// <summary>
            /// Method to convert the raw file to the desired type.
            /// </summary>
            /// <param name="type"></param>
            public void Convert()
            {
                string filename = Path.Combine(paths: new string[] { OutputFolder, fileInfo.Name });

                // decode the image using the MagickImage library
                using (MagickImage rawImage = new(path))
                {
                    // convert according selection
                    switch (OutputFileType)
                    {
                        case OutputFileTypes.png:
                            rawImage.Write(filename, MagickFormat.Png);
                            break;

                        case OutputFileTypes.tiff:
                            rawImage.Write(filename, MagickFormat.Tiff);
                            break;

                        default:
                            //type is .jpg
                            //newBitmap.ToJPG(filename: Path.Combine(new string[] { outputFolder, Name }));
                            rawImage.Write(filename, MagickFormat.Jpg);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Method to get a list of the currently loaded raw file names.
        /// </summary>
        /// <returns>Return a list containing the strings for all raw file names.</returns>
        private static List<string> GetRawFileNames()
        {
            List<string> rawFileNames = new();

            foreach (RawFile rawFile in rawFiles)
            {
                rawFileNames.Add(rawFile.Name + rawFile.Extension);
            }

            return rawFileNames;
        }

        /// <summary>
        /// Method to read an array of raw files into a list.
        /// </summary>
        /// /// <param name="filesToAdd"></param>
        public static void AddFiles(string[] filesToAdd)
        {
            // intialize columns if necessary
            if (dataTableFiles.Columns.Count == 0)
            {
                dataTableFiles.Columns.Add("Name", typeof(string));
                dataTableFiles.Columns.Add("Size", typeof(string));
                dataTableFiles.Columns.Add("Date created", typeof(DateTime));
            }

            // add rows to data table and list
            foreach (string path in filesToAdd)
            {
                RawFile rawFile = new(path);

                // add raw file properties to data table
                object[] values = { $"{rawFile.Name}{rawFile.Extension}", $"{ rawFile.FileSize } MB" , rawFile.CreationTime };
                dataTableFiles.Rows.Add(values);

                // add raw file object to list
                rawFiles.Add(rawFile);
            }
        }

        /// <summary>
        /// Method to remove files from list and data table.
        /// </summary>
        /// <param name="filesToRemove"></param>
        public static void RemoveFiles(List<int> indicesToRemove)
        {
            // list recessary to handle the data
            List<DataRow> rowsToDelete = new();
            List<RawFile> itemsToKeep = new();

            // check if row needs to be deleted
            int id = 0;
            foreach (DataRow row in dataTableFiles.Rows)
            {
                if (indicesToRemove.Contains(id) == true)
                {
                    // item is in the delete query
                    // add the item to the "to-be-deleted-list"
                    rowsToDelete.Add(row);
                }
                else
                {
                    // this row is to be kept
                    // add the corresponding RawFile object to the "to-be-kept-list". This list will replate the current rawFiles-attribute
                    itemsToKeep.Add(rawFiles[id]);
                }

                id++; // set the counter
            }

            // delete the rows from the data table
            foreach (DataRow row in rowsToDelete)
            {
                dataTableFiles.Rows.Remove(row);
            }

            // point the rawFiles list to the new itemsToKeep list containing only the file designated to be kept
            rawFiles = itemsToKeep;
        }

        /// <summary>
        /// Method to remove all files from the list and data table.
        /// </summary>
        public static void RemoveAllFiles()
        {
            // clear list
            rawFiles.Clear();

            int i = 0;
            //Remove All
            while (i < dataTableFiles.Rows.Count)
            {
                DataRow currentRow = dataTableFiles.Rows[i];
                if (currentRow.RowState != DataRowState.Deleted)
                {
                    currentRow.Delete();
                }
                else
                {
                    i++;
                }
            }
            foreach (DataColumn column in dataTableFiles.Columns)
            {
                column.Dispose();
            }


            /*
            // delete the rows from the data table
            foreach (DataRow row in dataTableFiles.Rows)
            {
                dataTableFiles.Rows.Remove(row);
            }
            */
        }

    }
}
