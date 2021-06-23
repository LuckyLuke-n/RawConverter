using System;
using System.Windows.Media.Imaging;

namespace RawConverter
{
    class BitmapFromPath
    {
        // attributes for this class
        private readonly BitmapImage image = new();

        /// <summary>
        /// Creates an instance of the BitmapFromPath class. The field "image" contains the Bitmap
        /// </summary>
        /// <param name="path"></param>
        public BitmapFromPath()
        {

        }

        /// <summary>
        /// Load image from path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns the BitmapImage object.</returns>
        public BitmapImage Load(string path)
        {
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Relative);
            image.EndInit();

            return image;
        }
    }
}
