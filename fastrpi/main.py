from fastapi import FastAPI
import test as t

app = FastAPI()

@app.get("/")
async def root():
    return t.test()

@app.get("/temperature")
async def read_temperature():
    return t.temperature()

@app.get("/pressure")
async def read_pressure():
    return t.pressure()

@app.get("/humidity")
async def read_humidity():
    return t.humidity()