using System.Collections.Generic;

namespace Geta.Commerce.ContentModelGenerator.Comparers
{
    public class NameSpaceComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int letter = y[0].CompareTo(x[0]);
            if (letter != 0) return letter;

            return x.Length.CompareTo(y.Length);
        }
    }
}