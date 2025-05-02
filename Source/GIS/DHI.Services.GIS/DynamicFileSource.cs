namespace DHI.Services.GIS
{
    using System;
    using System.IO;

    public class DynamicFileSource
    {
        private readonly string _pattern;

        public DynamicFileSource(string pattern)
        {
            _pattern = pattern;
        }

        public static string ReplaceWithDateTime(string s, DateTime dateTime)
        {
            var parts = s.Split('?');
            var replacedString = "";
            for (var i = 0; i < parts.Length; i++)
            {
                if (i % 2 == 0)
                {
                    replacedString += parts[i];
                }
                else
                {
                    var formatFirstBreaks = parts[i].Split('|');
                    var dateTimeFormat = formatFirstBreaks[0];
                    if (dateTimeFormat == "hour")
                    {
                        var h = dateTime.Hour;
                        var hVal = dateTime.ToString("HH");
                        for (var j = 1; j < formatFirstBreaks.Length; j++)
                        {
                            var hourFormatBreaks = formatFirstBreaks[j].Split(':');
                            var hMin = Convert.ToDouble(hourFormatBreaks[0]);
                            var hMax = Convert.ToDouble(hourFormatBreaks[1]);
                            if (h >= hMin && h <= hMax)
                            {
                                hVal = hourFormatBreaks[2];
                                break;
                            }
                        }

                        replacedString += dateTime.ToString(hVal);
                    }
                    else
                    {
                        replacedString += dateTime.ToString(dateTimeFormat);
                    }
                }
            }

            return replacedString;
        }

        public static string ReplaceWithParameter(string s, Parameters parameters)
        {
            var parts = s.Split('"');
            var replacedString = "";
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (i % 2 == 0)
                {
                    replacedString += part;
                }
                else
                {
                    replacedString += parameters[part];
                }
            }

            return replacedString;
        }

        public static string BestChoiceFile(string s)
        {
            var parts = s.Split(':');
            if (parts.Length > 1)
            {
                var choicePattern = parts[1];
                var choices = choicePattern.Split('|');
                for (var i = 0; i < choices.Length; i++)
                {
                    var choiceFile = s.Replace(":" + choicePattern + ":", choices[i]);
                    if (File.Exists(choiceFile))
                    {
                        return choiceFile;
                    }
                }

                return null;
            }

            if (File.Exists(s))
            {
                return s;
            }

            return null;
        }

        public string GetFile(DateTime timestamp, Parameters parameters = null)
        {
            var file = _pattern;
            file = ReplaceWithDateTime(file, timestamp);
            if (parameters != null)
            {
                file = ReplaceWithParameter(file, parameters);
            }

            file = BestChoiceFile(file);
            return file;
        }
    }
}
