-- use datamuncher;
DELIMITER //
DROP PROCEDURE IF EXISTS prc_content_add//


CREATE PROCEDURE prc_content_add (`case_number` VARCHAR(255), `path` VARCHAR(1024), `filetype` VARCHAR(8), `archive_date` DATETIME)
 BEGIN
  INSERT INTO casecontent(`case_number`, `path`, `filetype`, `archive_date`) VALUES(`case_number`, `path`, `filetype`, `archive_date`);
 END;
//

DELIMITER ;

