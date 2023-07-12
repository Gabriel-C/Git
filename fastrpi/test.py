from sense_hat import SenseHat
from time import sleep

sense = SenseHat()

def test():
    return "Hello world"

def temperature():
    temp = sense.get_temperature()
    return temp

def pressure():
    pressure = sense.get_pressure()
    return pressure

def humidity():
    humidity = sense.get_humidity()
    return humidity

def show_message(message):
    sense.clear()
    sense.show_message(message)
    sleep(2)
    sense.clear()
