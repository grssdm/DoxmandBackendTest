﻿using System;
using System.Text;

namespace DoxmandBackend.Common
{
    public class CommonMethods
    {
        private static readonly string _key = "3XgsdmUU3Hfn52y4Cu4nBmyjsUJxKrwNz7N2NvB5";

        public static string EncryptPassword(string password)
        {
            password += _key;
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(passwordBytes);
        }
    }
}
