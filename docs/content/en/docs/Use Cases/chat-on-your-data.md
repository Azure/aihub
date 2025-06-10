---
title: Chat on your Data
date: 2023-11-16
description: >
  Chat interface that can answer user queries with relevant documents, suggested follow-up questions, and citations, based on your own data.
categories: [Azure, OpenAI]
tags: [docs, cognitive-search]
weight: 1
---

Feel the power of artificial intelligence and cloud computing to provide a smart and scalable document search and retrieval solution. The solution uses Azure OpenAI, Cognitive Search, Container Apps, Application Insights, and Azure API Management to create a chat interface that can answer user queries with relevant documents, suggested follow-up questions, and citations. The solution also allows users to upload custom data files and perform vector search using semantic or hybrid methods. Additionally, the solution supports extensibility through plugins, charge back functionality, security features such as authentication and authorization, monitoring capabilities, and scalability options.

![Chat on your Data screenshot](/aihub/img/chatonyourdata.jpg)

**AI Hub** uses Azure Cognitive Search to serve an index of vectorized content, that will be used by our LLM (ChatGPT) to respond to user's query.

## How It Works

When you upload documents to AI Hub, they go through a comprehensive indexing process:

1. **Document Processing**: Files are uploaded to Azure Blob Storage and processed using Azure Form Recognizer to extract text while preserving structure
2. **Content Chunking**: Text is intelligently split into sections with overlap to maintain context
3. **Vectorization**: Each section is converted to embeddings using OpenAI's embedding models
4. **Indexing**: Content and vectors are stored in Azure Cognitive Search for fast retrieval

This process enables the system to understand your documents semantically and provide accurate, contextual responses to your queries.

For detailed information about the document indexing process, see the [Azure Cognitive Search concepts documentation](/aihub/docs/concepts/azure-cognitive-search/#document-indexing-process).

Learn more at the official documentation: [What is Azure Cognitive Search?](https://learn.microsoft.com/en-us/azure/search/search-what-is-azure-search).
