---
title: Financial Product Comparer Plugin
description: AI plugin that compares a financial product with what is available on the market.
weight: 3
categories: [Semantic Kernel, OpenAI]
---

Use this plugin to compare a given financial product with others available in the market by retrieving information via Bing.

To test this plugin, send a `POST` request to its REST API at `/plugins/compare` with the following request body:

- `product`: the name of the financial product to compare
- `queryPrompt`: the query used to retrive comparable products from Bing

Upon successful execution, OpenAI will return a response in natural language with the comparison description of the provided product with the information found on Bing using the also provided query.

Upon successful execution, OpenAI will return a response in natural language, providing a detailed comparison of the specified product with the information retrieved from Bing using the provided query.
