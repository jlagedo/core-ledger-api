-- Batch 2: Accounts 1100-2099 (1000 accounts)
INSERT INTO accounts (code, name, type_id, status, normal_balance, created_at, updated_at, row_version)
SELECT 
    code,
    CASE 
        WHEN code < 1200 THEN 'Receivable - Customer ' || chr(65 + ((code - 1100) % 26)) || lpad(((code - 1100) / 26)::text, 3, '0')
        WHEN code < 1300 THEN 'Inventory Item ' || lpad((code - 1200)::text, 3, '0')
        WHEN code < 1400 THEN 'Prepaid ' || (ARRAY['Insurance', 'Rent', 'Advertising', 'Utilities', 'Tases'])[((code - 1300) % 5) + 1] || ' - Location ' || ((code - 1300) % 10)::text
        WHEN code < 1500 THEN (ARRAY['Building', 'Machinery', 'Equipment', 'Vehicles', 'Furniture', 'Computers', 'Software', 'Land', 'Leasehold', 'Patents'])[((code - 1400) % 10) + 1] || ' - Location ' || ((code - 1400) % 20)::text
        WHEN code < 2000 THEN 'Asset Account ' || code::text || ' - Division ' || (code % 10)::text
        WHEN code < 2100 THEN 'Payable - Vendor ' || chr(65 + ((code - 2000) % 26)) || lpad(((code - 2000) / 26)::text, 3, '0')
    END as name,
    CASE 
        WHEN code < 2000 THEN 6  -- Assets
        ELSE 8  -- Payables
    END as type_id,
    1 as status,
    CASE 
        WHEN code < 2000 THEN 1  -- Assets have Debit normal balance
        ELSE 2  -- Liabilities have Credit normal balance
    END as normal_balance,
    NOW() as created_at,
    NOW() as updated_at,
    lpad((code - 1099)::text, 16, '0')::bytea as row_version
FROM generate_series(1100, 2099) as code;
