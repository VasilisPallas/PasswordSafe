using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordSafe
{
    class Secure
    {
        public static string Converttohex(string password)
        {
            char[] values = password.ToCharArray();
            string hexOutput = "";
            string hex = "";
            foreach (char letter in values)
            {
                // Get the integral value of the character. 
                int value = Convert.ToInt32(letter);
                // Convert the decimal value to a hexadecimal value in string form. 
                hexOutput = String.Format("{0:X}", value);
                hex += hexOutput + "Z";
            }
            hex = hex.Remove(hex.Length - 1, 1) + "";

            return hex;
        }



        public static string Converttostring(string password)
        {
            string[] hexValuesSplit = password.Split('Z');
            string pass = "";
            foreach (string hex in hexValuesSplit)
            {
                // Convert the number expressed in base-16 to an integer. 
                int value = Convert.ToInt16(hex, 16);
                // Get the character corresponding to the integral value. 
                string stringValue = Char.ConvertFromUtf32(value);
                char charValue = (char)value;
                pass += charValue;
            }
            return pass;

        }
    }
}
