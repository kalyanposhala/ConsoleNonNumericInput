// Shared DTO shapes mirroring the backend API contracts.
// Money fields are strings on the wire (serialized decimals) to avoid float rounding;
// convert with Number() only for display formatting, never for calculation.

export type LoanType = "Ordinary" | "Special";

export type UserRole = "Admin" | "Member";

export interface MemberSummary {
  memberId: string;
  khataNo: number;
  fullName: string;
  isActive: boolean;
  joinedOn: string; // ISO date
}

export interface ContributionRecord {
  contributionId: string;
  memberId: string;
  periodYear: number;
  periodMonth: number; // 1-12
  amount: string;
  paidOn: string; // ISO date
  notes?: string;
}

export interface Loan {
  loanId: string;
  memberId: string;
  loanType: LoanType;
  khataNo: number;
  principalDisbursed: string;
  outstandingPrincipal: string;
  interestRatePercent: string;
  guarantorMemberId?: string;
  disbursedOn: string; // ISO date
  closedOn?: string;
  status: "Active" | "Closed";
}

export interface LoanRepayment {
  repaymentId: string;
  loanId: string;
  periodYear: number;
  periodMonth: number;
  principalPaid: string;
  interestPaid: string;
  outstandingPrincipalAfter: string;
  paidOn: string;
}

export interface MonthlySummary {
  periodYear: number;
  periodMonth: number;
  shareContributions: string;
  ordinaryLoanPrincipalCollected: string;
  ordinaryLoanInterestCollected: string;
  specialLoanPrincipalCollected: string;
  specialLoanInterestCollected: string;
  fines: string;
  extraInstallments: string;
  formSales: string;
  otherIncome: string;
  totalReceipts: string;
  openingBalance: string;
  totalAvailable: string;
  ordinaryLoansDisbursed: string;
  specialLoansDisbursed: string;
  wages: string;
  stationery: string;
  otherExpenses: string;
  totalExpenditure: string;
  closingBalance: string;
}
