-- use datamuncher;
DELIMITER //
DROP PROCEDURE IF EXISTS prc_case_add//


CREATE PROCEDURE prc_case_add (`number` VARCHAR(255), `name` VARCHAR(255), `owner` VARCHAR(255), `create_date` DATE)
 BEGIN
  INSERT INTO caseinfo(`number`, `name`, `owner`, `create_date`) VALUES(`number`, `name`,`owner`, `create_date`);
 END;
//

DELIMITER ;
