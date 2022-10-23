
--CREATE TABLE customer (
--  id		INT PRIMARY KEY IDENTITY(1, 1),
--  name		VARCHAR(50) NOT NULL,
--  email     VARCHAR(100) UNIQUE NOT NULL,
--  password  VARCHAR(100) NOT NULL,
--  created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--  updated_at DATETIME2
--);

--ALTER TABLE customer
--ALTER COLUMN updated_at DATETIME2 NOT NULL;
--ALTER TABLE customer
--ADD CONSTRAINT customer_updated_at_default DEFAULT SYSDATETIME() FOR updated_at;
--GO

--CREATE OR ALTER TRIGGER set_customer_updated_at ON customer
--AFTER UPDATE
--NOT FOR REPLICATION
--AS 
--UPDATE customer
--SET updated_at = SYSDATETIME()
--FROM customer
--JOIN INSERTED ON customer.id = INSERTED.id;
--GO

--CREATE TABLE category (
--  id		INT PRIMARY KEY IDENTITY,
--  name		VARCHAR(50) NOT NULL,
--  parent_id  INT NULL,
--  created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--  updated_at DATETIME2 NOT NULL DEFAULT SYSDATETIME()
--  FOREIGN KEY (parent_id) REFERENCES category(id)
--);
--GO

--ALTER TABLE category
--ADD customer_id INT NULL;
--ALTER TABLE category
--ADD FOREIGN KEY (customer_id) REFERENCES customer(id);
--GO

--CREATE TRIGGER set_category_updated_at ON category
--AFTER UPDATE
--NOT FOR REPLICATION
--AS 
--UPDATE category
--SET updated_at = SYSDATETIME()
--FROM category
--JOIN INSERTED ON category.id = INSERTED.id;
--GO

--INSERT INTO category(name) VALUES ('Parent Category');
--INSERT INTO category(name, parent_id) VALUES ('Category', 1);

--CREATE TABLE question_type (
--  id		INT PRIMARY KEY IDENTITY(1, 1),
--  name		VARCHAR(50) NOT NULL,
--  icon     VARCHAR(50) NOT NULL,
--  link  VARCHAR(50) NOT NULL,
--  created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--  updated_at DATETIME2 NOT NULL DEFAULT SYSDATETIME()
--);
--GO

--CREATE TRIGGER set_question_type_updated_at ON question_type
--AFTER UPDATE
--NOT FOR REPLICATION
--AS 
--UPDATE question_type
--SET updated_at = SYSDATETIME()
--FROM question_type
--JOIN INSERTED ON question_type.id = INSERTED.id;
--GO

--INSERT INTO question_type(name, icon, link) VALUES ('Multiple Choice', 'card-checklist', 'multiple-choice');
--INSERT INTO question_type(name, icon, link) VALUES ('True False', 'check|x', 'true-false');
--INSERT INTO question_type(name, icon, link) VALUES ('Matching', 'arrow-left-right', 'matching');
--INSERT INTO question_type(name, icon, link) VALUES ('Free Text', 'input-cursor-text', 'free-text');
--INSERT INTO question_type(name, icon, link) VALUES ('Essay', 'justify-left', 'essay');

--CREATE TABLE question (
--  id				INT PRIMARY KEY IDENTITY(1, 1),
--  type_id			INT NOT NULL,
--  category_id		INT NOT NULL,
--  points			DECIMAL(10, 4) NOT NULL,
--  shuffle			TINYINT NULL,
--  selection		TINYINT NULL,
--  question		TEXT NOT NULL,
--  customer_id		INT NOT NULL,
--  created_at		DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--  updated_at		DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--  FOREIGN KEY (type_id) REFERENCES question_type(id),
--  FOREIGN KEY (category_id) REFERENCES category(id),
--  FOREIGN KEY (customer_id) REFERENCES customer(id)
--);
--GO

--ALTER TABLE question
--ADD penalty		DECIMAL(10, 4) NULL;
--GO

--CREATE TRIGGER set_question_updated_at ON question
--AFTER UPDATE
--NOT FOR REPLICATION
--AS 
--UPDATE question
--SET updated_at = SYSDATETIME()
--FROM question
--JOIN INSERTED ON question.id = INSERTED.id;
--GO

--CREATE TABLE answer (
--  id				INT PRIMARY KEY IDENTITY(1, 1),
--  answer			TEXT NOT NULL,
--  match			TEXT NULL,
--  question_id		INT NOT NULL,
--  correct			BIT NOT NULL,
--  points			DECIMAL(10, 4) NULL,
--  penalty			DECIMAL(10, 4) NULL,
--  customer_id		INT NOT NULL,
--  created_at		DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--  updated_at		DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--  FOREIGN KEY (question_id) REFERENCES question(id),
--  FOREIGN KEY (customer_id) REFERENCES customer(id)
--);
--GO

