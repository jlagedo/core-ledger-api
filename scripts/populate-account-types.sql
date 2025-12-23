-- Populate account_types table with common financial ledger account types
-- These are the most common account types used in financial applications

INSERT INTO account_types (description, created_at, updated_at, row_version) VALUES
('Assets', CURRENT_TIMESTAMP, NULL, decode(md5(random()::text), 'hex')),
('Liabilities', CURRENT_TIMESTAMP, NULL, decode(md5(random()::text), 'hex')),
('Equity', CURRENT_TIMESTAMP, NULL, decode(md5(random()::text), 'hex')),
('Revenue', CURRENT_TIMESTAMP, NULL, decode(md5(random()::text), 'hex')),
('Expenses', CURRENT_TIMESTAMP, NULL, decode(md5(random()::text), 'hex')),
('Cash', CURRENT_TIMESTAMP, NULL, decode(md5(random()::text), 'hex')),
('Accounts Receivable', CURRENT_TIMESTAMP, NULL, decode(md5(random()::text), 'hex')),
('Accounts Payable', CURRENT_TIMESTAMP, NULL, decode(md5(random()::text), 'hex')),
('Inventory', CURRENT_TIMESTAMP, NULL, decode(md5(random()::text), 'hex')),
('Cost of Goods Sold', CURRENT_TIMESTAMP, NULL, decode(md5(random()::text), 'hex'));

-- Verify the inserted data
SELECT id, description, created_at FROM account_types ORDER BY id;
