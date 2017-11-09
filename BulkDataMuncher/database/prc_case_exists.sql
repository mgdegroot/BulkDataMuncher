-- use datamuncher;
DELIMITER //
DROP PROCEDURE IF EXISTS prc_case_exists//


CREATE PROCEDURE prc_case_exists (`number` VARCHAR(45))
 BEGIN
  SELECT CASE WHEN EXISTS(SELECT * FROM caseinfo WHERE caseinfo.`number`=`number`) THEN TRUE ELSE FALSE END AS case_exists FROM caseinfo LIMIT 1;
 END;
//

DELIMITER ;