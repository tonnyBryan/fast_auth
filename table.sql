SET timezone = 'Indian/Antananarivo';
SHOW timezone;

CREATE DATABASE auth_data;

\connect auth_data;


CREATE TABLE Status(
   id_status SERIAL,
   description VARCHAR(50) ,
   PRIMARY KEY(id_status)
);

CREATE TABLE Users(
   id_user SERIAL,
   username VARCHAR(50) UNIQUE NOT NULL,
   password VARCHAR(255)  NOT NULL,
   email VARCHAR(100) UNIQUE  NOT NULL,
   nb_tentative INTEGER,
   id_status INTEGER NOT NULL,
   date_creation timestamp,
   PRIMARY KEY(id_user),
   FOREIGN KEY(id_status) REFERENCES Status(id_status)
);

CREATE TABLE authentification(
	id_auth SERIAL,
	id_user INTEGER NOT NULL,
	pin VARCHAR(10) NOT NULL,
	expiration timestamp,
	akey TEXT unique not null,
	PRIMARY KEY(id_auth)
);

CREATE TABLE Tokens(
	id_token SERIAL PRIMARY KEY,
	token TEXT not null,
	date_creation timestamp,
	date_expiration timestamp,
	id_user INT REFERENCES Users(id_user)
);

CREATE TABLE unique_key (
	skey text PRIMARY KEY,
	id_user INT REFERENCES Users(id_user)
);

CREATE TABLE reset_mail_request(
	id_reset SERIAL PRIMARY KEY,
	email VARCHAR(100) UNIQUE  NOT NULL,
	rkey TEXT unique
);


insert into Status(id_status, description) values (1 , 'normal');
insert into Status(id_status, description) values (2 , 'attente');
insert into Status(id_status, description) values (3 , 'bloque');
insert into Status(id_status, description) values (4 , 'supprime');


