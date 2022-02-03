using SimplePaymentProcessingApp.General;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePaymentProcessingApp.Credit
{
    public static class GiftorCredit
    {
        public static bool DetermineGiftCard(
            CreditTransactionRequest request
        )
        {
            if (request.CVV != null && request.Account != null)
            {
                if (request.Account.EndsWith(request.CVV))
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
    }
}