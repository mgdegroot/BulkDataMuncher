-- use datamuncher;
DELIMITER //
DROP PROCEDURE IF EXISTS prc_case_get//


CREATE PROCEDURE prc_case_get (`number` VARCHAR(255))
 BEGIN
  IF `number` = '0' THEN
   SELECT caseinfo.`number`, `name`, `owner`, `create_date`, `last_modify_date` FROM caseinfo;
  ELSE 
   SELECT caseinfo.`number`, `name`, `owner`, `create_date`, `last_modify_date` FROM caseinfo WHERE caseinfo.`number` = `number`;
  END IF;
  
 END;
//

DELIMITER ;
