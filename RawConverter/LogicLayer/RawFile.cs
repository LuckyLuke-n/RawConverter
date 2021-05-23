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
    class RawFile
    {
        // properties
        public FileInfo FileInfo { get; }
        public static string OutputFolder { get; set; }

        // variables
        private readonly string path;


        /// <summary>
        /// Creates a new RawFile object only reading the FileInfo.
        /// </summary>
        public RawFile(string path)
        {
            this.path = path;
            FileInfo fileInfo = new(path);
            FileInfo = fileInfo;
        }

        /// <summary>
        /// Method to convert the raw file to the desired type.
        /// </summary>
        /// <param name="type"></param>
        public void Convert(OutputFileTypes type)
        {
            string filename = Path.Combine(paths: new string[] { OutputFolder, FileInfo.Name, ".", type.ToString() });
            using (Stream rawFileStream = File.OpenRead(path))
            {
                /*
                // umbauen auf extension method?
                Image image = Image.FromStream(rawFileStream);
                image.Save(filename, ImageFormat.Jpeg);
                */
                
                Bitmap newBitmap = new(Image.FromStream(rawFileStream));
                newBitmap.ToJPG(filename: Path.Combine(new string[] { OutputFolder, FileInfo.Name}));
            }
        }


    }
}
