/*
a. "numCheckins" value for a business should be updated to the count of all check-in counts for that business. */
UPDATE Business_table
SET numCheckins = (Select Count(*) from Checkins_table 
WHERE Business_table.business_id = Checkins_table.business_id);

/*Similarly, "numTips" should be updated to the number of tips provided for that business.
You should query the "Checkins" and "Tips" tables to calculate these values.*/
UPDATE Business_table
SET numTips = T.mycount
From (Select business_id, Count(*) as mycount from Tip_table Group By business_id) as T
WHERE Business_table.business_id = T.business_id;


/*"totalLikes" value for a user should be updated to the sum of all likes for the user’s tips.*/

UPDATE Users_table
SET totalLikes = mysum
from (SELECT user_id, sum(likes) as mysum FROM Tip_table group by user_id) as T
where T.user_id = Users_table.user_id;


/* And
"tipCount" should be updated to the number of tips that the user provided for various businesses.
You should query the "Tips" table to calculate these values. */
UPDATE Users_table
SET tipCount = mycount
FROM (SELECT user_id, COUNT(*) as mycount FROM Tip_table group by user_id ) as T
WHERE Users_table.user_id = T.user_id;