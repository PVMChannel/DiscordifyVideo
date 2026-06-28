using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace DiscordifyVideo;

public class TimeSpanConverter : IValueConverter
{
    public static readonly TimeSpanConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            if (targetType.IsAssignableTo(typeof(double)))
            {
                return timeSpan.TotalMilliseconds;
            }
            else if (targetType.IsAssignableTo(typeof(string)))
            {
                return $"{timeSpan.Hours.ToString().PadLeft(2, '0')}:{timeSpan.Minutes.ToString().PadLeft(2, '0')}:{timeSpan.Seconds.ToString().PadLeft(2, '0')}.{Math.Round((double) timeSpan.Milliseconds / 100).ToString().PadLeft(2, '0')}";
            }
        }
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), 
                                                    BindingErrorType.Error);
        
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is double milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds);
        }
        if(value is string timeString)
        {
            try
            {
                string[] timeStringSplitByDot = timeString.Split('.');
                int[] timeStringSplitByColon = timeStringSplitByDot[0].Split(':').Select(val => Int32.Parse(val)).ToArray();
                return new TimeSpan(0, timeStringSplitByColon[0], timeStringSplitByColon[1], timeStringSplitByColon[2], Int32.Parse(timeStringSplitByDot[1].PadRight(4, '0')));
            }
            catch (Exception)
            {
                return BindingOperations.DoNothing;
            }
        }

        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), 
                                                    BindingErrorType.Error);
    }
}