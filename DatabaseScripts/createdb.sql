DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nickname` varchar(100) NOT NULL DEFAULT '',
  `username` varchar(100) NOT NULL DEFAULT '',
  `password` varchar(100) NOT NULL DEFAULT '',
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
