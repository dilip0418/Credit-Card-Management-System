using System.ComponentModel.DataAnnotations;

namespace CCMS3.Helpers.Validators
{
    public class CreditCardNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string cardNumber && !string.IsNullOrWhiteSpace(cardNumber))
            {
                if (!IsValidCreditCard(cardNumber))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }

        private static bool IsValidCreditCard(string cardNumber)
        {
            // Remove any non-digit characters
            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            // Check if it contains only digits and has valid length
            if (!long.TryParse(cardNumber, out _) || cardNumber.Length < 13 || cardNumber.Length > 19)
            {
                return false;
            }

            // Implement Luhn's algorithm
            int sum = 0;
            bool alternate = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(cardNumber[i].ToString());

                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }

                sum += n;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }
    }
}
