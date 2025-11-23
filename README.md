# School Management System - Backend API

A comprehensive RESTful API built with .NET Core 8 Web API for managing school operations including students, teachers, courses, classes, attendance, and grading with role-based access control.

---

## 🎯 Project Overview

This system provides complete backend functionality for managing educational institutions with three main user roles:
- **Admin**: Manages departments, courses, and users
- **Teacher**: Manages classes, attendance, assignments, and grading
- **Student**: Views classes, submits assignments, and tracks grades

---

## 🏗️ Project Structure

```
SchoolManagementSystem/
├── SchoolManagementSystem.API/              # Presentation Layer
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── Admin/
│   │   │   ├── DepartmentsController.cs
│   │   │   ├── CoursesController.cs
│   │   │   └── UsersController.cs
│   │   ├── Teacher/
│   │   │   ├── ClassesController.cs
│   │   │   ├── AttendanceController.cs
│   │   │   └── AssignmentsController.cs
│   │   └── Student/
│   │       └── StudentController.cs
│   ├── Middleware/
│   │   └── GlobalExceptionHandlerMiddleware.cs
│   ├── Converters/
│   │   └── DateTimeConverter.cs
│   ├── wwwroot/
│   │   └── uploads/
│   │       └── assignments/
│   ├── Program.cs
│   └── appsettings.json
├── SchoolManagementSystem.Core/             # Domain Layer
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Department.cs
│   │   ├── Course.cs
│   │   ├── Class.cs
│   │   ├── StudentClass.cs
│   │   ├── Attendance.cs
│   │   ├── Assignment.cs
│   │   ├── Submission.cs
│   │   ├── RefreshToken.cs
│   │   └── Notification.cs
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Department/
│   │   ├── Course/
│   │   ├── Class/
│   │   ├── Attendance/
│   │   ├── Assignment/
│   │   ├── Student/
│   │   └── Common/
│   ├── Interfaces/
│   └── Settings/
└── SchoolManagementSystem.Infrastructure/   # Data Layer
    ├── Data/
    │   └── AppDbContext.cs
    ├── Services/
    │   ├── PasswordService.cs
    │   ├── JwtService.cs
    │   ├── AuthService.cs
    │   ├── DepartmentService.cs
    │   ├── CourseService.cs
    │   ├── UserService.cs
    │   ├── ClassService.cs
    │   ├── AttendanceService.cs
    │   ├── AssignmentService.cs
    │   ├── StudentService.cs
    │   └── FileUploadService.cs
    └── Migrations/
```

---

## 🛠️ Tech Stack

### Backend Framework
- **.NET Core 9.0** - Web API
- **C# 12**

### Database
- **SQL Server** - Primary database
- **Entity Framework Core 9.0** - ORM

### Authentication & Security
- **JWT (JSON Web Token)** - Authentication with Access & Refresh tokens
- **BCrypt.Net-Next** - Password hashing (work factor 11)
- **Role-based Authorization** - Admin, Teacher, Student roles

### Additional Libraries
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT middleware
- **Microsoft.EntityFrameworkCore.SqlServer** - SQL Server provider
- **Microsoft.EntityFrameworkCore.Tools** - Migration tools

---

## 📦 Database Schema

### Entities Implemented

#### 1. User
- Id, Name, Email, Password (hashed), Role (Admin/Teacher/Student)
- CreatedDate, UpdatedDate, IsActive (soft delete)
- **Relationships**: One-to-Many with Classes, Departments, Attendances, Assignments, Submissions

#### 2. RefreshToken
- Id, Token, UserId, ExpiresAt, CreatedAt, IsRevoked, RevokedAt
- **Purpose**: JWT refresh token rotation for enhanced security

#### 3. Department
- Id, Name (unique), Description, HeadOfDepartmentId (Teacher)
- CreatedDate, UpdatedDate, IsActive
- **Relationships**: One-to-Many with Courses

#### 4. Course
- Id, Name, Code (unique per department), Description, DepartmentId, Credits
- CreatedDate, UpdatedDate, IsActive
- **Relationships**: Belongs to Department, One-to-Many with Classes

