import subprocess
import time
from sense_hat import SenseHat

time_step = 30

sense = SenseHat()
sense.clear()

while True:
    temp = sense.get_temperature()
    cpu_read = subprocess.check_output("vcgencmd measure_temp", shell=True)
    cpu_temp = float(cpu_read.decode().split("=")[1].split("'")[0])
    temp_c = temp - ((cpu_temp - temp)/5.466)
    temp_h = sense.get_temperature_from_humidity()
    temp_p = sense.get_temperature_from_pressure()
    print("Temp from temp sensor: ", temp)
    print("Temp from humidity sensor: ", temp_h)
    print("Temp from pressure sensor: ", temp_p)
    print("CPU temp: ", cpu_temp)
    print("Calibrated temp from temp sensor: ", temp_c)

    time.sleep(time_step)
