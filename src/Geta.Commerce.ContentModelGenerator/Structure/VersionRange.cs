using System;

namespace Geta.Commerce.ContentModelGenerator.Structure
{
    public class VersionRange
    {
        public Version From { get; set; }
        public Version To { get; set; }

        public static VersionRange Parse(string input)
        {
            var data = input.Split('-');

            return new VersionRange
            {
                From = Version.Parse(data[0]),
                To = Version.Parse(data[1])
            };
        }
    }
}