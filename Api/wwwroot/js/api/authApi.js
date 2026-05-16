import { fetchJson } from "./apiClient.js";

export async function login(email, password) {
    return fetchJson("/api/Auth/sessions", {
        method: "POST",
        body: { email, password },
        errorText: "Failed to create auth session",
    });
}

export async function refreshTokens() {
    return fetchJson("/api/Auth/tokens/refresh", {
        method: "POST",
        errorText: "Failed to refresh tokens",
    });
}