#### 5. Class (Batch)
- Id, Name, CourseId, TeacherId, Semester, StartDate, EndDate, IsActive
- CreatedDate, UpdatedDate
- **Relationships**: Belongs to Course and Teacher, Many-to-Many with Students

#### 6. StudentClass (Junction Table)
- Id, StudentId, ClassId, EnrollmentDate
- **Purpose**: Many-to-Many relationship between Students and Classes

#### 7. Attendance
- Id, ClassId, StudentId, Date, Status (Present/Absent/Late), MarkedByTeacherId
- CreatedDate
- **Relationships**: Belongs to Class, Student, and MarkedByTeacher

#### 8. Assignment
- Id, ClassId, Title, Description, DueDate, CreatedByTeacherId, CreatedDate
- **Relationships**: Belongs to Class, One-to-Many with Submissions

#### 9. Submission
- Id, AssignmentId, StudentId, SubmittedDate, FileUrl, Grade, GradedByTeacherId, Remarks
- **Relationships**: Belongs to Assignment and Student

#### 10. Notification
- Id, Title, Message, RecipientRole, RecipientId, CreatedDate, IsRead
- **Note**: Entity created but endpoints not yet implemented

---

## 🚀 Setup Instructions

### Prerequisites
- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **SQL Server** (LocalDB, Express, or Full version)
- **Visual Studio 2022** / **VS Code** / **Rider**

### Step 1: Clone Repository
```bash
git clone <your-repository-url>
cd SchoolManagementSystem
```

### Step 2: Update Connection String
Open `SchoolManagementSystem.API/appsettings.json` and configure your SQL Server connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SchoolManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Step 2: Restore NuGet Packages
```bash
dotnet restore
```

### Step 4: Create Database & Run Migrations

**Using Package Manager Console (Visual Studio):**
```powershell
Update-Database
```

**Using .NET CLI:**
```bash
dotnet ef database update --project SchoolManagementSystem.Infrastructure --startup-project SchoolManagementSystem.API
```

### Step 5: Create Upload Directory
```bash
# Windows (PowerShell)
New-Item -ItemType Directory -Path "SchoolManagementSystem.API\wwwroot\uploads\assignments" -Force

# Windows (CMD)
mkdir SchoolManagementSystem.API\wwwroot\uploads\assignments

# Linux/Mac
mkdir -p SchoolManagementSystem.API/wwwroot/uploads/assignments
```

### Step 6: Run Application
```bash
cd SchoolManagementSystem.API
dotnet run
```

Or press **F5** in Visual Studio.

The API will be available at:
- **HTTPS**: `https://localhost:7241`
- **HTTP**: `http://localhost:5107`
- **Swagger UI**: `https://localhost:/swagger/index.js` (root)

---

## 🗄️ Database Migration Commands

### Create New Migration
```bash
# Package Manager Console
Add-Migration MigrationName

# .NET CLI
dotnet ef migrations add MigrationName --project SchoolManagementSystem.Infrastructure --startup-project SchoolManagementSystem.API
```

### Apply Migrations
```bash
# Package Manager Console
Update-Database

# .NET CLI
dotnet ef database update --project SchoolManagementSystem.Infrastructure --startup-project SchoolManagementSystem.API
```

### Remove Last Migration
```bash
# Package Manager Console
Remove-Migration

# .NET CLI
dotnet ef migrations remove --project SchoolManagementSystem.Infrastructure --startup-project SchoolManagementSystem.API
```

### Generate SQL Script
```bash
# Package Manager Console
Script-Migration

# .NET CLI
dotnet ef migrations script --project SchoolManagementSystem.Infrastructure --startup-project SchoolManagementSystem.API
```

---

## 📚 API Documentation (Swagger)

Navigate to the root URL to access Swagger UI:
```
https://localhost:7xxx/
```

Swagger provides:
- Interactive API documentation
- Request/response examples
- Built-in testing interface
- JWT Bearer authentication support

---

## 🔐 Authentication Flow

