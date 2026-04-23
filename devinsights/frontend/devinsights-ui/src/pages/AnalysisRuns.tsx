import { useState, useEffect } from 'react';
import { analysisApi, AnalysisRun } from '../services/api';

export default function AnalysisRuns() {
  const [runs, setRuns] = useState<AnalysisRun[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => { analysisApi.getRuns().then(r => setRuns(r.data)).catch(e => setError(e.message)).finally(() => setLoading(false)); }, []);
  const statusBadge = (s: string) => { const m: Record<string, string> = { Completed: 'badge-green', Failed: 'badge-red', Running: 'badge-blue' }; return <span className={`badge ${m[s] ?? 'badge-gray'}`}>{s}</span>; };
  if (loading) return <div className="loading">Loading analysis runs...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  return (
    <div>
      <div className="page-header"><h1>⚡ Analysis Runs</h1><p>History of commit analysis jobs</p></div>
      <div className="data-table"><h3>Recent Runs</h3>
        <table><thead><tr><th>Repository</th><th>Started</th><th>Completed</th><th>Status</th><th>Commits</th><th>Error</th></tr></thead>
        <tbody>{runs.length === 0 ? <tr><td colSpan={6} style={{ textAlign: 'center', color: '#64748b', padding: '32px' }}>No analysis runs yet.</td></tr> : runs.map(r => (<tr key={r.id}><td>{r.repositoryName}</td><td style={{ fontSize: '0.8rem' }}>{new Date(r.startedAt).toLocaleString()}</td><td style={{ fontSize: '0.8rem' }}>{r.completedAt ? new Date(r.completedAt).toLocaleString() : '—'}</td><td>{statusBadge(r.status)}</td><td>{r.commitsAnalyzed}</td><td style={{ color: '#f87171', fontSize: '0.8rem' }}>{r.errorMessage ?? '—'}</td></tr>))}</tbody>
        </table>
      </div>
    </div>
  );
}
