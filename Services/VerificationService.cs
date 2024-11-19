using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class VerificationService
    {
        public static bool isValidEmail(string email)
        {
            var regularExpression = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            if (regularExpression.IsMatch(email))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public static bool isValidBirthDate(string birthDateString) {
            DateTime dt;
            bool dateConverted = DateTime.TryParse(birthDateString, out dt);
            if (dateConverted)
            {
                return true;
            }
            else
            {
                return false;
            }            
        }
        public static bool isValidCategory(string category)
        {
            if (category == "1" || category == "2")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool isValidUserName(string userName) {
            var regularExpression = new Regex(@"^[^\b]*$");
            if (regularExpression.IsMatch(userName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool isValidPassword(string password)
        {
            var regularExpression = new Regex(@"^[^\b]*$");
            if (regularExpression.IsMatch(password))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
