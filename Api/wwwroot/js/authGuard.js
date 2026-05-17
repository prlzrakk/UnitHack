import {
  getStoredAccessToken,
  getStoredRefreshToken,
  refreshStoredAuthTokens,
} from "./api/apiClient.js";

const hasAccessToken = Boolean(getStoredAccessToken());
const hasRefreshToken = Boolean(getStoredRefreshToken());

if (!hasAccessToken && hasRefreshToken) {
  try {
    await refreshStoredAuthTokens();
  } catch (error) {
    console.error("Failed to refresh auth session", error);
    redirectToAuth();
  }
} else if (!hasAccessToken) {
  redirectToAuth();
}

function redirectToAuth() {
  const current = `${window.location.pathname}${window.location.search}${window.location.hash}`;
  const authUrl = new URL("./auth.html", window.location.href);

  authUrl.searchParams.set("next", current);
  window.location.replace(authUrl);
}
