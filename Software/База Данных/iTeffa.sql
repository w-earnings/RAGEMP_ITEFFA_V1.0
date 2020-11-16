-- --------------------------------------------------------
-- Хост:                         127.0.0.1
-- Версия сервера:               5.6.13 - MySQL Community Server (GPL)
-- Операционная система:         Win32
-- HeidiSQL Версия:              11.0.0.5919
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;


-- Дамп структуры базы данных iteffa
CREATE DATABASE IF NOT EXISTS `iteffa` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE `iteffa`;

-- Дамп структуры для таблица iteffa.accounts
CREATE TABLE IF NOT EXISTS `accounts` (
  `socialclub` text NOT NULL,
  `login` varchar(155) NOT NULL,
  `hwid` varchar(155) NOT NULL,
  `coins` varchar(155) NOT NULL,
  `ip` varchar(155) NOT NULL,
  `character1` varchar(155) NOT NULL,
  `character2` varchar(155) NOT NULL,
  `character3` varchar(155) NOT NULL,
  `email` varchar(155) NOT NULL,
  `password` varchar(155) NOT NULL,
  `viplvl` varchar(155) NOT NULL,
  `vipdate` datetime NOT NULL,
  `promocodes` varchar(155) NOT NULL,
  `present` tinyint(1) NOT NULL DEFAULT '0',
  `idkey` int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.accounts: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `accounts` DISABLE KEYS */;
/*!40000 ALTER TABLE `accounts` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.adminaccess
CREATE TABLE IF NOT EXISTS `adminaccess` (
  `minrank` int(11) NOT NULL,
  `command` varchar(155) NOT NULL,
  `isadmin` tinyint(1) NOT NULL,
  `idkey` int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.adminaccess: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `adminaccess` DISABLE KEYS */;
/*!40000 ALTER TABLE `adminaccess` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.advertised
CREATE TABLE IF NOT EXISTS `advertised` (
  `ID` int(12) unsigned NOT NULL AUTO_INCREMENT,
  `Author` varchar(40) NOT NULL,
  `AuthorSIM` int(11) NOT NULL,
  `AD` varchar(150) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `Editor` varchar(40) DEFAULT NULL,
  `EditedAD` varchar(150) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
  `Opened` datetime NOT NULL,
  `Closed` datetime DEFAULT NULL,
  `Status` tinyint(4) DEFAULT '0',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Дамп данных таблицы iteffa.advertised: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `advertised` DISABLE KEYS */;
/*!40000 ALTER TABLE `advertised` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.alcoclubs
CREATE TABLE IF NOT EXISTS `alcoclubs` (
  `id` int(155) NOT NULL,
  `alco1` int(155) NOT NULL,
  `alco2` int(155) NOT NULL,
  `alco3` int(155) NOT NULL,
  `pricemod` varchar(155) NOT NULL,
  `mats` int(155) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.alcoclubs: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `alcoclubs` DISABLE KEYS */;
/*!40000 ALTER TABLE `alcoclubs` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.banned
CREATE TABLE IF NOT EXISTS `banned` (
  `uuid` int(155) NOT NULL,
  `name` text NOT NULL,
  `account` text NOT NULL,
  `time` varchar(155) NOT NULL,
  `until` varchar(155) NOT NULL,
  `ishard` bigint(155) NOT NULL,
  `ip` varchar(155) NOT NULL,
  `socialclub` text NOT NULL,
  `hwid` varchar(155) NOT NULL,
  `reason` text NOT NULL,
  `byadmin` text NOT NULL,
  PRIMARY KEY (`uuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.banned: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `banned` DISABLE KEYS */;
/*!40000 ALTER TABLE `banned` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.businesses
CREATE TABLE IF NOT EXISTS `businesses` (
  `id` int(255) NOT NULL,
  `owner` text NOT NULL,
  `sellprice` text NOT NULL,
  `type` text NOT NULL,
  `products` text NOT NULL,
  `enterpoint` text NOT NULL,
  `unloadpoint` text NOT NULL,
  `money` text NOT NULL,
  `mafia` text NOT NULL,
  `orders` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Дамп данных таблицы iteffa.businesses: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `businesses` DISABLE KEYS */;
/*!40000 ALTER TABLE `businesses` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.characters
CREATE TABLE IF NOT EXISTS `characters` (
  `uuid` bigint(255) NOT NULL,
  `adminlvl` int(255) NOT NULL,
  `money` int(255) NOT NULL,
  `firstname` text NOT NULL,
  `lastname` text NOT NULL,
  `fraction` int(255) NOT NULL,
  `fractionlvl` int(255) NOT NULL,
  `warns` int(255) NOT NULL,
  `biz` text NOT NULL,
  `hotel` int(255) NOT NULL,
  `hotelleft` int(255) NOT NULL,
  `sim` int(255) NOT NULL,
  `PetName` text,
  `eat` int(255) NOT NULL,
  `water` int(255) NOT NULL,
  `demorgan` int(255) NOT NULL,
  `arrest` int(255) NOT NULL,
  `unwarn` datetime NOT NULL,
  `unmute` int(255) NOT NULL,
  `bank` int(255) NOT NULL,
  `wanted` text,
  `lvl` int(255) NOT NULL,
  `exp` int(255) NOT NULL,
  `gender` tinyint(1) NOT NULL,
  `health` int(255) NOT NULL,
  `armor` int(255) NOT NULL,
  `licenses` text NOT NULL,
  `lastveh` text NOT NULL,
  `onduty` tinyint(1) NOT NULL,
  `lasthour` int(255) NOT NULL,
  `contacts` text NOT NULL,
  `achiev` text NOT NULL,
  `createdate` datetime NOT NULL,
  `pos` text NOT NULL,
  `work` int(255) NOT NULL,
  `idkey` int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.characters: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `characters` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.customization
CREATE TABLE IF NOT EXISTS `customization` (
  `uuid` bigint(255) NOT NULL,
  `gender` text NOT NULL,
  `parents` text NOT NULL,
  `features` text NOT NULL,
  `appearance` text NOT NULL,
  `hair` text NOT NULL,
  `clothes` text NOT NULL,
  `accessory` text NOT NULL,
  `tattoos` text NOT NULL,
  `eyebrowc` text NOT NULL,
  `beardc` text NOT NULL,
  `eyec` text NOT NULL,
  `blushc` text NOT NULL,
  `lipstickc` text NOT NULL,
  `chesthairc` text NOT NULL,
  `iscreated` tinyint(1) NOT NULL,
  `idkey` int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.customization: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `customization` DISABLE KEYS */;
/*!40000 ALTER TABLE `customization` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.eventcfg
CREATE TABLE IF NOT EXISTS `eventcfg` (
  `RewardLimit` int(155) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.eventcfg: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `eventcfg` DISABLE KEYS */;
/*!40000 ALTER TABLE `eventcfg` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.e_candidates
CREATE TABLE IF NOT EXISTS `e_candidates` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Votes` text NOT NULL,
  `Election` text NOT NULL,
  `Name` text NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.e_candidates: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `e_candidates` DISABLE KEYS */;
/*!40000 ALTER TABLE `e_candidates` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.e_points
CREATE TABLE IF NOT EXISTS `e_points` (
  `Election` int(155) NOT NULL AUTO_INCREMENT,
  `X` varchar(11) NOT NULL,
  `Y` varchar(11) NOT NULL,
  `Z` varchar(11) NOT NULL,
  `Dimension` int(11) NOT NULL,
  `Opened` text NOT NULL,
  PRIMARY KEY (`Election`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.e_points: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `e_points` DISABLE KEYS */;
/*!40000 ALTER TABLE `e_points` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.e_voters
CREATE TABLE IF NOT EXISTS `e_voters` (
  `Election` int(155) NOT NULL,
  `Login` text NOT NULL,
  `TimeVoted` text NOT NULL,
  `VotedFor` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.e_voters: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `e_voters` DISABLE KEYS */;
/*!40000 ALTER TABLE `e_voters` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.fractionaccess
CREATE TABLE IF NOT EXISTS `fractionaccess` (
  `idkey` int(155) NOT NULL AUTO_INCREMENT,
  `fraction` int(155) NOT NULL,
  `commands` text NOT NULL,
  `weapons` text NOT NULL,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Дамп данных таблицы iteffa.fractionaccess: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `fractionaccess` DISABLE KEYS */;
/*!40000 ALTER TABLE `fractionaccess` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.fractionranks
CREATE TABLE IF NOT EXISTS `fractionranks` (
  `idkey` int(155) NOT NULL AUTO_INCREMENT,
  `fraction` int(155) NOT NULL,
  `rank` int(155) NOT NULL,
  `payday` int(155) NOT NULL,
  `name` text NOT NULL,
  `clothesm` text NOT NULL,
  `clothesf` text NOT NULL,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Дамп данных таблицы iteffa.fractionranks: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `fractionranks` DISABLE KEYS */;
/*!40000 ALTER TABLE `fractionranks` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.fractions
CREATE TABLE IF NOT EXISTS `fractions` (
  `id` int(155) NOT NULL,
  `drugs` int(155) NOT NULL,
  `money` int(155) NOT NULL,
  `mats` int(155) NOT NULL,
  `medkits` int(155) NOT NULL,
  `lastserial` text NOT NULL,
  `weapons` text NOT NULL,
  `isopen` tinyint(1) NOT NULL,
  `fuellimit` int(155) NOT NULL,
  `fuelleft` int(155) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.fractions: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `fractions` DISABLE KEYS */;
/*!40000 ALTER TABLE `fractions` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.fractionvehicles
CREATE TABLE IF NOT EXISTS `fractionvehicles` (
  `fraction` int(155) NOT NULL,
  `number` text NOT NULL,
  `components` text NOT NULL,
  `model` text NOT NULL,
  `position` text NOT NULL,
  `rotation` text NOT NULL,
  `rank` int(155) NOT NULL,
  `colorprim` int(11) NOT NULL,
  `colorsec` int(11) NOT NULL,
  `idkey` int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.fractionvehicles: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `fractionvehicles` DISABLE KEYS */;
/*!40000 ALTER TABLE `fractionvehicles` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.furniture
CREATE TABLE IF NOT EXISTS `furniture` (
  `uuid` varchar(155) NOT NULL,
  `furniture` text NOT NULL,
  `data` text NOT NULL,
  PRIMARY KEY (`uuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.furniture: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `furniture` DISABLE KEYS */;
/*!40000 ALTER TABLE `furniture` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.gangspoints
CREATE TABLE IF NOT EXISTS `gangspoints` (
  `id` int(155) NOT NULL,
  `gangid` int(155) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.gangspoints: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `gangspoints` DISABLE KEYS */;
/*!40000 ALTER TABLE `gangspoints` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.garages
CREATE TABLE IF NOT EXISTS `garages` (
  `id` int(155) NOT NULL,
  `type` int(155) NOT NULL,
  `position` text NOT NULL,
  `rotation` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.garages: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `garages` DISABLE KEYS */;
/*!40000 ALTER TABLE `garages` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.houses
CREATE TABLE IF NOT EXISTS `houses` (
  `id` int(155) NOT NULL,
  `owner` text NOT NULL,
  `type` varchar(11) NOT NULL,
  `position` text NOT NULL,
  `price` text NOT NULL,
  `locked` tinyint(155) NOT NULL,
  `garage` text NOT NULL,
  `bank` text NOT NULL,
  `roommates` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.houses: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `houses` DISABLE KEYS */;
/*!40000 ALTER TABLE `houses` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.inventory
CREATE TABLE IF NOT EXISTS `inventory` (
  `items` text NOT NULL,
  `uuid` int(255) NOT NULL,
  `idkey` int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.inventory: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `inventory` DISABLE KEYS */;
/*!40000 ALTER TABLE `inventory` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.money
CREATE TABLE IF NOT EXISTS `money` (
  `id` varchar(155) NOT NULL,
  `holder` varchar(155) NOT NULL,
  `balance` varchar(155) NOT NULL,
  `type` varchar(155) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.money: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `money` DISABLE KEYS */;
/*!40000 ALTER TABLE `money` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.nicknames
CREATE TABLE IF NOT EXISTS `nicknames` (
  `srv` varchar(155) NOT NULL,
  `name` varchar(155) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.nicknames: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `nicknames` DISABLE KEYS */;
/*!40000 ALTER TABLE `nicknames` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.othervehicles
CREATE TABLE IF NOT EXISTS `othervehicles` (
  `type` varchar(155) NOT NULL,
  `number` text NOT NULL,
  `model` text NOT NULL,
  `position` text NOT NULL,
  `rotation` text NOT NULL,
  `color1` int(155) NOT NULL,
  `color2` int(155) NOT NULL,
  `price` int(155) NOT NULL,
  `idkey` int(155) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.othervehicles: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `othervehicles` DISABLE KEYS */;
/*!40000 ALTER TABLE `othervehicles` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.promocodes
CREATE TABLE IF NOT EXISTS `promocodes` (
  `name` text NOT NULL,
  `type` int(155) NOT NULL,
  `count` int(155) NOT NULL,
  `owner` int(155) NOT NULL,
  `idkey` int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.promocodes: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `promocodes` DISABLE KEYS */;
/*!40000 ALTER TABLE `promocodes` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.questions
CREATE TABLE IF NOT EXISTS `questions` (
  `ID` int(12) unsigned NOT NULL AUTO_INCREMENT,
  `Author` text NOT NULL,
  `Question` text CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `Respondent` text,
  `Response` text CHARACTER SET utf8 COLLATE utf8_bin,
  `Opened` datetime NOT NULL,
  `Closed` datetime DEFAULT NULL,
  `Status` tinyint(4) DEFAULT '0',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Дамп данных таблицы iteffa.questions: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `questions` DISABLE KEYS */;
/*!40000 ALTER TABLE `questions` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.rodings
CREATE TABLE IF NOT EXISTS `rodings` (
  `id` int(11) NOT NULL,
  `radius` varchar(255) DEFAULT NULL,
  `pos` varchar(255) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы iteffa.rodings: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `rodings` DISABLE KEYS */;
/*!40000 ALTER TABLE `rodings` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.safes
CREATE TABLE IF NOT EXISTS `safes` (
  `minamount` int(155) NOT NULL,
  `maxamount` int(155) NOT NULL,
  `pos` text NOT NULL,
  `address` text NOT NULL,
  `rotation` int(11) NOT NULL,
  `idkey` int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.safes: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `safes` DISABLE KEYS */;
/*!40000 ALTER TABLE `safes` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.vehicles
CREATE TABLE IF NOT EXISTS `vehicles` (
  `holder` varchar(155) NOT NULL,
  `model` varchar(155) NOT NULL,
  `health` int(155) NOT NULL,
  `fuel` int(155) NOT NULL,
  `components` text NOT NULL,
  `items` text NOT NULL,
  `position` varchar(255) DEFAULT '0',
  `rotation` varchar(255) DEFAULT '0',
  `keynum` int(155) NOT NULL DEFAULT '0',
  `dirt` float NOT NULL DEFAULT '0',
  `price` int(155) NOT NULL,
  `idkey` int(155) NOT NULL AUTO_INCREMENT,
  `number` varchar(155) NOT NULL,
  PRIMARY KEY (`idkey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.vehicles: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `vehicles` DISABLE KEYS */;
/*!40000 ALTER TABLE `vehicles` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.weapons
CREATE TABLE IF NOT EXISTS `weapons` (
  `id` varchar(155) NOT NULL,
  `lastserial` varchar(155) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы iteffa.weapons: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `weapons` DISABLE KEYS */;
/*!40000 ALTER TABLE `weapons` ENABLE KEYS */;

-- Дамп структуры для таблица iteffa.whitelist
CREATE TABLE IF NOT EXISTS `whitelist` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `socialclub` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Дамп данных таблицы iteffa.whitelist: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `whitelist` DISABLE KEYS */;
/*!40000 ALTER TABLE `whitelist` ENABLE KEYS */;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
