-- Delete old Accepted submissions for user to allow stats update
-- Replace USER_ID with the actual user ID from the logs: 527133d3-b4ad-44a6-9e2f-e6da431a5921
-- Status = 4 is Accepted (based on query results)

USE algotournament;

-- Option 1: Delete all Accepted submissions for problem 4 (recommended)
DELETE FROM Submissions
WHERE UserId = '527133d3-b4ad-44a6-9e2f-e6da431a5921'
AND ProblemId = 4
AND Status = 4; -- 4 = Accepted

-- Option 2: Delete only old Accepted submissions, keep the most recent (ID 44)
-- DELETE FROM Submissions
-- WHERE UserId = '527133d3-b4ad-44a6-9e2f-e6da431a5921'
-- AND ProblemId = 4
-- AND Status = 4 -- 4 = Accepted
-- AND Id < 44; -- Keep the most recent submission (ID 44)

-- Option 3: Reset user stats to 0 (if you want to start fresh)
-- UPDATE Users
-- SET ProblemsSolved = 0, Rating = 1200
-- WHERE Id = '527133d3-b4ad-44a6-9e2f-e6da431a5921';

-- Note: Run only ONE of the options above, not all three
-- After deleting, submit problem 4 again to test stats update
