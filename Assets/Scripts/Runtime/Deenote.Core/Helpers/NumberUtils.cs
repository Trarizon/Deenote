namespace Deenote.Core.Helpers
{
    public static class NumberUtils
    {
        public static void SortAsc(ref float left,ref float right)
        {
            if (left > right) {
                (left, right) = (right, left);
            }
        }
    }
}