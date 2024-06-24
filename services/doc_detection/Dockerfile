FROM python:3.11-slim

WORKDIR /app

RUN apt-get update && apt-get install -y \
    libgl1-mesa-glx \
    libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install -r requirements.txt

COPY . /app

# Установка переменной окружения PYTHONPATH
ENV PYTHONPATH "${PYTHONPATH}:/app"

CMD ["python", "src/main.py"]