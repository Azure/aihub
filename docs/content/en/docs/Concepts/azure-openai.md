---
title: Azure OpenAI
date: 2023-10-06
description: >
  The core of the Generative AI solution. 
categories: [Azure]
tags: [docs, openai]
weight: 2
---

Azure OpenAI Service provides REST API access to OpenAI's powerful language models including the GPT-4, GPT-35-Turbo, and Embeddings model series. In addition, the new GPT-4 and gpt-35-turbo model series have now reached general availability. These models can be easily adapted to your specific task including but not limited to content generation, summarization, semantic search, and natural language to code translation. Users can access the service through REST APIs, Python SDK, or our web-based interface in the Azure OpenAI Studio.

Important concepts about Azure OpenAI:

* [Azure OpenAI Studio](https://oai.azure.com)
* **Models available**
  * GPT-35-Turbo series: typical "chatGPT" model, recommended for most of the Azure OpenAI projects. When we might need more capability, GPT4 can me considered (take into account it will imply more latency and cost) 
  * GPT-4 series: they are the most advanced language models, available once you fill in the [GPT4 Request Form](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xURjE4QlhVUERGQ1NXOTlNT0w1NldTWjJCMSQlQCN0PWcu) 
  * Embeddings series: embeddings make it easier to do machine learning on large inputs representing words by capturing the semantic similarities in a vector space. Therefore, you can use embeddings to determine if two text chunks are semantically related or similar, and provide a score to assess similarity.
  
  Take into account that not all models are available in all Azure Regions, for Regional availability check the documentation: [Model summary table and region availability](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#model-summary-table-and-region-availability)

* **Deployment**: once you instantiate a specific model, it will be available as deployment. You can create and delete deployments of available models as you wish. This is managed through the AOAI Studio. 
* **Quotas**: the quotas available in Azure are allocated per model and per region, within a subscription. [Learn more about quotas](https://learn.microsoft.com/en-us/azure/ai-services/openai/quotas-limits). In the documentation you can find best practices to manage your quota.


**AI Hub** uses Azure OpenAI Embeddings model to vectorize the content and ChatGPT model to conversate with that content. 

More information at the official documentation: [What is Azure OpenAI](https://learn.microsoft.com/en-us/azure/ai-services/openai/overview)
