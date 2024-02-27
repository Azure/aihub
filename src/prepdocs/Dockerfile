FROM mcr.microsoft.com/devcontainers/python:3.10

RUN python3 -m venv ./scripts/.venv

COPY ./ ./scripts/

RUN chmod +x ./scripts/prepdocs.sh

RUN ./scripts/.venv/bin/python -m pip install -r ./scripts/requirements.txt

ENTRYPOINT ["./scripts/prepdocs.sh"]
