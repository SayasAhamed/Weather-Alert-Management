# 🌦️ Weather Alert Management System

A modern **full-stack weather monitoring and alert system** built using **React (Frontend)** and **ASP.NET Core Web API (Backend)**.
This application provides real-time weather insights, smart alerts, and a clean interactive dashboard.

---

## 📌 Overview

The **Weather Alert Management System** allows users to:

* 🌍 Search any city worldwide
* 📊 View detailed current weather conditions
* ⚠️ Receive intelligent weather alerts (storm, rain, heat, wind, etc.)
* 📅 Analyze a **5-day forecast**
* 🎨 Experience a modern, responsive UI

---


## ⚙️ Tech Stack

### 🖥️ Frontend

* ⚛️ React.js
* 🎨 Custom CSS (modern UI design) 
* 📦 Axios (API requests) 
* 🎯 React Icons (weather icons) 

### 🧠 Backend

* ASP.NET Core Web API
* OpenWeatherMap API integration 
* JSON processing with Newtonsoft

---

## 🚀 Features

### 🌤️ Real-Time Weather Data

* Temperature, humidity, pressure
* Wind speed, visibility, cloudiness
* Sunrise & sunset times
<br>
<div>
   <img src="Screenshots\1.png">
   <img src="Screenshots\2.png">
   <img src="Screenshots\3.png">
</div>
<br>

### ⚠️ Smart Alert System

Automatically generates alerts such as:

* Thunderstorm Warning
* Rain Advisory
* Heat Advisory
* Wind Advisory
* Freeze Warning

👉 Based on backend logic 

---

### 📅 5-Day Forecast

* Daily min/max temperature
* Weather conditions
* Rain probability (%)

---

### 🎨 Interactive UI

* Dynamic weather icons (based on condition) 
* Smooth animations
* Responsive layout
* Clean dashboard design

---

## 🧩 Project Architecture

```id="8x3qpl"
Frontend (React)
   ↓ API Calls (Axios)
Backend (ASP.NET Core API)
   ↓
OpenWeatherMap API
```

---

## 📁 Project Structure

```id="x2t9os"
Weather-Alert-Management/
│
├── backend/
│   └── WeatherController.cs
│
├── frontend-react/
│   ├── App.js
│   ├── WeatherAlert.js
│   ├── index.js
│   ├── styles.css
│   └── index.html
│
└── README.md
```

---

## ⚙️ Installation & Setup

### 1️⃣ Clone Repository

```bash id="cfwqne"
git clone https://github.com/SayasAhamed/Weather-Alert-Management.git
cd Weather-Alert-Management
```

---

### 2️⃣ Backend Setup

```bash id="6exs5k"
cd backend
dotnet restore
dotnet run
```

🔑 **Important:**
Add your OpenWeatherMap API key in configuration:

```
OpenWeatherMap:ApiKey=YOUR_API_KEY
```

---

### 3️⃣ Frontend Setup

```bash id="9h3gvl"
cd frontend-react
npm install
npm start
```

🌐 App runs at:

```
http://localhost:3000
```

---

## 🔌 API Endpoints

### 📍 Get Weather Alerts

```
GET /api/weather/alerts?city=Colombo
```

### 📍 Get Full Weather Details

```
GET /api/weather/details?city=Colombo
```

Returns:

* Current weather
* Alerts
* 5-day forecast

---

## 🧠 How It Works

1. User enters a city
2. React frontend sends request via Axios 
3. ASP.NET API fetches data from OpenWeatherMap 
4. Backend processes:

   * Weather conditions
   * Generates alerts
   * Aggregates forecast
5. Frontend displays results beautifully

---

## 📸 Screenshots

> 🔥 Add your app screenshots here (VERY important for portfolio)

---

## 🚧 Future Improvements

* 🔔 Real-time push notifications
* 📱 Mobile-first UI improvements
* 🌐 Deployment (Cloud hosting)
* 📊 Advanced analytics dashboard
* 🧠 AI-based weather predictions

---

## 👨‍💻 Author

**M.M. Sayas Ahamed**
🎓 Undergraduate | BICT
💻 Tech Creator | YouTuber

---

## ⭐ Support

If you like this project:

* ⭐ Star this repository
* 🍴 Fork it
* 🛠️ Contribute

---

## 📄 License

This project is for educational and personal use.
