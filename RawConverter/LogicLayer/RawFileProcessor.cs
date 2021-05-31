using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageMagick;

namespace RawConverter
{
    static class RawFileProcessor
    {
        // Properties
        public static DataTable dataTableFiles = new();
        public static string FilterString { get { return $"Raw files|*{UserSettings.Default.InputFileType}"; } } // for selected file type in browser file window
        public static string OutputFolder { get; set; }
        public static OutputFileTypes OutputFileType = OutputFileTypes.jpg; // preset is .jpg
        public static InputFileTypes InputFileType { get; }

        // attributes
        public static List<RawFile> listRawFiles = new();


        /// <summary>
        /// Class for the raw file item.
        /// </summary>
        public class RawFile
        {
            // properties
            public string Name { get; }
            public string Extension { get; }
            public DateTime CreationTime { get; }
            public double FileSize { get; }
            public InputFileTypes FileType {get; set;}

            // variables
            public readonly string path;
            private readonly List<string> listFileTypes = Enum.GetNames(typeof(InputFileTypes)).ToList();
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
                FileType = (InputFileTypes)listFileTypes.IndexOf(UserSettings.Default.InputFileType);
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

            // add rows to data table and list
            foreach (string path in filesToAdd)
            {
                RawFile rawFile = new(path);

                // add raw file properties to data table
                object[] values = { $"{rawFile.Name}{rawFile.Extension}", $"{ rawFile.FileSize } MB" , rawFile.CreationTime };
                dataTableFiles.Rows.Add(values);

                // add raw file object to list
                listRawFiles.Add(rawFile);
            }
        }

        /// <summary>
        /// Method to remove files from list.
        /// </summary>
        /// <param name="filesToRemove"></param>
        static void RemoveFiles(int[] filesToRemove)
        {
            // delete files from data tabel and list
            foreach (int id in filesToRemove)
            {
                dataTableFiles.Rows[id].Delete();
                listRawFiles.RemoveAt(id);
            }
        }
    }
}
