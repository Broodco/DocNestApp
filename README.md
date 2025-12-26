# DocNestApp

**DocNestApp** is a personal document tracker designed to help individuals and households keep control over their administrative documents and deadlines.

The application centralizes document metadata and files, tracks expiration dates, and sends reminders before important deadlines — all while remaining intentionally small, pragmatic, and finishable.

This project is **portfolio-oriented** and focuses on **clean delivery, modern .NET practices, and architectural restraint**.

---

## 🎯 Project Goals

DocNest aims to:

- Centralize administrative documents in one place
- Track document expiration dates
- Send reminders before deadlines
- Support multiple household members
- Remain simple, shippable, and maintainable

This is **not** a full commercial product.
It is a **finished MVP** designed to demonstrate sound engineering decisions.

---

## 🚫 Non-Goals

To keep delivery speed high, DocNest intentionally avoids:

- Microservices
- Message brokers (Kafka, RabbitMQ, etc.)
- Outbox / Inbox patterns
- Event sourcing
- Over-engineered RBAC systems
- Premature abstractions

Advanced features may be explored later, but they are explicitly **out of scope** for the MVP.

---

## 🧠 Architectural Principles

### Vertical Slice Architecture
Features are implemented as **vertical slices**, each owning its full execution path:

- HTTP endpoint
- Validation
- Persistence
- Response shaping

This minimizes cross-cutting changes and avoids excessive scaffolding.

Example structure:

Features/
└─ Documents/
└─ Create/
├─ Endpoint.cs
├─ Command.cs
├─ Validator.cs
└─ Handler.cs


### Clean-ish Boundaries
Clean Architecture principles are applied **only where they reduce friction**.

- Domain logic stays clean when it enforces real invariants
- Infrastructure details are allowed to leak early if it improves speed
- Logic is promoted (to value objects or services) only when repetition or complexity justifies it

---

## 🧩 Domain Modeling Guidelines

### Keep logic inside a slice when:
- It is used only once
- It is trivial
- It does not enforce invariants

### Promote to a Value Object when:
- Validation rules repeat
- Formatting must be consistent
- Invariants must never be broken

**Examples:**
- `ExpiryDate`
- `ReminderPolicy`
- `DocumentNumber`

### Promote to a Domain Service when:
- Logic spans multiple entities
- It does not naturally belong to a single aggregate

### Promote to an Application Service when:
- Orchestrating infrastructure (storage, email, background jobs)
- The concern is *how* rather than *what is correct*

---

## 🚀 MVP Scope

The MVP is intentionally limited to **three core user journeys**:

1. **Add a document**
   - Metadata (type, owner, expiration date, tags)
   - File upload
2. **Search and view documents**
   - Full-text or tag-based search
   - Download or view stored files
3. **Expiration reminders**
   - Background worker checks expiration dates
   - Notifications are generated before deadlines

Any feature not supporting these journeys goes into a `NOT_NOW.md` list.

---

## 🧱 Technical Stack

### Backend
- **.NET** (latest LTS)
- **FastEndpoints**
- **EF Core**
- **PostgreSQL**
- **FluentValidation**
- **OpenAPI / Swagger**

### Local Development
- **.NET Aspire** for orchestration
  - API
  - PostgreSQL
  - Background worker

Aspire is used strictly as a **development-time orchestrator**.  
All services can run standalone without Aspire.

---

## 🔧 Aspire Philosophy

- Aspire wires services together locally via configuration
- No Aspire-specific dependencies in domain or application logic
- Services must run with `dotnet run` outside of Aspire

This keeps the application easily exportable to:
- Docker Compose
- Cloud hosting platforms
- CI environments

---

## 🔥 Avoiding Boilerplate Burnout

This project intentionally avoids:

- Generic repositories
- Excessive layering
- Abstract base services “for future reuse”
- Framework-heavy CQRS implementations

Instead, it favors:
- Direct `DbContext` usage
- Explicit queries per feature
- Clear, boring, readable code
- Duplication over premature generalization

---

## 🧠 Finishing Strategy (Important)

DocNest follows a strict **“Portfolio Done”** definition:

The project is considered **done** when:
- It runs with a single command (Aspire)
- Demo data is seeded
- The 3 MVP user journeys are complete
- A README and screenshots exist
- A demo is deployed **or** a demo video is recorded

Anything beyond this is optional.

---

## 🛑 NOT_NOW Philosophy

Ideas that are interesting but out of scope are captured in `NOT_NOW.md`.

This allows:
- Preserving ideas without losing momentum
- Preventing scope creep
- Maintaining focus on finishing

---

## 🧪 Testing Strategy (Planned)

- Integration tests for core slices
- Minimal unit testing for domain invariants
- Focus on testing real user flows rather than internals

---

## 📌 Status

🚧 **In development**  
Currently focusing on:
- Core domain modeling
- First vertical slice: document creation

---

## 📜 License

This project is intended for educational and portfolio purposes.
Licensed under MIT License.
