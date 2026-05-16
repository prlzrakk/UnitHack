import { fetchJson, getStoredRefreshToken, saveAuthTokens } from "./apiClient.js";

export async function login(email, password) {
    const tokens = await fetchJson("/api/Auth/sessions", {
        method: "POST",
        body: { email, password },
        skipAuth: true,
        errorText: "Failed to create auth session",
    });

    saveAuthTokens(tokens);
    return tokens;
}

export async function refreshTokens() {
    const tokens = await fetchJson("/api/Auth/tokens/refresh", {
        method: "POST",
        authToken: getStoredRefreshToken(),
        errorText: "Failed to refresh tokens",
    });

    saveAuthTokens(tokens);
    return tokens;
}
