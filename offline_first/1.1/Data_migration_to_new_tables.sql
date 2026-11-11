PRAGMA foreign_keys = OFF;
BEGIN TRANSACTION;

DROP VIEW View_ClientMovements;
DROP VIEW View_SuppliersMovements;


-- *************************************** --
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
    u.user_name,
    u.name, 
    u.DNI, 
    u.email, 
    u.phone, 
    u.address, 
    u.password_hash,
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


-- *************************************** --
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
    paid DECIMAL(18,2) DEFAULT 0,
    total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (client_id) REFERENCES Clients(id),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

INSERT INTO SalesInvoices_new (Id, InvoiceNumber, client_id, user_id, invoice_date,
                                paid_full, paid, total)
SELECT
    sim.NewUuid,
    'INV-261110-' || printf('%05d', ROW_NUMBER() OVER ()) AS InvoiceNumber,
    cm.NewUuid AS client_id,
    um.NewUuid AS user_id,
    si.invoice_date,
    si.paid_full,
    si.paid,
    si.total
FROM SalesInvoices si
JOIN SalesInvoicesIdMapping sim ON si.id = sim.OldIntId
JOIN ClientIdMapping cm ON si.client_id = cm.OldIntId
JOIN UserIdMapping um ON si.user_id = um.OldIntId;

-- Replace old table
DROP TABLE SalesInvoices;
ALTER TABLE SalesInvoices_new RENAME TO SalesInvoices;


 --------------------------------------------------
-- 2. MIGRATE 'SalesInvoiceItems' TABLE
--------------------------------------------------

-- Create temporary mapping table for SalesInvoiceItems
CREATE TEMP TABLE SalesInvoiceItemsIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

-- Generate new GUIDs for each existing SalesInvoiceItems record
INSERT INTO SalesInvoiceItemsIdMapping (OldIntId, NewUuid)
SELECT 
    id, 
    lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
    substr(lower(hex(randomblob(2))), 2) || '-a' || 
    substr(lower(hex(randomblob(2))), 2) || '-' || 
    lower(hex(randomblob(6)))
FROM SalesInvoiceItems;

