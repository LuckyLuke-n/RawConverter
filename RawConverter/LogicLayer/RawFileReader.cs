using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawConverter
{
    class RawFileReader
    {
        public string InputFileType { get; set; }
        public string[] FilesRead { get; }
                
        /// <summary>
        /// Class to read the selected files into a list.
        /// </summary>
        public RawFileReader(string[] filesToRead)
        {
            // set property
            FilesRead = filesToRead;
        }

        /// <summary>
        /// Method to read an array of raw files into a list.
        /// </summary>
        /// <returns>The list containinf all read raw files.</returns>
        public List<RawFile> Read()
        {
            List<RawFile> fileList = new();




            return fileList;
        }
    }
}
