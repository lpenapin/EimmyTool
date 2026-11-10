PRAGMA foreign_keys = OFF;
BEGIN TRANSACTION;

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
-- 3. CLEAN UP TEMPORARY MAPPING TABLES
--------------------------------------------------
-- NOTE: Keep `ProductIdMapping` and `ClientIdMapping` if you are running 
-- this in a single giant script, as child tables like `SalesInvoiceItems` 
-- and `SalesInvoices` will need to join against them to map their foreign keys!

COMMIT;
PRAGMA foreign_keys = ON;