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

const REFRESH_PATH = "/api/Auth/tokens/refresh";
let refreshRequest = null;

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

    let response = await sendJsonRequest(url, { method, body, authToken, skipAuth });

    if (response.status === 401 && shouldRefreshAuth(path, { authToken, skipAuth })) {
        await refreshStoredAuthTokens();
        response = await sendJsonRequest(url, { method, body, authToken, skipAuth });
    }

    return readJsonResponse(response, errorText);
}

function sendJsonRequest(url, {
    method = "GET",
    body = null,
    authToken = null,
    skipAuth = false,
} = {}) {
    return fetch(url, {
        method,
        headers: {
            Accept: "application/json",
            ...getAuthHeader(authToken, skipAuth),
            ...(body ? { "Content-Type": "application/json" } : {}),
        },
        body: body ? JSON.stringify(body) : null,
    });
}

async function readJsonResponse(response, errorText) {
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

function shouldRefreshAuth(path, { authToken = null, skipAuth = false } = {}) {
    return !skipAuth &&
        !authToken &&
        path !== REFRESH_PATH &&
        Boolean(getStoredRefreshToken());
}

export async function refreshStoredAuthTokens() {
    if (refreshRequest) {
        return refreshRequest;
    }

    const refreshToken = getStoredRefreshToken();

    if (!refreshToken) {
        clearAuthTokens();
        throw new Error("Refresh token is missing");
    }

    refreshRequest = (async () => {
        const response = await fetch(REFRESH_PATH, {
            method: "POST",
            headers: {
                Accept: "application/json",
                Authorization: `Bearer ${refreshToken}`,
            },
        });

        if (!response.ok) {
            clearAuthTokens();
            const error = new Error(await getResponseErrorText(response, "Failed to refresh session"));
            error.status = response.status;
            throw error;
        }

        const tokens = await readJsonResponse(response, "Failed to refresh session");
        saveAuthTokens(tokens);
        return tokens;
    })().finally(() => {
        refreshRequest = null;
    });

    return refreshRequest;
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
        setStoredValue(ACCESS_TOKEN_KEYS, accessToken);
    }

    if (refreshToken) {
        setStoredValue(REFRESH_TOKEN_KEYS, refreshToken);
    }
}

export function clearAuthTokens() {
    [...ACCESS_TOKEN_KEYS, ...REFRESH_TOKEN_KEYS].forEach((key) => {
        localStorage.removeItem(key);
        sessionStorage.removeItem(key);
    });
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

function setStoredValue(keys, value) {
    keys.forEach((key) => {
        localStorage.setItem(key, value);
    });
}
