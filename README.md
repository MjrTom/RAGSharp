
# RAGSharp: Lightweight RAG Pipeline for .NET

RAGSharp is a lightweight, extensible Retrieval-Augmented Generation (RAG) library built entirely in C#. It provides clean, minimal, and predictable components for building RAG pipelines: document loading, token-aware text splitting, vector storage, and semantic search.

🚀 **Why RAGSharp? The .NET RAG Gap**  
Most modern RAG development is heavily centered around the Python ecosystem (LangChain, LlamaIndex). If you are a C#/.NET developer, your choices for RAG are often restrictive:

- **Microsoft Semantic Kernel**: Feature-rich, but often heavy, Azure-first, and focused on agent orchestration rather than a minimal RAG indexing/retrieval pipeline.
- **External APIs/Services**: Relying on cloud services or awkward Python bindings, which adds friction to pure C# projects.

RAGSharp fills this gap. It gives you a local-first, minimal, pure .NET RAG solution that works out of the box with self-hosted models (like those run via LM Studio) or official OpenAI endpoints.

✅ **Pure C# / .NET**: No Python runtime or heavy dependencies.  
✅ **Local-First Design**: Easy configuration for LM Studio and other local OpenAI-compatible endpoints.  
✅ **Composability**: Minimal code, built on clean interfaces (IVectorStore, IEmbeddingClient).  
✅ **Persistent Storage**: File-based vector storage included by default.

## 📦 Installation 

```bash
dotnet add package RAGSharp
```

## ✨ Quick Start
The goal of RAGSharp is simplicity. You only need to define your embedding model and your storage preference.

This example uses a local embedding model hosted by LM Studio (http://127.0.0.1:1234/v1) and an in-memory vector store.

```csharp

var docs = await new FileLoader().LoadAsync("sample.txt");

var retriever = new RagRetriever(
    embeddings: new OpenAIEmbeddingClient(
        baseUrl: "http://127.0.0.1:1234/v1",
        apiKey: "lmstudio",
        defaultModel: "text-embedding-3-small"
    ),
    store: new InMemoryVectorStore()
);

await retriever.AddDocumentsAsync(docs);

var results = await retriever.Search("quantum mechanics");
foreach (var r in results)
    Console.WriteLine($"{r.Score:F2}: {r.Content}");

```


## ⚙️ Core Features & Architecture
RAGSharp is built around a pluggable architecture, where every major component is an interface. 
Every RAG pipeline in RAGSharp follows this flow. Each part is pluggable (custom loaders, stores, embedding clients).
```
   [ DocumentLoader ] → [ TextSplitter ] → [ Embeddings ] → [ VectorStore ] → [ Retriever ]
```
- **IDocumentLoader** → FileLoader, DirectoryLoader, UrlLoader, WebSearchLoader

- **ITextSplitter** → RecursiveTextSplitter (paragraph → sentence → token windows)

- **IEmbeddingClient** → OpenAIEmbeddingClient (swap for local models easily)

- **IVectorStore** → InMemoryVectorStore, FileVectorStore (persistent)

### 1. Document Loading (RAGSharp.IO)
Loaders fetch raw data and convert it into a list of Document objects.

| Loader          | Description                                      |
|-----------------|--------------------------------------------------|
| FileLoader      | Load a single text file.                         |
| DirectoryLoader | Load all files matching a pattern from a directory. |
| UrlLoader       | Scrapes a web page (cleans HTML, removes noise) and extracts plain text. |
| WebSearchLoader | Searches and loads content directly from Wikipedia articles. |

### 2. Text Splitting (RAGSharp.Text)
The library uses a token-aware Recursive Text Splitter, inspired by best practices in the RAG community, to maintain semantic coherence.

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

- Splits first by large delimiters (e.g., \n\n for paragraphs).
- If chunks are too large, it recursively splits by smaller delimiters (e.g., sentences).
- Final fallback uses a sliding token window with overlap to ensure token-accurate chunks.

This ensures semantically clean chunks without breaking math, code, or paragraphs awkwardly.
Uses SharpToken for accurate GPT-family token counting.

### 3. Embeddings & Tokenization (RAGSharp.Embeddings)

| Component                | Description                                                                 |
|--------------------------|-----------------------------------------------------------------------------|
| OpenAIEmbeddingClient    | The default client. Works with both official OpenAI and LM Studio endpoints. |
| IEmbeddingClient         | Interface for connecting to any provider (Cohere, Google, Azure, etc.).     |
| SharpTokenTokenizer      | Tokenizer for accurate token length and splitting, crucial for RAG performance. |

### 4. Vector Stores (RAGSharp.Stores)

| Store                | Use Case                                                                 |
|----------------------|--------------------------------------------------------------------------|
| InMemoryVectorStore  | Fastest for demos, testing, and short-lived processes.                   |
| FileVectorStore      | Persistent, file-backed storage (uses JSON), perfect for retaining your indexed knowledge base between runs. |

### 5. Retriever (RAGSharp.RAG)
The RagRetriever orchestrates the entire pipeline:

- Accepts new documents.
- Passes them to the ITextSplitter.
- Sends chunks to the IEmbeddingClient.
- Stores the resulting vectors in the IVectorStore.
- Performs semantic search (dot product/cosine similarity) against the store based on a query.


## 📚 Examples & Documentation
Clone the repo and run the sample app.
```
git clone https://github.com/mrrazor22/ragsharp
cd ragsharp/SampleApp
dotnet run
```


| Example              | Description                                                                 |
|----------------------|--------------------------------------------------------------------------------|
| Example1_QuickStart  | The basic 4-step process: Load File → Init Retriever → Add Docs → Search.   |
| Example2_FilesSearch | Loading multiple documents from a directory with persistent storage.        |
| Example3_WebDocSearch| Loading content from the web (UrlLoader and WebSearchLoader).               |
| Example4_Barebones   | Using the low-level API: manual embedding and cosine similarity.            |
| Example5_Advanced    | Full pipeline customization, injecting custom splitter, logger, and persistent store. |


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
RAGSharp is distributed under the MIT License.

## 💡 Status
RAGSharp is currently actively evolving. It is suitable for prototypes, research, and integrating a minimal RAG pipeline into production .NET applications where simplicity and a pure C# stack are priorities.
