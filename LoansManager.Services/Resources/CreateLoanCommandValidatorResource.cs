﻿namespace LoansManager.Services.Resources
{
    public class CreateLoanCommandValidatorResource
    {
        public const string LenderNotNullOrEmpty = "Lender must be defined.";
        public const string BorrowerNotNullOrEmpty = "Borrower must be defined.";
        public const string LenderDoesNotExtist = "Lender does not exist.";
        public const string BorrowerDoesNotExist = "Borrower does not exist.";
        public const string BorrowerAndLenderMustDiffer = "Borrower and lender must differ.";
    }
}
