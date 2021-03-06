﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace DM_Skills.Converters
{
    /// <summary>
    /// Klassen bruger interface`et IValueConverter
    /// Klassen laver en timespan om til det format 
    /// vi gerne vil have vist
    /// </summary>
    class TimeSpanToStringConverter : IValueConverter
    {

        /// <summary>
        /// Lav en TimeSpan om til en læselig tekst
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value== null || !(value is TimeSpan))
                return "";

            //Hent tallet og gør hvis det er mindre en 10 at der er 0 foran
            string minutes      = ((TimeSpan)value).Minutes.ToString().PadLeft(2, '0');
            string seconds      = ((TimeSpan)value).Seconds.ToString().PadLeft(2, '0');
            string milliseconds = (((TimeSpan)value).Milliseconds/10).ToString().PadLeft(2, '0'); //dividere 10 for at fjerne et 0

            //if (parameter != null && parameter.Equals("HideOnNull") && ((TimeSpan)value).TotalMilliseconds == 0)
            //    return "";
            return string.Format("{0}:{1}:{2}", minutes, seconds, milliseconds);
        }

        /// <summary>
        /// Her laver vi en tekst om til en timespan
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeParams = value.ToString().Split(':');
            var isInt = Regex.IsMatch(value as string, @"^\d+:\d+:\d+$");
            if (!isInt)
                return null;


            int minutes = System.Convert.ToInt32(timeParams[0]);
            int seconds = System.Convert.ToInt32(timeParams[1]);
            int mSeconds = System.Convert.ToInt32(timeParams[2])*10;
            return new TimeSpan(0, 0, minutes, seconds, mSeconds);
        }
    }
}
