---
title: Azure Functions
date: 2023-10-06
description: >
  Serveles application used to bacth-process documents and create embeddings.
categories: [Azure]
tags: [docs, functions, serverless]
weight: 6
---

Azure Functions is a serverless solution that allows you to write less code, maintain less infrastructure, and save on costs. Instead of worrying about deploying and maintaining servers, the cloud infrastructure provides all the up-to-date resources needed to keep your applications running.

Functions provides a comprehensive set of event-driven triggers and bindings that connect your functions to other services without having to write extra code. You focus on the code that matters most to you, in the most productive language for you, and Azure Functions handles the rest. 

**Activate GenAI with Azure** uses Azure Function to create chunks of the documents text and create embeddings to be added to the Azure Cognitive Search index. 

Learn more about Azure Functions: [What is Azure Function?](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview?pivots=programming-language-csharp). For the best experience with the Functions documentation, choose your preferred development language from the list of native Functions languages at the top of the article.