-- DISABLE FOREIGN KEYS TO ALLOW DROPPING TABLES
SET FOREIGN_KEY_CHECKS = 0;

-- DROP TABLES IF EXIST
DROP TABLE IF EXISTS OrderItems;

DROP TABLE IF EXISTS Orders;

DROP TABLE IF EXISTS Products;
-- Users table is kept or can be dropped if you want a full reset.
-- For now we keep Users to avoid forcing re-registration every time,
-- BUT if you changed User model extensively invoke: DROP TABLE IF EXISTS Users;

-- CREATE PRODUCTS TABLE
CREATE TABLE Products (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Sku VARCHAR(50) NOT NULL UNIQUE,
    Description TEXT,
    Price DECIMAL(10, 2) NOT NULL,
    Category VARCHAR(50) NOT NULL, -- T-Shirt, Sweat, Casaco
    Gender VARCHAR(20) NOT NULL, -- Homem, Mulher
    ImageUrl VARCHAR(500), -- URL da imagem
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- CREATE ORDERS TABLE
CREATE TABLE Orders (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    OrderDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    Status VARCHAR(50) DEFAULT 'Pendente',
    FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
);

-- CREATE ORDER ITEMS TABLE
CREATE TABLE OrderItems (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    OrderId BIGINT NOT NULL,
    ProductId BIGINT NOT NULL,
    ProductName VARCHAR(255),
    Price DECIMAL(10, 2) NOT NULL,
    Quantity INT NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders (Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products (Id) ON DELETE CASCADE
);

-- RE-ENABLE FOREIGN KEYS
SET FOREIGN_KEY_CHECKS = 1;

-- SEED DATA (CATALOGO)
-- T-SHIRTS
INSERT INTO
    Products (
        Name,
        Sku,
        Description,
        Price,
        Category,
        Gender,
        ImageUrl
    )
VALUES (
        'T-Shirt Básica Homem',
        'TSHIRT-H-001',
        'T-Shirt de algodão confortável.',
        15.00,
        'T-Shirt',
        'Homem',
        'https://via.placeholder.com/300?text=T-Shirt+Homem'
    ),
    (
        'T-Shirt Básica Mulher',
        'TSHIRT-M-001',
        'T-Shirt com corte feminino.',
        15.00,
        'T-Shirt',
        'Mulher',
        'https://via.placeholder.com/300?text=T-Shirt+Mulher'
    );

-- SWEATS
INSERT INTO
    Products (
        Name,
        Sku,
        Description,
        Price,
        Category,
        Gender,
        ImageUrl
    )
VALUES (
        'Sweat Capuz Homem',
        'SWEAT-H-001',
        'Sweat quente com capuz.',
        35.00,
        'Sweat',
        'Homem',
        'https://via.placeholder.com/300?text=Sweat+Homem'
    ),
    (
        'Sweat Capuz Mulher',
        'SWEAT-M-001',
        'Sweat confortável para o dia-a-dia.',
        35.00,
        'Sweat',
        'Mulher',
        'https://via.placeholder.com/300?text=Sweat+Mulher'
    );

-- CASACOS
INSERT INTO
    Products (
        Name,
        Sku,
        Description,
        Price,
        Category,
        Gender,
        ImageUrl
    )
VALUES (
        'Casaco Impermeável Homem',
        'JACKET-H-001',
        'Casaco resistente à chuva.',
        60.00,
        'Casaco',
        'Homem',
        'https://via.placeholder.com/300?text=Casaco+Homem'
    ),
    (
        'Casaco Bomber Mulher',
        'JACKET-M-001',
        'Casaco estilo bomber moderno.',
        55.00,
        'Casaco',
        'Mulher',
        'https://via.placeholder.com/300?text=Casaco+Mulher'
    );