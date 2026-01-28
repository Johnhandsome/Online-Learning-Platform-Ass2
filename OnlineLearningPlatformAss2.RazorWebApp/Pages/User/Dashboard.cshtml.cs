using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.User
{
    public class DashboardModel : PageModel
    {
        public string UserName { get; set; } = "Alex";
        public int LearningStreak { get; set; } = 5;
        public int CompletedCourses { get; set; } = 12;
        public int TotalHours { get; set; } = 48;

        public List<CourseViewModel> EnrolledCourses { get; set; } = new();
        public List<CourseViewModel> RecommendedCourses { get; set; } = new();

        public void OnGet()
        {
            // Mock Data
            UserName = "Alex";
            LearningStreak = 15;
            CompletedCourses = 4;
            TotalHours = 126;

            EnrolledCourses = new List<CourseViewModel>
            {
                new CourseViewModel
                {
                    Id = 1,
                    Title = "Advanced Molecular Biology",
                    Category = "Science",
                    Progress = 75,
                    TotalLessons = 20,
                    CompletedLessons = 15,
                    ImageUrl = "https://images.unsplash.com/photo-1542831371-29b0f74f9713?q=80&w=2070&auto=format&fit=crop"
                },
                new CourseViewModel
                {
                    Id = 2,
                    Title = "UI/UX Design Principles",
                    Category = "Design",
                    Progress = 30,
                    TotalLessons = 12,
                    CompletedLessons = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1664575602276-acd073f104c1?q=80&w=2070&auto=format&fit=crop"
                }
            };

            RecommendedCourses = new List<CourseViewModel>
            {
                 new CourseViewModel
                {
                    Id = 3,
                    Title = "Financial Analytics 101",
                    Category = "Business",
                    Rating = 4.8,
                    Students = 1200,
                    ImageUrl = "https://images.unsplash.com/photo-1554224155-8d04cb21cd6c?q=80&w=2070&auto=format&fit=crop"
                },
                new CourseViewModel
                {
                    Id = 4,
                    Title = "Web Development Bootcamp",
                    Category = "Technology",
                    Rating = 4.9,
                    Students = 3500,
                    ImageUrl = "https://images.unsplash.com/photo-1587620962725-abab7fe55159?q=80&w=2031&auto=format&fit=crop"
                },
                new CourseViewModel
                {
                    Id = 5,
                    Title = "Digital Marketing Strategy",
                    Category = "Marketing",
                    Rating = 4.7,
                    Students = 850,
                    ImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?q=80&w=2070&auto=format&fit=crop"
                }
            };
        }

        public class CourseViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public int Progress { get; set; } // 0-100
            public int TotalLessons { get; set; }
            public int CompletedLessons { get; set; }
            public double Rating { get; set; }
            public int Students { get; set; }
        }
    }
}
