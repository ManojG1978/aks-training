FROM python:3.7

WORKDIR /app

RUN pip install azure.servicebus

COPY process_sb_message.py /app

CMD ["python", "process_sb_message.py"]