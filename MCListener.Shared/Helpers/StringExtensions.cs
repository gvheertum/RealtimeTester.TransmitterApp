namespace MCListener.Shared.Helpers
{
    public static class StringExtensions
    {
        public static string GetFromIndex(this string[] data, int idx)
        {
            if (data?.Length > idx) { return data[idx]; }
            return null;
        }
    }
}