### 1. Register New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@school.com",
  "password": "Password@123",
  "role": 3
}
```

**Role Values:**
- `1` = Admin
- `2` = Teacher
- `3` = Student

**Password Requirements:**
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character (@$!%*?&)

### 2. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@school.com",
  "password": "Password@123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": 1,
    "name": "John Doe",
    "email": "john@school.com",
    "role": "Student",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "Xy8kL2pQ9vZ...",
    "expiresAt": "2025-11-16 15:30"
  }
}
```

### 3. Using Access Token
Add to all authenticated requests:
```
Authorization: Bearer <accessToken>
```

### 4. Refresh Access Token
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "Xy8kL2pQ9vZ..."
}
```

### 5. Logout (Revoke Token)
```http
POST /api/auth/revoke-token
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "refreshToken": "Xy8kL2pQ9vZ..."
}
```

---

## 📋 Sample API Requests

### Authentication Endpoints

#### Get Current User Info
```http
GET /api/auth/me
Authorization: Bearer <accessToken>
```

---

### Admin Endpoints

#### 1. Department Management

**Create Department:**
```http
POST /api/admin/departments
Authorization: Bearer <admin_token>

{
  "name": "Computer Science",
  "description": "Department of Computer Science and Engineering",
  "headOfDepartmentId": 2
}
```

**Get All Departments (with pagination):**
```http
GET /api/admin/departments?pageNumber=1&pageSize=10&searchTerm=computer
Authorization: Bearer <admin_token>
```

**Get Department by ID:**
```http
GET /api/admin/departments/1
Authorization: Bearer <admin_token>
```

**Update Department:**
```http
PUT /api/admin/departments/1
Authorization: Bearer <admin_token>

{
  "name": "Computer Science & Engineering",
  "description": "Updated description",
  "headOfDepartmentId": 2
}
```

**Delete Department (Soft Delete):**
```http
DELETE /api/admin/departments/1
Authorization: Bearer <admin_token>
```

#### 2. Course Management

**Create Course:**
```http
POST /api/admin/courses
Authorization: Bearer <admin_token>

{
  "name": "Introduction to Programming",
  "code": "CS101",
  "description": "Basic programming concepts",
  "departmentId": 1,
  "credits": 3
}
```

**Get All Courses (with filters):**
```http
GET /api/admin/courses?pageNumber=1&pageSize=10&departmentId=1&minCredits=3&maxCredits=4
Authorization: Bearer <admin_token>
```

#### 3. User Management

**Get All Users:**
```http
GET /api/admin/users?pageNumber=1&pageSize=10&role=3&isActive=true&searchTerm=john
Authorization: Bearer <admin_token>
```

**Get User Statistics:**
```http
GET /api/admin/users/statistics
Authorization: Bearer <admin_token>
```

**Update User:**
```http
PUT /api/admin/users/5
Authorization: Bearer <admin_token>

{
  "name": "John Updated",
  "email": "john.updated@school.com",
  "role": 3,
  "newPassword": "NewPassword@123"
}
```

**Delete User (Soft Delete):**
```http
DELETE /api/admin/users/5
Authorization: Bearer <admin_token>
```

---

### Teacher Endpoints

#### 1. Class Management

**Create Class:**
```http
POST /api/teacher/classes
Authorization: Bearer <teacher_token>

{
  "name": "CS101 - Fall 2025 - Section A",
  "courseId": 1,
  "semester": "Fall 2025",
  "startDate": "2025-09-01",
  "endDate": "2025-12-15"
}
```

**Get My Classes:**
```http
GET /api/teacher/classes?pageNumber=1&pageSize=10&searchTerm=CS101
Authorization: Bearer <teacher_token>
```

**Enroll Student:**
```http
POST /api/teacher/classes/1/enroll-student
Authorization: Bearer <teacher_token>

{
  "studentId": 5
}
```

**Get Class Students:**
```http
GET /api/teacher/classes/1/students
Authorization: Bearer <teacher_token>
```

**Update Class:**
```http
PUT /api/teacher/classes/1
Authorization: Bearer <teacher_token>

