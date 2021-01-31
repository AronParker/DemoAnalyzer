using DemoInfo;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DemoAnalyzer
{
    public class TeamToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var team = (Team)value;

            switch (parameter)
            {
                case "headline":
                    {
                        switch (team)
                        {
                            case Team.Terrorist:
                                return Brushes.IndianRed;
                            case Team.CounterTerrorist:
                                return Brushes.MediumSlateBlue;
                            case Team.Spectate:
                                return Brushes.LightGray;
                            default:
                                throw new Exception("Unsupported team");
                        }
                    }
                case "body":
                    {
                        switch (team)
                        {
                            case Team.Terrorist:
                                return Brushes.LightPink;
                            case Team.CounterTerrorist:
                                return Brushes.LightBlue;
                            case Team.Spectate:
                                return Brushes.White;
                            default:
                                throw new Exception("Unsupported team");
                        }
                    }
                default:
                    throw new Exception("Unsupported parameter");
            }

        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
