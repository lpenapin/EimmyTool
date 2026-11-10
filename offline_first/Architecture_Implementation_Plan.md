# Implementation Plan: Offline-First Architecture and Server Synchronization

## 1. Executive Summary

The goal of this project is to transform the existing desktop application (*e-tool*) with a local SQLite database into an **Offline-First** architecture. This ensures the application operates without interruptions or network dependencies while securely synchronizing data with a central server to power a future Web Dashboard.

## 2. Project Roadmap

### Phase 1: Local Data Model Adaptation

* **Objective:** Prepare the local SQLite database to handle globally unique identifiers and capture local change events.

* **Tasks:**

  1. **Migration to GUIDs/UUIDs:** Replace auto-incrementing primary keys with GUIDs (`string` or `uniqueidentifier`) across all main tables.

  2. **Create `SyncQueue` Table (Outbox Pattern):** Structure a local control table to capture pending mutations.

     * *Fields:* `Id` (GUID), `EntityName` (VARCHAR), `OperationType` (INSERT, UPDATE, DELETE), `PayloadJson` (TEXT), `CreatedAt` (DATETIME), `Status` (INTEGER: 0=Pending, 1=Synced, 2=Error), `RetryCount` (INT).

  3. **Audit Fields:** Ensure all business tables include `CreatedAt`, `UpdatedAt`, and `IsDeleted` (enabling soft deletes instead of physical row deletions).

### Fase 2: Local Writing Logic and Outbox Pattern

* **Objective:** Guarantee that every local CRUD operation automatically records a corresponding synchronization event.

* **Tasks:**

  1. **Repository Updates:** Update `Save`, `Update`, and `Delete` methods to execute inside a local SQLite transaction.

  2. **Atomic Dual-Write:** Within the same local transaction:

     * Apply the change to the business table.

     * Insert the JSON-encoded payload event into `SyncQueue`.

  3. **Offline Continuity Testing:** Validate full end-to-end CRUD operations on the desktop client while completely disconnected from the network.

### Phase 3: Central Backend & API Development

* **Objective:** Build the central REST API service that receives event batches from desktop clients and persists them to the central database (SQL Server).

* **Tasks:**

  1. **Synchronization Endpoints Design:**

     * `POST /api/sync/push`: Receives batch records from the local client queue.

     * `GET /api/sync/pull`: Returns remote changes that occurred since the client's last sync timestamp.

  2. **Batch Processing Engine:** Build controller logic to process transaction batches atomically and return execution status for each item.

  3. **Authentication & Security:** Implement secure authentication tokens (JWT or API Keys per device) to authorize requests.

### Phase 4: Background Synchronization Engine (Worker Service)

* **Objective:** Execute synchronization transparently in the background without blocking the user interface.

* **Tasks:**

  1. **Connectivity Monitor:** Create a background service that periodically checks network availability (e.g., every 60 seconds).

  2. **Push Worker:**

     * Fetch pending records (`Status = 0`) from local `SyncQueue` ordered by creation date.

     * Package records into batches of 50–100 items.

     * Send payload to `POST /api/sync/push`.

     * Mark local records as `Synced` (`Status = 1`) upon HTTP 200/201 response.

  3. **Pull Worker:** Query remote changes from the server to update the local SQLite database with updates made from other endpoints.

  4. **Error Handling & Exponential Backoff:** Increment retry counters and handle transient network failures gracefully.

### Phase 5: Preparation for Future Web Dashboard

* **Objective:** Ensure central server data is consolidated, scalable, and ready to be consumed by a web portal.

* **Tasks:**

  1. **Central Data Consolidation:** Ensure server schema handles multi-tenant or multi-device entries seamlessly without collisions.

  2. **Read-Only Analytics API:** Build optimized Web API endpoints for web dashboard metrics and key KPIs.

  3. **Multi-User Access Control:** Establish roles and permissions for web user access.

## 3. Risk Management & Mitigation Matrix

| Risk | Impact | Mitigation Strategy | 
| ----- | ----- | ----- | 
| **Concurrent Edit Conflicts** | High | Implement *Last-Write-Wins (LWW)* rule based on UTC timestamps (`UpdatedAt`). | 
| **Mid-Sync Network Failures** | Medium | Use small batch sizes and API idempotency (checking event `Id` to prevent duplicate writes). | 
| **SQLite DB Bloat** | Low | Run periodic cleanup background tasks to purge synced events older than 30 days. | 

## 4. Best Practices & Key Recommendations

1. **Adopt GUIDs Immediately:** Replace auto-incrementing integer IDs with GUIDs prior to building the sync engine to prevent primary key collisions when merging multi-device data.

2. **Enforce Soft Deletes:** Avoid physical SQL `DELETE` queries locally. Use an `IsDeleted` flag so deletions can be propagated cleanly to the server as an update payload.

3. **API Idempotency:** Design backend handlers to detect duplicate requests using unique event IDs, returning HTTP 200 without creating duplicate records.

4. **Airplane Mode Stress Testing:** Continually test network dropouts during high-volume transactions to verify outbox resiliency.