{
  "name": "CS101 - Fall 2025 - Section A (Updated)",
  "semester": "Fall 2025",
  "startDate": "2025-09-01",
  "endDate": "2025-12-20"
}
```

**Deactivate Class:**
```http
PUT /api/teacher/classes/1/deactivate
Authorization: Bearer <teacher_token>
```

#### 2. Attendance Management

**Mark Single Attendance:**
```http
POST /api/teacher/attendance
Authorization: Bearer <teacher_token>

{
  "classId": 1,
  "studentId": 5,
  "date": "2025-11-16",
  "status": 1
}
```

**Status Values:**
- `1` = Present
- `2` = Absent
- `3` = Late

**Bulk Mark Attendance:**
```http
POST /api/teacher/attendance/bulk
Authorization: Bearer <teacher_token>

{
  "classId": 1,
  "date": "2025-11-16",
  "attendances": [
    { "studentId": 5, "status": 1 },
    { "studentId": 6, "status": 2 },
    { "studentId": 7, "status": 3 }
  ]
}
```

**Get Attendance History:**
```http
GET /api/teacher/attendance/1?studentId=5&fromDate=2025-11-01&toDate=2025-11-30&status=1&pageNumber=1&pageSize=10
Authorization: Bearer <teacher_token>
```

**Get Attendance Summary:**
```http
GET /api/teacher/attendance/1/summary
Authorization: Bearer <teacher_token>
```

**Get Student Attendance:**
```http
GET /api/teacher/attendance/1/student/5
Authorization: Bearer <teacher_token>
```

#### 3. Assignment Management

**Create Assignment:**
```http
POST /api/teacher/assignments
Authorization: Bearer <teacher_token>

{
  "classId": 1,
  "title": "Week 1 - Introduction Assignment",
  "description": "Complete the programming exercises",
  "dueDate": "2025-11-30"
}
```

**Get Class Assignments:**
```http
GET /api/teacher/assignments/class/1?pageNumber=1&pageSize=10
Authorization: Bearer <teacher_token>
```

**Get Assignment Submissions:**
```http
GET /api/teacher/assignments/1/submissions
Authorization: Bearer <teacher_token>
```

**Grade Submission:**
```http
POST /api/teacher/assignments/submissions/1/grade
Authorization: Bearer <teacher_token>

{
  "grade": 85.5,
  "remarks": "Good work! Needs improvement in code organization."
}
```

**Update Assignment:**
```http
PUT /api/teacher/assignments/1
Authorization: Bearer <teacher_token>

{
  "title": "Week 1 - Updated Assignment",
  "description": "Updated description",
  "dueDate": "2025-12-05"
}
```

**Delete Assignment:**
```http
DELETE /api/teacher/assignments/1
Authorization: Bearer <teacher_token>
```

---

### Student Endpoints

#### Get Dashboard
```http
GET /api/student/dashboard
Authorization: Bearer <student_token>
```

**Response includes:**
- Total classes & active classes
- Total assignments, pending, submitted, graded
- Average grade
- Attendance percentage

#### Get Enrolled Classes
```http
GET /api/student/classes
Authorization: Bearer <student_token>
```

#### Get Attendance
```http
GET /api/student/attendance?classId=1
Authorization: Bearer <student_token>
```

#### Get Grades
```http
GET /api/student/grades
Authorization: Bearer <student_token>
```

#### Get Assignments
```http
GET /api/student/assignments?classId=1&onlyPending=true
Authorization: Bearer <student_token>
```

#### Submit Assignment (with File URL)
```http
POST /api/student/assignments/1/submit
Authorization: Bearer <student_token>

{
  "fileUrl": "/uploads/assignments/document.pdf"
}
```

#### Submit Assignment (with File Upload)
```http
POST /api/student/assignments/1/submit-file
Authorization: Bearer <student_token>
Content-Type: multipart/form-data

