DROP TRIGGER IF EXISTS updateUserTipCount ON Tip_table;
DROP TRIGGER IF EXISTS updateBusinessTipCount ON Tip_table;
DROP TRIGGER IF EXISTS updateBusinessCheckins ON checkins_table;
DROP TRIGGER IF EXISTS updateUserLikes ON Tip_table;

/*a. Whenever a user provides a tip for a business, the "numTips" value for that business and the
"tipCount" value for the user should be updated.*/


CREATE OR REPLACE Function updateNumTipsUser() RETURNS trigger as '
BEGIN
    UPDATE users_table
        SET tipCount = tipCount + 1
        WHERE user_id = NEW.user_id;
    RETURN NEW;
END
'LANGUAGE plpgsql; 

CREATE TRIGGER  updateUserTipCount
AFTER INSERT ON Tip_table
FOR EACH ROW
EXECUTE PROCEDURE updateNumTipsUser();

CREATE OR REPLACE Function updateNumTipsBusiness() RETURNS trigger as '
BEGIN
    UPDATE Business_table
        SET numTips = numTips + 1
        WHERE business_id = NEW.business_id;
    RETURN NEW;
END
'LANGUAGE plpgsql; 

CREATE TRIGGER  updateBusinessTipCount
AFTER INSERT ON Tip_table
FOR EACH ROW
EXECUTE PROCEDURE updateNumTipsBusiness();

/*b. Similarly, when a customer checks-in a business, the "numCheckins" attribute value for that
business should be updated.*/

CREATE OR REPLACE Function updateNumCheckinsBusiness() RETURNS trigger as '
BEGIN
    UPDATE Business_table
        SET numCheckins = numCheckins + 1
        WHERE business_id = NEW.business_id;
    RETURN NEW;
END
'LANGUAGE plpgsql; 

CREATE TRIGGER  updateBusinessCheckins
AFTER INSERT ON Checkins_table
FOR EACH ROW
EXECUTE PROCEDURE updateNumCheckinsBusiness();

/*c. When a user likes a tip, the "totalLikes" attribute value for the user who wrote that tip should be
updated. */

CREATE OR REPLACE Function updateUserLikeCount() RETURNS trigger as '
BEGIN
    UPDATE Users_table
        SET totalLikes = totalLikes + 1
        WHERE user_id = NEW.user_id;
    RETURN NEW;
END
'LANGUAGE plpgsql;

CREATE TRIGGER updateUserLikes
AFTER UPDATE OF likes ON Tip_table
FOR EACH ROW
EXECUTE PROCEDURE updateUserLikeCount();