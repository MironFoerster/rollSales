PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE customers (name text primary key, stdamount integer, tempamount integer, unpaidrolls integer, unpaidcosts integer);
INSERT INTO customers VALUES('opa',10,0,0,0);
INSERT INTO customers VALUES('kretschmer',16,0,0,0);
INSERT INTO customers VALUES('exner',10,0,0,0);
INSERT INTO customers VALUES('dahl',8,0,0,0);
CREATE TABLE logs (date text, customer text, rolls integer);
COMMIT;