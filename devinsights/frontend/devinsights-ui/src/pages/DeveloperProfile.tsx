import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { developersApi } from '../services/api';
import type { TechCommit, AIWork, CommitDetail } from '../services/api';

const COLORS = ['#38bdf8', '#818cf8', '#34d399', '#fbbf24', '#f87171', '#a78bfa'];

export default function DeveloperProfile() {
  const { id } = useParams<{ id: string }>();
  const devId = parseInt(id ?? '0');
  const [technologies, setTechnologies] = useState<TechCommit[]>([]);
  const [aiWork, setAIWork] = useState<AIWork[]>([]);
  const [commits, setCommits] = useState<CommitDetail[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => {
    Promise.all([developersApi.getTechnologies(devId), developersApi.getAIWork(devId), developersApi.getCommits(devId)])
      .then(([t, a, c]) => { setTechnologies(t.data); setAIWork(a.data); setCommits(c.data); })
      .catch(e => setError(e.message)).finally(() => setLoading(false));
  }, [devId]);
  if (loading) return <div className="loading">Loading developer profile...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  const aiPieData = [{ name: 'AI Work', value: commits.filter(c => c.isAIRelated).length }, { name: 'Regular', value: commits.filter(c => !c.isAIRelated).length }];
  return (
    <div>
      <div className="page-header"><Link to="/" style={{ color: '#94a3b8', textDecoration: 'none', fontSize: '0.875rem' }}>← Back</Link><h1 style={{ marginTop: 8 }}>👤 Developer Profile</h1><p>{commits.length} commits in the last 90 days</p></div>
      <div className="charts-grid">
        <div className="chart-card"><h3>Technology Breakdown</h3><ResponsiveContainer width="100%" height={280}><BarChart data={technologies.slice(0,10)} layout="vertical"><CartesianGrid strokeDasharray="3 3" stroke="#334155" /><XAxis type="number" stroke="#64748b" /><YAxis dataKey="technology" type="category" stroke="#64748b" width={80} tick={{ fontSize: 12 }} /><Tooltip contentStyle={{ background: '#1e293b', border: '1px solid #334155', color: '#e2e8f0' }} /><Bar dataKey="commitCount" name="Commits" fill="#38bdf8" radius={[0,4,4,0]} /></BarChart></ResponsiveContainer></div>
        <div className="chart-card"><h3>AI Work Distribution</h3><ResponsiveContainer width="100%" height={280}><PieChart><Pie data={aiPieData} cx="50%" cy="50%" outerRadius={100} dataKey="value" label>{aiPieData.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}</Pie><Tooltip contentStyle={{ background: '#1e293b', border: '1px solid #334155', color: '#e2e8f0' }} /></PieChart></ResponsiveContainer></div>
      </div>
      {aiWork.length > 0 && <div className="data-table" style={{ marginBottom: 24 }}><h3>AI Work Types</h3><table><thead><tr><th>AI Work Type</th><th>Commits</th><th>Avg Confidence</th></tr></thead><tbody>{aiWork.map(w => <tr key={w.aiWorkType}><td>{w.aiWorkType}</td><td>{w.commitCount}</td><td><span className="badge badge-green">{(w.avgConfidence * 100).toFixed(0)}%</span></td></tr>)}</tbody></table></div>}
      <div className="data-table"><h3>Recent Commits</h3><table><thead><tr><th>Date</th><th>Message</th><th>Repository</th><th>Technologies</th><th>AI</th></tr></thead><tbody>{commits.slice(0,50).map(c => (<tr key={c.commitId}><td style={{ whiteSpace: 'nowrap', fontSize: '0.8rem' }}>{new Date(c.commitDate).toLocaleDateString()}</td><td style={{ maxWidth: 300, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{c.message}</td><td>{c.repositoryName}</td><td><div className="tech-tags">{c.technologies.slice(0,3).map(t => <span key={t} className="tech-tag">{t}</span>)}</div></td><td>{c.isAIRelated && <span className="badge badge-green">AI</span>}</td></tr>))}</tbody></table></div>
    </div>
  );
}
