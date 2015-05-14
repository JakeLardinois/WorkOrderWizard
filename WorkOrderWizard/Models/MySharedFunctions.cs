using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkOrderWizard.Models
{
    public static class MySharedFunctions
    {
        public static double? TryParseNullableDouble(string text)
        {
            double value;
            return double.TryParse(text, out value) ? value : (double?)null;
        }
    }
}