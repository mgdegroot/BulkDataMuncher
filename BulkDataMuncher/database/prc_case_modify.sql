-- use datamuncher;
DELIMITER //
DROP PROCEDURE IF EXISTS prc_case_modify//


CREATE PROCEDURE prc_case_modify (`number` VARCHAR(255), `name` VARCHAR(255), `owner` VARCHAR(255), `create_date` DATE)
 BEGIN
  UPDATE caseinfo SET `name` = `name`, `owner`=`owner`, `create_date`=`create_date` WHERE caseinfo.`number` = `number`;
 END;
//

DELIMITER ;
