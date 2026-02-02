using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatformAss2.Service.Services;

public class DatabaseSeedService
{
    private readonly OnlineLearningContext _context;

    public DatabaseSeedService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        
        // Seed Roles
        await SeedRolesAsync();
        
        // Seed Categories
        await SeedCategoriesAsync();

        // Seed Assessment Questions
        await SeedAssessmentQuestionsAsync();

        // Seed Sample Courses
        await SeedSampleCoursesAsync();
    }

    private async Task SeedRolesAsync()
    {
        if (!await _context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Description = "Administrator with full system access",
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Instructor",
                    Description = "Course instructor who can create and manage courses",
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                    Description = "Regular learner user",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (!await _context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Web Development",
                    Description = "Learn web development technologies including HTML, CSS, JavaScript, and frameworks",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Data Science",
                    Description = "Master data analysis, machine learning, and statistical modeling",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Design",
                    Description = "UI/UX design, graphic design, and creative skills",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Business",
                    Description = "Business strategy, management, and entrepreneurship",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Marketing",
                    Description = "Digital marketing, SEO, social media, and advertising",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAssessmentQuestionsAsync()
    {
        if (!await _context.AssessmentQuestions.AnyAsync())
        {
            var questions = new[]
            {
                new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "What is your current experience level with programming?",
                    QuestionType = "SingleChoice",
                    OrderIndex = 1,
                    IsActive = true
                },
                new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "Which area interests you most?",
                    QuestionType = "SingleChoice",
                    OrderIndex = 2,
                    IsActive = true
                },
                new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "What is your primary goal for learning?",
                    QuestionType = "SingleChoice",
                    OrderIndex = 3,
                    IsActive = true
                },
                new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "How much time can you dedicate to learning per week?",
                    QuestionType = "SingleChoice",
                    OrderIndex = 4,
                    IsActive = true
                },
                new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "Which learning style works best for you?",
                    QuestionType = "SingleChoice",
                    OrderIndex = 5,
                    IsActive = true
                }
            };

            await _context.AssessmentQuestions.AddRangeAsync(questions);
            await _context.SaveChangesAsync();

            // Add options for each question
            await SeedAssessmentOptionsAsync(questions);
        }
    }

    private async Task SeedAssessmentOptionsAsync(AssessmentQuestion[] questions)
    {
        var options = new List<AssessmentOption>();

        // Question 1: Programming experience
        var q1 = questions[0];
        options.AddRange(new[]
        {
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q1.Id,
                OptionText = "Complete beginner - I've never coded before",
                SkillLevel = "Beginner",
                Category = "Programming"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q1.Id,
                OptionText = "Some experience - I've done basic tutorials",
                SkillLevel = "Beginner",
                Category = "Programming"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q1.Id,
                OptionText = "Intermediate - I can build simple applications",
                SkillLevel = "Intermediate",
                Category = "Programming"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q1.Id,
                OptionText = "Advanced - I'm comfortable with complex projects",
                SkillLevel = "Advanced",
                Category = "Programming"
            }
        });

        // Question 2: Interest area
        var q2 = questions[1];
        options.AddRange(new[]
        {
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q2.Id,
                OptionText = "Web Development - Building websites and web applications",
                SkillLevel = "None",
                Category = "Web Development"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q2.Id,
                OptionText = "Data Science - Analyzing data and machine learning",
                SkillLevel = "None",
                Category = "Data Science"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q2.Id,
                OptionText = "Design - UI/UX and graphic design",
                SkillLevel = "None",
                Category = "Design"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q2.Id,
                OptionText = "Business - Management and entrepreneurship",
                SkillLevel = "None",
                Category = "Business"
            }
        });

        // Question 3: Learning goal
        var q3 = questions[2];
        options.AddRange(new[]
        {
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q3.Id,
                OptionText = "Career change - I want to switch to a new field",
                SkillLevel = "None",
                Category = "Career Development"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q3.Id,
                OptionText = "Skill improvement - I want to advance in my current role",
                SkillLevel = "None",
                Category = "Professional Growth"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q3.Id,
                OptionText = "Personal interest - Learning for fun and curiosity",
                SkillLevel = "None",
                Category = "Personal Development"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q3.Id,
                OptionText = "Academic requirements - For school or certification",
                SkillLevel = "None",
                Category = "Academic"
            }
        });

        // Question 4: Time commitment
        var q4 = questions[3];
        options.AddRange(new[]
        {
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q4.Id,
                OptionText = "1-3 hours - I have limited time but want to learn",
                SkillLevel = "None",
                Category = "Time Management"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q4.Id,
                OptionText = "4-7 hours - I can dedicate regular time to learning",
                SkillLevel = "None",
                Category = "Time Management"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q4.Id,
                OptionText = "8-15 hours - Learning is a high priority for me",
                SkillLevel = "None",
                Category = "Time Management"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q4.Id,
                OptionText = "15+ hours - I'm fully committed to intensive learning",
                SkillLevel = "None",
                Category = "Time Management"
            }
        });

        // Question 5: Learning style
        var q5 = questions[4];
        options.AddRange(new[]
        {
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q5.Id,
                OptionText = "Video tutorials - I learn best by watching demonstrations",
                SkillLevel = "None",
                Category = "Learning Style"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q5.Id,
                OptionText = "Hands-on projects - I prefer learning by doing",
                SkillLevel = "None",
                Category = "Learning Style"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q5.Id,
                OptionText = "Reading and articles - I like detailed written explanations",
                SkillLevel = "None",
                Category = "Learning Style"
            },
            new AssessmentOption
            {
                Id = Guid.NewGuid(),
                QuestionId = q5.Id,
                OptionText = "Interactive exercises - I enjoy quizzes and practice problems",
                SkillLevel = "None",
                Category = "Learning Style"
            }
        });

        await _context.AssessmentOptions.AddRangeAsync(options);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSampleCoursesAsync()
    {
        if (!await _context.Courses.AnyAsync())
        {
            // Get categories and instructor role
            var webDevCategory = await _context.Categories.FirstAsync(c => c.Name == "Web Development");
            var dataCategory = await _context.Categories.FirstAsync(c => c.Name == "Data Science");
            var designCategory = await _context.Categories.FirstAsync(c => c.Name == "Design");
            var businessCategory = await _context.Categories.FirstAsync(c => c.Name == "Business");

            // Create a default instructor user
            var instructorRole = await _context.Roles.FirstAsync(r => r.Name == "Instructor");
            var instructor = new User
            {
                Id = Guid.NewGuid(),
                Username = "instructor",
                Email = "instructor@learnhub.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreateAt = DateTime.UtcNow,
                RoleId = instructorRole.Id
            };
            _context.Users.Add(instructor);
            await _context.SaveChangesAsync();

            var courses = new[]
            {
                new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "Complete Web Development Bootcamp",
                    Description = "Learn HTML, CSS, JavaScript, React, Node.js and build real-world projects",
                    Price = 49.99m,
                    CategoryId = webDevCategory.Id,
                    InstructorId = instructor.Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=400&h=225&fit=crop"
                },
                new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "Data Science with Python",
                    Description = "Master data analysis, visualization, and machine learning with Python",
                    Price = 59.99m,
                    CategoryId = dataCategory.Id,
                    InstructorId = instructor.Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400&h=225&fit=crop"
                },
                new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "UI/UX Design Fundamentals",
                    Description = "Learn user interface and user experience design principles",
                    Price = 39.99m,
                    CategoryId = designCategory.Id,
                    InstructorId = instructor.Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=400&h=225&fit=crop"
                },
                new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "Digital Marketing Mastery",
                    Description = "Complete guide to digital marketing, SEO, and social media",
                    Price = 44.99m,
                    CategoryId = businessCategory.Id,
                    InstructorId = instructor.Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=400&h=225&fit=crop"
                },
                new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "Advanced CSS Layouts",
                    Description = "Master Grid, Flexbox, and modern CSS layout techniques",
                    Price = 29.99m,
                    CategoryId = webDevCategory.Id,
                    InstructorId = instructor.Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1627398242454-45a1465c2479?w=400&h=225&fit=crop"
                },
                new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "Machine Learning Fundamentals",
                    Description = "Introduction to machine learning algorithms and applications",
                    Price = 69.99m,
                    CategoryId = dataCategory.Id,
                    InstructorId = instructor.Id,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=400&h=225&fit=crop"
                }
            };

            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();
        }
    }
}
