using Melior.InterviewQuestion.Data;
using Melior.InterviewQuestion.Types;
using System.Collections.Generic;
using System.Configuration;

namespace Melior.InterviewQuestion.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _accountDataStore;
        private readonly IBackupAccountDataStore _backupAccountDataStore;

        public PaymentService(AccountDataStore accountDataStore,BackupAccountDataStore backupAccountDataStore)
        {
            _accountDataStore = accountDataStore;
            _backupAccountDataStore = backupAccountDataStore;
        }
        public MakePaymentResult MakePayment(MakePaymentRequest request, Account account)
        {
            var dataStoreType = GetSettingValue("DataStoreType");

            var accountRetrieved = AccountLookup(account, request, dataStoreType);     

            var paymentResult = new MakePaymentResult();
         
            // Getting Payment scheme request and fetching value of Enum in AllowedPaymentSchemes
            MappingEnums.TryGetValue(request.PaymentScheme, out AllowedPaymentSchemes val);

            var success = accountRetrieved?.AllowedPaymentSchemes.HasFlag(val) ?? false;

            // checking if account is in a valid state
            switch (request.PaymentScheme)
            {               
                case PaymentScheme.FasterPayments:
                    success &= accountRetrieved.Balance > request.Amount;                                    
                    break;

                case PaymentScheme.Chaps:
                    success &= accountRetrieved.Status == AccountStatus.Live;
                    break;
            }

            paymentResult.Success = success;
          
            // If the payment is a success then we will go ahead and update the account.
            if (paymentResult.Success) 
            {
                UpdateAccount(accountRetrieved, request, dataStoreType);
            }

            return paymentResult;
        }
        public Account AccountLookup(Account account, MakePaymentRequest request, string dataStoreType) 
        {
            if(account == null)
            {
                if (dataStoreType == "Backup")
                {                   
                    account = _backupAccountDataStore.GetAccount(request.DebtorAccountNumber);
                }
                else
                {                 
                    account = _accountDataStore.GetAccount(request.DebtorAccountNumber);
                }
            }
            return account;
        }     

        public void UpdateAccount(Account account,MakePaymentRequest request, string dataStoreType)
        {
            account.Balance -= request.Amount;

            if (dataStoreType == "Backup")
            {
                _backupAccountDataStore.UpdateAccount(account);
            }
            else
            {
                _accountDataStore.UpdateAccount(account);
            }
        }
        public bool AccountValidation(MakePaymentResult result, Account account)
        {
            return false;
        }
        private string GetSettingValue(string value)
        {
            return ConfigurationManager.AppSettings[value];
        }

        private IDictionary<PaymentScheme, AllowedPaymentSchemes> MappingEnums = new Dictionary<PaymentScheme, AllowedPaymentSchemes>
        {
            { PaymentScheme.FasterPayments, AllowedPaymentSchemes.FasterPayments },
            { PaymentScheme.Bacs, AllowedPaymentSchemes.Bacs  },
            { PaymentScheme.Chaps, AllowedPaymentSchemes.Chaps }
        };
    }
}
