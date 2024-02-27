#!/bin/sh

echo 'Running "prepdocs.py"'
./scripts/.venv/bin/python ./scripts/prepdocs.py '/data/*' --storageaccount "$AZURE_STORAGE_ACCOUNT" --container "$AZURE_STORAGE_CONTAINER" --searchservice "$AZURE_SEARCH_SERVICE" --openaihost "$OPENAI_HOST" --openaiservice "$AZURE_OPENAI_SERVICE" --openaikey "$OPENAI_API_KEY" --openaiorg "$OPENAI_ORGANIZATION" --openaideployment "$AZURE_OPENAI_EMB_DEPLOYMENT" --openaimodelname "$AZURE_OPENAI_EMB_MODEL_NAME" --index "$AZURE_SEARCH_INDEX" --formrecognizerservice "$AZURE_FORMRECOGNIZER_SERVICE" --tenantid "$AZURE_TENANT_ID" --localpdfparser -v
