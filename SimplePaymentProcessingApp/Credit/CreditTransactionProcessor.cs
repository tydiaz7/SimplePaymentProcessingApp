using SimplePaymentProcessingApp.General;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePaymentProcessingApp.Credit
{
    public static class CreditTransactionProcessor
    {
        private static List<CreditTransactionRequest> CreditTransactionRequestHistory = new List<CreditTransactionRequest>();

        /// <summary>
        /// Processes a credit transaction and generates an informative response based on the given flags.
        /// </summary>
        /// <param name="request">Deserialized request data object.</param>
        /// <param name="checkDuplicate">If true, the request will be compared with previous ones to ensure that it's unique.</param>
        /// <param name="validateExpirationDate">If true, the expiration date will be checked to ensure that it has not passed.</param>
        /// <param name="requireCardholderName">If true, the cardholder name will be checked to ensure it is present and valid.</param>
        /// <param name="waiveFee">If true, the processing fee will be waived.</param>
        /// <param name="AlwaysReqSig">If true, a signature will be required for all transactions regardless of approval status.</param>
        /// <returns>A serializable response object that represents the processor's response to the given request.</returns>
        public static TransactionResponse ProcessTransaction(
            CreditTransactionRequest request,
            bool gift,
            bool checkDuplicate,
            bool validateExpirationDate,
            bool requireCardholderName,
            bool waiveFee,
            bool AlwaysReqSig ///Always Require Signature is a boolean already, passing it to the returns will ensure that if it is checked all responses will require a signature
            )

        {
            // Ensure required request fields are present and valid.
            if (!request.Amount.HasValue || request.Amount < 0)
            {
                return new TransactionResponse(CommandStatus.Declined, "Amount invalid or not specified.", 0, AlwaysReqSig);
            }
            else if (!GiftorCredit.DetermineGiftCard(request) && (request.CardNumber == null || request.CardNumber.Length != 16))
            {
                return new TransactionResponse(CommandStatus.Declined, "Card number is invalid or not specified.", 0, AlwaysReqSig);
            }
            else if (!request.ExpirationDate.HasValue)
            {
                return new TransactionResponse(CommandStatus.Declined, "Expiration date is invalid or not specified", 0, AlwaysReqSig);
            }

            // Store nullable required request fields in nonnull local variables.
            decimal amount = request.Amount.Value;
            // Ternary operator to ensure the correct non-null value is added to variable cardNumber
            string cardNumber = request.CardNumber != null ? request.CardNumber : request.Account != null ? request.Account : "";
            DateTime expirationDate = request.ExpirationDate.Value;

            // If checkDuplicate is enabled, look through the history for any duplicates.
            if (checkDuplicate && CreditTransactionRequestHistory.Any((r) => r.Equals(request)))
            {
                return new TransactionResponse(CommandStatus.Declined, "Duplicate transaction already exists.", 0, AlwaysReqSig);
            }

            // Add the transaction so that it can be duplicate-checked in the future.
            CreditTransactionRequestHistory.Add(request);

            // If validateExpirationDate is enabled, check that the expiration date has not passed.
            if (validateExpirationDate && request.ExpirationDate < DateTime.Now)
            {
                return new TransactionResponse(CommandStatus.Declined, "Card expired.", 0, AlwaysReqSig);
            }


            // If requireCardholderName is enabled, make sure that the cardholder name has been provided.
            if (requireCardholderName)
            {
                // This will reject all entries with less than or more than a single space, which will also reject improperly formatted fields
                if (request.CardholderName == null || !(request.CardholderName.Count(x => x == ' ') == 1))
                {
                    return new TransactionResponse(CommandStatus.Declined, "Cardholder name invalid or not provided.", 0, AlwaysReqSig);
                }

            }

            decimal fee;
            // If waiveFee is enabled, simply set the fee to zero.
            if (waiveFee)
            {
                fee = 0m;
            }

            // Otherwise, calculate the fee based on the card brand, which is determined by examining the card number.
            else
            {
                fee = GiftorCredit.CalculateFee(request, amount, GiftorCredit.DetermineCardBrand(request, cardNumber));
            }

            // Return approval response.
            return new TransactionResponse(CommandStatus.Approved, "Transaction approved.", fee, true /* Successful transactions require a signature. */);
        }


    }
}
