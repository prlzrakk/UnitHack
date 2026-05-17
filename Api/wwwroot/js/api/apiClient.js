export function buildUrl(path, params = new URLSearchParams()) {
    const query = params.toString();
    return `${path}${query ? `?${query}` : ""}`;
}

const ACCESS_TOKEN_KEYS = [
    "accessToken",
    "AccessToken",
    "boardify.accessToken",
    "jwt",
    "token",
];

const REFRESH_TOKEN_KEYS = [
    "refreshToken",
    "RefreshToken",
    "boardify.refreshToken",
];

export async function fetchJson(path, {
    method = "GET",
    params = new URLSearchParams(),
    body = null,
    errorText = "Request failed",
    authToken = null,
    skipAuth = false,
} = {}) {
    const url = buildUrl(path, params);
    console.log("API FETCH =>", method, url);

    const response = await fetch(url, {
        method,
        headers: {
            Accept: "application/json",
            ...getAuthHeader(authToken, skipAuth),
            ...(body ? { "Content-Type": "application/json" } : {}),
        },
        body: body ? JSON.stringify(body) : null,
    });

    if (!response.ok) {
        const error = new Error(await getResponseErrorText(response, errorText));
        error.status = response.status;
        throw error;
    }

    if (response.status === 204) {
        return null;
    }

    const text = await response.text();
    return text ? JSON.parse(text) : null;
}

async function getResponseErrorText(response, fallbackText) {
    const text = await response.text().catch(() => "");

    if (!text) {
        return `${fallbackText}: ${response.status}`;
    }

    try {
        const data = JSON.parse(text);
        const message = data?.message ?? data?.Message;
        return message ? `${fallbackText}: ${message}` : `${fallbackText}: ${response.status}`;
    } catch {
        return `${fallbackText}: ${response.status}`;
    }
}

export function saveAuthTokens(tokens) {
    const accessToken = tokens?.accessToken ?? tokens?.AccessToken;
    const refreshToken = tokens?.refreshToken ?? tokens?.RefreshToken;

    if (accessToken) {
        localStorage.setItem("accessToken", accessToken);
        localStorage.setItem("boardify.accessToken", accessToken);
    }

    if (refreshToken) {
        localStorage.setItem("refreshToken", refreshToken);
        localStorage.setItem("boardify.refreshToken", refreshToken);
    }
}

export function getStoredAccessToken() {
    return getStoredValue(ACCESS_TOKEN_KEYS);
}

export function getStoredRefreshToken() {
    return getStoredValue(REFRESH_TOKEN_KEYS);
}

function getAuthHeader(authToken, skipAuth) {
    if (skipAuth) {
        return {};
    }

    const token = authToken ?? getStoredAccessToken();
    return token ? { Authorization: `Bearer ${token}` } : {};
}

function getStoredValue(keys) {
    for (const key of keys) {
        const value = localStorage.getItem(key) ?? sessionStorage.getItem(key);
        if (value) {
            return value;
        }
    }

    return null;
}
