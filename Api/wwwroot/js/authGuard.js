import { getStoredAccessToken } from "./api/apiClient.js";

const hasToken = Boolean(getStoredAccessToken());

if (!hasToken) {
  const current = `${window.location.pathname}${window.location.search}${window.location.hash}`;
  const authUrl = new URL("./auth.html", window.location.href);

  authUrl.searchParams.set("next", current);
  window.location.replace(authUrl);
}
