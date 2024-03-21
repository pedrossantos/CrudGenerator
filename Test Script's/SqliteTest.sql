DROP TABLE IF EXISTS Funcionario;
DROP TABLE IF EXISTS Cargo;
DROP TABLE IF EXISTS Empresa;

CREATE TABLE Empresa
(
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	nome TEXT NOT NULL,
	email TEXT NOT NULL,

	CONSTRAINT IDX_NomeEmail UNIQUE(nome, email)
);

CREATE TABLE Cargo
(
	id INTEGER NOT NULL,
	idEmpresa INTEGER NOT NULL CHECK(idEmpresa > 0),
	descricao TEXT NOT NULL UNIQUE,

	CONSTRAINT PK_CargoId PRIMARY KEY (id, idEmpresa),
	CONSTRAINT FK_EmpresaId FOREIGN KEY (idEmpresa)
		REFERENCES Empresa(id)
		ON UPDATE CASCADE
);

CREATE TABLE Funcionario
(
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	nome TEXT NOT NULL UNIQUE,
	email TEXT NOT NULL DEFAULT ' ',
	idCargo INTEGER NOT NULL CHECK(idCargo > 0),
	idEmpresa INTEGER NOT NULL CHECK(idEmpresa > 0),
	dataCriacao DATETIME DEFAULT(datetime('now')),

	columnBlob BLOB DEFAULT(NULL),
	columnBoolean BOOLEAN DEFAULT(false),
	columnDate DATE DEFAULT('2000-01-01'),
	columnDateTime DATETIME DEFAULT('2000-01-01 12:00:00'),
	columnDateTimeWithTimeZone DATETIME DEFAULT('2000-01-01 12:00:00 -03:00'),
	columnInteger INTEGER DEFAULT(1),
	columnNumeric NUMERIC DEFAULT(1),
	columnReal REAL DEFAULT(1),
	columnText TEXT DEFAULT('A'),
	

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
