--
-- Create user and privileges
--
drop user 'accuser'@'%';
flush privileges;
CREATE USER 'accuser'@'%' IDENTIFIED BY 'unity';
GRANT ALL PRIVILEGES ON * . * TO 'accuser'@'%' IDENTIFIED BY 'unity';
GRANT CREATE ON *.* TO 'accuser'@'%' IDENTIFIED BY 'unity';
commit;
flush privileges;
