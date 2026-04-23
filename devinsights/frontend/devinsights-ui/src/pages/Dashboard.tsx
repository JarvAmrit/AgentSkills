import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell, LineChart, Line } from 'recharts';
import { dashboardApi, DashboardSummary } from '../services/api';

const COLORS = ['#38bdf8', '#818cf8', '#34d399', '#fbbf24', '#f87171', '#a78bfa', '#fb923c', '#e879f9'];

export default function Dashboard() {
  const [data, setData] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => { dashboardApi.getSummary().then(r => setData(r.data)).catch(e => setError(e.message)).finally(() => setLoading(false)); }, []);
  if (loading) return <div className="loading">Loading dashboard...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  if (!data) return null;
  const aiPieData = [{ name: 'AI Work', value: data.aiWorkPercentage }, { name: 'Regular Work', value: 100 - data.aiWorkPercentage }];
  return (
    <div>
      <div className="page-header"><h1>📊 Dashboard</h1><p>Developer productivity overview for the last 90 days</p></div>
      <div className="stats-grid">
        <div className="stat-card"><div className="stat-label">Total Commits</div><div className="stat-value">{data.totalCommits.toLocaleString()}</div><div className="stat-sub">Last 90 days</div></div>
        <div className="stat-card"><div className="stat-label">Developers</div><div className="stat-value">{data.totalDevelopers}</div><div className="stat-sub">Active contributors</div></div>
        <div className="stat-card"><div className="stat-label">Repositories</div><div className="stat-value">{data.totalRepositories}</div><div className="stat-sub">Tracked repos</div></div>
        <div className="stat-card"><div className="stat-label">AI Work</div><div className="stat-value">{data.aiWorkPercentage.toFixed(1)}%</div><div className="stat-sub">AI-related commits</div></div>
      </div>
      <div className="charts-grid">
        <div className="chart-card"><h3>Technology Distribution</h3>
          <ResponsiveContainer width="100%" height={300}><BarChart data={data.techDistribution.slice(0,10)}><CartesianGrid strokeDasharray="3 3" stroke="#334155" /><XAxis dataKey="technology" stroke="#64748b" tick={{ fontSize: 12 }} /><YAxis stroke="#64748b" /><Tooltip contentStyle={{ background: '#1e293b', border: '1px solid #334155', color: '#e2e8f0' }} /><Bar dataKey="commitCount" name="Commits" fill="#38bdf8" radius={[4,4,0,0]} /></BarChart></ResponsiveContainer>
        </div>
        <div className="chart-card"><h3>AI vs Regular Work</h3>
          <ResponsiveContainer width="100%" height={300}><PieChart><Pie data={aiPieData} cx="50%" cy="50%" outerRadius={100} dataKey="value" label>{aiPieData.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}</Pie><Tooltip formatter={(val: number) => `${val.toFixed(1)}%`} contentStyle={{ background: '#1e293b', border: '1px solid #334155', color: '#e2e8f0' }} /></PieChart></ResponsiveContainer>
        </div>
        <div className="chart-card" style={{ gridColumn: '1 / -1' }}><h3>Daily Activity</h3>
          <ResponsiveContainer width="100%" height={250}><LineChart data={data.dailyActivity}><CartesianGrid strokeDasharray="3 3" stroke="#334155" /><XAxis dataKey="date" stroke="#64748b" tick={{ fontSize: 11 }} /><YAxis stroke="#64748b" /><Tooltip contentStyle={{ background: '#1e293b', border: '1px solid #334155', color: '#e2e8f0' }} /><Legend /><Line type="monotone" dataKey="commitCount" name="Total Commits" stroke="#38bdf8" strokeWidth={2} dot={false} /><Line type="monotone" dataKey="aiCommitCount" name="AI Commits" stroke="#818cf8" strokeWidth={2} dot={false} /></LineChart></ResponsiveContainer>
        </div>
      </div>
      <div className="data-table"><h3>Top Developers</h3>
        <table><thead><tr><th>Developer</th><th>Commits</th><th>AI Work %</th><th>Top Technologies</th></tr></thead>
        <tbody>{data.topDevelopers.map(dev => (<tr key={dev.id}><td><Link to={`/developer/${dev.id}`} className="developer-link">{dev.displayName}</Link><div style={{ fontSize: '0.75rem', color: '#64748b' }}>{dev.email}</div></td><td>{dev.commitCount}</td><td><span className={`badge ${dev.aiWorkPercentage > 20 ? 'badge-green' : 'badge-gray'}`}>{dev.aiWorkPercentage.toFixed(1)}%</span></td><td><div className="tech-tags">{dev.topTechnologies.map(t => <span key={t} className="tech-tag">{t}</span>)}</div></td></tr>))}</tbody>
        </table>
      </div>
    </div>
  );
}
