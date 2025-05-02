namespace DHI.Services.Jobs.Executer;

using System.Collections.Specialized;
using System.Text.RegularExpressions;

public class CommandLineParser
{
    private readonly StringDictionary _parameters;

    public CommandLineParser(string[] args)
    {
        _parameters = new StringDictionary();
        var spliter = new Regex(@"^-{1,2}|^/|=", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        var remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        string? parameter = null;

        // Valid parameters forms:
        // {-,/,--}param{ ,=,:}((",')value(",'))
        // Examples: 
        // -param1 value1 --param2 /param3:"Test-:-work" 
        //   /param4=happy -param5 '--=nice=--'
        foreach (var txt in args)
        {
            var parts = spliter.Split(txt, 3);
            switch (parts.Length)
            {
                case 1:
                    if (parameter != null)
                    {
                        if (!_parameters.ContainsKey(parameter))
                        {
                            parts[0] = remover.Replace(parts[0], "$1");
                            _parameters.Add(parameter, parts[0]);
                        }

                        parameter = null;
                    }
                    break;
                case 2:
                    if (parameter != null)
                    {
                        if (!_parameters.ContainsKey(parameter))
                        {
                            _parameters.Add(parameter, "true");
                        }
                    }
                    parameter = parts[1];
                    break;
                case 3:
                    if (parameter != null)
                    {
                        if (!_parameters.ContainsKey(parameter))
                        {
                            _parameters.Add(parameter, "true");
                        }
                    }
                    parameter = parts[1];

                    if (!_parameters.ContainsKey(parameter))
                    {
                        parts[2] = remover.Replace(parts[2], "$1");
                        _parameters.Add(parameter, parts[2]);
                    }

                    parameter = null;
                    break;
            }
        }

        if (parameter != null)
        {
            if (!_parameters.ContainsKey(parameter))
            {
                _parameters.Add(parameter, "true");
            }
        }
    }

    public string? this[string param] => _parameters[param];
}