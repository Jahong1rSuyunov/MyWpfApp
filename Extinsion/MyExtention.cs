using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWpfApp.Extinsion
{
    public static class MyExtention
    {
        public static string CutStringUpToDot(this string input)
        {
            int dotIndex = input.IndexOf('.');
            if (dotIndex == -1)
            {
                return input;
            }
            else
            {
                return input.Substring(0, dotIndex);
            }

        }

        public static string ConvertType(this string mimeType)
        {
            var result = string.Empty;
            switch (mimeType)
            {
                case "text/csv":
                    result = "csv";
                    break;

                case "application/vnd.google-apps.spreadsheet":
                    result = "xlsx";
                    break;

                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    result = "xlsx";
                    break;

                case "application/vnd.ms-excel":
                    result = "xlsx";
                    break;

                case "application/vnd.google-apps.folder":
                    result = "folder";
                    break;

                default:
                    result = "noune";
                    break;
            }
            return result;

        }

        public static string ReplaceSpaceWithCharacter(this string input, char replaceWith)
        {
            return input.Replace(' ', replaceWith);
        }


    }
}
