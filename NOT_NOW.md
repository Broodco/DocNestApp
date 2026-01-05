# NOT_NOW.md

This file captures ideas, features, and technical improvements that are **explicitly out of scope** for the DocNest MVP.

They may be valuable later, but are postponed to preserve focus, delivery speed, and finishability.

---

## 🧑‍💻 Authentication & Authorization

- Real authentication (OAuth, OpenID Connect, external IdP)
- User registration, login, password reset
- Role-based access control (RBAC)
- Household invitations / sharing documents between users
- Fine-grained permissions per document or subject

**Reason:**  
The MVP uses a fixed `DevUser` to demonstrate user isolation without authentication complexity.

---

## 📁 Document & File Management

- Multiple files per document
- File versioning / history
- Replacing or deleting files
- Virus scanning
- Cloud storage providers (S3, Azure Blob, GCS)
- CDN / signed download URLs
- Thumbnail or preview generation (PDF, images)

**Reason:**  
The MVP supports **at most one optional file per document**, stored locally.

---

## 🏷️ Metadata & Organization

- Tags / labels
- Categories / folders
- Custom document schemas per type
- Hierarchical subjects (household trees)
- Document relationships (linked documents)

**Reason:**  
Simple metadata (title, type, expiration date) is sufficient for core user journeys.

---

## 🔍 Search & Querying

- Full-text search engines (Postgres FTS, Elasticsearch, Meilisearch)
- Advanced filtering (AND/OR groups, saved searches)
- Sorting on arbitrary fields
- Faceted search / aggregations

**Reason:**  
Basic listing with light filtering meets MVP requirements.

---

## ⏰ Reminders & Notifications (Advanced)

- Email notifications
- SMS / push notifications
- Notification templates
- User-configurable reminder schedules
- Snoozing reminders
- Reminder history / audit trail

**Reason:**  
The MVP will generate reminders internally (worker + persistence/logging) without external delivery.

---

## 📊 Auditing & Compliance

- Full audit log per entity
- Soft deletes
- Retention policies
- GDPR tooling (export/delete user data)
- Legal/compliance features

**Reason:**  
Basic timestamps are enough for MVP; no compliance claims are made.

---

## 🧩 Architecture & Infrastructure

- Microservices
- Message brokers (Kafka, RabbitMQ, Azure Service Bus)
- Outbox / Inbox patterns
- Event sourcing
- Distributed transactions
- Domain events infrastructure
- CQRS frameworks
- Generic repositories / Unit of Work abstractions
- Heavy base entity inheritance

**Reason:**  
A single-process application with EF Core and simple vertical slices is intentional.

---

## 🖥️ Frontend & UX

- Full SPA frontend
- Mobile application
- Advanced UI/UX design
- Accessibility compliance
- Theming / customization

**Reason:**  
Swagger or a minimal admin-style UI is sufficient for demonstrating functionality.

---

## 📦 Product Extensions

- Subscriptions as a first-class aggregate
- Billing / payment tracking
- Expense analysis
- Shared household dashboards

**Reason:**  
These are valid product ideas, but outside the MVP’s goal of being **finished**.

---

## 🧪 Testing & Quality (Advanced)

- Property-based testing
- Performance / load testing
- Contract testing
- Chaos testing
- Snapshot testing

**Reason:**  
The MVP will focus on integration tests covering real user journeys.

---

## 🚀 Deployment & Ops

- Production cloud deployment
- CI/CD pipelines
- Infrastructure-as-code
- Secrets management
- Horizontal scaling

**Reason:**  
Local Aspire orchestration is sufficient for development and demo purposes.

---

## ✅ Status

This file is **intentional** and **binding** for the MVP.  
Items may be revisited *only after* the project is considered **Portfolio Done**.