file: [Select File]
```

**Allowed File Types:** .pdf, .doc, .docx, .txt, .zip, .rar
**Max File Size:** 10 MB

#### Download Submission
```http
GET /api/student/submissions/1/download
Authorization: Bearer <student_token>
```

---

## 🎯 Implemented Features

### ✅ Authentication & Authorization
- [x] JWT-based authentication with access tokens
- [x] Refresh token rotation for security
- [x] BCrypt password hashing (work factor 11)
- [x] Role-based authorization (Admin, Teacher, Student)
- [x] Register, Login, Refresh Token, Revoke Token endpoints
- [x] Password validation (strength requirements)
- [x] Email uniqueness validation

### ✅ Admin Features
- [x] Department CRUD operations
  - [x] Unique department name validation
  - [x] Head of Department assignment (must be Teacher)
  - [x] Soft delete with active courses check
- [x] Course CRUD operations
  - [x] Unique course code per department
  - [x] Department existence validation
  - [x] Credit validation (1-20)
  - [x] Soft delete with active classes check
- [x] User Management
  - [x] View all users with filters (role, status, search)
  - [x] Update user details and role
  - [x] Update user password
  - [x] Soft delete with dependency checks
  - [x] User statistics dashboard

### ✅ Teacher Features
- [x] Class Management
  - [x] Create classes with course validation
  - [x] Auto-assign teacher from JWT token
  - [x] Enroll students with duplicate prevention
  - [x] View only own classes
  - [x] Update class details
  - [x] Deactivate classes
  - [x] View enrolled students
- [x] Attendance Management
  - [x] Mark single student attendance
  - [x] Bulk attendance marking
  - [x] Attendance history with filters
  - [x] Attendance summary & statistics
  - [x] Student-specific attendance view
  - [x] Update existing attendance records
- [x] Assignment Management
  - [x] Create assignments
  - [x] View class assignments
  - [x] View submissions
  - [x] Grade submissions (0-100)
  - [x] Add remarks to grades
  - [x] Update assignments
  - [x] Delete assignments (if no submissions)

### ✅ Student Features
- [x] Dashboard with statistics
  - [x] Classes count
  - [x] Assignments count
  - [x] Average grade
  - [x] Attendance percentage
- [x] View enrolled classes
- [x] View personal attendance
- [x] View all grades
- [x] View assignments
  - [x] Filter by class
  - [x] Filter pending only
  - [x] See overdue status
- [x] Submit assignments
  - [x] Submit with file URL
  - [x] Submit with file upload
  - [x] Duplicate submission prevention
  - [x] Enrollment validation
- [x] Download own submissions

### ✅ Technical Features
- [x] Global exception handling middleware
- [x] Custom DateTime JSON converter (yyyy-MM-dd HH:mm)
- [x] Pagination for all list endpoints
- [x] Filtering and search capabilities
- [x] Soft delete pattern
- [x] File upload service
- [x] File type validation
- [x] File size limits (10 MB)
- [x] Built-in logging with ILogger
- [x] Swagger/OpenAPI documentation
- [x] CORS configuration

### ❌ Not Implemented (Optional/Bonus)
- [ ] Caching (in-memory)
- [ ] Advanced logging (Serilog/NLog)
- [ ] Email notifications
- [ ] Notification endpoints for students
- [ ] FluentValidation library
- [ ] AutoMapper

---

## 🔒 Security Features

### Authentication
- JWT tokens with 60-minute expiration
- Refresh tokens with 7-day expiration
- Token rotation on refresh
- Secure token revocation

### Password Security
- BCrypt hashing with work factor 11
- Password strength validation:
  - Minimum 8 characters
  - Uppercase, lowercase, digit, special character required
  
### Authorization
- Role-based access control
- Resource ownership validation
- Teachers can only access their own classes
- Students can only access their own data
- Admins cannot delete themselves

### File Upload Security
- File type whitelist validation
- File size limit (10 MB)
- Unique filename generation (GUID)
- Restricted upload directory

### Data Protection
- Soft delete for data preservation
- Audit trails with CreatedDate/UpdatedDate
- Input validation with Data Annotations
- Parameterized queries (EF Core)

---

## 🎯 Business Rules Implemented

### Department Management
✅ Department names must be unique (case-insensitive)
✅ Only teachers can be Head of Department
✅ Cannot delete department with active courses

### Course Management
✅ Course codes must be unique per department
✅ Course code stored in uppercase
✅ Department must exist and be active
✅ Credits must be between 1-20
✅ Cannot delete course with active classes

### Class Management
✅ Course must exist and be active
✅ Teacher automatically assigned from JWT token
✅ End date must be after start date
✅ Cannot enroll same student twice in same class
✅ Teachers can only manage their own classes

### Attendance
✅ Only assigned teacher can mark attendance
✅ Student must be enrolled in class
✅ Marking same student on same date updates existing record
✅ Status must be Present (1), Absent (2), or Late (3)
✅ Stores only date part (time ignored)

### Assignment & Grading
✅ Due date cannot be in the past
✅ Only class teacher can create assignments
✅ Only assigned teacher can grade submissions
✅ Students can only submit once per assignment
✅ Students must be enrolled in class to submit
✅ Grade must be between 0-100
✅ Cannot delete assignment with existing submissions

### User Management
✅ Email must be unique
✅ Cannot change role if user has dependencies
✅ Cannot delete teacher who is head of department
✅ Cannot delete teacher with active classes
✅ Cannot delete student with active enrollments

---

## 🐛 Troubleshooting

### Database Connection Issues

**Error:** Cannot connect to SQL Server

**Solutions:**
1. Ensure SQL Server is running
2. Check connection string in `appsettings.json`
3. Verify server name (localhost, ., (localdb)\mssqllocaldb, etc.)
4. For LocalDB: `SqlLocalDB info` to check instance name

### Migration Issues

**Error:** Migration already applied

**Solution:**
```bash
# Check migration history
dotnet ef migrations list

