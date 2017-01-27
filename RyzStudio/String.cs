using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RyzStudio
{
    public class String
    {
        public static string EncodeTo64(string value)
        {
            try
            {
                byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(value);
                return System.Convert.ToBase64String(toEncodeAsBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string DecodeFrom64(string value)
        {
            try
            {
                byte[] encodedDataAsBytes = System.Convert.FromBase64String(value);
                return System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}