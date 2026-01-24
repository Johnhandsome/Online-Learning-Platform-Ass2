-- Seed Assessment Questions and Options with proper UTF-8 encoding
-- This script populates the assessment system with sample questions

SET NOCOUNT ON;

-- Question 1: Interest in Web Development
DECLARE @WebDevQuestionId UNIQUEIDENTIFIER = NEWID();
DECLARE @WebDevCategoryId UNIQUEIDENTIFIER;

-- Get Web Development category (adjust based on your actual categories)
SELECT TOP 1 @WebDevCategoryId = category_id FROM Categories WHERE name LIKE '%Web%' OR name LIKE '%Development%';

INSERT INTO Assessment_Questions (question_id, question_text, question_type, category_id, order_index, is_active)
VALUES (@WebDevQuestionId, N'Bạn có quan tâm đến lập trình Web không?', 'Interest', @WebDevCategoryId, 1, 1);

INSERT INTO Assessment_Options (option_id, question_id, option_text, skill_level, order_index)
VALUES 
    (NEWID(), @WebDevQuestionId, N'Rất quan tâm, tôi muốn trở thành Web Developer', NULL, 1),
    (NEWID(), @WebDevQuestionId, N'Có quan tâm, muốn tìm hiểu thêm', NULL, 2),
    (NEWID(), @WebDevQuestionId, N'Không quan tâm lắm', NULL, 3);

-- Question 2: Current Web Development Skill Level
DECLARE @WebSkillQuestionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Assessment_Questions (question_id, question_text, question_type, category_id, order_index, is_active)
VALUES (@WebSkillQuestionId, N'Trình độ lập trình Web hiện tại của bạn?', 'SkillLevel', @WebDevCategoryId, 2, 1);

INSERT INTO Assessment_Options (option_id, question_id, option_text, skill_level, order_index)
VALUES 
    (NEWID(), @WebSkillQuestionId, N'Chưa biết gì, hoàn toàn mới', 'Beginner', 1),
    (NEWID(), @WebSkillQuestionId, N'Biết cơ bản HTML/CSS', 'Beginner', 2),
    (NEWID(), @WebSkillQuestionId, N'Đã làm được một số dự án nhỏ', 'Intermediate', 3),
    (NEWID(), @WebSkillQuestionId, N'Có kinh nghiệm làm việc thực tế', 'Advanced', 4);

-- Question 3: Interest in Mobile Development
DECLARE @MobileQuestionId UNIQUEIDENTIFIER = NEWID();
DECLARE @MobileCategoryId UNIQUEIDENTIFIER;

SELECT TOP 1 @MobileCategoryId = category_id FROM Categories WHERE name LIKE '%Mobile%' OR name LIKE '%App%';

INSERT INTO Assessment_Questions (question_id, question_text, question_type, category_id, order_index, is_active)
VALUES (@MobileQuestionId, N'Bạn có muốn học phát triển ứng dụng di động không?', 'Interest', @MobileCategoryId, 3, 1);

INSERT INTO Assessment_Options (option_id, question_id, option_text, skill_level, order_index)
VALUES 
    (NEWID(), @MobileQuestionId, N'Có, tôi muốn phát triển ứng dụng di động', NULL, 1),
    (NEWID(), @MobileQuestionId, N'Có thể, nếu có cơ hội', NULL, 2),
    (NEWID(), @MobileQuestionId, N'Không, tôi không quan tâm', NULL, 3);

-- Question 4: Programming Experience
DECLARE @ExpQuestionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Assessment_Questions (question_id, question_text, question_type, category_id, order_index, is_active)
VALUES (@ExpQuestionId, N'Kinh nghiệm lập trình tổng quát của bạn?', 'SkillLevel', NULL, 4, 1);

INSERT INTO Assessment_Options (option_id, question_id, option_text, skill_level, order_index)
VALUES 
    (NEWID(), @ExpQuestionId, N'Chưa từng lập trình', 'Beginner', 1),
    (NEWID(), @ExpQuestionId, N'Dưới 1 năm', 'Beginner', 2),
    (NEWID(), @ExpQuestionId, N'1-3 năm', 'Intermediate', 3),
    (NEWID(), @ExpQuestionId, N'Trên 3 năm', 'Advanced', 4);

-- Question 5: Learning Goal
DECLARE @GoalQuestionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Assessment_Questions (question_id, question_text, question_type, category_id, order_index, is_active)
VALUES (@GoalQuestionId, N'Mục tiêu học tập của bạn là gì?', 'MultipleChoice', NULL, 5, 1);

INSERT INTO Assessment_Options (option_id, question_id, option_text, skill_level, order_index)
VALUES 
    (NEWID(), @GoalQuestionId, N'Tìm việc làm trong ngành IT', NULL, 1),
    (NEWID(), @GoalQuestionId, N'Nâng cao kỹ năng hiện tại', NULL, 2),
    (NEWID(), @GoalQuestionId, N'Học để làm dự án cá nhân', NULL, 3),
    (NEWID(), @GoalQuestionId, N'Chỉ muốn tìm hiểu', NULL, 4);

-- Question 6: Interest in Data Science
DECLARE @DataQuestionId UNIQUEIDENTIFIER = NEWID();
DECLARE @DataCategoryId UNIQUEIDENTIFIER;

SELECT TOP 1 @DataCategoryId = category_id FROM Categories WHERE name LIKE '%Data%' OR name LIKE '%AI%' OR name LIKE '%Machine%';

INSERT INTO Assessment_Questions (question_id, question_text, question_type, category_id, order_index, is_active)
VALUES (@DataQuestionId, N'Bạn có quan tâm đến Data Science/AI không?', 'Interest', @DataCategoryId, 6, 1);

INSERT INTO Assessment_Options (option_id, question_id, option_text, skill_level, order_index)
VALUES 
    (NEWID(), @DataQuestionId, N'Rất quan tâm, muốn theo đuổi nghiệp Data Science', NULL, 1),
    (NEWID(), @DataQuestionId, N'Có quan tâm nhưng chưa chắc chắn', NULL, 2),
    (NEWID(), @DataQuestionId, N'Không quan tâm', NULL, 3);

-- Question 7: Time Commitment
DECLARE @TimeQuestionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Assessment_Questions (question_id, question_text, question_type, category_id, order_index, is_active)
VALUES (@TimeQuestionId, N'Bạn có thể dành bao nhiêu thời gian mỗi tuần để học?', 'MultipleChoice', NULL, 7, 1);

INSERT INTO Assessment_Options (option_id, question_id, option_text, skill_level, order_index)
VALUES 
    (NEWID(), @TimeQuestionId, N'Dưới 5 giờ/tuần', NULL, 1),
    (NEWID(), @TimeQuestionId, N'5-10 giờ/tuần', NULL, 2),
    (NEWID(), @TimeQuestionId, N'10-20 giờ/tuần', NULL, 3),
    (NEWID(), @TimeQuestionId, N'Trên 20 giờ/tuần (học full-time)', NULL, 4);

PRINT N'✓ Assessment questions and options seeded successfully!';
PRINT N'✓ Total: 7 questions, 25 options';
