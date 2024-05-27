---
title: OpenAI Plugins
description: AI Hub &mdash; OpenAI plugins examples.
weight: 3
---

OpenAI Plugins are powerful tools that connect systems (like ChatGPT) to third-party applications. Think of a plugin like a new skill you teach to your phone or computer. It’s a mini-program that lets your software do something new, like asking a chatbot questions. These plugins enable AI applications powered by LLMs to interact with APIs defined by developers, enhancing their capabilities and allowing it to perform a wide range of actions beyond the training of the inherent AI model.

On the other hand, Microsoft’s Semantic Kernel is an open-source SDK that lets you easily build AI-related projects, especially OpenAI plugins. It provides the following features:

1. **Interoperability**: Semantic Kernel has adopted the OpenAI plugin specification as the standard for plugins. This creates an ecosystem of interoperable plugins that can be used across all major AI apps and services like ChatGPT, Bing, and Microsoft 365. Any plugins you build with Semantic Kernel can be exported, so they are usable in these platforms.

2. **Importing Plugins**: Semantic Kernel makes it easy to import plugins defined with an OpenAI specification. Your code can use Semantic Kernel to leverage these plugins into your application.

3. **Native and Prompts Functions**: In any plugin, you can create two types of functions: prompts and native functions. With native functions, you can use C# or Python code to directly build features to manipulate data or perform other operations beyond the capabilities of an LLM; and with prompts, you can create reusable semantic instructions for a wide variaety of LLMs, not just OpenAI, with templates to handle variables, conditionals and loops.
