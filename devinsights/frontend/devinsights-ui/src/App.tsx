import { Routes, Route, NavLink, Navigate } from 'react-router-dom';
import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import { loginRequest } from './config/msalConfig';
import Dashboard from './pages/Dashboard';
import DeveloperProfile from './pages/DeveloperProfile';
import Repositories from './pages/Repositories';
import AnalysisRuns from './pages/AnalysisRuns';
import './App.css';

function App() {
  const isAuthenticated = useIsAuthenticated();
  const { instance } = useMsal();
  const handleLogin = () => instance.loginPopup(loginRequest).catch(console.error);
  const handleLogout = () => instance.logoutPopup().catch(console.error);

  if (!isAuthenticated) {
    return (
      <div className="login-container">
        <div className="login-card">
          <h1>🔍 DevInsights</h1>
          <p>Developer Productivity Analytics Dashboard</p>
          <button className="btn-primary" onClick={handleLogin}>Sign in with Microsoft</button>
        </div>
      </div>
    );
  }

  return (
    <div className="app-layout">
      <nav className="sidebar">
        <div className="sidebar-header"><h2>🔍 DevInsights</h2></div>
        <ul className="nav-list">
          <li><NavLink to="/" end className={({ isActive }) => isActive ? 'active' : ''}>📊 Dashboard</NavLink></li>
          <li><NavLink to="/repositories" className={({ isActive }) => isActive ? 'active' : ''}>📁 Repositories</NavLink></li>
          <li><NavLink to="/analysis" className={({ isActive }) => isActive ? 'active' : ''}>⚡ Analysis Runs</NavLink></li>
        </ul>
        <div className="sidebar-footer"><button className="btn-secondary" onClick={handleLogout}>Sign out</button></div>
      </nav>
      <main className="main-content">
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/developer/:id" element={<DeveloperProfile />} />
          <Route path="/repositories" element={<Repositories />} />
          <Route path="/analysis" element={<AnalysisRuns />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
