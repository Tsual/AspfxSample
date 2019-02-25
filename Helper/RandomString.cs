using System;
using System.Collections.Generic;
using System.Text;

namespace Helper
{
    class Byte16String
    {
        static readonly Random random = new Random();
        const string ef16_pfc = "0123456789ABCDEF";
        public static string RandomString(int length)
        {
            byte[] bta = new byte[length];
            random.NextBytes(bta);
            return Encode(bta);
        }
        public static byte[] Decode(string str)
        {
            byte[] result = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
                result[i] =Convert.ToByte( ef16_pfc.IndexOf(str[i]));
            return result;
        }
        public static string Encode(byte[] bta) {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var b in bta)
                stringBuilder.Append(ef16_pfc[b]);
            return stringBuilder.ToString();
        }
    }
}
