DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nickname` varchar(100) NOT NULL DEFAULT '',
  `username` varchar(100) NOT NULL DEFAULT '',
  `password` varchar(100) NOT NULL DEFAULT '',
  `salt` tinyblob,
  `flags` int(11) NOT NULL DEFAULT 0,
  `created` datetime NOT NULL,
  `updated` datetime NOT NULL,
  PRIMARY KEY (`id`)
);

DROP TABLE IF EXISTS `tokens`;
CREATE TABLE `tokens` (
  `userid` int(11) NOT NULL,
  `token` varchar(100) NOT NULL DEFAULT '',
  `created` datetime NOT NULL,
  PRIMARY KEY (`userid`,`token`)
);

DROP TABLE IF EXISTS `logtable`;
CREATE TABLE `logtable` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `created` datetime NOT NULL,
  `loglevel` int(11) NOT NULL,
  `groupname` varchar(100) NOT NULL DEFAULT '',
  `message` varchar(1024) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`)
);