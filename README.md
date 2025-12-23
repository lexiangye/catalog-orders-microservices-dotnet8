# Catalogo & Ordini Microservices (.NET 8)

Progettino semplice a microservizi in C#/.NET 8 composto da:
- **CatalogService**: prodotti + scorte (source of truth)
- **OrderService**: ordini + stato ordine

Caratteristiche principali:
- **Un database MySQL per microservizio**
- **Entity Framework Core + migrations** (niente script SQL manuali)
- **Comunicazione sincrona HTTP** (typed HttpClient) con **retry + timeout + circuit breaker**
- **Comunicazione asincrona Kafka** con **mini-saga coreografata**:
  - `OrderCreated` → `StockReserved` / `StockReservationFailed` → ordine `CONFIRMED` / `REJECTED`
  - (opzionale/extra) compensazione: `OrderCancelled` → rilascio scorte

---

## Descrizione repository (GitHub “About”)

Progettino semplice a microservizi in .NET 8 (Catalogo + Ordini) con MySQL (EF Core migrations), chiamate HTTP resilienti (retry/timeout/circuit breaker) e saga event-driven su Kafka. Avvio locale con Docker Compose.

---

## Convenzioni di lavoro (branch e commit)

Per mantenere una cronologia pulita e un flusso di lavoro prevedibile, usiamo queste convenzioni:

### Branch
- **`main`**: contiene sempre il codice stabile (pronto all’esecuzione).
- **`feature/<descrizione>`**: un branch per ogni funzionalità o attività incrementale.  
  Esempi:
  - `feature/catalog-api`
  - `feature/order-saga-kafka`
  - `feature/http-resilience`
- **`hotfix/<descrizione>`**: correzioni urgenti su bug bloccanti (tipicamente da `main`).  
  Esempi:
  - `hotfix/compose-env-vars`
  - `hotfix/migrations-startup`

> Suggerimento pratico: apri una Pull Request verso `main` quando la feature è completa e la build/compose passano.

### Commit (Conventional Commits)
Adottiamo lo standard **Conventional Commits** per rendere la cronologia chiara e (se serve) generare changelog automatici.

Formati tipici:
- `feat: ...` → nuova funzionalità
- `fix: ...` → correzione bug
- `chore: ...` → manutenzione (config, refactor non funzionale, aggiornamenti tool)
- `docs: ...` → documentazione (README, note)
- `test: ...` → test
- `refactor: ...` → refactor senza cambiare comportamento

Esempi:
- `feat: add create-order endpoint`
- `fix: handle mysql not ready on startup`
- `chore: add github actions for ghcr`
- `docs: update run instructions`
- `refactor: simplify stock reservation logic`

---

## Struttura repository

- `/shared/` → libreria **Shared** pubblicata come pacchetto **NuGet** (GitHub Packages)
- `/services/CatalogService/` → soluzione separata + 5 layer (WebApi, Business, ClientHttp, Repository, Shared via NuGet)
- `/services/OrderService/` → soluzione separata + 5 layer (WebApi, Business, ClientHttp, Repository, Shared via NuGet)
- `/compose/` → file Docker Compose

---

## Prerequisiti

Per eseguirlo in locale servono:
- **Docker** + **Docker Compose v2**
- (opzionale, per sviluppo senza Docker) **.NET SDK 8**

---

## Avvio in locale (consigliato)

### 1) Avvio completo con Docker Compose (sviluppo)

Dal root della repo:

    docker compose -f compose/compose.dev.yml up --build

Attendi che:
- i container MySQL siano `healthy`
- i due servizi risultino in ascolto (nei log vedrai “Now listening on ...”)
- le migrations vengano applicate automaticamente all’avvio (nei log vedrai righe di EF Core / migrazioni)

### 2) Swagger / endpoint

Apri Swagger dal browser (porte definite nel compose):
- CatalogService: `http://localhost:<porta_catalog>/swagger`
- OrderService: `http://localhost:<porta_order>/swagger`

> Le porte esatte sono definite nei file `compose/*.yml`.

### 3) Test rapido end-to-end (manuale)

1. **CatalogService**: crea un prodotto con quantità iniziale a magazzino  
2. **OrderService**: crea un ordine per quel prodotto  
3. Verifica:
   - scorte scalate nel CatalogService
   - ordine aggiornato a `CONFIRMED` (se stock OK) oppure `REJECTED` (se stock insufficiente)

---

## Stop e pulizia

Stop dei container:

    docker compose -f compose/compose.dev.yml down

Stop + rimozione volumi (cancella anche i dati DB):

    docker compose -f compose/compose.dev.yml down -v

---

## Note operative

- Ogni microservizio mantiene **il proprio database** e **le proprie migrations**
- Le chiamate HTTP tra servizi usano politiche di resilienza (retry/timeout/circuit breaker)
- La gestione ordine+scorte è basata su eventi Kafka (saga coreografata)
