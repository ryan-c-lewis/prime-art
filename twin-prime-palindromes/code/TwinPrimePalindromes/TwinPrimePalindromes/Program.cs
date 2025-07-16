using System.Numerics;
using System.Text;

namespace TwinPrimePalindromes;

public class Program
{
    public static void Main()
    {
        GeneratePersianRug();
    }

    // Decides how to order the results. Leads to different kinds of visuals.
    public enum OrderingStyle
    {
        Raw, // will strictly order all results by the value itself; leads to fuzzy edges and less distinct figures in the visual
        TrimOnlyStart, // ?? dumb?
        TrimBoth // leads to clean edges and clear figures in the visual; I'm preferring this one currently because of the visual result, but it might prevent it from being mathematically meaningful. idk I'd have to thinka bout it.
    }
    
    public static void GeneratePersianRug()
    {
        OrderingStyle orderingStyle = OrderingStyle.TrimBoth;
        string fileNameForRawPrimes = $"out_primes_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        string fileNameForRawVisual = $"out_visual_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        string fileNameForBitmap = $"out_visual_{DateTime.Now:yyyyMMdd_HHmmss}.bmp";
        string fileNameForProportions = $"out_proportions_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        Dictionary<BigInteger, string> binaryPalindromePrimeMiddles = new Dictionary<BigInteger, string>();
        Dictionary<int, double> proportionThatAreTwinPrimeMiddles = new Dictionary<int, double>();

        int minLength = 1; // start at binary numbers with this many digits
        int maxLength = 40; // end at binary numbers with this many digits
        for (int length = minLength; length < maxLength; length++)
        {
            // Generate all palindromes of this length. e.g. for length 4 it would be:
            //  0000
            //  0110
            //  1001
            //  1111
            List<string> binaryPalindromes = PalindromeGenerator.GenerateBinaryPalindromes(length);
            int twinPrimeMiddleCount = 0;
            
            foreach (string binaryPalindrome in binaryPalindromes)
            {
                try
                {
                    BigInteger binaryPalindromeValue = BinaryStringToBigInteger(binaryPalindrome);
                    // Perfectly precise prime checking would be much slower and probably not change the result very much
                    bool isTwinPrimeMiddle = PrimeChecker.IsProbablyPrime(binaryPalindromeValue - 1)
                                             && PrimeChecker.IsProbablyPrime(binaryPalindromeValue + 1);

                    if (isTwinPrimeMiddle)
                    {
                        //Console.WriteLine($"({length} of {maxLength}) {binaryPalindrome}");
                        binaryPalindromePrimeMiddles[binaryPalindromeValue] = binaryPalindrome;
                        twinPrimeMiddleCount++;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Error on {binaryPalindrome}", e);
                }
            }

            // TODO I assume this is changing similarly to what would be expected from the prime number theorem, but I haven't checked
            proportionThatAreTwinPrimeMiddles[length] = (double)twinPrimeMiddleCount / binaryPalindromes.Count;
            Console.WriteLine($"Proportion at length {length}: {proportionThatAreTwinPrimeMiddles[length]}");
            File.AppendAllLines(fileNameForProportions, [$"{length},{proportionThatAreTwinPrimeMiddles[length]}]"]);
        }

        string TrimForOrdering(string input)
        {
            if (orderingStyle == OrderingStyle.Raw)
                return string.Empty; // only order by the number itself, not its visual appearance
            if (orderingStyle == OrderingStyle.TrimOnlyStart)
                return input.TrimStart('0');
            if (orderingStyle ==  OrderingStyle.TrimBoth)
                return input.Trim('0');
            throw new NotImplementedException(orderingStyle.ToString());
        }

        int longestBinaryLength = binaryPalindromePrimeMiddles.Values.Max(x => x.Length);
        // Decision about how to order the results is important... what are we actually trying to accomplish? It's not obvious.
        List<KeyValuePair<BigInteger, string>> orderedPalindromes = binaryPalindromePrimeMiddles
            .OrderBy(pair => TrimForOrdering(pair.Value).Length)
            .ThenBy(x => x.Key)
            .ToList();
            
        foreach (KeyValuePair<BigInteger, string> palindrome in orderedPalindromes)
        {
            // Dump the raw values into a file so we can have that if needed
            File.AppendAllLines(fileNameForRawPrimes, [palindrome.Key + "," + palindrome.Value]);
            
            // Create an ASCII visual equivalent to the bitmap one
            StringBuilder line =  new StringBuilder();
            int paddingToAddPerSide = (longestBinaryLength - palindrome.Value.Length);
            for (int n = 0; n < paddingToAddPerSide; n++)
                line.Append(' ');
            line.Append(palindrome.Value.Replace("0", "  ").Replace("1", "██"));
            for (int n = 0; n < paddingToAddPerSide; n++)
                line.Append(' ');
            Console.WriteLine(line);
            File.AppendAllLines(fileNameForRawVisual, [line.ToString()]);
        }
        
        // Create a bitmap visual. This file isn't formatted properly to be opened in most apps but a browser can open it.
        RawBitmap bmp = new(longestBinaryLength * 2, orderedPalindromes.Count * 2);
        for (int y = 0; y < orderedPalindromes.Count; y++)
        {
            KeyValuePair<BigInteger, string> palindrome = orderedPalindromes[y];
            int offset = longestBinaryLength - palindrome.Value.Length;
            for (int x = 0; x < palindrome.Value.Length; x++)
            {
                bool isOne = palindrome.Value[x] == '1';
                if (isOne)
                {
                    bmp.SetPixel(x*2 + offset, y*2, RawColor.Gray(255));
                    bmp.SetPixel(x*2 + offset + 1, y*2, RawColor.Gray(255));
                    bmp.SetPixel(x*2 + offset, y*2+1, RawColor.Gray(255));
                    bmp.SetPixel(x*2 + offset + 1, y*2+1, RawColor.Gray(255));
                }
            }
        }
        bmp.Save(fileNameForBitmap);
    }
    
    static BigInteger BinaryStringToBigInteger(string binary)
    {
        return BigInteger.Parse("0" + binary, System.Globalization.NumberStyles.AllowHexSpecifier)
            .ToByteArray().Length == 1 && binary.Length <= 8
            ? new BigInteger(Convert.ToByte(binary, 2))
            : ConvertBinaryToBigInteger(binary);
    }

    static BigInteger ConvertBinaryToBigInteger(string binary)
    {
        BigInteger result = 0;
        foreach (char c in binary)
        {
            result <<= 1;
            if (c == '1') result |= 1;
            else if (c != '0') throw new FormatException("Invalid binary character: " + c);
        }
        return result;
    }
}