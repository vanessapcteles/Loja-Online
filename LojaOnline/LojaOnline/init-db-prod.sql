CREATE DATABASE IF NOT EXISTS LojaOnline;
USE LojaOnline;

-- Criar tabela Products se não existir
CREATE TABLE IF NOT EXISTS `Products` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Sku` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Price` decimal(65,30) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Inserir dados apenas se a tabela estiver vazia
INSERT INTO `Products` (`Name`, `Sku`, `Description`, `Price`, `CreatedAt`)
SELECT * FROM (SELECT 'Produto Exemplo 1', 'SKU-001', 'Descrição do produto teste 1', 19.99, NOW()) AS tmp
WHERE NOT EXISTS (
    SELECT Name FROM Products WHERE Sku = 'SKU-001'
) LIMIT 1;
