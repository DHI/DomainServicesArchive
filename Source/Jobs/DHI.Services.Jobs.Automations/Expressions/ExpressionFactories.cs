namespace DHI.Services.Jobs.Automations.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class ExpressionFactories
    {
        private static readonly Regex _subExpressionRegex = new Regex(@"\((?>\((?<c>)|[^()]+|\)(?<-c>))*(?(c)(?!))\)", RegexOptions.Compiled);
        private static readonly Regex _expressionRegex = new Regex(@"[^()]*(?=(?:[^)]*\([^(]*\))*[^()]*$)", RegexOptions.Compiled);

        private static TILogical Parse<TILogical>(this string rulesNotation, string expressionSplitString, ref Dictionary<string, bool> items) where TILogical : List<ILogical>
        {
            var result = (List<ILogical>)Activator.CreateInstance<TILogical>();
            var subExpressionMatch = _subExpressionRegex.Match(rulesNotation);
            var expressionMatchString = _subExpressionRegex.Replace(rulesNotation, string.Empty);
            var expressionMatch = _expressionRegex.Match(expressionMatchString);
            var matchList = new List<(bool IsSubExpression, Match Match)>();

            while (subExpressionMatch.Success)
            {
                if (!string.IsNullOrEmpty(subExpressionMatch.Value))
                {
                    matchList.Add((true, subExpressionMatch));
                }

                subExpressionMatch = subExpressionMatch.NextMatch();
            }

            while (expressionMatch.Success)
            {
                if (!string.IsNullOrEmpty(expressionMatch.Value))
                {
                    matchList.Add((false, expressionMatch));
                }

                expressionMatch = expressionMatch.NextMatch();
            }

            if (!matchList.Any())
            {
                throw new ExpressionParsingException($"No expressions or sub expressions could be matched from {rulesNotation}");
            }

            if (matchList.Sum(m => m.Match.Length) != rulesNotation.Length)
            {
                throw new ExpressionParsingException($"Matched sections of {rulesNotation} did not comprise the full statement {matchList.Select(m => m.Match.Value).Aggregate((p, n) => $"{p}, {n}")}");
            }

            foreach (var tuple in matchList.OrderBy(ml => ml.Match.Index))
            {
                if (tuple.IsSubExpression)
                {
                    var expText = tuple.Match.Value.Substring(1, tuple.Match.Value.Length - 2);
                    var subExpressionTypeMatch = _subExpressionRegex.Replace(expText, string.Empty);

                    if (subExpressionTypeMatch.Contains(" OR "))
                    {
                        result.Add(OrFromExpressionNotation(tuple.Match.Value, ref items));
                    }
                    else
                    {
                        result.Add(AndFromExpressionNotation(tuple.Match.Value, ref items));
                    }
                }
                else
                {
                    var split = tuple.Match.Value.Split(new string[] { expressionSplitString }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in split)
                    {
                        result.Add(new LeafType() { ItemKey = item, EvaluatedValue = items[item] });
                    }
                }
            }

            return (TILogical)result;
        }

        private static Or OrFromExpressionNotation(string rulesNotation, ref Dictionary<string, bool> items)
        {
            rulesNotation = rulesNotation.Substring(1, rulesNotation.Length - 2);

            try
            {
                return rulesNotation.Parse<Or>(" OR ", ref items);
            }
            catch (ExpressionParsingException ex)
            {
                throw new ExpressionParsingException($"Downstream parsing failed of OR expression {rulesNotation}", ex);
            }
        }

        private static And AndFromExpressionNotation(string rulesNotation, ref Dictionary<string, bool> items)
        {
            rulesNotation = rulesNotation.Substring(1, rulesNotation.Length - 2);

            try
            {
                return rulesNotation.Parse<And>(" AND ", ref items);
            }
            catch (ExpressionParsingException ex)
            {
                throw new ExpressionParsingException($"Downstream parsing failed of AND expression {rulesNotation}", ex);
            }
        }

        public static bool EvaluateExpressionNotation(string rulesNotation, ref Dictionary<string, bool> items)
        {
            var rootTrue = Guid.NewGuid().ToString();
            items.Add(rootTrue, true);
            rulesNotation = $"({rulesNotation}) AND {rootTrue}";
            if (string.IsNullOrEmpty(rulesNotation))
            {
                return false;
            }

            var root = rulesNotation.Parse<List<ILogical>>(" AND ", ref items);

            return root
                .Select(node => node.Evaluate())
                .Aggregate(true, (current, nodeResult) => current && nodeResult);
            
        }
    }
}