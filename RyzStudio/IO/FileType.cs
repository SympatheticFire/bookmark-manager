using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RyzStudio.IO
{
    public class FileType
    {
        protected static readonly byte[] BMP = { 66, 77 };
        protected static readonly byte[] GIF = { 71, 73, 70, 56 };
        protected static readonly byte[] ICO = { 0, 0, 1, 0 };
        protected static readonly byte[] JPG = { 255, 216, 255 };
        protected static readonly byte[] PNG = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 };

        public static bool IsImage(byte[] byteArray)
        {
            if (byteArray == null)
            {
                return false;
            }

            if (byteArray.Length <= 0)
            {
                return false;
            }

            if (byteArray.Take(2).SequenceEqual(BMP))
            {
                return true;
            }

            if (byteArray.Take(4).SequenceEqual(GIF))
            {
                return true;
            }

            if (byteArray.Take(4).SequenceEqual(ICO))
            {
                return true;
            }

            if (byteArray.Take(3).SequenceEqual(JPG))
            {
                return true;
            }

            if (byteArray.Take(16).SequenceEqual(PNG))
            {
                return true;
            }

            return false;
        }
    }
}
