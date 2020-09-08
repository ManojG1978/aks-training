import logging
import json
import os
import time
from azure.servicebus import QueueClient, Message

try:
    connection_string = os.environ['ServiceBusConnectionString']
    in_queue_name = os.environ['IN_QUEUE_NAME']
    out_queue_name = os.environ['OUT_QUEUE_NAME']
    sleep_time = int(os.environ['SLEEP_TIME'])
except KeyError:
    print('Error: missing environment variable ServiceBusConnectionString, IN_QUEUE_NAME, OUT_QUEUE_NAME or SLEEP_TIME')
    exit(1)

# Create the QueueClient
in_queue_client = QueueClient.from_connection_string(connection_string, in_queue_name)

# Receive the message from the queue
with in_queue_client.get_receiver() as queue_receiver:
    messages = queue_receiver.fetch_next(timeout=3)
    for message in messages:
        out_queue_client = QueueClient.from_connection_string(connection_string, out_queue_name)

        url_dict = json.loads(str(message))
        url_dict["processed"] = "true"

        message.complete()

        out_message = Message(json.dumps(url_dict))
        out_queue_client.send(out_message)

        print(f"Processed input message and posted enriched message for URL {url_dict['URL']}")
# Sleep for a while, simulating a long-running job
time.sleep(sleep_time)