
namespace JoeBlogs
{
    static class StringExtensionMethods
    {
        public static string ToUpperFirstLetter(this string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            // convert to char array of the string
            char[] letters = source.ToCharArray();

            // upper case the first char
            letters[0] = char.ToUpper(letters[0]);

            // return the array made of the new char array
            return new string(letters);
        }
    }
}