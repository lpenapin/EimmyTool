CREATE TABLE IF NOT EXISTS Users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
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

CREATE TABLE IF NOT EXISTS Clients (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    DNI TEXT,
    email TEXT,
    phone TEXT,
    address TEXT,
    debt DECIMAL(18,2) DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE NOT EXISTS Products_new (
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

CREATE TABLE IF NOT EXISTS Suppliers (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    DNI TEXT NOT NULL,
    email TEXT,
    phone TEXT,
    address TEXT,
    debt DECIMAL(18,2) DEFAULT 0,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS SalesInvoices (
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

CREATE TABLE IF NOT EXISTS SalesInvoiceItems (
    id TEXT PRIMARY KEY NOT NULL,
    invoice_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (invoice_id) REFERENCES SalesInvoices(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(id)
);

CREATE TABLE IF NOT EXISTS Salaries (
    id TEXT PRIMARY KEY NOT NULL,
    salary_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    user_id INTEGER,
    hours_worked DECIMAL(18,2),
    hour_cost DECIMAL(18,2),
    total_paid DECIMAL(18,2),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS PurchaseInvoices (
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

CREATE TABLE IF NOT EXISTS PurchaseInvoiceItems (
    id TEXT PRIMARY KEY NOT NULL,
    invoice_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    unit_cost DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (invoice_id) REFERENCES PurchaseInvoices(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(id)
);

CREATE TABLE IF NOT EXISTS PayDebtInvoices (
    id TEXT PRIMARY KEY NOT NULL,
    client_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paid DECIMAL(18,2),
    FOREIGN KEY (client_id) REFERENCES Clients(id),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS PayCreditInvoices (
    id TEXT PRIMARY KEY NOT NULL,
    supplier_id TEXT NOT NULL,
    user_id TEXT NOT NULL,
    invoice_date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paid DECIMAL(18,2),
    FOREIGN KEY (supplier_id) REFERENCES Suppliers(id),
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS InventoryMovements (
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

CREATE TABLE IF NOT EXISTS GeneralExpenses (
    id TEXT PRIMARY KEY NOT NULL,
    invoice_ref TEXT NOT NULL,
    details TEXT NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    user_id TEXT,
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS CashFlow (
    id TEXT PRIMARY KEY NOT NULL,
    date TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    type TEXT,
    detail TEXT,
    value DECIMAL(18,2),
    reference_table TEXT,
    reference_id TEXT,
    user_id TEXT,
    FOREIGN KEY (user_id) REFERENCES Users(id)
);

CREATE TABLE IF NOT EXISTS ReturnInvoices (
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

CREATE TABLE IF NOT EXISTS ReturnInvoiceItems (
    id TEXT PRIMARY KEY NOT NULL,
    return_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    FOREIGN KEY (return_id) REFERENCES ReturnInvoices(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(id)
);

CREATE TABLE IF NOT EXISTS ReturnSupplierInvoices (
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

CREATE TABLE IF NOT EXISTS ReturnSupplierInvoiceItems (
    id TEXT PRIMARY KEY NOT NULL,
    return_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    FOREIGN KEY (return_id) REFERENCES ReturnSupplierInvoices(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(id)
);

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