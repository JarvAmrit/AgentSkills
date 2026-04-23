import axios from 'axios';
import { msalInstance } from '../main';
import { apiRequest } from '../config/msalConfig';

const apiClient = axios.create({ baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000' });

apiClient.interceptors.request.use(async (config) => {
  try {
    const accounts = msalInstance.getAllAccounts();
    if (accounts.length > 0) {
      const response = await msalInstance.acquireTokenSilent({ ...apiRequest, account: accounts[0] });
      config.headers.Authorization = `Bearer ${response.accessToken}`;
    }
  } catch { /* not authenticated */ }
  return config;
});

export interface DashboardSummary { totalCommits: number; totalDevelopers: number; totalRepositories: number; aiWorkPercentage: number; topDevelopers: DeveloperActivity[]; techDistribution: TechDistribution[]; dailyActivity: DailyActivity[]; }
export interface DeveloperActivity { id: number; displayName: string; email: string; commitCount: number; aiWorkPercentage: number; topTechnologies: string[]; }
export interface TechDistribution { technology: string; commitCount: number; }
export interface DailyActivity { date: string; commitCount: number; aiCommitCount: number; }
export interface Developer { id: number; displayName: string; email: string; createdAt: string; }
export interface CommitDetail { commitId: string; message: string; commitDate: string; repositoryName: string; technologies: string[]; isAIRelated: boolean; }
export interface TechCommit { technology: string; commitCount: number; }
export interface AIWork { aiWorkType: string; commitCount: number; avgConfidence: number; }
export interface Repository { id: number; organization: string; project: string; repoName: string; lastSyncedAt: string | null; createdAt: string; }
export interface AnalysisRun { id: number; repositoryName: string; startedAt: string; completedAt: string | null; status: string; commitsAnalyzed: number; errorMessage: string | null; }

export const dashboardApi = { getSummary: () => apiClient.get<DashboardSummary>('/api/dashboard/summary'), getDeveloperSummary: (id: number) => apiClient.get<DeveloperActivity>(`/api/dashboard/developer/${id}`) };
export const developersApi = { getAll: () => apiClient.get<Developer[]>('/api/developers'), getCommits: (id: number, days = 90) => apiClient.get<CommitDetail[]>(`/api/developers/${id}/commits?days=${days}`), getTechnologies: (id: number, days = 90) => apiClient.get<TechCommit[]>(`/api/developers/${id}/technologies?days=${days}`), getAIWork: (id: number, days = 90) => apiClient.get<AIWork[]>(`/api/developers/${id}/aiwork?days=${days}`) };
export const repositoriesApi = { getAll: () => apiClient.get<Repository[]>('/api/repositories'), triggerSync: (org: string, project: string, repoName: string) => apiClient.post('/api/repositories/sync', { organization: org, project, repoName }) };
export const analysisApi = { getRuns: () => apiClient.get<AnalysisRun[]>('/api/analysis/runs'), trigger: () => apiClient.post('/api/analysis/trigger') };