-- Create new SalesInvoiceItems table with TEXT primary key
CREATE TABLE SalesInvoiceItems_new (
    id TEXT PRIMARY KEY NOT NULL,
    invoice_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (invoice_id) REFERENCES SalesInvoices(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(id)
);

-- Copy data using the mapping table
INSERT INTO SalesInvoiceItems_new (Id, invoice_id, product_id, quantity, unit_price, total)
SELECT
    siim.NewUuid,
    sim.NewUuid AS invoice_id,
    pm.NewUuid AS product_id,
    sii.quantity,
    sii.unit_price,
    sii.total
FROM SalesInvoiceItems sii
JOIN SalesInvoiceItemsIdMapping siim ON sii.id = siim.OldIntId
JOIN SalesInvoicesIdMapping sim ON sii.invoice_id = sim.OldIntId
JOIN ProductIdMapping pm ON sii.product_id = pm.OldIntId;

-- Replace old table
DROP TABLE SalesInvoiceItems;
ALTER TABLE SalesInvoiceItems_new RENAME TO SalesInvoiceItems;

 --------------------------------------------------
-- 3. MIGRATE 'Salaries' TABLE
--------------------------------------------------

-- Create temporary mapping table for Salaries
CREATE TEMP TABLE SalariesIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

-- Generate new GUIDs for each existing Salaries record
INSERT INTO SalariesIdMapping (OldIntId, NewUuid)
SELECT 
    id, 
    lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
    substr(lower(hex(randomblob(2))), 2) || '-a' || 
    substr(lower(hex(randomblob(2))), 2) || '-' || 
    lower(hex(randomblob(6)))
FROM Salaries;

-- Create new Salaries table with TEXT primary key
CREATE TABLE Salaries_new (
    id TEXT PRIMARY KEY NOT NULL,
    salary_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    user_id INTEGER,
    hours_worked DECIMAL(18,2),
    hour_cost DECIMAL(18,2),
    total_paid DECIMAL(18,2),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

-- Copy data using the mapping table
INSERT INTO Salaries_new (id, salary_date, user_id, hours_worked, hour_cost, total_paid)
SELECT
    sm.NewUuid,
    s.salary_date,
    um.NewUuid AS user_id,
    s.hours_worked,
    s.hour_cost,
    s.total_paid
FROM Salaries s
JOIN SalariesIdMapping sm ON s.id = sm.OldIntId
JOIN UserIdMapping um ON s.user_id = um.OldIntId;

-- Replace old table
DROP TABLE Salaries;
ALTER TABLE Salaries_new RENAME TO Salaries;


--------------------------------------------------
-- 4. MIGRATE 'PurchaseInvoices' TABLE
--------------------------------------------------
CREATE TEMP TABLE PurchaseInvoicesIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO PurchaseInvoicesIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM PurchaseInvoices;

CREATE TABLE PurchaseInvoices_new (
    id TEXT PRIMARY KEY NOT NULL,
    supplier_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paid_full INTEGER NOT NULL DEFAULT 1,
    paid DECIMAL(18,2),
    total DECIMAL(18,2) NOT NULL,
    supplier_invoice TEXT,
    FOREIGN KEY (supplier_id) REFERENCES Suppliers(id),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

INSERT INTO PurchaseInvoices_new (id, supplier_id, user_id, invoice_date, paid_full, paid, total, supplier_invoice)
SELECT 
    pim.NewUuid,
    sum_m.NewUuid AS supplier_id,
    um.NewUuid AS user_id,
    pi.invoice_date,
    pi.paid_full,
    pi.paid,
    pi.total,
    pi.supplier_invoice
FROM PurchaseInvoices pi
JOIN PurchaseInvoicesIdMapping pim ON pi.id = pim.OldIntId
JOIN SupplierIdMapping sum_m ON pi.supplier_id = sum_m.OldIntId
JOIN UserIdMapping um ON pi.user_id = um.OldIntId;

DROP TABLE PurchaseInvoices;
ALTER TABLE PurchaseInvoices_new RENAME TO PurchaseInvoices;


--------------------------------------------------
-- 5. MIGRATE 'PurchaseInvoiceItems' TABLE
--------------------------------------------------
CREATE TEMP TABLE PurchaseInvoiceItemsIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO PurchaseInvoiceItemsIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM PurchaseInvoiceItems;

CREATE TABLE PurchaseInvoiceItems_new (
    id TEXT PRIMARY KEY NOT NULL,
    invoice_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    unit_cost DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (invoice_id) REFERENCES PurchaseInvoices(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(id)
);

INSERT INTO PurchaseInvoiceItems_new (id, invoice_id, product_id, quantity, unit_cost, total)
SELECT 
    piim.NewUuid,
    pim.NewUuid AS invoice_id,
    pm.NewUuid AS product_id,
    pii.quantity,
    pii.unit_cost,
    pii.total
FROM PurchaseInvoiceItems pii
JOIN PurchaseInvoiceItemsIdMapping piim ON pii.id = piim.OldIntId
JOIN PurchaseInvoicesIdMapping pim ON pii.invoice_id = pim.OldIntId
JOIN ProductIdMapping pm ON pii.product_id = pm.OldIntId;

DROP TABLE PurchaseInvoiceItems;
ALTER TABLE PurchaseInvoiceItems_new RENAME TO PurchaseInvoiceItems;


--------------------------------------------------
-- 6. MIGRATE 'PayDebtInvoices' TABLE
--------------------------------------------------
CREATE TEMP TABLE PayDebtInvoicesIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO PayDebtInvoicesIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM PayDebtInvoices;

CREATE TABLE PayDebtInvoices_new (
    id TEXT PRIMARY KEY NOT NULL,
    client_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paid DECIMAL(18,2),
    FOREIGN KEY (client_id) REFERENCES Clients(id),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

INSERT INTO PayDebtInvoices_new (id, client_id, user_id, invoice_date, paid)
SELECT 
    pdim.NewUuid,
    cm.NewUuid AS client_id,
    um.NewUuid AS user_id,
    pdi.invoice_date,
    pdi.paid
FROM PayDebtInvoices pdi
JOIN PayDebtInvoicesIdMapping pdim ON pdi.id = pdim.OldIntId
JOIN ClientIdMapping cm ON pdi.client_id = cm.OldIntId
JOIN UserIdMapping um ON pdi.user_id = um.OldIntId;

DROP TABLE PayDebtInvoices;
ALTER TABLE PayDebtInvoices_new RENAME TO PayDebtInvoices;


--------------------------------------------------
-- 7. MIGRATE 'PayCreditInvoices' TABLE
--------------------------------------------------
CREATE TEMP TABLE PayCreditInvoicesIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO PayCreditInvoicesIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM PayCreditInvoices;

CREATE TABLE PayCreditInvoices_new (
    id TEXT PRIMARY KEY NOT NULL,
    supplier_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paid DECIMAL(18,2),
    FOREIGN KEY (supplier_id) REFERENCES Suppliers(id),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

INSERT INTO PayCreditInvoices_new (id, supplier_id, user_id, invoice_date, paid)
SELECT 
    pcim.NewUuid,
    sum_m.NewUuid AS supplier_id,
    um.NewUuid AS user_id,
    pci.invoice_date,
    pci.paid
FROM PayCreditInvoices pci
JOIN PayCreditInvoicesIdMapping pcim ON pci.id = pcim.OldIntId
JOIN SupplierIdMapping sum_m ON pci.supplier_id = sum_m.OldIntId
JOIN UserIdMapping um ON pci.user_id = um.OldIntId;

DROP TABLE PayCreditInvoices;
ALTER TABLE PayCreditInvoices_new RENAME TO PayCreditInvoices;


--------------------------------------------------
-- 8. MIGRATE 'InventoryMovements' TABLE
--------------------------------------------------
CREATE TEMP TABLE InventoryMovementsIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO InventoryMovementsIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM InventoryMovements;

CREATE TABLE InventoryMovements_new (
    id TEXT PRIMARY KEY NOT NULL,
    user_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    movement_type TEXT NOT NULL,
    reference TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES Products(id),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

INSERT INTO InventoryMovements_new (id, user_id, product_id, quantity, movement_type, reference, created_at)
SELECT 
    im_m.NewUuid,
    um.NewUuid AS user_id,
    pm.NewUuid AS product_id,
    im.quantity,
    im.movement_type,
    im.reference,
    im.created_at
FROM InventoryMovements im
JOIN InventoryMovementsIdMapping im_m ON im.id = im_m.OldIntId
JOIN UserIdMapping um ON im.user_id = um.OldIntId
JOIN ProductIdMapping pm ON im.product_id = pm.OldIntId;

DROP TABLE InventoryMovements;
ALTER TABLE InventoryMovements_new RENAME TO InventoryMovements;


--------------------------------------------------
-- 9. MIGRATE 'GeneralExpenses' TABLE
--------------------------------------------------
CREATE TEMP TABLE GeneralExpensesIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO GeneralExpensesIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM GeneralExpenses;

CREATE TABLE GeneralExpenses_new (
    id TEXT PRIMARY KEY NOT NULL,
    invoice_ref TEXT NOT NULL,
    details TEXT NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    user_id TEXT,
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

INSERT INTO GeneralExpenses_new (id, invoice_ref, details, total, date, user_id)
SELECT 
    gem.NewUuid,
    ge.invoice_ref,
    ge.details,
    ge.total,
    ge.date,
    um.NewUuid AS user_id
FROM GeneralExpenses ge
JOIN GeneralExpensesIdMapping gem ON ge.id = gem.OldIntId
LEFT JOIN UserIdMapping um ON ge.user_id = um.OldIntId;

DROP TABLE GeneralExpenses;
ALTER TABLE GeneralExpenses_new RENAME TO GeneralExpenses;


--------------------------------------------------
-- 10. MIGRATE 'CashFlow' TABLE
--------------------------------------------------
CREATE TEMP TABLE CashFlowIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO CashFlowIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM CashFlow;

CREATE TABLE CashFlow_new (
    id TEXT PRIMARY KEY NOT NULL,
    date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    type TEXT,
    detail TEXT,
    value DECIMAL(18,2),
    reference_table TEXT,
    reference_id TEXT, -- Changed to TEXT to support GUID references
    user_id TEXT,
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

INSERT INTO CashFlow_new (id, date, type, detail, value, reference_table, reference_id, user_id)
SELECT 
    cfm.NewUuid,
    cf.date,
    cf.type,
    cf.detail,
    cf.value,
    cf.reference_table,
    cf.reference_id, -- Note: If reference_id linked to SalesInvoices/PurchaseInvoices integer IDs, map accordingly if needed
    um.NewUuid AS user_id
FROM CashFlow cf
JOIN CashFlowIdMapping cfm ON cf.id = cfm.OldIntId
LEFT JOIN UserIdMapping um ON cf.user_id = um.OldIntId;

DROP TABLE CashFlow;
ALTER TABLE CashFlow_new RENAME TO CashFlow;


--------------------------------------------------
-- 11. MIGRATE 'ReturnInvoices' TABLE
--------------------------------------------------
CREATE TEMP TABLE ReturnInvoicesIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO ReturnInvoicesIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM ReturnInvoices;

CREATE TABLE ReturnInvoices_new (
    id TEXT PRIMARY KEY NOT NULL,
    client_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sale_invoice TEXT,
    total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (sale_invoice) REFERENCES SalesInvoices(id),
    FOREIGN KEY (user_id) REFERENCES Users(id),
    FOREIGN KEY (client_id) REFERENCES Clients(id)
);

INSERT INTO ReturnInvoices_new (id, client_id, user_id, invoice_date, sale_invoice, total)
SELECT 
    rim.NewUuid,
    cm.NewUuid AS client_id,
    um.NewUuid AS user_id,
    ri.invoice_date,
    sim.NewUuid AS sale_invoice, -- Mapped to SalesInvoices new GUID
    ri.total
FROM ReturnInvoices ri
JOIN ReturnInvoicesIdMapping rim ON ri.id = rim.OldIntId
JOIN ClientIdMapping cm ON ri.client_id = cm.OldIntId
JOIN UserIdMapping um ON ri.user_id = um.OldIntId
LEFT JOIN SalesInvoicesIdMapping sim ON ri.sale_invoice = sim.OldIntId;

DROP TABLE ReturnInvoices;
ALTER TABLE ReturnInvoices_new RENAME TO ReturnInvoices;


--------------------------------------------------
-- 12. MIGRATE 'ReturnInvoiceItems' TABLE
--------------------------------------------------
CREATE TEMP TABLE ReturnInvoiceItemsIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO ReturnInvoiceItemsIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM ReturnInvoiceItems;

CREATE TABLE ReturnInvoiceItems_new (
    id TEXT PRIMARY KEY NOT NULL,
    return_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    FOREIGN KEY (return_id) REFERENCES ReturnInvoices(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(id)
);

INSERT INTO ReturnInvoiceItems_new (id, return_id, product_id, quantity)
SELECT 
    riim.NewUuid,
    rim.NewUuid AS return_id,
    pm.NewUuid AS product_id,
    rii.quantity
FROM ReturnInvoiceItems rii
JOIN ReturnInvoiceItemsIdMapping riim ON rii.id = riim.OldIntId
JOIN ReturnInvoicesIdMapping rim ON rii.return_id = rim.OldIntId
JOIN ProductIdMapping pm ON rii.product_id = pm.OldIntId;

DROP TABLE ReturnInvoiceItems;
ALTER TABLE ReturnInvoiceItems_new RENAME TO ReturnInvoiceItems;


--------------------------------------------------
-- 13. MIGRATE 'ReturnSupplierInvoices' TABLE
--------------------------------------------------
CREATE TEMP TABLE ReturnSupplierInvoicesIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO ReturnSupplierInvoicesIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM ReturnSupplierInvoices;

CREATE TABLE ReturnSupplierInvoices_new (
    id TEXT PRIMARY KEY NOT NULL,
    supplier_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    buy_invoice TEXT,
    total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (buy_invoice) REFERENCES PurchaseInvoices(id),
    FOREIGN KEY (user_id) REFERENCES Users(id),
    FOREIGN KEY (supplier_id) REFERENCES Suppliers(id)
);

INSERT INTO ReturnSupplierInvoices_new (id, supplier_id, user_id, invoice_date, buy_invoice, total)
SELECT 
    rsim.NewUuid,
    sum_m.NewUuid AS supplier_id,
    um.NewUuid AS user_id,
    rsi.invoice_date,
    pim.NewUuid AS buy_invoice, -- Mapped to PurchaseInvoices new GUID
    rsi.total
FROM ReturnSupplierInvoices rsi
JOIN ReturnSupplierInvoicesIdMapping rsim ON rsi.id = rsim.OldIntId
JOIN SupplierIdMapping sum_m ON rsi.supplier_id = sum_m.OldIntId
JOIN UserIdMapping um ON rsi.user_id = um.OldIntId
LEFT JOIN PurchaseInvoicesIdMapping pim ON rsi.buy_invoice = pim.OldIntId;

DROP TABLE ReturnSupplierInvoices;
ALTER TABLE ReturnSupplierInvoices_new RENAME TO ReturnSupplierInvoices;


--------------------------------------------------
-- 14. MIGRATE 'ReturnSupplierInvoiceItems' TABLE
--------------------------------------------------
CREATE TEMP TABLE ReturnSupplierInvoiceItemsIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

INSERT INTO ReturnSupplierInvoiceItemsIdMapping (OldIntId, NewUuid)
SELECT id, lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
       substr(lower(hex(randomblob(2))), 2) || '-a' || 
       substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6)))
FROM ReturnSupplierInvoiceItems;

CREATE TABLE ReturnSupplierInvoiceItems_new (
    id TEXT PRIMARY KEY NOT NULL,
    return_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    FOREIGN KEY (return_id) REFERENCES ReturnSupplierInvoices(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(id)
);

INSERT INTO ReturnSupplierInvoiceItems_new (id, return_id, product_id, quantity)
SELECT 
    rsiim.NewUuid,
    rsim.NewUuid AS return_id,
    pm.NewUuid AS product_id,
    rsii.quantity
FROM ReturnSupplierInvoiceItems rsii
JOIN ReturnSupplierInvoiceItemsIdMapping rsiim ON rsii.id = rsiim.OldIntId
JOIN ReturnSupplierInvoicesIdMapping rsim ON rsii.return_id = rsim.OldIntId
JOIN ProductIdMapping pm ON rsii.product_id = pm.OldIntId;

DROP TABLE ReturnSupplierInvoiceItems;
ALTER TABLE ReturnSupplierInvoiceItems_new RENAME TO ReturnSupplierInvoiceItems;


-- ************************************************** --
-- >> PHASE C : CLEAN UP ALL TEMPORARY MAPPING TABLES << --
-- ************************************************** --
DROP TABLE IF EXISTS ProductIdMapping;
DROP TABLE IF EXISTS ClientIdMapping;
DROP TABLE IF EXISTS UserIdMapping;
DROP TABLE IF EXISTS SupplierIdMapping;
DROP TABLE IF EXISTS SalesInvoicesIdMapping;
DROP TABLE IF EXISTS SalesInvoiceItemsIdMapping;
DROP TABLE IF EXISTS SalariesIdMapping;
DROP TABLE IF EXISTS PurchaseInvoicesIdMapping;
DROP TABLE IF EXISTS PurchaseInvoiceItemsIdMapping;
DROP TABLE IF EXISTS PayDebtInvoicesIdMapping;
DROP TABLE IF EXISTS PayCreditInvoicesIdMapping;
DROP TABLE IF EXISTS InventoryMovementsIdMapping;
DROP TABLE IF EXISTS GeneralExpensesIdMapping;
DROP TABLE IF EXISTS CashFlowIdMapping;
DROP TABLE IF EXISTS ReturnInvoicesIdMapping;
DROP TABLE IF EXISTS ReturnInvoiceItemsIdMapping;
DROP TABLE IF EXISTS ReturnSupplierInvoicesIdMapping;
DROP TABLE IF EXISTS ReturnSupplierInvoiceItemsIdMapping;

CREATE VIEW IF NOT EXISTS View_ClientMovements AS
    SELECT 
        client_id, 
        invoice_date, 
        ri.id,
        SUM(rii.quantity) AS Quantity,
        'Devolución' AS Description, 
        (total * -1) AS Paid, 
        0 as total
    FROM ReturnInvoices ri
    JOIN ReturnInvoiceItems rii ON rii.return_id = ri.id
    GROUP BY rii.return_id
UNION ALL
    SELECT 
        client_id, 
        invoice_date, 
        si.id,
        SUM(sii.quantity) AS Quantity,
        'Compra' As Description, 
        paid As Paid,
        si.total as total
    FROM SalesInvoices si
    JOIN SalesInvoiceItems sii ON sii.invoice_id = si.id
    GROUP BY sii.invoice_id
UNION ALL
    SELECT 
        client_id, 
        invoice_date, 
        id,
        0 AS Quantity,
        'Abono deuda' As Description, 
        paid As Paid,
        paid as total
    FROM PayDebtInvoices pdi;

CREATE VIEW IF NOT EXISTS View_SuppliersMovements AS
    SELECT 
        supplier_id, 
        invoice_date, 
        ri.id,
        SUM(rii.quantity) AS Quantity,
        'Devolución' AS Description, 
        (total * -1) AS Paid,
        0 as total
    FROM ReturnSupplierInvoices ri
    JOIN ReturnSupplierInvoiceItems rii ON rii.return_id = ri.id
    GROUP BY rii.return_id
UNION ALL
    SELECT 
        supplier_id, 
        invoice_date, 
        pi.id,
        SUM(pii.quantity) AS Quantity,
        'Compra' As Description, 
        paid As Paid,
        pi.total as total
    FROM PurchaseInvoices pi
    JOIN PurchaseInvoiceItems pii ON pii.invoice_id = pi.id
    GROUP BY pii.invoice_id
UNION ALL
    SELECT 
        supplier_id, 
        invoice_date, 
        id,
        0 AS Quantity,
        'Abono deuda' As Description, 
        paid As Paid,
        paid as total
    FROM PayCreditInvoices;

COMMIT;
PRAGMA foreign_keys = ON;
