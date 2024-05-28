---
title: Call Transcript Plugin
description: AI plugin that analyzes a call given the transcript
weight: 2
categories: [Semantic Kernel, OpenAI]
tags: [openai, plugins, semantic-kernel]
---

Use this plugin to analyze a call using the provided transcript. It offers functionality that is quite similar to the use case example of [Call Center Analytics]({{< ref "/docs/Use Cases/call-center-analytics.md" >}}).

To test this plugin, send a `POST` request to its REST API at `/plugins/transcript` with the following request body:

- `transcript`: the text with the call transcript.

Upon successful execution, OpenAI will return a response with the following information extracted and formatted as bullet points:

- Reason for the call
- Name of the agent
- Name of the caller
- Caller's sentiment
- A summary of the call

Please note that the information provided may vary depending on the content of the supplied call transcript.
