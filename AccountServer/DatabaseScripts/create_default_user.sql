-- Adminer 4.8.1 MySQL 5.5.5-10.9.5-MariaDB-1:10.9.5+maria~ubu2204 dump

SET NAMES utf8;
SET time_zone = '+00:00';
SET foreign_key_checks = 0;
SET sql_mode = 'NO_AUTO_VALUE_ON_ZERO';

USE `account`;

SET NAMES utf8mb4;

DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nickname` varchar(100) NOT NULL DEFAULT '',
  `username` varchar(100) NOT NULL DEFAULT '',
  `password` varchar(100) NOT NULL DEFAULT '',
  `email` varchar(100) NOT NULL DEFAULT '',
  `salt` tinyblob DEFAULT NULL,
  `flags` int(11) NOT NULL DEFAULT 0,
  `created` datetime NOT NULL,
  `updated` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `users` (`id`, `nickname`, `username`, `password`, `email`, `salt`, `flags`, `created`, `updated`) VALUES
(1,	'DefaultUser',	'dummyuser',	'I3AVLRCsVbg2hpQGiZrp7mJFjTjHe60hlm0+J4dbHpo=',	'defaultuser@gmail.com',	'÷¡?˙ëÃJnz<Ë¬ñ°˝',	0,	'2023-04-11 14:22:23',	'2023-04-11 14:22:23');

-- 2024-05-12 14:23:46
