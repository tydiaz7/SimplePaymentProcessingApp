using SimplePaymentProcessingApp.General;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePaymentProcessingApp.Credit
{
    public class GiftorCredit
    {
        /// <summary>
        /// Static, readonly, immutable Dictionary that describes the relationship between card brands and the processing fees associated with them.
        /// </summary>
        private static readonly ImmutableDictionary<CardBrand, decimal> CardBrandFeeMultipliers = new Dictionary<CardBrand, decimal>()
        {
            // TIL 'm' is the suffix for decimal.
            { CardBrand.Visa, 0.04m },
            { CardBrand.MasterCard, 0.08m },
            { CardBrand.Discover, 0.12m },
            { CardBrand.Unknown, 0.16m }
        }
        .ToImmutableDictionary();

        private static readonly ImmutableDictionary<CardBrand, decimal> GiftCardBrandFeeMultipliers = new Dictionary<CardBrand, decimal>()
        {
            {CardBrand.Visa, 0.05m},
            {CardBrand.MasterCard, 0.10m},
            {CardBrand.Discover, 0.15m},
            {CardBrand.Unknown, 0.25m}
        }
        .ToImmutableDictionary();
        public static bool DetermineGiftCard(CreditTransactionRequest request)
        {
            if (request.CVV != null && request.Account != null)
            {
                if (request.Account.EndsWith(request.CVV) && request.Account.Length == 18) //Makes sure all Gift Cards are valid before returning true
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Helper function to calculate the fee for a given card brand.
        /// </summary>
        /// <param name="amount">Amount of money to calculate with.</param>
        /// <param name="cardBrand">Card brand to calculate for.</param>
        /// <returns>Amount of money present in the fee.</returns>
        public static decimal CalculateFee(CreditTransactionRequest request, decimal amount, CardBrand cardBrand)
        {
            if (DetermineGiftCard(request))
            {
                return amount * GiftCardBrandFeeMultipliers[cardBrand];
            }
            else
            {
                return amount * CardBrandFeeMultipliers[cardBrand];
            }
        }
        /// <summary>
        /// Determines the card brand of a given card number by analyzing its first four digits.
        /// </summary>
        /// <param name="cardNumber">Card number to examine.</param>
        /// <returns>Card brand of the given card number.</returns>
        public static CardBrand DetermineCardBrand(CreditTransactionRequest request, string cardNumber)
        {
            if (DetermineGiftCard(request))
            {
                if (cardNumber.StartsWith("001615"))
                {
                    return CardBrand.Visa;
                }
                else if (cardNumber.StartsWith("061680"))
                {
                    return CardBrand.MasterCard;
                }
                else if (cardNumber.StartsWith("100101"))
                {
                    return CardBrand.Discover;
                }
                else
                {
                    return CardBrand.Unknown;
                }
            }
            else
                if (cardNumber.StartsWith("1024"))
            {
                return CardBrand.Visa;
            }
            else if (cardNumber.StartsWith("2048"))
            {
                return CardBrand.MasterCard;
            }
            else if (cardNumber.StartsWith("4096"))
            {
                return CardBrand.Discover;
            }
            else
            {
                return CardBrand.Unknown;
            }
        }
    }
}