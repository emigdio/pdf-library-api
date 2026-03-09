# 📚 PDF Library API

A simple **REST API built with ASP.NET Core** that allows you to browse and download PDF files stored in **Cloudflare R2 object storage**.

The project is designed as a **portfolio demonstration** of a modern backend architecture using:

* ASP.NET Core Web API
* Docker
* Cloudflare R2 (S3-compatible object storage)
* Render cloud deployment
* Swagger/OpenAPI documentation

---

# 🏗 Architecture

```
Client
   │
   ▼
Render (Docker container)
   │
   ▼
ASP.NET Core API
   │
   │  S3-compatible API
   ▼
Cloudflare R2
   │
   ▼
PDF files
```

The API **does not store files locally**.
Instead, it retrieves metadata from Cloudflare R2 and generates **signed URLs** so clients can download files directly from storage.

---

# 🚀 Features

* List available PDF files
* Download PDFs using **secure signed URLs**
* Cloud storage integration (Cloudflare R2)
* Dockerized application
* Swagger UI for easy API testing
* Designed for deployment on **Render Free tier**

---

# 📂 Project Structure

```
PdfLibraryApi
│
├── Controllers
│   └── BooksController.cs
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

The API requires the following environment variables to connect to Cloudflare R2:

```
R2_ACCOUNT_ID=
R2_ACCESS_KEY_ID=
R2_SECRET_ACCESS_KEY=
R2_BUCKET=
R2_PREFIX=books/
```

Example:

```
R2_ACCOUNT_ID=1234567890abcdef
R2_ACCESS_KEY_ID=xxxxx
R2_SECRET_ACCESS_KEY=xxxxx
R2_BUCKET=pdf-library
R2_PREFIX=books/
```

---

# 📡 API Endpoints

## List available PDFs

```
GET /api/books
```

Example response:

```json
[
  {
    "id": "clean-architecture",
    "fileName": "clean-architecture.pdf"
  }
]
```

---

## Download a PDF

```
GET /api/books/{id}/file
```

Example:

```
GET /api/books/clean-architecture/file
```

The API returns a **redirect to a signed URL** from Cloudflare R2.

---

# 🐳 Running with Docker

Build the image:

```
docker build -t pdfapi .
```

Run the container:

```
docker run -p 8080:8080 pdfapi
```

Open Swagger:

```
http://localhost:8080/swagger
```

---

# ☁️ Deployment

This project is deployed using **Render**.

Steps:

1. Push repository to GitHub
2. Create a new **Web Service** in Render
3. Select **Docker environment**
4. Add the environment variables listed above
5. Deploy

After deployment:

```
https://your-service-name.onrender.com
```

---

# 🧪 Testing the API

Open Swagger UI:

```
/swagger
```

Example:

```
https://pdf-library-api.onrender.com/swagger
```

---

# 🧰 Technologies Used

* **.NET / ASP.NET Core**
* **AWS S3 SDK for .NET**
* **Cloudflare R2**
* **Docker**
* **Render Cloud**
* **Swagger / OpenAPI**

---

# 📌 Why Cloudflare R2?

Cloudflare R2 provides **S3-compatible object storage** without egress fees, making it ideal for serving downloadable files like PDFs.

---

# 📈 Possible Improvements

Future improvements could include:

* Metadata database (PostgreSQL)
* Authentication
* File uploads
* Caching layer

---

# 👨‍💻 Author

**Emigdio Camacho**

Software Engineer
Mobile & Backend Developer

GitHub:
https://github.com/emigdiocamacho
