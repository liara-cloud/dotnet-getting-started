
# Blog Website For Test

## Installation


```bash
  git clone https://github.com/liara-cloud/dotnet-getting-started.git
```
```bash
  cd dotnet-getting-started
```
```bash
  git checkout blog
```
```bash
  cp .env.example .env
```
- if you're using windows, just rename .env.example to .env
```bash
  touch wwwroot/images
```
```bash
  touch data/demo.sqlite
```
- if you're using windows, create images folder in wwwroot directory and data/demo.sqlite in root 
- if you're in development Env:
```bash
  dotnet watch 
```
- if you're in production Env:
```bash
  dotnet run
```
