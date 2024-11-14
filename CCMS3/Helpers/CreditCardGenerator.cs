using CCMS3.Models;
using System.Text;

namespace CCMS3.Helpers
{
    public static class CreditCardGenerator
    {
        private readonly static Random random = new();
        public static string GenerateCardNumber(string prefix, int length)
        {

            // Step 1: Start with the IIN prefix
            StringBuilder cardNumber = new(prefix);

            // Step 2: Generate random digits until reaching the specified length - 1 (excluding the check digit)
            while (cardNumber.Length < length - 1)
            {
                cardNumber.Append(random.Next(0, 10));
            }

            // Step 3: Calculate the Luhn check digit and append it to the card number
            int checkDigit = CalculateLuhnCheckDigit(cardNumber.ToString());
            cardNumber.Append(checkDigit);

            return cardNumber.ToString();
        }

        private static int CalculateLuhnCheckDigit(string number)
        {
            int sum = 0;
            bool isSecond = true;

            // Start from the rightmost digit and move to the left
            for (int i = number.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(number[i].ToString());

                if (isSecond)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
                isSecond = !isSecond;
            }

            // Calculate the check digit
            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit;
        }

        public static string GenerateCVV(int length = 3)
        {
            // Ensure the CVV length is 3 or 4
            if (length < 3 || length > 4)
                throw new ArgumentException("CVV length must be 3 or 4 digits.");

            // Generate a random CVV with the specified length
            StringBuilder cvv = new();
            for (int i = 0; i < length; i++)
            {
                cvv.Append(random.Next(0, 10));
            }

            return cvv.ToString();
        }


        public static CreditLimitResponse CalculateCreditLimitAndInterest(decimal annualIncome, int employmentStatus)
        {
            var creditLimitResponse = new CreditLimitResponse();

            // Determine credit limit based on income bands and employment status
            if (annualIncome >= 100000 && (employmentStatus == 3 || employmentStatus == 1))
            {
                creditLimitResponse.CreditLimit = 50000;
                creditLimitResponse.InterestRate = 0.15M; // 15% interest rate for high-income, stable employment
            }
            else if (annualIncome >= 50000 && employmentStatus == 3)
            {
                creditLimitResponse.CreditLimit = 20000;
                creditLimitResponse.InterestRate = 0.18M; // 18% interest rate for medium income, full-time employment
            }
            else if (annualIncome >= 50000 && employmentStatus == 4)
            {
                creditLimitResponse.CreditLimit = 15000;
                creditLimitResponse.InterestRate = 0.20M; // 20% interest rate for medium income, part-time employment
            }
            else if (annualIncome >= 30000 && employmentStatus == 3)
            {
                creditLimitResponse.CreditLimit = 10000;
                creditLimitResponse.InterestRate = 0.22M; // 22% interest rate for lower income, full-time employment
            }
            else
            {
                creditLimitResponse.CreditLimit = 5000;
                creditLimitResponse.InterestRate = 0.25M; // 25% interest rate for low income or unstable employment
            }

            return creditLimitResponse;
        }
    }

    public class CreditLimitResponse
    {
        public decimal CreditLimit { get; set; }
        public decimal InterestRate { get; set; }
    }
}
