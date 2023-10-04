using Melior.InterviewQuestion.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melior.InterviewQuestion.Data
{
    public interface IBackupAccountDataStore
    {
        public void UpdateAccount(Account account);
        public Account GetAccount(string accountNumber);
    }
}
