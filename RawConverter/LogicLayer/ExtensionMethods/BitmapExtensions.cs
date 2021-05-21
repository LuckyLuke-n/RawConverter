using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RawConverter
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Converts the bitmap image into a jpg image. The quality is set to "lossless"
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="filename"></param>
        public static void ToJPG(this Bitmap bitmap, string filename)
        {
            EncoderParameters encoderParameters = new(count: 1);
            encoderParameters.Param[0] = new EncoderParameter(encoder: System.Drawing.Imaging.Encoder.Quality, value: 100L);
            bitmap.Save(filename, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        public static void SaveJPG100(this Bitmap bmp, Stream stream)
        {
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            bmp.Save(stream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }
    }
}
