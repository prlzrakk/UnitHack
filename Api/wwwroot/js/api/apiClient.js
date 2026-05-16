export function buildUrl(path, params = new URLSearchParams()) {
    const query = params.toString();
    return `${path}${query ? `?${query}` : ""}`;
}

export async function fetchJson(path, {
    method = "GET",
    params = new URLSearchParams(),
    body = null,
    errorText = "Request failed",
} = {}) {
    const url = buildUrl(path, params);
    console.log("API FETCH =>", method, url);

    const response = await fetch(url, {
        method,
        headers: {
            Accept: "application/json",
            ...(body ? { "Content-Type": "application/json" } : {}),
        },
        body: body ? JSON.stringify(body) : null,
    });

    if (!response.ok) {
        throw new Error(`${errorText}: ${response.status}`);
    }

    if (response.status === 204) {
        return null;
    }

    const text = await response.text();
    return text ? JSON.parse(text) : null;
}
