import { useState, useEffect } from 'react';
import { repositoriesApi } from '../services/api';
import type { Repository } from '../services/api';

export default function Repositories() {
  const [repos, setRepos] = useState<Repository[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [syncing, setSyncing] = useState(false);
  useEffect(() => { repositoriesApi.getAll().then(r => setRepos(r.data)).catch(e => setError(e.message)).finally(() => setLoading(false)); }, []);
  const handleSync = async (repo: Repository) => { setSyncing(true); try { await repositoriesApi.triggerSync(repo.organization, repo.project, repo.repoName); alert('Sync triggered!'); } catch (e: unknown) { alert(`Failed: ${e instanceof Error ? e.message : 'Unknown error'}`); } finally { setSyncing(false); } };
  if (loading) return <div className="loading">Loading repositories...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  return (
    <div>
      <div className="page-header"><h1>📁 Repositories</h1><p>{repos.length} repositories tracked</p></div>
      <div className="data-table"><h3>Tracked Repositories</h3>
        <table><thead><tr><th>Repository</th><th>Organization</th><th>Project</th><th>Last Synced</th><th>Actions</th></tr></thead>
        <tbody>{repos.length === 0 ? <tr><td colSpan={5} style={{ textAlign: 'center', color: '#64748b', padding: '32px' }}>No repositories configured.</td></tr> : repos.map(r => (<tr key={r.id}><td><strong>{r.repoName}</strong></td><td>{r.organization}</td><td>{r.project}</td><td>{r.lastSyncedAt ? <span className="badge badge-green">{new Date(r.lastSyncedAt).toLocaleString()}</span> : <span className="badge badge-yellow">Never synced</span>}</td><td><button className="btn-small" onClick={() => handleSync(r)} disabled={syncing}>{syncing ? 'Syncing...' : 'Trigger Sync'}</button></td></tr>))}</tbody>
        </table>
      </div>
    </div>
  );
}
