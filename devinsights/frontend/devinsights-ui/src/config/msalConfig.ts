import { Configuration, PopupRequest } from '@azure/msal-browser';

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_AD_CLIENT_ID || 'YOUR_SPA_CLIENT_ID',
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_AD_TENANT_ID || 'common'}`,
    redirectUri: import.meta.env.VITE_REDIRECT_URI || 'http://localhost:5173',
  },
  cache: { cacheLocation: 'sessionStorage', storeAuthStateInCookie: false },
};
export const loginRequest: PopupRequest = { scopes: ['openid', 'profile', 'User.Read'] };
export const apiRequest = { scopes: [`api://${import.meta.env.VITE_AZURE_AD_CLIENT_ID || 'YOUR_SPA_CLIENT_ID'}/access_as_user`] };
