version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: src/QuokkaPack.API/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_URLS=http://+:8080

  web:
    build:
      context: .
      dockerfile: src/QuokkaPack.Razor/Dockerfile
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_URLS=http://+:80
    depends_on:
      - api
