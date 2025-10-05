# RAGSharp: Lightweight RAG Pipeline for .NET

RAGSharp is a lightweight, extensible Retrieval-Augmented Generation (RAG) library built entirely in C#. It provides clean, minimal, and predictable components for building RAG pipelines: document loading, token-aware text splitting, vector storage, and semantic search.

## 🚀 Why RAGSharp?

**Choose RAGSharp if you want:**
- **Just the RAG essentials** - load, chunk, embed, search
- **Local-first** - Works with any OpenAI-compatible API (OpenAI, LM Studio, Ollama, vLLM, etc.) out of the box.
- **No database required** - File-based storage out of the box, extensible to Postgres/Qdrant/etc.
- **Simple and readable** - Straightforward interfaces you can understand quickly

RAGSharp doesn’t aim to replace frameworks like Semantic Kernel or Kernel Memory — instead, it is a minimal, local-friendly pipeline you can understand in one sitting and drop into an existing app.

## 📦 Installation 

```bash
dotnet add package RAGSharp
```

**Dependencies:**
- `OpenAI` - Official SDK for embeddings 
- `SharpToken` - GPT tokenization for accurate chunking
- `HtmlAgilityPack` - HTML parsing for web loaders
- `Microsoft.Extensions.Logging` - Logging abstractions


## ✨ Quick Start
Load documents, index them, and search - in under 15 lines:

