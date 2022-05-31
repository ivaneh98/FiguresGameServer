using System.Text.RegularExpressions;

namespace FiguresGameServer
{
    internal class Utilites
    {
        public static DateTime ConvertToDateTime(string str)
        {
            string pattern = @"(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})";
            if (Regex.IsMatch(str, pattern))
            {
                Match match = Regex.Match(str, pattern);
                int year = Convert.ToInt32(match.Groups[1].Value);
                int month = Convert.ToInt32(match.Groups[2].Value);
                int day = Convert.ToInt32(match.Groups[3].Value);
                int hour = Convert.ToInt32(match.Groups[4].Value);
                int minute = Convert.ToInt32(match.Groups[5].Value);
                int second = Convert.ToInt32(match.Groups[6].Value);
                return new DateTime(year, month, day, hour, minute, second);
            }
            else
            {
                pattern = @"(\d{2})/(\d{2})/(\d{4}) (\d{2}):(\d{2}):(\d{2})";
                if (Regex.IsMatch(str, pattern))
                {
                    Match match = Regex.Match(str, pattern);
                    int day = Convert.ToInt32(match.Groups[1].Value);
                    int year = Convert.ToInt32(match.Groups[3].Value);
                    int month = Convert.ToInt32(match.Groups[2].Value);
                    int hour = Convert.ToInt32(match.Groups[4].Value);
                    int minute = Convert.ToInt32(match.Groups[5].Value);
                    int second = Convert.ToInt32(match.Groups[6].Value);
                    return new DateTime(year, month, day, hour, minute, second);
                }
                else
                {
                    pattern = @"(\d{4})/(\d{2})/(\d{2}) (\d{2}):(\d{2}):(\d{2})";
                    if (Regex.IsMatch(str, pattern))
                    {
                        Match match = Regex.Match(str, pattern);
                        int year = Convert.ToInt32(match.Groups[1].Value);
                        int month = Convert.ToInt32(match.Groups[2].Value);
                        int day = Convert.ToInt32(match.Groups[3].Value);
                        int hour = Convert.ToInt32(match.Groups[4].Value);
                        int minute = Convert.ToInt32(match.Groups[5].Value);
                        int second = Convert.ToInt32(match.Groups[6].Value);
                        return new DateTime(year, month, day, hour, minute, second);
                    }
                    else
                    {
                        throw new Exception("Unable to parse.");
                    }
                }
            }
        }

    }
}
