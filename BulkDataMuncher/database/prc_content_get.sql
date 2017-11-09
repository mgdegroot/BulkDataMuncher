-- use datamuncher;
DELIMITER //
DROP PROCEDURE IF EXISTS prc_content_get//


CREATE PROCEDURE prc_content_get (`case_number` VARCHAR(255))
 BEGIN
  SELECT casecontent.`case_number`, `path`, `filetype`, `archive_date` FROM casecontent WHERE casecontent.`case_number` = `case_number`;
  
 END;
//

DELIMITER ;
