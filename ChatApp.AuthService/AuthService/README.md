# AuthService Microservice

## ✅ Overview
AuthService handles user authentication, registration, password management, and token generation for the system.

---

## 🚀 Tech Stack
- **.NET 8**
- **MongoDB**
- **xUnit + Moq + FluentAssertions** (Unit Testing)
- **Serilog** (Logging)
- **Coverlet + ReportGenerator** (Test Coverage)
- **PowerShell Scripts** (Automation)

---

## 🔧 Features
- User Registration
- Login with JWT Token
- Forgot Password
- Reset Password
- Forgot Username
- Secure Password Hashing (PBKDF2)
- Unit Test Coverage Reports

---

## 🧪 Testing
Run tests and generate coverage reports with:
```bash
.\run-tests.ps1
```
Coverage report will be available in the `CoverageReport` folder.

---

## 🐳 Docker (Coming Soon)
Dockerization steps will be added later.

---

## 📂 Project Structure
```
AuthService/
│
├── Controllers/
├── DTOs/
├── Models/
├── Services/
├── Repositories/
├── Utils/
├── Tests/
├── CoverageReport/
└── run-tests.ps1
```

---

## 🔒 Security
- Passwords hashed securely.
- JWT tokens signed and validated.
- Logging with Serilog.

---

## 🏗️ Next Steps
- Add Docker support.
- Deploy to AWS.
- Add Integration Tests.
- Complete CI/CD pipeline.

