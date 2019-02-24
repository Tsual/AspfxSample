using System;
using System.Collections.Generic;
using System.Text;

namespace Helper
{
    class RandomString
    {
        static readonly Random random = new Random();
        const string ef16_pfc = "0123456789ABCDEF";
        public static string ef16(int length)
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (length-- > 0)
            {
                stringBuilder.Append(ef16_pfc[random.Next(16)]);
            }
            return stringBuilder.ToString();
        }
    }
}
