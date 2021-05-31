using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

namespace RawConverter
{
    class ddRawFile
    {
        // properties
        public string Name { get; }
        public DateTime CreationTime { get; }
        public double FileSize { get; }
        private OutputFileTypes OutputFileType { get; set; }

        // variables
        private readonly string path;


        /// <summary>
        /// Creates a new RawFile object only reading the FileInfo.
        /// </summary>
        public ddRawFile(string path)
        {
            this.path = path;
            FileInfo fileInfo = new(path);
            Name = fileInfo.Name;
            CreationTime = fileInfo.CreationTime;
            FileSize = Math.Round((float)fileInfo.Length / 1000000, 3, MidpointRounding.AwayFromZero); // B to MB
            OutputFileType = RawFileProcessor.OutputFileType;
        }

        /// <summary>
        /// Method to convert the raw file to the desired type.
        /// </summary>
        /// <param name="type"></param>
        public void Convert(OutputFileTypes type, string outputFolder)
        {
            string filename = Path.Combine(paths: new string[] { outputFolder, Name, ".", type.ToString() });
            Stream imageStreamSource = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            // decode according selection
            //switch ()

            // convert according selection
            switch (type)
            {
                case OutputFileTypes.tiff:
                    break;

                case OutputFileTypes.jpg:
                    break;

                default:
                    //type is .jpg
                    //newBitmap.ToJPG(filename: Path.Combine(new string[] { outputFolder, Name }));
                    break;
            }
        }
    }
}
