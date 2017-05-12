using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIDTuner1
{
    public class Casting
    {
        Encoding ascii = Encoding.ASCII;
        Encoding unicode = Encoding.Unicode;
        

        public string utfToAscii(string str)
        {
            byte[] uni = Encoding.Unicode.GetBytes(str);

            // Convert to ASCII
            byte[] asciiB = Encoding.Convert(unicode, ascii, uni);
            char[] asciiC = new char[ascii.GetCharCount(asciiB, 0, asciiB.Length)];
            ascii.GetChars(asciiB, 0, asciiB.Length, asciiC, 0);
            string asciiS = new string(asciiC);
            Console.WriteLine(asciiS);

            return asciiS;
        }

        public string asciiToUtf(string str)
        {

            byte[] asc = Encoding.ASCII.GetBytes(str);
            byte[] uniB = Encoding.Convert(ascii, unicode, asc);

            char[] uniC = new char[unicode.GetCharCount(uniB, 0, uniB.Length)];
            unicode.GetChars(uniB, 0, uniB.Length, uniC, 0);
            string uniS = new string(uniC);

            Console.WriteLine(uniS);

            return uniS;
        }

        public byte[] stringToArray(string str)
        {
            byte[] asciiB = BitConverter.GetBytes(str);
            return asciiB;
        }

        public byte[] floatToArray(float flt)
        {
            byte[] floatB = BitConverter.GetBytes(flt);
            return floatB;
        }

        public float arrayToFloat(List<byte> list)
        {
            byte[] arr = list.ToArray();
            float flt = BitConverter.ToSingle(arr, 0);
            return flt;

        }

        public string arrayToString(List<byte> list)
        {
            byte[] arr = list.ToArray();
            string str = BitConverter.ToString(arr);
            return str;
        }

    }



}
