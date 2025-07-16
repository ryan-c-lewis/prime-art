namespace TwinPrimePalindromes;

public class PalindromeGenerator
{
    public static List<string> GenerateBinaryPalindromes(int n)
    {
        var result = new List<string>();
        int halfLength = (n + 1) / 2;
        int limit = 1 << halfLength;  // 2^(n+1)/2

        for (int i = 0; i < limit; i++)
        {
            string firstHalf = Convert.ToString(i, 2).PadLeft(halfLength, '0');
            string fullPalindrome = n % 2 == 0
                ? firstHalf + Reverse(firstHalf)
                : firstHalf + Reverse(firstHalf.Substring(0, halfLength - 1));
            result.Add(fullPalindrome);
        }

        return result;
    }

    private static string Reverse(string s)
    {
        char[] arr = s.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }
}