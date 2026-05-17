import { getMe } from "./api/userApi.js";

const TOKEN_KEYS = [
  "accessToken",
  "AccessToken",
  "boardify.accessToken",
  "jwt",
  "token",
  "refreshToken",
  "RefreshToken",
  "boardify.refreshToken",
];

const emailInput = document.getElementById("profileEmail");
const nameInput = document.getElementById("profileName");
const idInput = document.getElementById("profileId");
const message = document.getElementById("userMessage");
const logoutButton = document.getElementById("logoutButton");

logoutButton?.addEventListener("click", logout);

loadProfile();

async function loadProfile() {
  setMessage("Загружаем профиль...", "success");

  try {
    const user = await getMe();
    renderUser(user);
    setMessage("");
  } catch (error) {
    console.error(error);
    setMessage("Не удалось загрузить профиль. Войди заново.");
  }
}

function renderUser(user = {}) {
  const email = readValue(user, "email", "Email") || "Email не указан";
  const name =
      readValue(user, "name", "Name") ||
      readValue(user, "userName", "UserName") ||
      "Имя не указано";
  const id = readValue(user, "id", "Id") || "ID не найден";

  emailInput.value = email;
  nameInput.value = name;
  idInput.value = id;
}

function logout() {
  TOKEN_KEYS.forEach((key) => {
    localStorage.removeItem(key);
    sessionStorage.removeItem(key);
  });

  const authUrl = new URL("./auth.html", window.location.href);
  authUrl.searchParams.set("mode", "login");
  window.location.replace(authUrl);
}

function setMessage(text, variant = "error") {
  message.textContent = text;
  message.classList.toggle("is-success", variant === "success");
}

function readValue(source, camelKey, pascalKey) {
  return source?.[camelKey] ?? source?.[pascalKey] ?? "";
}