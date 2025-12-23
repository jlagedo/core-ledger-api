#!/usr/bin/env python3

def generate_accounts():
    sql = []
    sql.append("-- Insert 10,000 sample financial accounts")
    sql.append("-- Generated on " + str(datetime.datetime.now()))
    sql.append("")
    sql.append("INSERT INTO accounts (code, name, type_id, status, normal_balance, created_at, updated_at) VALUES")
    
    count = 0
    
    # Asset Accounts (1000-1999) - 2000 accounts
    for i in range(1000, 3000):
        if i < 2000:
            # Assets
            if i < 1100:
                name = f"Cash Account {i-1000:03d}"
                type_id = 6
                normal_balance = 1
            elif i < 1200:
                name = f"Receivable - Customer {chr(65 + (i-1100) % 26)}{(i-1100)//26:03d}"
                type_id = 7
                normal_balance = 1
            elif i < 1300:
                name = f"Inventory Item {i-1200:03d}"
                type_id = 9
                normal_balance = 1
            elif i < 1400:
                prepaid_types = ["Insurance", "Rent", "Advertising", "Utilities", "Taxes"]
                name = f"Prepaid {prepaid_types[(i-1300) % 5]} - Location {(i-1300) % 10}"
                type_id = 1
                normal_balance = 1
            elif i < 1500:
                asset_types = ["Building", "Machinery", "Equipment", "Vehicles", "Furniture", "Computers", "Software", "Land", "Leasehold", "Patents"]
                name = f"{asset_types[(i-1400) % 10]} - Location {(i-1400) % 20}"
                type_id = 1
                normal_balance = 1
            else:
                name = f"Asset Account {i} - Division {i % 10}"
                type_id = 1
                normal_balance = 1
        else:
            # Liabilities
            if i < 2100:
                name = f"Payable - Vendor {chr(65 + (i-2000) % 26)}{(i-2000)//26:03d}"
                type_id = 8
                normal_balance = 2
            elif i < 2200:
                accrued_types = ["Salaries", "Wages", "Interest", "Taxes", "Utilities", "Rent", "Benefits", "Commissions"]
                name = f"Accrued {accrued_types[(i-2100) % 8]} - Department {(i-2100) % 15}"
                type_id = 2
                normal_balance = 2
            elif i < 2300:
                tax_types = ["Income Tax", "Sales Tax", "Property Tax", "Payroll Tax", "Excise Tax", "VAT"]
                jurisdictions = ["Federal", "State A", "State B", "State C", "County 1", "County 2", "City A", "City B", "City C", "City D"]
                name = f"{tax_types[(i-2200) % 6]} Payable - {jurisdictions[(i-2200) % 10]}"
                type_id = 2
                normal_balance = 2
            elif i < 2400:
                debt_types = ["Bank Loan", "Bond Payable", "Mortgage", "Debenture", "Note Payable"]
                name = f"{debt_types[(i-2300) % 5]} - Facility {(i-2300) % 12}"
                type_id = 2
                normal_balance = 2
            else:
                name = f"Liability Account {i} - Region {i % 8}"
                type_id = 2
                normal_balance = 2
        
        sql.append(f"({i}, '{name}', {type_id}, 1, {normal_balance}, NOW(), NOW()),")
        count += 1
    
    # Equity Accounts (3000-3999) - 1000 accounts
    for i in range(3000, 4000):
        if i < 3100:
            stock_types = ["Common Stock", "Preferred Stock", "Treasury Stock", "Stock Options"]
            name = f"{stock_types[(i-3000) % 4]} - Class {chr(65 + (i-3000) % 3)}"
            type_id = 3
        elif i < 3200:
            years = ["2015", "2016", "2017", "2018", "2019", "2020", "2021", "2022", "2023", "2024"]
            name = f"Retained Earnings - {years[(i-3100) % 10]}"
            type_id = 3
        else:
            name = f"Equity Account {i} - Fund {i % 20}"
            type_id = 3
        
        sql.append(f"({i}, '{name}', {type_id}, 1, 2, NOW(), NOW()),")
        count += 1
    
    # Revenue Accounts (4000-4999) - 1500 accounts
    for i in range(4000, 5500):
        if i < 4100:
            name = f"Sales Revenue - Product Line {chr(65 + (i-4000) % 26)}{(i-4000)//26:02d}"
            type_id = 4
        elif i < 4200:
            service_types = ["Consulting", "Support", "Maintenance", "Training", "Subscription", "Licensing", "Hosting", "Processing", "Analysis", "Design"]
            name = f"Service Revenue - {service_types[(i-4100) % 10]} - Region {(i-4100) % 5}"
            type_id = 4
        elif i < 4300:
            other_rev_types = ["Interest Income", "Dividend Income", "Rental Income", "Royalty Income", "Gain on Sale", "Fee Income", "Commission Income", "Miscellaneous Income"]
            name = f"{other_rev_types[(i-4200) % 8]} - Source {(i-4200) % 15}"
            type_id = 4
        else:
            name = f"Revenue Account {i} - Division {i % 12}"
            type_id = 4
        
        sql.append(f"({i}, '{name}', {type_id}, 1, 2, NOW(), NOW()),")
        count += 1
    
    # Expense Accounts (5500-9999) - 4500 accounts
    for i in range(5500, 10000):
        if i < 5600:
            name = f"COGS - Product Category {chr(65 + (i-5500) % 26)}{(i-5500)//26:03d}"
            type_id = 10
        elif i < 5700:
            dept_types = ["Executive", "Engineering", "Sales", "Marketing", "Finance", "HR", "IT", "Operations", "Legal", "Admin", "Production", "Quality", "Research", "Customer Service", "Logistics"]
            name = f"Salaries - {dept_types[(i-5600) % 15]} - Level {(i-5600) % 5}"
            type_id = 5
        elif i < 5800:
            op_exp_types = ["Rent", "Utilities", "Insurance", "Repairs", "Advertising", "Marketing", "Travel", "Entertainment", "Supplies", "Software", "Training", "Professional Fees"]
            name = f"{op_exp_types[(i-5700) % 12]} - Location {(i-5700) % 25}"
            type_id = 5
        else:
            name = f"Expense Account {i} - Cost Center {i % 50}"
            type_id = 5
        
        sql.append(f"({i}, '{name}', {type_id}, 1, 1, NOW(), NOW()),")
        count += 1
    
    # Remove last comma and add semicolon
    sql[-1] = sql[-1][:-1] + ";"
    
    return "\n".join(sql)

if __name__ == "__main__":
    import datetime
    sql = generate_accounts()
    with open("/Users/jlagedo/Developer/angular/core-ledger-api/scripts/seed-accounts-full.sql", "w") as f:
        f.write(sql)
    print(f"Generated SQL script with 10,000 accounts")
