DROP TABLE IF EXISTS Funcionario;
DROP TABLE IF EXISTS Cargo;
DROP TABLE IF EXISTS Empresa;

CREATE TABLE Empresa
(
	id SERIAL NOT NULL,
	nome VARCHAR(128) NOT NULL,
	email VARCHAR(128) NOT NULL,

	CONSTRAINT PK_EmpresaId PRIMARY KEY (id),
	CONSTRAINT IDX_NomeEmail UNIQUE(nome, email)
);

CREATE TABLE Cargo
(
	id INT NOT NULL,
	idEmpresa INT NOT NULL CHECK(idEmpresa > 0),
	descricao VARCHAR(128) NOT NULL UNIQUE,

	CONSTRAINT PK_CargoId PRIMARY KEY (id, idEmpresa),
	CONSTRAINT FK_EmpresaId FOREIGN KEY (idEmpresa)
		REFERENCES Empresa(id)
		ON UPDATE CASCADE
);

CREATE TABLE Funcionario
(
	id SERIAL NOT NULL,
	nome VARCHAR(128) NOT NULL UNIQUE,
	email VARCHAR(128) NOT NULL DEFAULT ' ',
	idCargo INT NOT NULL CHECK(idCargo > 0),
	idEmpresa INT NOT NULL CHECK(idEmpresa > 0),
	dataCriacao TIMESTAMP DEFAULT(now()),

	colunBigInt BIGINT DEFAULT(1),
	columnNumeric NUMERIC DEFAULT(1),
	columnNumericWithRange NUMERIC(10) DEFAULT(1),
	columnBit BIT,
	columnSmallInt SMALLINT DEFAULT(1),
	columnDecimal DECIMAL DEFAULT(1),
	columnDecimalWithPrecision DECIMAL(10,2) DEFAULT(1),
	columnUuid UUID,
	columnInt INT DEFAULT(1),
	columnMoney MONEY DEFAULT(1),

	columnFloat FLOAT DEFAULT(1),
	columnReal REAL DEFAULT(1),

	columnDate DATE DEFAULT('2000-01-01'),
	columnDateTimeOffset TIMESTAMP DEFAULT('2000-01-01 12:00:00 +00:00'),
	columnTime TIME DEFAULT('15:00:00'),

	columnChar CHAR DEFAULT('A'),
	columnCharWithLength CHAR(100) DEFAULT('A'),
	columnVarChar VARCHAR DEFAULT('A'),
	columnVarCharWithLength VARCHAR(128) DEFAULT('A'),
	columnText TEXT DEFAULT('A'),

	columnNChar NCHAR DEFAULT('A'),
	columnNCharWithLength NCHAR(100) DEFAULT('A'),

	columnByteA BYTEA,
	columnXml XML DEFAULT('A'),

	CONSTRAINT PK_FuncionarioId PRIMARY KEY (id),
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
