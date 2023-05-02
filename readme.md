# Ohjeet ratkaisun k‰ynnist‰miseen ja testaamiseen.

Ratkaisussa k‰ytettyj‰ teknologioita:
 - Visual Studio 2022
 - .NET Framework
 - C#
 - postgreSQL
 - pgAdmin 4
 - Npgsql
 - xUnit

## Tietokanta
Ohjelmma toimii postgreSQL tietokannan kanssa.
Annan tarkemmat ohjeet tietokannan luomiseen pgAdmin 4:lla ja pelk‰st‰‰n SQL:n muita keinoja varten.
T‰ss‰ oletetaan ett‰ postgreSQL superuserin k‰ytt‰j‰nimi on "postgres". Muuta nimi annetussa koodissa tarvittaessa.

### pgAdmin 4
Aluksi luo serveri Anna nimeksi "tuplaus" ja portiksi "5433". Ohjeet on kirjoitettu n‰ill‰ tiedoilla, mutta tarvittaessa ne voi muuttaa.  
Seuraavaksi luo uusi Tietokanta (*Klikkaa serveri‰ oikeallapainikkeella -> Create -> Database...*). Anna tietokannan nimeksi "tuplaus".  
Seuraavaksi avaa *Query Tool* (*Alt Shift Q*).  
T‰ss‰ v‰lilehdess‰ kopio *Query* kentt‰‰n seuraava SQL:

```postgresql
-- DROP TABLE IF EXISTS tuplausGameEvents;
-- DROP TABLE IF EXISTS players;

CREATE TABLE players (
	playerID int PRIMARY KEY,
	playerName varchar(32),
	saldo int
);

CREATE TABLE tuplausGameEvents (
	eventTime timestamp DEFAULT CURRENT_TIMESTAMP,
	playerID int REFERENCES players(playerID),
	stake int,
	choice char(5),
	card int,
	prize int,
	PRIMARY KEY(eventTime, playerID)
);
```
T‰m‰n j‰lkeen juoksuta koodi (*F5*).

Nyt tietokannan pit‰isi olla valmis.

### SQL muita keinoja varten
T‰t‰ ei ole testattu.

#### Tietokanta
```postgrsql
-- Database: tuplaus

-- DROP DATABASE IF EXISTS tuplaus;

CREATE DATABASE tuplaus
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'English_Finland.1252'
    LC_CTYPE = 'English_Finland.1252'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;
```
#### Taulukot
```postgrsql
-- Table: players

-- DROP TABLE IF EXISTS players;

CREATE TABLE IF NOT EXISTS players
(
    playerid integer NOT NULL,
    playername character varying(32),
    saldo integer,
    CONSTRAINT players_pkey PRIMARY KEY (playerid)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS players
    OWNER to postgres;
```
```postgrsql
-- Table: tuplausgameevents

-- DROP TABLE IF EXISTS tuplausgameevents;

CREATE TABLE IF NOT EXISTS tuplausgameevents
(
    eventtime timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    playerid integer NOT NULL,
    stake integer,
    choice character(5)",
    card integer,
    prize integer,
    CONSTRAINT tuplausgameevents_pkey PRIMARY KEY (eventtime, playerid),
    CONSTRAINT tuplausgameevents_playerid_fkey FOREIGN KEY (playerid)
        REFERENCES players (playerid) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS tuplausgameevents
    OWNER to postgres;
```

## K‰ynnistys
Seuraavaksi katsotaan miten ohjelman saa k‰yntiin, ja jos tietokanta ei ole k‰ynniss‰, miten sen saa k‰yntiin.  
Ratkaisu on Visual Studio Solution, eli se pit‰‰ k‰ynnist‰‰ Visual Studiolla.  
Ohjelma tarvitsee admin oikeukset serverille yhdist‰miseen. T‰m‰n voi saavuttaa k‰ynnist‰m‰ll‰ Visual Studio adminina (*Klikkaa visual studiota oikealla painikkeella -> Run as administrator*)
Ratkaisun voi lˆyt‰‰ visual studion aloitus ikkunasta painamalla "Open a project or solution".  
Se avaa tiedosto selaimen, jossa pit‰‰ navigoida ratkaisun juureen ja valita *.sln* tiedosto.  
  
Avaa *Solution Explorerista* "tuplaus_teht.Service" projektin alta *DBHander.cs*.  
Etsi rivilt‰ 31 lˆytyv‰ *connectionString*.
```csharp
string connectionString = "host=localhost;port=5433;database=tuplaus;username=postgres;password=****";
```
**Muokkaa t‰t‰ merkkijonoa paikkaamalla salasana "****" postgres k‰ytt‰j‰n salasanalla.**  
Muuta myˆs muut tiedot, jos ne eroavat asetuksistasi. Esim. username ja port.
  
Pelimoottorin voi k‰ynnist‰‰ valitsemalla "tuplaus_teht.Service" yl‰palkista ja painamalla "Start". T‰m‰ ei ole kuitenkaan tarpeellista testejen juoksuttamiseen.  

## Testaus
Avaa *Test Explorer* painamalla yl‰palkista "Test" ja t‰st‰ avautuneesta valiksosta "Test Explorer".
Voit juoksuttaa testit t‰ss‰ n‰kym‰ss‰ painamalla nappulaa, jonka kuvake on kaksi p‰‰llekk‰ist‰ nuolta.  
**Testit tulevat toimimaan vain jos tietokanta serveri on p‰‰ll‰ ja vaativat ett‰ tietokannassa ei lˆydy pelaajia, joiden ID on 0-25.** 

### Tietokanta serveri
pgAdminilla tietokannan luodessa sen pit‰isi olla jo k‰ynniss‰, mutta jos ei ole, sen voi k‰ynnist‰‰ menem‰ll‰ PosgreSQL:n "/bin" tiedostoon PowerShellill‰.  
T‰m‰n voi tehd‰ PowerShelliss‰ seuraavalla komennolla, olettaen PostgreSQL on versiossa 15 ja ett‰ se on oletustallennussijainnissa.  
```powershell
cd "C:\Program Files\PostgreSQL\15\bin"
```
T‰‰ll‰ voi juoksuttaa seuraavan komennon k‰ynnist‰‰kseen tietokanta serverin.
```powershell
pg_ctl.exe start -D "C:\Program Files\PostgreSQL\15\data"
```
Komennon alkuun saattaa joutua pist‰‰ `.\`, jolloin komento olisi seuraava.
```powershell
.\pg_ctl.exe start -D "C:\Program Files\PostgreSQL\15\data"
```
Nyt serverin pit‰isi olla k‰ynniss‰, ja ohjelman voi k‰ynnist‰‰ ja testata.