--CREATE TRIGGER set_answer_updated_at ON answer
--AFTER UPDATE
--NOT FOR REPLICATION
--AS 
--UPDATE answer
--SET updated_at = SYSDATETIME()
--FROM answer
--JOIN INSERTED ON answer.id = INSERTED.id;
--GO

--CREATE TABLE test (
--  id				INT PRIMARY KEY IDENTITY(1, 1),
--  name				VARCHAR(30) NOT NULL,
--  introduction		TEXT NULL,
--  limit				INT NULL,
--  category_id		INT NOT NULL,
--  customer_id		INT NOT NULL,
--  created_at		DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--  updated_at		DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--  FOREIGN KEY (category_id) REFERENCES category(id),
--  FOREIGN KEY (customer_id) REFERENCES customer(id)
--);
--GO

--CREATE TRIGGER set_test_updated_at ON test
--AFTER UPDATE
--NOT FOR REPLICATION
--AS 
--UPDATE test
--SET updated_at = SYSDATETIME()
--FROM test
--JOIN INSERTED ON test.id = INSERTED.id;
--GO

--CREATE TABLE page (
--	id				INT PRIMARY KEY IDENTITY(1, 1),
--	name			VARCHAR(30) NULL,
--	limit			INT NULL,
--	position		INT NOT NULL DEFAULT '0',
--	test_id			INT NOT NULL,
--	customer_id		INT NOT NULL,
--	created_at		DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--	updated_at		DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--	FOREIGN KEY (test_id) REFERENCES test(id),
--	FOREIGN KEY (customer_id) REFERENCES customer(id)
--);
--GO

--ALTER TABLE page
--ADD shuffle BIT NOT NULL DEFAULT '0';
--GO

--CREATE TRIGGER set_page_updated_at ON page
--AFTER UPDATE
--NOT FOR REPLICATION
--AS 
--UPDATE page
--SET updated_at = SYSDATETIME()
--FROM page
--JOIN INSERTED ON page.id = INSERTED.id;
--GO

--CREATE TABLE test_question (
--	id				INT PRIMARY KEY IDENTITY(1, 1),
--	page_id			INT NOT NULL,
--	position		INT NOT NULL DEFAULT '0',
--	random			BIT NOT NULL DEFAULT '0',
	
--	question_id		INT NULL,
	
--	category_id		INT NULL,
--	subcategory_id	INT NULL,
--	type_id			INT NULL,
--	number			INT NULL,
--	question_ids	VARCHAR(200),

--	customer_id		INT NOT NULL,
--	created_at		DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
--	updated_at		DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

--	FOREIGN KEY (page_id) REFERENCES page(id),
--	FOREIGN KEY (question_id) REFERENCES question(id),
--	FOREIGN KEY (category_id) REFERENCES category(id),
--	FOREIGN KEY (subcategory_id) REFERENCES category(id),
--	FOREIGN KEY (type_id) REFERENCES question_type(id),
--	FOREIGN KEY (customer_id) REFERENCES customer(id),
--);
--GO

--CREATE TRIGGER set_test_question_updated_at ON test_question
--AFTER UPDATE
--NOT FOR REPLICATION
--AS 
--UPDATE test_question
--SET updated_at = SYSDATETIME()
--FROM test_question
--JOIN INSERTED ON test_question.id = INSERTED.id;
--GO

--use TestBuilderDb;
--GO
--EXEC sp_rename 'dbo.test_question', 'page_question';

--DROP TRIGGER set_test_question_updated_at;
--GO

--CREATE TRIGGER set_page_question_updated_at ON page_question
--AFTER UPDATE
--NOT FOR REPLICATION
--AS 
--UPDATE page_question
--SET updated_at = SYSDATETIME()
--FROM page_question
--JOIN INSERTED ON page_question.id = INSERTED.id;
--GO

--ALTER TABLE test 
--DROP COLUMN limit;
--GO


--select Id, Name as 'A.B'
--from test

SELECT q.id AS Id, q.type_id AS TypeId, q.shuffle AS Shuffle, q.selection AS Selection, 
    q.question AS _Question, q.points AS Points, q.penalty AS Penalty,
    c.id AS 'Category.Id', c.name AS 'Category.Name',
    cp.id AS 'Category.Parent.Id', cp.name AS 'Category.Parent.Name'
FROM question q
INNER JOIN category c ON c.id = q.category_id AND (c.customer_id = 1 OR c.customer_id IS NULL)
LEFT JOIN category cp ON cp.id = c.parent_id AND (cp.customer_id = 1 OR cp.customer_id IS NULL)
WHERE q.id IN (1, 2) AND q.customer_id = 1
ORDER BY q.id
