namespace ProductAPI.Utilities
{
    public static class RandomGenerator
    {
        public static string GetRandomAlphaNumericValue(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static int GetRandomIntegerValue(int length)
        {
            Random random = new Random();
            const string chars = "0123456789";
            int.TryParse( new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray()), out int result);
            return result;
        }
    }
}
