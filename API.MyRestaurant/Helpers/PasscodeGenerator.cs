using System;
using System.Security.Cryptography;
using System.Text;

namespace API.MyRestaurant.Helpers
{
    public static class PasscodeGenerator
    {
        public static string Generate(int length = 6)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var result = new StringBuilder(length);
            foreach (var b in bytes)
            {
                result.Append(validChars[b % validChars.Length]);
            }

            return result.ToString();
        }
    }
}
