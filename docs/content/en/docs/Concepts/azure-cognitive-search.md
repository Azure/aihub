---
title: Azure Cognitive Search
date: 2023-10-06
description: >
  Used to build a rich search experience over private, heterogeneous content in web, mobile, and enterprise applications.
categories: [Azure]
tags: [docs, cognitive-search]
weight: 4
---

Azure Cognitive Search (formerly known as "Azure Search") is a cloud search service that gives developers infrastructure, APIs, and tools for building a rich search experience over private, heterogeneous content in web, mobile, and enterprise applications.

Search is foundational to any app that surfaces text to users, where common scenarios include catalog or document search, online retail apps, or data exploration over proprietary content. When you create a search service, you'll work with the following capabilities:

* A search engine for full text and vector search over a search index containing user-owned content
* Rich indexing, with lexical analysis and optional AI enrichment for content extraction and transformation
* Rich query syntax for vector queries, text search, fuzzy search, autocomplete, geo-search and more
* Programmability through REST APIs and client libraries in Azure SDKs
* Azure integration at the data layer, machine learning layer, and AI (Azure AI services)

![Imagen arquitectura Azure Cognitive Search](https://learn.microsoft.com/en-us/azure/search/media/search-what-is-azure-search/azure-search-diagram.svg)

**AI Hub** uses Azure Cognitive Search to serve an index of vectorized content, that will be used by our LLM (ChatGPT) to respond to user's query.

## Document Indexing Process

AI Hub implements a comprehensive document indexing pipeline that transforms your documents into searchable, vectorized content. This process enables powerful semantic search capabilities and allows the LLM to provide accurate, contextually relevant responses based on your data.

### Overview of the Indexing Pipeline

The document indexing process in AI Hub consists of several key stages:

1. **Document Upload and Storage**
2. **Text Extraction and Analysis** 
3. **Content Chunking and Sectioning**
4. **Vector Embedding Generation**
5. **Search Index Population**

### Step-by-Step Process

#### 1. Document Upload and Storage
- Documents are uploaded to Azure Blob Storage for secure and scalable storage
- PDF documents are automatically split into individual pages, with each page stored as a separate blob
- Other document formats are stored as single blobs
- Each blob is named using a consistent pattern: `filename-page.pdf` for PDFs or `filename.ext` for other formats

#### 2. Text Extraction and Analysis
- **Azure Form Recognizer (Document Intelligence)** is used to extract text while preserving document structure
- The service analyzes document layout, including tables, forms, and text positioning
- Tables are converted to HTML format to maintain their structure and relationships
- Text extraction handles various document formats including PDFs, images, and structured documents

#### 3. Content Chunking and Sectioning
- Extracted text is intelligently split into smaller, manageable sections
- **Section Parameters:**
  - Maximum section length: 1,000 characters
  - Section overlap: 100 characters (to preserve context across boundaries)
  - Sentence boundary detection to avoid breaking mid-sentence
- Each section includes metadata:
  - Unique section ID
  - Source page reference
  - Source file name
  - Content category

#### 4. Vector Embedding Generation
- Each text section is converted to vector embeddings using **OpenAI's text-embedding-ada-002 model**
- Embeddings are 1,536-dimensional vectors that capture semantic meaning
- **Batch Processing:** Multiple sections are processed together for efficiency
- Rate limiting and retry logic handle API throttling gracefully

#### 5. Search Index Population
The Azure Cognitive Search index is created with the following schema:

| Field | Type | Description | Properties |
|-------|------|-------------|------------|
| `id` | String | Unique identifier for each section | Key field |
| `content` | String | The actual text content | Searchable, analyzed with Microsoft English analyzer |
| `embedding` | Collection(Single) | 1,536-dimensional vector | Searchable, used for vector search |
| `category` | String | Document category | Filterable, facetable |
| `sourcepage` | String | Source page reference | Filterable, facetable |
| `sourcefile` | String | Original filename | Filterable, facetable |

#### Search Capabilities
The index supports multiple search methods:
- **Vector Search:** Uses HNSW (Hierarchical Navigable Small World) algorithm with cosine similarity
- **Semantic Search:** Leverages semantic configurations for improved relevance
- **Hybrid Search:** Combines traditional text search with vector similarity
- **Filtered Search:** Allows filtering by source file, page, or category

### Configuration and Customization

The indexing process can be customized through various parameters:
- **Chunk size and overlap** can be adjusted based on document types
- **Embedding models** can be switched (though text-embedding-ada-002 is recommended)
- **Categories** can be assigned to organize documents
- **Batch processing** optimizes for throughput while respecting API limits

### Implementation

The document indexing pipeline is implemented in the `prepdocs.py` script, which:
- Handles authentication with Azure services
- Manages document processing workflows
- Implements retry logic for resilience
- Provides verbose logging for monitoring
- Supports both individual and batch processing modes

This robust indexing process ensures that your documents are transformed into a searchable, semantically-aware knowledge base that powers AI Hub's intelligent chat capabilities.

Learn more at the official documentation: [What is Azure Cognitive Search?](https://learn.microsoft.com/en-us/azure/search/search-what-is-azure-search).

Learning Path:[Implement knowledge mining with Azure Cognitive Search](https://learn.microsoft.com/en-us/training/paths/implement-knowledge-mining-azure-cognitive-search/)
