using System;
using System.IO;
using System.Text;

namespace CoreLedger.Scripts
{
    public class GenerateAccountSeed
    {
        public static void Main()
        {
            var sql = new StringBuilder();
            
            // Header
            sql.AppendLine("-- Insert 10,000 sample financial accounts");
            sql.AppendLine("-- Generated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sql.AppendLine();
            sql.AppendLine("INSERT INTO accounts (code, name, type_id, status, normal_balance, created_at, updated_at) VALUES");
            
            int count = 0;
            int totalAccounts = 10000;
            
            // Asset Accounts (1000-1999) - 2000 accounts
            count += GenerateAssetAccounts(sql, count, 2000);
            
            // Liability Accounts (2000-2999) - 2000 accounts  
            count += GenerateLiabilityAccounts(sql, count, 2000);
            
            // Equity Accounts (3000-3999) - 1000 accounts
            count += GenerateEquityAccounts(sql, count, 1000);
            
            // Revenue Accounts (4000-4999) - 1500 accounts
            count += GenerateRevenueAccounts(sql, count, 1500);
            
            // Expense Accounts (5000-9999) - 3500 accounts
            count += GenerateExpenseAccounts(sql, count, 3500);
            
            // Remove the last comma and add semicolon
            var result = sql.ToString();
            result = result.Remove(result.Length - 3, 3) + ";";
            
            File.WriteAllText("/Users/jlagedo/Developer/angular/core-ledger-api/scripts/seed-accounts-full.sql", result);
            Console.WriteLine($"Generated {count} accounts successfully!");
        }
        
        static int GenerateAssetAccounts(StringBuilder sql, int startCount, int targetCount)
        {
            int count = startCount;
            
            // Cash Accounts (1000-1099) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(10{i:D2}, 'Cash Account {i + 1}', 6, 1, 1, NOW(), NOW()),");
                count++;
            }
            
            // Accounts Receivable (1100-1199) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(11{i:D2}, 'Receivable - Customer {(char)(65 + i % 26)}{i / 26:000}', 7, 1, 1, NOW(), NOW()),");
                count++;
            }
            
            // Inventory (1200-1299) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(12{i:D2}, 'Inventory Item {i + 1}', 9, 1, 1, NOW(), NOW()),");
                count++;
            }
            
