import { login } from "./api/authApi.js";
import { createUser } from "./api/userApi.js";

const body = document.body;

const panels = {
  start: document.getElementById("startPanel"),
  login: document.getElementById("loginPanel"),
  register: document.getElementById("registerPanel"),
};

const message = document.getElementById("authMessage");
const showLoginBtn = document.getElementById("showLogin");
const showRegisterBtn = document.getElementById("showRegister");
const loginToRegisterBtn = document.getElementById("loginToRegister");
const registerToLoginBtn = document.getElementById("registerToLogin");
const loginForm = document.getElementById("loginForm");
const registerForm = document.getElementById("registerForm");

showLoginBtn?.addEventListener("click", () => setMode("login"));
showRegisterBtn?.addEventListener("click", () => setMode("register"));
loginToRegisterBtn?.addEventListener("click", () => setMode("register"));
registerToLoginBtn?.addEventListener("click", () => setMode("login"));

loginForm?.addEventListener("submit", async (event) => {
  event.preventDefault();

  const email = loginForm.email.value.trim();
  const password = loginForm.password.value;

  if (!email || !password) {
    setMessage("Заполни email и пароль");
    return;
  }

  await runAuthAction(loginForm, async () => {
    await login(email, password);
    setMessage("Готово, открываем Boardify...", "success");
    redirectToApp();
  }, "Не удалось войти. Проверь email и пароль.");
});

registerForm?.addEventListener("submit", async (event) => {
  event.preventDefault();

  const email = registerForm.email.value.trim();
  const name = registerForm.name.value.trim();
  const password = registerForm.password.value;

  if (!email || !name || !password) {
    setMessage("Заполни email, имя и пароль");
    return;
  }

  if (password.length < 6) {
    setMessage("Пароль должен быть не короче 6 символов");
    return;
  }

  await runAuthAction(registerForm, async () => {
    await createUser(email, password, name);
    await login(email, password);
    setMessage("Аккаунт создан, открываем Boardify...", "success");
    redirectToApp();
  }, "Не удалось зарегистрироваться. Возможно, email уже занят.");
});

function setMode(mode, options = {}) {
  const nextMode = ["start", "login", "register"].includes(mode) ? mode : "start";

  Object.entries(panels).forEach(([panelName, panel]) => {
    if (!panel) {
      return;
    }

    panel.hidden = panelName !== nextMode;
  });

  body.classList.remove("auth-mode-start", "auth-mode-login", "auth-mode-register");
  body.classList.add(`auth-mode-${nextMode}`);

  if (!options.keepMessage) {
    setMessage("");
  }

  const url = new URL(window.location.href);

  if (nextMode === "start") {
    url.searchParams.delete("mode");
  } else {
    url.searchParams.set("mode", nextMode);
  }

  window.history.replaceState({}, "", url);

  const firstInput = panels[nextMode]?.querySelector("input");

  if (firstInput) {
    setTimeout(() => firstInput.focus(), 0);
  }
}

async function runAuthAction(form, action, fallbackMessage) {
  setFormDisabled(form, true);
  setMessage("");

  try {
    await action();
  } catch (error) {
    console.error(error);
    setMessage(error?.message || fallbackMessage);
  } finally {
    setFormDisabled(form, false);
  }
}

function setFormDisabled(form, isDisabled) {
  form.querySelectorAll("button, input").forEach((element) => {
    element.disabled = isDisabled;
  });
}

function setMessage(text, variant = "error") {
  message.textContent = text;
  message.classList.toggle("is-success", variant === "success");
}

function redirectToApp() {
  const params = new URLSearchParams(window.location.search);
  const next = params.get("next") || "./index.html";

  setTimeout(() => {
    window.location.replace(next);
  }, 250);
}

const params = new URLSearchParams(window.location.search);
setMode(params.get("mode") || "start", { keepMessage: true });