# Remove last migration if needed
dotnet ef migrations remove
```

**Error:** Database does not exist

**Solution:**
```bash
# Create database and apply all migrations
dotnet ef database update
```

### File Upload Issues

**Error:** Directory not found (wwwroot/uploads/)

**Solution:**
```bash
mkdir SchoolManagementSystem.API\wwwroot\uploads\assignments
```

**Error:** File too large

**Solution:** Max file size is 10 MB. Check file size before upload.

### Authentication Issues

**Error:** 401 Unauthorized

**Solutions:**
1. Ensure Bearer token is in Authorization header
2. Check token hasn't expired (60 min)
3. Use refresh token to get new access token

**Error:** 403 Forbidden

**Solution:** User role doesn't have permission for this endpoint

### Common Validation Errors

**Password validation failed:**
- Must be at least 8 characters
- Must contain uppercase, lowercase, digit, and special character

**Duplicate enrollment:**
- Student is already enrolled in this class

**Due date in past:**
- Assignment due date cannot be in the past

---

## 📧 Demo Video Scenarios

### Scenario 1: Admin Setup (2-3 minutes)
1. Register Admin user
2. Login as Admin
3. Create Department (Computer Science)
4. Create Course (CS101 - Introduction to Programming)
5. Register Teacher user
6. Assign Teacher as Head of Department

### Scenario 2: Teacher Workflow (3-4 minutes)
1. Login as Teacher
2. Create Class for CS101 course
3. Register 3 Student users
4. Enroll students in class
5. Mark attendance (bulk)
6. Create assignment
7. View submissions
8. Grade a submission

### Scenario 3: Student Workflow (2-3 minutes)
1. Login as Student
2. View dashboard (statistics)
3. View enrolled classes
4. View assignments
5. Submit assignment with file upload
6. View attendance records
7. Check grades

---

## 📝 API Response Format

All endpoints return consistent JSON format:

### Success Response
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { /* response data */ }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "statusCode": 400,
  "traceId": "00-xxxxx-xxxxx-00"
}
```

### Paginated Response
```json
{
  "success": true,
  "message": "Data retrieved successfully",
  "data": [ /* items */ ],
  "pagination": {
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

---

## 📧 Submission

**Developer:** Mohamed Saber  
**Email:** mohamedsabertamer1@gmail.com  
---

## 📄 Project Status

**Status:** ✅ Completed  
**Version:** 1.0.0  
**Last Updated:** November 2025  
**Framework:** .NET Core 8.0  
**Database:** SQL Server

---
**End of Documentation**