            // Prepaid Expenses (1300-1399) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(13{i:D2}, 'Prepaid Expense {(PrepaidTypes[i % 5])} - Location {i % 10}', 1, 1, 1, NOW(), NOW()),");
                count++;
            }
            
            // Fixed Assets (1400-1499) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(14{i:D2}, '{AssetTypes[i % 10]} - Location {i % 20}', 1, 1, 1, NOW(), NOW()),");
                count++;
            }
            
            // Generate remaining asset accounts with sub-accounts
            for (int i = 1500; i < 1999 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"({i}, 'Asset Account {i} - Division {i % 10}', 1, 1, 1, NOW(), NOW()),");
                count++;
            }
            
            return count - startCount;
        }
        
        static int GenerateLiabilityAccounts(StringBuilder sql, int startCount, int targetCount)
        {
            int count = startCount;
            
            // Accounts Payable (2000-2099) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(20{i:D2}, 'Payable - Vendor {(char)(65 + i % 26)}{i / 26:000}', 8, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            // Accrued Expenses (2100-2199) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(21{i:D2}, 'Accrued {(AccruedTypes[i % 8])} - Department {i % 15}', 2, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            // Taxes Payable (2200-2299) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(22{i:D2}, '{TaxTypes[i % 6]} Payable - {Jurisdictions[i % 10]}', 2, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            // Long-term Debt (2300-2399) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(23{i:D2}, '{DebtTypes[i % 5]} - Facility {i % 12}', 2, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            // Generate remaining liability accounts
            for (int i = 2400; i < 2999 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"({i}, 'Liability Account {i} - Region {i % 8}', 2, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            return count - startCount;
        }
        
        static int GenerateEquityAccounts(StringBuilder sql, int startCount, int targetCount)
        {
            int count = startCount;
            
            // Stock Accounts (3000-3099) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(30{i:D2}, '{StockTypes[i % 4]} - Class {(char)(65 + i % 3)}', 3, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            // Retained Earnings (3100-3199) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(31{i:D2}, 'Retained Earnings - {Years[i % 10]}', 3, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            // Generate remaining equity accounts
            for (int i = 3200; i < 3999 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"({i}, 'Equity Account {i} - Fund {i % 20}', 3, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            return count - startCount;
        }
        
        static int GenerateRevenueAccounts(StringBuilder sql, int startCount, int targetCount)
        {
            int count = startCount;
            
            // Sales Revenue (4000-4099) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(40{i:D2}, 'Sales Revenue - Product Line {(char)(65 + i % 26)}{i / 26:00}', 4, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            // Service Revenue (4100-4199) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(41{i:D2}, 'Service Revenue - {ServiceTypes[i % 10]} - Region {i % 5}', 4, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            // Other Revenue (4200-4299) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(42{i:D2}, '{OtherRevenueTypes[i % 8]} - Source {i % 15}', 4, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            // Generate remaining revenue accounts
            for (int i = 4300; i < 4999 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"({i}, 'Revenue Account {i} - Division {i % 12}', 4, 1, 2, NOW(), NOW()),");
                count++;
            }
            
            return count - startCount;
        }
        
        static int GenerateExpenseAccounts(StringBuilder sql, int startCount, int targetCount)
        {
            int count = startCount;
            
            // COGS (5000-5099) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(50{i:D2}, 'COGS - Product Category {(char)(65 + i % 26)}{i / 26:000}', 10, 1, 1, NOW(), NOW()),");
                count++;
            }
            
            // Salaries and Wages (5100-5199) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(51{i:D2}, 'Salaries - {DepartmentTypes[i % 15]} - Level {i % 5}', 5, 1, 1, NOW(), NOW()),");
                count++;
            }
            
            // Operating Expenses (5200-5299) - 100 accounts
            for (int i = 0; i < 100 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"(52{i:D2}, '{OperatingExpenseTypes[i % 12]} - Location {i % 25}', 5, 1, 1, NOW(), NOW()),");
                count++;
            }
            
            // Generate remaining expense accounts
            for (int i = 5300; i < 9999 && count < startCount + targetCount; i++)
            {
                sql.AppendLine($"({i}, 'Expense Account {i} - Cost Center {i % 50}', 5, 1, 1, NOW(), NOW()),");
                count++;
            }
            
            return count - startCount;
        }
        
        static string[] PrepaidTypes = { "Insurance", "Rent", "Advertising", "Utilities", "Taxes" };
        static string[] AssetTypes = { "Building", "Machinery", "Equipment", "Vehicles", "Furniture", "Computers", "Software", "Land", "Leasehold", "Patents" };
        static string[] AccruedTypes = { "Salaries", "Wages", "Interest", "Taxes", "Utilities", "Rent", "Benefits", "Commissions" };
        static string[] TaxTypes = { "Income Tax", "Sales Tax", "Property Tax", "Payroll Tax", "Excise Tax", "VAT" };
        static string[] Jurisdictions = { "Federal", "State A", "State B", "State C", "County 1", "County 2", "City A", "City B", "City C", "City D" };
        static string[] DebtTypes = { "Bank Loan", "Bond Payable", "Mortgage", "Debenture", "Note Payable" };
        static string[] StockTypes = { "Common Stock", "Preferred Stock", "Treasury Stock", "Stock Options" };
        static string[] Years = { "2015", "2016", "2017", "2018", "2019", "2020", "2021", "2022", "2023", "2024" };
        static string[] ServiceTypes = { "Consulting", "Support", "Maintenance", "Training", "Subscription", "Licensing", "Hosting", "Processing", "Analysis", "Design" };
        static string[] OtherRevenueTypes = { "Interest Income", "Dividend Income", "Rental Income", "Royalty Income", "Gain on Sale", "Fee Income", "Commission Income", "Miscellaneous Income" };
        static string[] DepartmentTypes = { "Executive", "Engineering", "Sales", "Marketing", "Finance", "HR", "IT", "Operations", "Legal", "Admin", "Production", "Quality", "Research", "Customer Service", "Logistics" };
        static string[] OperatingExpenseTypes = { "Rent", "Utilities", "Insurance", "Repairs", "Advertising", "Marketing", "Travel", "Entertainment", "Supplies", "Software", "Training", "Professional Fees" };
    }
}
