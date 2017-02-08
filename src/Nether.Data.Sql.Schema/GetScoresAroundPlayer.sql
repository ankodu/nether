﻿/* Copyright (c) Microsoft. All rights reserved.
Licensed under the MIT license. See LICENSE file in the project root for full license information. */

CREATE PROCEDURE [dbo].GetScoresAroundPlayer
	@Gamertag NVARCHAR(50),
	@Radius INT
AS
DECLARE @PlayerRank int

EXEC GetPlayerRank @Gamertag, @PlayerRank OUTPUT

IF (@PlayerRank >= 0)
BEGIN
	SELECT 
		* 
	FROM (
		SELECT 
			Gamertag, 
			MAX(Score) AS Score, 
			MAX(CustomTag) AS CustomTag, 
			ROW_NUMBER() OVER (ORDER BY MAX(Score) DESC) AS Ranking
		FROM Scores 
		GROUP BY GamerTag
	) AS T 
	WHERE Ranking BETWEEN (@PlayerRank-@Radius) AND (@PlayerRank + @Radius)
END
RETURN 0
