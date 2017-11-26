namespace Geta.Commerce.ContentModelGenerator.Parsers
{
    public enum ParserState
    {
        Includes = 0,
        Namespace = 1,
        Class = 2,
        Members = 3
    }
}