```csharp
using RAGSharp.Embeddings.Providers;
using RAGSharp.IO;
using RAGSharp.RAG;
using RAGSharp.Stores;

var docs = await new FileLoader().LoadAsync("sample.txt");

// OpenAIEmbeddingClient works with any OpenAI-compatible API
var retriever = new RagRetriever(
    embeddings: new OpenAIEmbeddingClient(
        baseUrl: "http://127.0.0.1:1234/v1",
        apiKey: "lmstudio",
        defaultModel: "text-embedding-3-small"
    ),
    store: new InMemoryVectorStore()
);

await retriever.AddDocumentsAsync(docs);

var results = await retriever.Search("quantum mechanics",  topK: 3);
foreach (var r in results)
    Console.WriteLine($"{r.Score:F2}: {r.Content}");

```
This example uses a local embedding model hosted by LM Studio ([docs](https://lmstudio.ai/docs/app/api)) and an in-memory vector store.

Works with:
- OpenAI API (api.openai.com)
- LM Studio (localhost:1234)
- Ollama via OpenAI shim
- Any OpenAI-compatible embedding service

## ⚙️ Architecture. 
Every RAG pipeline in RAGSharp follows this flow. Each part is pluggable:
```
   [ DocumentLoader ] → [ TextSplitter ] → [ Embeddings ] → [ VectorStore ] → [ Retriever ]
```
That’s it — the essential building blocks for RAG, without the noise.

### 🔌 Extensibility
Built on simple interfaces. Bring your own provider:

Embeddings:
```csharp
public interface IEmbeddingClient
{
    Task<float[]> GetEmbeddingAsync(string text);
    Task<IReadOnlyList<float[]>> GetEmbeddingsAsync(IEnumerable<string> texts);
}
```

Vector Store:
```csharp
public interface IVectorStore
{
    Task AddAsync(VectorRecord item);
    Task AddBatchAsync(IEnumerable<VectorRecord> items);
    Task<IReadOnlyList<SearchResult>> SearchAsync(float[] query, int topK);
    bool Contains(string id);
}
```
No vendor lock-in. Want Claude or Gemini embeddings? Cohere? A Postgres or Qdrant backend? Just implement the interface and drop it in.

## 📚 Core Components

### 1. Document Loading (RAGSharp.IO)
Loaders fetch raw data and convert it into a list of Document objects.

| Loader          | Description                                      |
|-----------------|--------------------------------------------------|
| `FileLoader`      | Load a single text file.                         |
| `DirectoryLoader` | Load all files matching a pattern from a directory. |
| `UrlLoader`       | Scrapes a web page (cleans HTML, removes noise) and extracts plain text. |
| `WebSearchLoader` | Searches and loads content directly from Wikipedia articles. |

Custom loaders: Implement ```IDocumentLoader``` for PDFs, Word docs, databases, etc.

### 2. Embeddings & Tokenization (RAGSharp.Embeddings)

| Component                | Description                                                                 |
|--------------------------|-----------------------------------------------------------------------------|
| `OpenAIEmbeddingClient`  | **Included.** Works with any OpenAI-compatible API |
| `IEmbeddingClient`       | Interface for custom providers (Claude, Gemini, Cohere, etc.) |
| `SharpTokenTokenizer`    | **Included.** GPT-family token counting |
| `ITokenizer`             | Interface for custom tokenizers (Uses `SharpToken`) |


### 3. Text Splitting (RAGSharp.Text)
`RecursiveTextSplitter` - Token-aware recursive splitter that respects semantic boundaries:

```csharp
var tokenizer = new SharpTokenTokenizer("gpt-4");
var splitter = new RecursiveTextSplitter(
    tokenizer, 
    chunkSize: 512,    // tokens
    chunkOverlap: 50
);
```

Here’s how it works:
```
- Input Text
    ↓
Split by paragraphs (\n\n)
    ↓
For each paragraph:
    ├─ Fits in chunk size? → Yield whole paragraph
    └─ Too large? → Split by sentences
                      ↓
                For each sentence:
                    ├─ Buffer + sentence fits? → Add to buffer
                    └─ Too large?
                         ├─ Yield buffer
                         └─ Single sentence too large? → Token window split
                                                       └─ Sliding window with overlap
```

- Splits by paragraphs first
- Falls back to sentences if chunks too large
- Final fallback: sliding token window with overlap
- Uses `SharpTokenTokenizer` for token counting

Implement `ITextSplitter` for custom splitting strategies.


### 4. Vector Stores (RAGSharp.Stores)

| Store                | Use Case                                                                 |
|----------------------|--------------------------------------------------------------------------|
| `IVectorStore`         | 4-method interface for any database (Postgres, Qdrant, Redis, etc.)      |
| `InMemoryVectorStore`  | Fastest for demos, testing, and short-lived processes.                   |
| `FileVectorStore`      | Persistent, file-backed storage (uses JSON), perfect for retaining your indexed knowledge base between runs. |

#### Why no built-in database support?
- Most RAG use cases work fine with file storage (small knowledge bases)
- Database needs vary (Postgres vs Qdrant vs Redis)
- Implement ```IVectorStore``` in ~50 lines for your database
- Keeps core library lightweight

### 5. Retriever (RAGSharp.RAG)
The RagRetriever orchestrates the entire pipeline:

- Accepts new documents.
- Chunks them using ```ITextSplitter```.
- Sends chunks to the ```IEmbeddingClient```.
- Stores the resulting vectors in the ```IVectorStore```.
- Performs semantic search (dot product/cosine similarity) against the store based on a query.
- Optional: Pass an ```ILogger``` to track ingestion progress, by defualt it uses ```ConsoleLogger```


## 📚 Examples & Documentation
Clone the repo and run the sample app.
```
git clone https://github.com/mrrazor22/ragsharp
cd ragsharp/SampleApp
dotnet run
```

| Example              | Description                                                                 |
|----------------------|--------------------------------------------------------------------------------|
| `Example1_QuickStart`  | The basic 4-step process: Load File → Init Retriever → Add Docs → Search.   |
| `Example2_FilesSearch` | Loading multiple documents from a directory with persistent storage.        |
| `Example3_WebDocSearch`| Loading content from the web (UrlLoader and WebSearchLoader).               |
| `Example4_Barebones`   | Using the low-level API: manual embedding and cosine similarity.            |
| `Example5_Advanced`    | Full pipeline customization, injecting custom splitter, logger, and persistent store. |


### 📂 Project Structure
```
RAGSharp/
├── Embeddings/        (IEmbeddingClient, ITokenizer, providers)
├── IO/                (FileLoader, DirectoryLoader, UrlLoader, WebSearchLoader)
├── RAG/               (RagRetriever, IRagRetriever)
├── Stores/            (InMemoryVectorStore, FileVectorStore)
├── Text/              (RecursiveTextSplitter, ITextSplitter)
├── Logging/           (ConsoleLogger)
└── Utils/             (HashingHelper, VectorExtensions)

SampleApp/
├── data/              (sample.txt, documents/)
└── Examples/          (Example1..Example5)

```

## 📜 License
MIT License.

## 💡 Status
Early stage, suitable for prototypes and production apps where simplicity matters.