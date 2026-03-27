namespace superint.ProjectBootstrapper.Shared.Extensions
{
    public static class StringExtensions
    {
        public static string ToPascalCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var words = input.Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries);
            var result = string.Concat(words.Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));

            return result;
        }

        public static string ToCamelCase(this string input)
        {
            var pascalCase = input.ToPascalCase();
            if (string.IsNullOrEmpty(pascalCase))
                return pascalCase;

            return char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..];
        }

        public static string ToKebabCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return string.Concat(input.Select((c, i) => i > 0 && char.IsUpper(c) ? "-" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
        }
    }
}