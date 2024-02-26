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

    columnCHAR CHAR DEFAULT('1'),
    columnCHARWithRange CHAR(10) DEFAULT('1'),
    columnVARCHAR VARCHAR(10) DEFAULT('1'),
    columnBINARY BINARY DEFAULT(1),
    columnBINARYWithRange BINARY(10) DEFAULT(1),
    columnVARBINARY VARBINARY(10) DEFAULT(1),
    columnTINYBLOB TINYBLOB DEFAULT(1),
    columnTINYTEXT TINYTEXT DEFAULT('1'),
    columnTEXT TEXT DEFAULT('1'),
    columnTEXTWithRange TEXT(10) DEFAULT('1'),
    columnBLOB BLOB DEFAULT(1),
    columnBLOBWithRange BLOB(10) DEFAULT(1),
    columnMEDIUMTEXT MEDIUMTEXT DEFAULT('1'),
    columnMEDIUMBLOB MEDIUMBLOB DEFAULT(1),
    columnLONGTEXT LONGTEXT DEFAULT('1'),
    columnLONGBLOB LONGBLOB DEFAULT(1),
    columnBIT BIT DEFAULT(1),
    columnBITWithRange BIT(10) DEFAULT(1),
    columnTINYINT TINYINT DEFAULT(1),
    columnTINYINTWithRange TINYINT(10) DEFAULT(1),
    columnBOOL BOOL DEFAULT(1),
    columnBOOLEAN BOOLEAN DEFAULT(1),
    columnSMALLINT SMALLINT DEFAULT(1),
    columnSMALLINTWithRange SMALLINT(10) DEFAULT(1),
    columnMEDIUMINT MEDIUMINT DEFAULT(1),
    columnMEDIUMINTWithRange MEDIUMINT(10) DEFAULT(1),
    columnINT INT DEFAULT(1),
    columnINTWithRange INT(10) DEFAULT(1),
    columnINTEGER INTEGER DEFAULT(1),
    columnINTEGERWithRange INTEGER(10) DEFAULT(1),
    columnBIGINT BIGINT DEFAULT(1),
    columnBIGINTWithRange BIGINT(10) DEFAULT(1),
    columnFLOAT FLOAT DEFAULT(1),
    columnFLOATWithRangeAndPrecision FLOAT(10, 2) DEFAULT(1),
    columnFLOATWithRange FLOAT(10) DEFAULT(1),
    columnDOUBLE DOUBLE DEFAULT(1),
    columnDOUBLEWithRange DOUBLE(10, 2) DEFAULT(1),
    columnDOUBLEPRECISION  DOUBLE PRECISION DEFAULT(1),
    columnDOUBLEPRECISIONWithRange  DOUBLE PRECISION(10, 2) DEFAULT(1),
    columnDECIMAL DECIMAL DEFAULT(1),
    columnDECIMALWithRange DECIMAL(10, 2) DEFAULT(1),
    columnDEC DEC DEFAULT(1),
    columnDECWithRange DEC(10, 2) DEFAULT(1),
    columnDATE DATE DEFAULT('2024-02-06'),
    columnDATETIME DATETIME DEFAULT('2024-02-06 14:33:01'),
    columnTIMESTAMP TIMESTAMP DEFAULT('2024-02-06 14:33:01'),
    columnTIME TIME DEFAULT('14:33:01'),
    columnYEAR YEAR DEFAULT(2024),

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
