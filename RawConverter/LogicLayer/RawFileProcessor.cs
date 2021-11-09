using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using ImageMagick;

namespace RawConverter
{
    static class RawFileProcessor
    {
        // ATTRIBUTES
        private static readonly List<string> listInputFileTypes = Enum.GetNames(typeof(InputFileTypes)).ToList();
        private static readonly List<string> listOutputFileTypes = Enum.GetNames(typeof(OutputFileTypes)).ToList();

        // PROPERTIES
        /// <summary>
        /// List of raw files to be processed later on.
        /// </summary>
        public static ObservableCollection<RawFile> RawFiles { get; set; } = new();
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
            public string FullName { get; private set; }

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
                FullName = Name + Extension;
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

            foreach (RawFile rawFile in RawFiles)
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
            // add rows to data table and list
            foreach (string path in filesToAdd)
            {
                RawFile rawFile = new(path);

                // add raw file object to list
                RawFiles.Add(rawFile);
            }
        }

        /// <summary>
        /// Method to remove files from list and data table.
        /// </summary>
        /// <param name="filesToRemove"></param>
        public static void RemoveFiles(List<int> indicesToRemove)
        {
            // sort the index list ascending than reverse it in order to iterate through the list and delete items from behind
            indicesToRemove.Sort();
            indicesToRemove.Reverse();

            foreach (int index in indicesToRemove)
            {
                RawFiles.RemoveAt(index);
            }
        }

        /// <summary>
        /// Method to remove all files from the list and data table.
        /// </summary>
        public static void RemoveAllFiles()
        {
            // clear list
            RawFiles.Clear();
        }

    }
}
