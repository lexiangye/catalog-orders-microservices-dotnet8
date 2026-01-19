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
- **CI/CD**: Workflow automatici via GitHub Actions per la pubblicazione dei pacchetti NuGet e delle immagini Docker su GHCR.

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

### Configurazione Iniziale (NuGet)

Questo progetto utilizza pacchetti ospitati su **GitHub Packages**. Per effettuare il restore delle dipendenze, è necessario autenticarsi configurando le seguenti **Variabili d'Ambiente** sul proprio sistema:

1. **`GITHUB_USER`**: Il tuo username GitHub.
2. **`GITHUB_TOKEN`**: Un tuo **Personal Access Token (PAT)** (Classic) con permesso `read:packages`.

> **Nota:** Non inserire credenziali nel codice. Il file `nuget.config` leggerà automaticamente queste variabili dal tuo sistema operativo.

---

## Avvio in locale

### 1) Avvio completo con Docker Compose (sviluppo)

Dal root della repo:

    docker compose -f compose/compose.dev.yml up --build

Attendi che:
- i container MySQL siano `healthy`
- i due servizi risultino in ascolto (nei log vedrai “Now listening on ...”)
- le migrations vengano applicate automaticamente all’avvio (nei log vedrai righe di EF Core / migrazioni)


### 2) Service map (link rapidi)

> Qui trovi tutto quello che ti serve per aprire Swagger e la CAP Dashboard senza cercare nel compose.

#### Applicazioni

| Servizio | Host | Swagger | CAP Dashboard |
| :-- | :-- | :-- | :-- |
| **CatalogService** | `http://localhost:5052` | [`/swagger`](http://localhost:5052/swagger) | [`/cap`](http://localhost:5052/cap) |
| **OrderService** | `http://localhost:5053` | [`/swagger`](http://localhost:5053/swagger) | [`/cap`](http://localhost:5053/cap) |

#### Infrastruttura (porte)

| Componente | Porta host | Note |
| :-- | --: | :-- |
| MySQL (Catalog) | `3306` | DB CatalogService |
| MySQL (Order) | `3307` | DB OrderService |
| Kafka Broker | `9092` | bootstrap server |

### 3) Test rapido end-to-end (manuale)

1. **CatalogService**: crea un prodotto con quantità iniziale a magazzino
2. **OrderService**: crea un ordine per quel prodotto
3. Verifica:
   - scorte scalate nel CatalogService
   - ordine aggiornato a `CONFIRMED` (se stock OK) oppure `REJECTED` (se stock insufficiente)


---

## Monitoraggio e Affidabilità (CAP Dashboard)

Il progetto implementa i pattern **Transactional Outbox** e **Transactional Inbox** tramite **DotNetCore.CAP**. Questo assicura che la comunicazione tra i microservizi sia sempre consistente con lo stato dei rispettivi database.

Ogni servizio espone una dashboard di controllo:
- **CatalogService Dashboard**: `http://localhost:5052/cap`
- **OrderService Dashboard**: `http://localhost:5053/cap`

### I Pattern implementati:

1.  **Transactional Outbox (Sezione "Published")**: 
    - Quando un servizio salva dati sul DB e deve inviare un messaggio (es. `OrderCreated`), CAP salva il messaggio in una tabella locale (`cap.published`) nella stessa transazione del DB. 
    - Garantisce che il messaggio venga inviato a Kafka **solo se** l'operazione sul database ha successo.

2.  **Transactional Inbox (Sezione "Received")**: 
    - Quando un messaggio arriva da Kafka, viene prima persistito nella tabella locale `cap.received` e poi elaborato.
    - **Idempotenza**: Se Kafka invia lo stesso messaggio due volte, CAP riconosce l'ID già presente nell'Inbox e non riesegue la logica di business, evitando duplicazioni (es. scalare due volte lo stock per lo stesso ordine).
    - **Resilienza**: Se il codice di elaborazione fallisce, il messaggio rimane nell'Inbox in stato "Failed", permettendo il monitoraggio e il recupero.

### Funzionalità della Dashboard:
- **Audit**: Ispezione del contenuto JSON di ogni messaggio inviato e ricevuto.
- **Error Log**: Visualizzazione immediata delle eccezioni lanciate dai Subscriber.
- **Manual Retry**: Possibilità di forzare la rielaborazione di un messaggio fallito (Inbox) o il rinvio di uno non consegnato (Outbox).

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
