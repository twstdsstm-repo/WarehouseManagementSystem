# 🏭 Warehouse Management System

![Docker](https://img.shields.io/badge/Docker-2CA5E0?style=flat&logo=docker&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat&logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-20232A?style=flat&logo=react&logoColor=61DAFB)

Система управления складом с полноценным API (C# .NET 8) и веб-интерфейсом (React). Проект упакован в Docker-контейнеры для простого развертывания.

---

## 🔧 Tech Stack: C# | ASP.NET | Entity Framework Core | React | Postgresql | Docker

## 📦 Системные требования

- [Docker](https://www.docker.com/products/docker-desktop) (версия 24.0+)
- Для разработки дополнительно:
  - [.NET 8 SDK](https://dotnet.microsoft.com/download)
  - [Node.js 20.x](https://nodejs.org/)

---

## 🚀 Запуск проекта

### 1. Клонирование репозитория
```bash
git clone https://github.com/twstdsstm-repo/WarehouseManagmentSystem.git
cd WarehouseManagmentSystem
```

### 2. Запуск Docker
Чтобы запустить проект, выполните следующую команду в терминале:
```bash
docker-compose up --build
```
Процесс сборки создаст два контейнера:

- Backend (C# .NET 8 API)
- Frontend (React)

По умолчанию ваш проект будет доступен по следующим адресам:

- API: http://localhost:5030
- Frontend: http://localhost:3000

---

## 🔧 Если потребуется убрать зависимость от Docker/Изменить логин и пароль от БД

Потребуется отдельно запустить сервер/клиента, можно сделать это путем использования терминала и командной строки.
### 1. Измение строки подключения в ./Warehouse.API/appsettings.json

Нужно изменить строку "DefaultConnection" на :
```bash
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=5432;database=warehouse_db;username=your_username;password=your_password"
  }
```

### 2. Запуск для Backend

Создаем зависимость, запускаем проект, в одной командной строке.

```bash
cd .\backend\
cd .\Warehouse.API\
dotnet restore
dotnet run
```

### 3. Запуск для Frontend

Запускаем клиента уже в терминале/другой командной строке.

```bash
cd .\frontend\
npm install
npm start
```

---