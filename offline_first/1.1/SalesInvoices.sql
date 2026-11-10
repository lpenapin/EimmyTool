BEGIN TRANSACTION;


-- 1. Create a temporary mapping table to track Old Integer ID -> New GUID
CREATE TEMP TABLE InvoiceIdMapping (
    OldIntId INTEGER PRIMARY KEY,
    NewUuid TEXT NOT NULL
);

-- 2. Populate the mapping table as you recreate SalesInvoices
-- (Assuming your migration script inserts records and assigns new GUIDs)
INSERT INTO InvoiceIdMapping (OldIntId, NewUuid)
SELECT 
    old.Id AS OldIntId,
    lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
    substr(lower(hex(randomblob(2))), 2) || '-a' || 
    substr(lower(hex(randomblob(2))), 2) || '-' || 
    lower(hex(randomblob(6))) AS NewUuid
FROM SalesInvoices AS old;

-- 3. Recreate SalesInvoices with GUID primary key
CREATE TABLE SalesInvoices_new (
    Id TEXT PRIMARY KEY NOT NULL,
    InvoiceNumber TEXT UNIQUE NOT NULL,
    client_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paid_full INTEGER NOT NULL DEFAULT 1,
    paid REAL,
    total REAL NOT NULL,
    FOREIGN KEY (client_id) REFERENCES Clients(id),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

DECLARE @YearStr VARCHAR(2);
SET @YearStr = FORMAT(GETUTCDATE(), 'yy');

INSERT INTO SalesInvoices_new (Id, InvoiceNumber, client_id, user_id, invoice_date,
                                paid_full, paid, total)
SELECT m.NewUuid,
    CONCAT('INV-', @YearStr, '-', RIGHT('0000' + CAST(ROW_NUMBER() OVER (ORDER BY o.invoice_date) AS VARCHAR(4)), 4)),
    o.client_id,
    o.user_id,
    o.invoice_date,
    o.paid_full,
    o.paid,
    o.total
FROM SalesInvoices o
JOIN InvoiceIdMapping m ON o.Id = m.OldIntId;

DROP TABLE SalesInvoices;
ALTER TABLE SalesInvoices_new RENAME TO SalesInvoices;

-- 4. MIGRATE THE CHILD TABLE (InvoiceItems)
CREATE TABLE IF NOT EXISTS SalesInvoiceItems_new (
    id TEXT PRIMARY KEY NOT NULL,
    invoice_id TEXT NOT NULL,
    product_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price REAL NOT NULL,
    total REAL NOT NULL,
    FOREIGN KEY (invoice_id) REFERENCES SalesInvoices(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(id)
);

-- Copy child data over, translating old INT foreign key to the new GUID foreign key

INSERT INTO SalesInvoiceItems_new(id, invoice_id, product_id,quantity,unit_price,total)
SELECT
    lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || 
    substr(lower(hex(randomblob(2))), 2) || '-a' || 
    substr(lower(hex(randomblob(2))), 2) || '-' || 
    lower(hex(randomblob(6))) AS id,
    m.NewUuid AS invoice_id,
    ssi.product_id,
    ssi.quantity,
    ssi.unit_price,
    ssi.total
FROM SalesInvoiceItems sii 
JOIN InvoiceIdMapping m ON sii.invoice_id = m.OldIntId;

DROP TABLE SalesInvoiceItems;
ALTER TABLE SalesInvoiceItems_new RENAME TO SalesInvoiceItems;



COMMIT;