# *Ethics Review Management System*  
*A lightweight MVC application for managing research ethics submissions, inspired by Infonetica Research Management Software.*

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) [![Tech Stack](https://img.shields.io/badge/Tech-ASP.NET%20MVC%208-blueviolet)](https://dotnet.microsoft.com/apps/aspnet) [![Build Status](https://img.shields.io/badge/Build-passing-brightgreen)](#)

---

## 📌 *Overview*
The **Ethics Review Management System** is an ASP.NET MVC application designed to streamline the ethics review workflow used in academic and research institutions.

It supports three user roles:

- *Researchers* – create and submit ethics applications  
- *Reviewers* – evaluate assigned submissions  
- *Administrators* – manage users and assign reviewers  

The system focuses on *ethical accountability*, *transparency*, and *smooth approval workflows*.

---

## *Features*

### *Researcher*
- Create and edit ethics submissions  
- Upload supporting documents  
- Save as *Draft* and later *Submit*  
- View submission status & reviewer feedback  
- Re-submit after revisions  

### *Reviewer*
- Personalized dashboard with assigned submissions  
- View full submission details and documents  
- Approve or Request Revisions  
- Provide structured feedback  

### *Admin*
- Manage users and roles  
- Assign reviewers to submissions  
- View all submissions & statuses  
- Oversee workflow from draft to final approval  


---

## *Database Schema (Simplified)*

### *Users*
| Field | Type | Notes |
|------|------|-------|
| UserID | int | PK |
| Name | nvarchar |  |
| Email | nvarchar | Unique |
| Role | enum | *Researcher / Reviewer / Admin* |

### *Submissions*
| Field | Type | Notes |
|------|------|-------|
| SubmissionID | int | PK |
| ResearcherID | FK → Users |
| Title | nvarchar | |
| Summary | nvarchar(max) | |
| Methodology | nvarchar(max) | |
| Risks | nvarchar(max) | |
| Status | enum | *Draft/Submitted/Revision Required/Approved* |

### *Documents*
| Field | Type | Notes |
|------|------|-------|
| DocumentID | int | PK |
| SubmissionID | FK |
| FilePath | nvarchar | |

### *ReviewDecisions*
| Field | Type | Notes |
|------|------|-------|
| DecisionID | int | PK |
| SubmissionID | FK |
| ReviewerID | FK |
| Decision | enum | *Approve / Request Revisions* |
| Comments | nvarchar(max) | |

---

## *Technology Stack*
- *ASP.NET MVC 8*  
- *C#*  
- *Entity Framework / MS SQL Server*  
- *Bootstrap 5*  
- *Identity for authentication*  
- *Razor Views*

---

## *Installation*
# 1️⃣ Clone the repo
git clone https://github.com/yourusername/EthicsReviewSystem.git
cd EthicsReviewSystem

# 2️⃣ Configure the database
# Open appsettings.json and update the connection string:
# "ConnectionStrings": {
#   "DefaultConnection": "Server=.;Database=EthicsDB;Trusted_Connection=True;"
# }

# 3️⃣ Apply migrations
dotnet ef database update

# 4️⃣ Run the app
dotnet run
