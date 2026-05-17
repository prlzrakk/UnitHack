import { fetchJson } from "./apiClient.js";

export async function createUser(email, password, name = null) {
    return fetchJson("/api/users", {
        method: "POST",
        body: { email, password, name },
        errorText: "Failed to create user",
    });
}

export async function getMe() {
    return fetchJson("/api/users/me", {
        errorText: "Failed to fetch current user",
    });
}

export async function searchUsers(query, limit = 10) {
    return fetchJson("/api/users", {
        params: new URLSearchParams({
            query,
            limit: String(limit),
        }),
        errorText: "Failed to search users",
    });
}
