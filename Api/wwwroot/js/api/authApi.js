import { fetchJson, refreshStoredAuthTokens, saveAuthTokens } from "./apiClient.js";

export async function login(email, password) {
    const tokens = await fetchJson("/api/Auth/sessions", {
        method: "POST",
        body: { email, password },
        skipAuth: true,
        errorText: "Не удалось войти",
    });

    saveAuthTokens(tokens);
    return tokens;
}

export async function refreshTokens() {
    return refreshStoredAuthTokens();
}
