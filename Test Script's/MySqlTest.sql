CREATE DATABASE TesteDatabaseLocal;

USE TesteDatabaseLocal;
DROP TABLE IF EXISTS Funcionario;
DROP TABLE IF EXISTS Cargo;
DROP TABLE IF EXISTS Empresa;

CREATE TABLE Empresa
(
	id INT NOT NULL AUTO_INCREMENT,
	nome VARCHAR(128) NOT NULL,
	email VARCHAR(128) NOT NULL,

	CONSTRAINT PK_EmpresaId PRIMARY KEY CLUSTERED (id),
	CONSTRAINT IDX_NomeEmail UNIQUE(nome, email)
);

CREATE TABLE Cargo
(
	id INT NOT NULL,
	idEmpresa INT NOT NULL,
	descricao VARCHAR(128) NOT NULL UNIQUE,

	CONSTRAINT PK_CargoId PRIMARY KEY CLUSTERED (id, idEmpresa),
	CONSTRAINT FK_EmpresaId FOREIGN KEY (idEmpresa)
		REFERENCES Empresa(id)
		ON UPDATE CASCADE
);

CREATE TABLE Funcionario
(
	id INT NOT NULL AUTO_INCREMENT,
	nome VARCHAR(128) NOT NULL UNIQUE,
	email VARCHAR(128) NOT NULL DEFAULT ' ',
	idCargo INT NOT NULL,
	idEmpresa INT NOT NULL,
	dataCriacao TIMESTAMP DEFAULT(NOW()),

    columnChar CHAR DEFAULT('1'),
    columnCharWithRange CHAR(10) DEFAULT('1'),
    columnVarChar VARCHAR(10) DEFAULT('1'),
    columnBinary BINARY DEFAULT(1),
    columnBinaryWithRange BINARY(10) DEFAULT(1),
    columnVarBinary VARBINARY(10) DEFAULT(1),
    columnTinyBlob TINYBLOB DEFAULT(1),
    columnTinyText TINYTEXT DEFAULT('1'),
    columnText TEXT DEFAULT('1'),
    columnTextWithRange TEXT(10) DEFAULT('1'),
    columnBlob BLOB DEFAULT(1),
    columnBlobWithRange BLOB(10) DEFAULT(1),
    columnMediumText MEDIUMTEXT DEFAULT('1'),
    columnMediumBlob MEDIUMBLOB DEFAULT(1),
    columnLongText LONGTEXT DEFAULT('1'),
    columnLongBlob LONGBLOB DEFAULT(1),
    columnBit BIT DEFAULT(1),
    columnBitWithRange BIT(10) DEFAULT(1),
    columnTinyInt TINYINT DEFAULT(1),
    columnTinyIntWithRange TINYINT(10) DEFAULT(1),
    columnBool BOOL DEFAULT(1),
    columnBoolean BOOLEAN DEFAULT(1),
    columnSmallInt SMALLINT DEFAULT(1),
    columnSmallIntWithRange SMALLINT(10) DEFAULT(1),
    columnMediumInt MEDIUMINT DEFAULT(1),
    columnMediumIntWithRange MEDIUMINT(10) DEFAULT(1),
    columnInt INT DEFAULT(1),
    columnIntWithRange INT(10) DEFAULT(1),
    columnInteger INTEGER DEFAULT(1),
    columnIntegerWithRange INTEGER(10) DEFAULT(1),
    columnBigInt BIGINT DEFAULT(1),
    columnBigIntWithRange BIGINT(10) DEFAULT(1),
    columnFloat FLOAT DEFAULT(1),
    columnFloatWithRangeAndPrecision FLOAT(10, 2) DEFAULT(1),
    columnFloatWithRange FLOAT(10) DEFAULT(1),
    columnDouble DOUBLE DEFAULT(1),
    columnDoubleWithRange DOUBLE(10, 2) DEFAULT(1),
    columnDoublePrecision DOUBLE PRECISION DEFAULT(1),
    columnDoublePrecisionWithRange  DOUBLE PRECISION(10, 2) DEFAULT(1),
    columnDecimal DECIMAL DEFAULT(1),
    columnDecimalWithRange DECIMAL(10, 2) DEFAULT(1),
    columnDec DEC DEFAULT(1),
    columnDecWithRange DEC(10, 2) DEFAULT(1),
    columnDate DATE DEFAULT('2024-02-06'),
    columnDateTime DATETIME DEFAULT('2024-02-06 14:33:01'),
    columnTimeStamp TIMESTAMP DEFAULT('2024-02-06 14:33:01'),
    columnTime TIME DEFAULT('14:33:01'),
    columnYear YEAR DEFAULT(2024),

	CONSTRAINT PK_FuncionarioId PRIMARY KEY CLUSTERED (id),
	CONSTRAINT FK_CargoId FOREIGN KEY (idCargo,idEmpresa)
		REFERENCES Cargo(id, idEmpresa)
		ON UPDATE CASCADE
);

INSERT INTO Empresa(nome, email) VALUES('Empresa Teste1', 'email@empresateste1.com.br');
INSERT INTO Empresa(nome, email) VALUES('Empresa Teste2', 'email@empresateste1.com.br');

INSERT INTO Cargo(id, idEmpresa, descricao) VALUES(1, 1, 'Cargo Empresa 1');
INSERT INTO Cargo(id, idEmpresa, descricao) VALUES(1, 2, 'Cargo Empresa 2');

INSERT INTO Funcionario(nome, email, idCargo, idEmpresa) VALUES('Funcionario 1', 'funcionario1@empresateste1.com.br', 1, 1);
INSERT INTO Funcionario(nome, idCargo, idEmpresa) VALUES('Funcionario 2', 1, 2);

SELECT * FROM Empresa;
SELECT * FROM Cargo	;
SELECT * FROM Funcionario;
