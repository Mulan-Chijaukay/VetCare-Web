# VetCare - Management System for Veterinary Clinics 🐾
VetCare is a web application built with **ASP.NET Core MVC 8.0** designed to manage veterinary clinic operations efficiently. It integrates secure user authentication, pet medical records, and a specialized appointment management system.
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512bd4?logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoft-sql-server)
![Entity Framework](https://img.shields.io/badge/EF%20Core-8.0-512bd4)
![License](https://img.shields.io/badge/License-MIT-green)

---

## 🌟 Key Features

* **📅 Smart Scheduling**: Real-time server-side validation to prevent overlapping appointments for veterinarians.
* **🐕 Follow-up System**: Automated dashboard for pets needing return visits based on clinical suggestions.
* **🔐 Secure Access**: Role-based access control for Administrators, Veterinarians, and Pet Owners.
* **🎨 Professional UI**: Figma-inspired responsive design using Bootstrap 5 and SweetAlert2.

## 🛠️ Technology Stack

| Category | Technology |
| :--- | :--- |
| **Backend** | C#, ASP.NET Core MVC 8.0 |
| **ORM** | Entity Framework Core |
| **Database** | Microsoft SQL Server |
| **Frontend** | HTML5, CSS3, JavaScript, Bootstrap 5 |
| **Alerts** | SweetAlert2 |

---

## 🚀 Getting Started

Follow these steps to set up the project locally:

### 1️⃣ Clone the Repository
```bash
git clone [https://github.com/Mulan-Chijaukay/VetCare-Web.git](https://github.com/Mulan-Chijaukay/VetCare-Web.git)
cd VetCare-Web


2️⃣ Database Configuration
Open appsettings.json and update your connection string:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=VetCareDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
3️⃣ Apply Migrations
Execute the following command in the Package Manager Console to create the database schema:

Update-Database
Or via terminal:

dotnet ef database update
4️⃣ Run Application
Press F5 in Visual Studio or use the CLI:

dotnet run --project VetCare.Web



