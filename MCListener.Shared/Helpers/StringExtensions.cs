using System;

namespace MCListener.Shared.Helpers
{
    public static class StringExtensions
    {
        public static string GetFromIndex(this string[] data, int idx)
        {
            if (data?.Length > idx) { return data[idx]; }
            return null;
        }

        public static int? GetIntFromIndex(this string[] data, int idx)
        {
            var str = GetFromIndex(data, idx);
            if(string.IsNullOrWhiteSpace(str)) { return null; }
            if(Int32.TryParse(str, out int res))
            {
                return res;
            }
            return null;
        }
    }
}
