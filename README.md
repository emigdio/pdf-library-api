# 📚 PDF Library API

A **REST API built with ASP.NET Core** that allows you to browse and download PDF files stored in **Cloudflare R2 object storage**, and manages their metadata using a **Supabase (PostgreSQL)** database.

The project is designed as a **portfolio demonstration** of a modern backend architecture using:

* ASP.NET Core Web API
* Entity Framework Core (PostgreSQL)
* Docker
* Cloudflare R2 (S3-compatible object storage)
* Supabase (Managed Postgres Database)
* Render cloud deployment
* Swagger/OpenAPI documentation

---

# 🏗 Architecture

```text
Client
   │
   ▼
Render (Docker container)
   │
   ├──▶ Supabase (PostgreSQL) -> Metadata (Books table)
   │
   ▼
ASP.NET Core API
   │
   │  S3-compatible API
   ▼
Cloudflare R2 -> PDF files
```

The API **does not store files locally**.
Instead, it stores book metadata (Title, Author, PageCount, etc.) in a PostgreSQL database hosted by Supabase, and generates **secure signed URLs** so clients can download files directly from Cloudflare R2.

---

# 🚀 Features

* List available PDF files with complete metadata from PostgreSQL
* Download PDFs using **secure signed URLs**
* Robust relational database integration via **Entity Framework Core**
* Cloud storage integration (Cloudflare R2)
* Dockerized application environment
* Swagger UI for easy API testing
* Designed for deployment on **Render Free tier**

---

# 📂 Project Structure

```text
PdfLibraryApi
│
├── Controllers
│   └── BooksController.cs
│
├── Data
│   ├── AppDbContext.cs
│   └── Migrations/
│
├── Services
│   └── R2Storage.cs
│
├── Models
│   └── Book.cs
│
├── Program.cs
├── PdfLibraryApi.csproj
├── Dockerfile
└── README.md
```

---

# ⚙️ Environment Variables

The API requires the following environment variables to connect to its services:

### Database (Supabase)
```text
DB_CONNECTION_STRING=postgres://[user]:[password]@[pooler-url]:5432/postgres
```

### Cloud Storage (Cloudflare R2)
```text
R2_ACCOUNT_ID=1234567890abcdef
R2_ACCESS_KEY_ID=xxxxx
R2_SECRET_ACCESS_KEY=xxxxx
R2_BUCKET=pdf-library
R2_PREFIX=books/
```

---

# 📡 API Endpoints

## List available PDFs

```http
GET /api/books
```

Example response:

```json
[
  {
    "id": "123e4567-e89b-12d3...",
    "title": "Clean Architecture",
    "author": "Robert C. Martin",
    "pageCount": 432,
    "fileName": "clean-architecture.pdf",
    "createdAt": "2026-04-03T00:00:00Z"
  }
]
```

---

## Download a PDF

```http
GET /api/books/{id}/file
```

The API returns a **redirect to a signed URL** generated dynamically from Cloudflare R2.

---

# 🐳 Running with Docker

Build the image:

```bash
docker build -t pdfapi .
```

Run the container:

```bash
docker run -p 8080:8080 pdfapi
```

Open Swagger:

```text
http://localhost:8080/swagger
```

---

# ☁️ Deployment

This project is deployed using **Render**.

Steps:

1. Push repository to GitHub
2. Create a new **Web Service** in Render
3. Select **Docker environment**
4. Add the environment variables listed above (R2 & DB_CONNECTION_STRING)
5. Deploy

After deployment:

```text
https://your-service-name.onrender.com
```

> **Note on Supabase:** For Render environments, use the **Transaction Pooler URL** in Session mode (`:5432`) to prevent Entity Framework Core timeouts during database migrations.

---

# 🧪 Testing the API

Open Swagger UI:

```text
/swagger
```

Example:

```text
https://pdf-library-api.onrender.com/swagger
```

---

# 🧰 Technologies Used

* **.NET / ASP.NET Core**
* **Entity Framework Core (Npgsql)**
* **PostgreSQL (Supabase)**
* **AWS S3 SDK for .NET**
* **Cloudflare R2**
* **Docker**
* **Render Cloud**
* **Swagger / OpenAPI**

---

# 📌 Why Cloudflare R2 & Supabase?

* **Cloudflare R2** provides **S3-compatible object storage** without egress fees, making it ideal for serving downloadable files like PDFs.
* **Supabase** provides extremely fast, managed PostgreSQL databases with built-in connection pooling, perfect for integrating via Entity Framework Core.

---

# 📈 Possible Improvements

Future improvements could include:

* Authentication (JWT)
* File uploads via API endpoints
* Caching layer

---

# 👨‍💻 Author

**Emigdio Camacho**

Software Engineer  
Mobile & Backend Developer  

GitHub:
https://github.com/emigdiocamacho
