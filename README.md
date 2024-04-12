# AI Hub

Learn more at the official documentation: [AI Hub](https://azure.github.io/aihub/)

## High-level Architecture

The following diagram shows the high-level architecture of the **AI Hub** solution:

![High-level Architecture](/docs/static/img/AI-Hub-HLD.png)

## Uses Cases

### Chat on your own data
Feel the power of artificial intelligence and cloud computing to provide a smart and scalable document search and retrieval solution. The solution uses Azure OpenAI, Cognitive Search, Container Apps, Application Insights, and Azure API Management to create a chat interface that can answer user queries with relevant documents, suggested follow-up questions, and citations. The solution also allows users to upload custom data files and perform vector search using semantic or hybrid methods. Additionally, the solution supports extensibility through plugins, charge back functionality, security features such as authentication and authorization, monitoring capabilities, and scalability options.

**AI Hub** uses Azure Cognitive Search to serve an index of vectorized content, that will be used by our LLM (ChatGPT) to respond to user's query.

Learn more at the official documentation: [What is Azure Cognitive Search?](https://learn.microsoft.com/en-us/azure/search/search-what-is-azure-search).

### Call Center Analytics
Analyze call center call trancripts (that might come from your Call Center technology, or having transcribed with Azure Speech Services).

Use the predefined template to analyze the call center call transcript, generate a new one, and customize the query to analyze the transcript of your call center.

### Image Analyzer
Analyze your image using GPT4 and Azure Vision Services.
Upload an image and the Image Analyzer will analyze it using Azure Vision Services formats supported .jpg, .png

### Brand Analyzer
Analyze your brand's internet reputation by inserting the name of the company.

Just enter the name of the company and the Brand Analyzer will search in Bing for mentions of the company and analyze the sentiment of the mentions.
You can also search for a specific product or service of the company just modifiying thr promt custumizing the query.

### Form Analyzer
Analyze and chat with your documents using GPT4 and Azure Document Intelligence.

Just upload a .pdf document and the Form Analyzer will extract the text and analyze it with Azure Document Intelligence, and then you can chat with the document using GPT4.
You can also modify the prompt to extract cusntom information from the document. 

### Document Comparison
Compare different versions of your documents with the powerful combination of GPT-4 and Azure Document Intelligence.

Just upload two .pdf documents to extract the content using OCR capabilities of Azure Document Intelligence, and then you can ask the differences between both documents with the power of GPT4.

### Content Safety

In today's digital age, online platforms are increasingly becoming hubs for user-generated content, ranging from text and images to videos. While this surge in content creation fosters a vibrant online community, it also brings forth challenges related to content moderation and ensuring a safe environment for users. Azure AI Content Safety offers a robust solution to address these concerns, providing a comprehensive set of tools to analyze and filter content for potential safety risks.

Use Case Scenario: **Social Media Platform Moderation**

Consider a popular social media platform with millions of users actively sharing diverse content daily. To maintain a positive user experience and adhere to community guidelines, the platform employs Azure AI Content Safety to automatically moderate and filter user-generated content.

**Image Moderation:**
Azure AI Content Safety capabilities are leveraged to analyze images uploaded by users. The system can detect and filter out content that violates community standards, such as explicit or violent imagery. This helps prevent the dissemination of inappropriate content and ensures a safer environment for users of all ages.

**Text Moderation:**
The Text Moderator is employed to analyze textual content, including comments, captions, and messages. The platform can set up filters to identify and block content containing hate speech, harassment, or other forms of harmful language. This not only protects users from offensive content but also contributes to fostering a positive online community.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

### Thanks to everyone who has contributed!

<a href="https://github.com/Azure/aihub/graphs/contributors">
  <img src="https://contributors-img.web.app/image?repo=Azure/aihub" />
</a>

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
