---
title: Semantic Kernel
date: 2023-10-06
description: >
  An open-source SDK that lets you easily combine AI services like OpenAI, Azure OpenAI, and Hugging Face with conventional programming languages like C# and Python.
categories: [SDK]
tags: [docs, tools]
weight: 8
---

Semantic Kernel is an open-source SDK that lets you easily combine AI services like OpenAI, Azure OpenAI, and Hugging Face with conventional programming languages like C# and Python. By doing so, you can create AI apps that combine the best of both worlds.

Microsoft powers its Copilot system with a stack of AI models and plugins. At the center of this stack is an AI orchestration layer that allows us to combine AI models and plugins together to create brand new experiences for users.

![Image of Copilots](https://learn.microsoft.com/en-us/semantic-kernel/media/copilot-stack.png)

To help developers build their own Copilot experiences on top of AI plugins, we have released Semantic Kernel, a lightweight open-source SDK that allows you to orchestrate AI plugins. With Semantic Kernel, you can leverage the same AI orchestration patterns that power Microsoft 365 Copilot and Bing in your own apps, while still leveraging your existing development skills and investments.

![Image of Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/media/mind-and-body-of-semantic-kernel.png)

## Semantic Kernel and the AI Hub

At present, the **AI Hub** does not use the Semantic Kernel directly in its use case examples. However, it does demonstrate its usage through two specific examples that show how easely is to create OpenAI plugins using Semantic Kernel as an SDK.

Here are the available plugin examples:

1. **Call Analysis**: This plugin uses Semantic Kernel to implement features similar to those demonstrated in the [Call Center Analytics]({{< ref "/docs/Use Cases/call-center-analytics.md" >}}) use case.

2. **Financial Product Comparison**: This plugin employs Semantic Kernel to combine native and prompt functions to compare a specific financial product with others in the market. It combines prompts with the [Web Search Engine Plugin](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.plugins.web.websearchengineplugin), which is further integrated with the [Bing Search connector](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.plugins.web.bing.bingconnector).

For more information, please refer to the [OpenAI Plugins]({{< relref "/docs/OpenAI Plugins" >}}) section. You can also learn more about Semantic Kernel in its [official documentation](https://learn.microsoft.com/en-us/semantic-kernel/overview/).
