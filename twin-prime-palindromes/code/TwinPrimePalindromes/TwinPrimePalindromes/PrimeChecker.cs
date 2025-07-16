using System.Numerics;

namespace TwinPrimePalindromes;

public static class PrimeChecker
{
    private static HashSet<BigInteger> _notPrimes = new HashSet<BigInteger>();
    private static HashSet<BigInteger> _primes = new HashSet<BigInteger>();

    public static bool IsPrime(BigInteger number)
    {
        if (number <= 1) return false;
        if (number == 2 || number == 3) return true;
        if (number % 2 == 0 || number % 3 == 0) return false;

        if (_notPrimes.Contains(number))
            return false;
        if (_primes.Contains(number))
            return true;

        BigInteger limit = Sqrt(number);
        for (BigInteger i = 5; i <= limit; i += 6)
        {
            if (number % i == 0 || number % (i + 2) == 0)
            {
                _notPrimes.Add(number);
                return false;
            }
        }

        _primes.Add(number);
        return true;
    }
    
    // Miller-Rabin primality test
    public static bool IsProbablyPrime(BigInteger n, int witnesses = 10)
    {
        if (n <= 1) return false;
        if (n == 2 || n == 3) return true;
        if (n % 2 == 0) return false;

        // Write n - 1 as 2^r * d
        BigInteger d = n - 1;
        int r = 0;
        while (d % 2 == 0)
        {
            d /= 2;
            r++;
        }

        Random rng = new Random();
        byte[] bytes = new byte[n.ToByteArray().LongLength];

        for (int i = 0; i < witnesses; i++)
        {
            BigInteger a;
            do
            {
                rng.NextBytes(bytes);
                a = new BigInteger(bytes);
            }
            while (a < 2 || a >= n - 2);

            BigInteger x = BigInteger.ModPow(a, d, n);
            if (x == 1 || x == n - 1)
                continue;

            bool passed = false;
            for (int j = 0; j < r - 1; j++)
            {
                x = BigInteger.ModPow(x, 2, n);
                if (x == n - 1)
                {
                    passed = true;
                    break;
                }
            }

            if (!passed)
                return false;
        }

        return true;
    }

    // Integer square root for BigInteger
    private static BigInteger Sqrt(BigInteger n)
    {
        if (n <= 0) return 0;
        if (n < 4) return 1;

        BigInteger left = 1;
        BigInteger right = n;

        while (left <= right)
        {
            BigInteger mid = (left + right) / 2;
            BigInteger midSquared = mid * mid;

            if (midSquared == n)
                return mid;
            if (midSquared < n)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return right;
    }
}