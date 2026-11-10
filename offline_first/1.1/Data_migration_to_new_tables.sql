PRAGMA foreign_keys = OFF;
BEGIN TRANSACTION;

 -- >> PHASE A : MIGRATE PARENTS TABLES << --   

--------------------------------------------------
-- 1. MIGRATE 'Products' TABLE
--------------------------------------------------

-- Create temporary mapping table for Products
CREATE TEMP TABLE ProductIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

-- Generate new GUIDs for each existing product record
INSERT INTO ProductIdMapping (OldIntId, NewUuid)
SELECT 
    id, 
    lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
    substr(lower(hex(randomblob(2))), 2) || '-a' || 
    substr(lower(hex(randomblob(2))), 2) || '-' || 
    lower(hex(randomblob(6)))
FROM Products;

-- Create new Products table with TEXT primary key
CREATE TABLE Products_new (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    SKU TEXT,
    cost_price DECIMAL(18,2),
    sale_price DECIMAL(18,2),
    retail_price DECIMAL(18,2),
    stock INTEGER DEFAULT 0,
    is_active INTEGER DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Copy data using the mapping table
INSERT INTO Products_new (id, name, description, SKU, cost_price, sale_price, retail_price, stock, is_active, created_at)
SELECT 
    m.NewUuid, 
    p.name, 
    p.description, 
    p.SKU, 
    p.cost_price, 
    p.sale_price, 
    p.retail_price, 
    p.stock, 
    p.is_active, 
    p.created_at
FROM Products p
JOIN ProductIdMapping m ON p.id = m.OldIntId;

-- Replace old table
DROP TABLE Products;
ALTER TABLE Products_new RENAME TO Products;

--------------------------------------------------
-- 2. MIGRATE 'Clients' TABLE
--------------------------------------------------

-- Create temporary mapping table for Clients
CREATE TEMP TABLE ClientIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

-- Generate new GUIDs for each existing client record
INSERT INTO ClientIdMapping (OldIntId, NewUuid)
SELECT 
    id, 
    lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
    substr(lower(hex(randomblob(2))), 2) || '-a' || 
    substr(lower(hex(randomblob(2))), 2) || '-' || 
    lower(hex(randomblob(6)))
FROM Clients;

-- Create new Clients table with TEXT primary key
CREATE TABLE Clients_new (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    DNI TEXT,
    email TEXT,
    phone TEXT,
    address TEXT,
    debt DECIMAL(18,2) DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Copy data using the mapping table
INSERT INTO Clients_new (id, name, DNI, email, phone, address, debt, created_at)
SELECT 
    m.NewUuid, 
    c.name, 
    c.DNI, 
    c.email, 
    c.phone, 
    c.address, 
    c.debt, 
    c.created_at
FROM Clients c
JOIN ClientIdMapping m ON c.id = m.OldIntId;

-- Replace old table
DROP TABLE Clients;
ALTER TABLE Clients_new RENAME TO Clients;

--------------------------------------------------
-- 3. MIGRATE 'Users' TABLE
--------------------------------------------------

-- Create temporary mapping table for Users
CREATE TEMP TABLE UserIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

-- Generate new GUIDs for each existing user record
INSERT INTO UserIdMapping (OldIntId, NewUuid)
SELECT 
    id, 
    lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
    substr(lower(hex(randomblob(2))), 2) || '-a' || 
    substr(lower(hex(randomblob(2))), 2) || '-' || 
    lower(hex(randomblob(6)))
FROM Users;

-- Create new Users table with TEXT primary key
CREATE TABLE Users_new (
    id TEXT PRIMARY KEY NOT NULL,
    user_name TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL,
    DNI TEXT NOT NULL,
    email TEXT,
    phone TEXT,
    address TEXT,
    password_hash TEXT NOT NULL,
    reset_password INTEGER NOT NULL DEFAULT 1,
    is_admin INTEGER NOT NULL DEFAULT 0, -- 0 = normal, 1 = admin
    is_active INTEGER NOT NULL DEFAULT 1,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Copy data using the mapping table
INSERT INTO Users_new (id, user_name,name, DNI, email, phone, address,password_hash,reset_password,is_admin,is_active,created_at)
SELECT 
    m.NewUuid,
    u.user_name 
    u.name, 
    u.DNI, 
    u.email, 
    u.phone, 
    u.address, 
    u.password_hash
    u.reset_password,
    u.is_admin,
    u.is_active,
    u.created_at
FROM Users u
JOIN UserIdMapping m ON u.id = m.OldIntId;

-- Replace old table
DROP TABLE Users;
ALTER TABLE Users_new RENAME TO Users;

--------------------------------------------------
-- 4. MIGRATE 'Suppliers' TABLE
--------------------------------------------------

-- Create temporary mapping table for Suppliers
CREATE TEMP TABLE SupplierIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

-- Generate new GUIDs for each existing Supplier record
INSERT INTO SupplierIdMapping (OldIntId, NewUuid)
SELECT 
    id, 
    lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
    substr(lower(hex(randomblob(2))), 2) || '-a' || 
    substr(lower(hex(randomblob(2))), 2) || '-' || 
    lower(hex(randomblob(6)))
FROM Suppliers;

-- Create new Suppliers table with TEXT primary key
CREATE TABLE Suppliers_new (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    DNI TEXT NOT NULL,
    email TEXT,
    phone TEXT,
    address TEXT,
    debt DECIMAL(18,2) DEFAULT 0,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Copy data using the mapping table
INSERT INTO Suppliers_new (id, name, DNI, email, phone, address, debt, created_at)
SELECT 
    m.NewUuid, 
    s.name, 
    s.DNI, 
    s.email, 
    s.phone, 
    s.address, 
    s.debt, 
    s.created_at
FROM Suppliers s
JOIN SupplierIdMapping m ON s.id = m.OldIntId;

-- Replace old table
DROP TABLE Suppliers;
ALTER TABLE Suppliers_new RENAME TO Suppliers;


 -- >> PHASE B : MIGRATE CHILD TABLES << --  

 --------------------------------------------------
-- 1. MIGRATE 'SalesInvoices' TABLE
--------------------------------------------------

-- Create temporary mapping table for SalesInvoices
CREATE TEMP TABLE SalesInvoicesIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

-- Generate new GUIDs for each existing SalesInvoices record
INSERT INTO SalesInvoicesIdMapping (OldIntId, NewUuid)
SELECT 
    id, 
    lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
    substr(lower(hex(randomblob(2))), 2) || '-a' || 
    substr(lower(hex(randomblob(2))), 2) || '-' || 
    lower(hex(randomblob(6)))
FROM SalesInvoices;

-- Create new SalesInvoices table with TEXT primary key
CREATE TABLE SalesInvoices_new (
    id TEXT PRIMARY KEY NOT NULL,
    InvoiceNumber TEXT UNIQUE NOT NULL,
    client_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paid_full INTEGER NOT NULL DEFAULT 1,
    paid DECIMAL(18,2) DEFAULT 0,,
    total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (client_id) REFERENCES Clients(id),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

-- Copy data using the mapping table
DECLARE @YearStr VARCHAR(2);
SET @YearStr = FORMAT(GETUTCDATE(), 'yy');

INSERT INTO SalesInvoices_new (Id, InvoiceNumber, client_id, user_id, invoice_date,
                                paid_full, paid, total)
    sim.NewUuid,
    CONCAT('INV-', @YearStr, '-', RIGHT('0000' + CAST(ROW_NUMBER() OVER (ORDER BY o.invoice_date) AS VARCHAR(4)), 4)),
    cm.NewUuid AS client_id,
    um.NewUuid AS user_id,
    si.invoice_date,
    si.paid_full,
    si.paid,
    si.total
FROM SalesInvoices si
JOIN SalesInvoicesIdMapping sim ON si.id = sim.OldIntId
JOIN ClientIdMapping cm ON si.client_id = cm.OldIntId
JOIN UserIdMapping um ON si.user_id = um.OldIntId

-- Replace old table
DROP TABLE SalesInvoices;
ALTER TABLE SalesInvoices_new RENAME TO SalesInvoices;

-- SalesInvoiceItems
-- Salaries
-- PurchaseInvoices
-- PurchaseInvoiceItems
-- PayDebtInvoices
-- PayCreditInvoices
-- InventoryMovements
-- GeneralExpenses
-- CashFlow
-- ReturnInvoices
-- ReturnInvoiceItems
-- ReturnSupplierInvoices
-- ReturnSupplierInvoiceItems


 -- >> PHASE C : CLEAN UP TEMPORARY MAPPING TABLES << --  

-- ProductIdMapping
-- ClientIdMapping
-- UserIdMapping
-- SupplierIdMapping
-- SalesInvoicesIdMapping

-- NOTE: Keep `ProductIdMapping` and `ClientIdMapping` if you are running 
-- this in a single giant script, as child tables like `SalesInvoiceItems` 
-- and `SalesInvoices` will need to join against them to map their foreign keys!

COMMIT;
PRAGMA foreign_keys = ON;