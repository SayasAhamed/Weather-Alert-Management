import React, { useState, useEffect } from 'react';
import axios from 'axios';
import {
  WiDaySunny, WiCloud, WiRain, WiThunderstorm, WiSnow, WiFog,
  WiStrongWind, WiRaindrops, WiThermometer, WiBarometer, WiHumidity,
  WiSunrise, WiSunset
} from 'react-icons/wi';

const ICON_MAP = {
  Thunderstorm: WiThunderstorm,
  Drizzle: WiRain,
  Rain: WiRain,
  Snow: WiSnow,
  Mist: WiFog,
  Smoke: WiFog,
  Haze: WiFog,
  Dust: WiFog,
  Fog: WiFog,
  Sand: WiFog,
  Ash: WiFog,
  Squall: WiStrongWind,
  Tornado: WiStrongWind,
  Clear: WiDaySunny,
  Clouds: WiCloud
};

const WeatherAlert = () => {
  const [city, setCity] = useState('Colombo');
  const [data, setData] = useState(null);
  const [alerts, setAlerts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState(null);

  const fetchDetails = async (queryCity) => {
    setLoading(true);
    setErr(null);
    try {
      const res = await axios.get(
        `http://localhost:5000/api/weather/details?city=${encodeURIComponent(queryCity)}`
      );
      setData(res.data);
      setAlerts(res.data.alerts || []);
    } catch (e) {
      console.error(e);
      setErr('Failed to fetch weather data. Please check the city name or try again.');
      setData(null);
      setAlerts([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchDetails(city); }, []); // initial

  const onSubmit = (e) => {
    e.preventDefault();
    if (city && city.trim()) fetchDetails(city.trim());
  };

  const CurrentIcon = data ? (ICON_MAP[data.current?.main] || WiCloud) : WiCloud;

  return (
    <div className="wx-root">
      <header className="wx-header">
        <h1>Weather Alert Management</h1>
        <form onSubmit={onSubmit} className="wx-search">
          <input
            type="text"
            value={city}
            onChange={(e) => setCity(e.target.value)}
            placeholder="Search city (e.g., Colombo, London)"
            aria-label="Search city"
          />
          <button type="submit">Search</button>
        </form>
      </header>

      {loading && <div className="wx-loading">Loading…</div>}
      {err && <div className="wx-error">{err}</div>}

      {data && (
        <>
          <section className="wx-hero card">
            <div className="wx-hero-main">
              <div className="wx-hero-icon bounce">
                <CurrentIcon size={80} />
              </div>
              <div className="wx-hero-text">
                <h2>
                  {data.current.city}, {data.current.country}
                </h2>
                <div className="wx-hero-temp">
                  {Math.round(data.current.temp)}°C
                  <span className="feels">feels {Math.round(data.current.feels_like)}°C</span>
                </div>
                <div className="wx-hero-desc">{data.current.description}</div>
              </div>
            </div>

            <div className="wx-details-grid">
              <Detail label="Humidity" value={`${data.current.humidity}%`} icon={<WiHumidity size={28} />} />
              <Detail label="Pressure" value={`${data.current.pressure} hPa`} icon={<WiBarometer size={28} />} />
              <Detail label="Wind" value={`${Math.round((data.current.wind_speed ?? 0) * 3.6)} km/h`} icon={<WiStrongWind size={28} />} />
              <Detail label="Clouds" value={`${data.current.cloudiness}%`} icon={<WiCloud size={28} />} />
              <Detail label="Visibility" value={`${Math.round((data.current.visibility ?? 0)/1000)} km`} icon={<WiFog size={28} />} />
              <Detail label="Condition" value={data.current.main} icon={<WiThermometer size={28} />} />
            </div>

            <div className="wx-sun">
              <div><WiSunrise size={28} /> <span>Sunrise</span> <strong>{fmtTime(data.current.sunrise)}</strong></div>
              <div><WiSunset size={28} /> <span>Sunset</span> <strong>{fmtTime(data.current.sunset)}</strong></div>
            </div>

            {alerts?.length > 0 && (
              <div className="wx-alerts">
                {alerts.map((a, i) => (
                  <div key={i} className="badge">{a}</div>
                ))}
              </div>
            )}
          </section>

          <section className="wx-forecast">
            <h3>5-Day Forecast</h3>
            <div className="forecast-row">
              {data.forecast.map((d) => {
                const Icon = ICON_MAP[d.main] || WiCloud;
                return (
                  <div className="forecast-card card" key={d.date}>
                    <div className="date">{fmtDate(d.date)}</div>
                    <div className="icon"><Icon size={48} /></div>
                    <div className="desc">{d.description}</div>
                    <div className="temps">
                      <span className="max">{Math.round(d.max)}°</span>
                      <span className="min">{Math.round(d.min)}°</span>
                    </div>
                    <div className="pop"><WiRaindrops size={20} /> {Math.round(d.pop)}%</div>
                  </div>
                );
              })}
            </div>
          </section>
        </>
      )}
    </div>
  );
};

const Detail = ({ label, value, icon }) => (
  <div className="detail">
    <div className="icn">{icon}</div>
    <div className="lbl">{label}</div>
    <div className="val">{value ?? '—'}</div>
  </div>
);

function fmtTime(iso) {
  if (!iso) return '—';
  const d = new Date(iso);
  return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function fmtDate(yyyyMMdd) {
  const [y,m,dd] = yyyyMMdd.split('-').map(Number);
  const d = new Date(Date.UTC(y, m-1, dd));
  return d.toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' });
}

export default WeatherAlert;
