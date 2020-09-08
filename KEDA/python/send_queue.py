#!/usr/bin/env python


# pylint: disable=C0111

import os
from time import strptime
from azure.servicebus import QueueClient, Message
import json
from datetime import datetime

CONNECTION_STR = os.environ['ServiceBusConnectionString']
QUEUE_NAME = "inqueue"
MESSAGE_NUM = 100

def get_datetime_as_string():
    return datetime.now().strftime("%Y-%m-%d %H:%M:%S")


url_message = {'URL': 'amazon.com', 'ts': get_datetime_as_string()}

def send_single_message(sender):
    message = Message(json.dumps(url_message))
    sender.send(message)


def send_a_list_of_messages(sender):
    url_message = {'URL': 'twitter.com', 'ts': get_datetime_as_string()}
    messages = [Message(json.dumps(url_message)) for _ in range(MESSAGE_NUM)]
    sender.send(messages)


def send_batch_message(sender):
    url_message = {'URL': 'google.com', 'ts': get_datetime_as_string()}
    batch_message = sender.create_batch()
    for _ in range(MESSAGE_NUM):
        try:
            batch_message.add(Message(json.dumps(url_message)))
        except ValueError:
            # BatchMessage object reaches max_size.
            # New BatchMessage object can be created here to send more data.
            break
    sender.send(batch_message)


queue_client = QueueClient.from_connection_string(CONNECTION_STR, QUEUE_NAME)
send_a_list_of_messages(queue_client)

# send_single_message(sender)
# send_a_list_of_messages(sender)
# send_batch_message(sender)
print("Send message is done.")
