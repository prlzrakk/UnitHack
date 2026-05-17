import { fetchJson } from "./apiClient.js";

export async function getExample() {
    return fetchJson("/api/Example/get_example", {
        errorText: "Failed to fetch example",
    });
}
