const TUTORIAL_STORAGE_KEY = "boardify.tutorialSeen";
const TUTORIAL_CSS_ID = "boardifyTutorialStyles";
const TUTORIAL_CSS_HREF = "./css/tutorial.css";

export function showFirstLoginTutorial() {
    if (wasTutorialSeen() || document.querySelector("[data-tutorial-overlay]")) {
        return;
    }

    ensureTutorialStyles();

    const overlay = document.createElement("div");
    overlay.className = "tutorial-overlay";
    overlay.dataset.tutorialOverlay = "true";
    overlay.innerHTML = createTutorialMarkup();

    document.body.appendChild(overlay);
    document.body.classList.add("tutorial-locked");

    const close = () => closeTutorial(overlay);

    overlay.querySelector("[data-tutorial-close]")?.addEventListener("click", close);
    overlay.querySelector("[data-tutorial-done]")?.addEventListener("click", close);
    overlay.addEventListener("click", (event) => {
        if (event.target === overlay) {
            close();
        }
    });

    const onKeyDown = (event) => {
        if (event.key === "Escape") {
            close();
        }
    };

    document.addEventListener("keydown", onKeyDown);
    overlay.cleanupTutorial = () => {
        document.removeEventListener("keydown", onKeyDown);
    };

    requestAnimationFrame(() => {
        overlay.classList.add("is-open");
        overlay.querySelector("[data-tutorial-done]")?.focus();
    });
}

function createTutorialMarkup() {
    return `
        <section class="tutorial-card" role="dialog" aria-modal="true" aria-labelledby="tutorialTitle">
            <button class="tutorial-close" data-tutorial-close type="button" aria-label="Закрыть">
                <span></span>
                <span></span>
            </button>

            <p class="tutorial-eyebrow">Быстрый старт</p>
            <h2 id="tutorialTitle">С чего начать?</h2>

            <ol class="tutorial-steps">
                <li>
                    <span class="tutorial-step-index">1</span>
                    <span>Откройте меню: нажмите на три полоски слева сверху.</span>
                </li>
                <li>
                    <span class="tutorial-step-index">2</span>
                    <span>Перейдите в «Команды» и нажмите «Добавить»: задайте название, цвет и пригласите участников по имени или email.</span>
                </li>
                <li>
                    <span class="tutorial-step-index">3</span>
                    <span>Откройте «Проекты» и создайте первый проект для своей команды.</span>
                </li>
                <li>
                    <span class="tutorial-step-index">4</span>
                    <span>Внутри проекта добавьте kanban-доску.</span>
                </li>
                <li>
                    <span class="tutorial-step-index">5</span>
                    <span>Готово: как админ команды вы можете создавать колонки, задачи и подзадачи.</span>
                </li>
            </ol>

            <button class="tutorial-done" data-tutorial-done type="button">
                Понятно, начнем
            </button>
        </section>
    `;
}

function closeTutorial(overlay) {
    markTutorialSeen();
    overlay.classList.remove("is-open");
    document.body.classList.remove("tutorial-locked");
    overlay.cleanupTutorial?.();

    setTimeout(() => {
        overlay.remove();
    }, 220);
}

function ensureTutorialStyles() {
    if (document.getElementById(TUTORIAL_CSS_ID)) {
        return;
    }

    const link = document.createElement("link");
    link.id = TUTORIAL_CSS_ID;
    link.rel = "stylesheet";
    link.href = TUTORIAL_CSS_HREF;
    document.head.appendChild(link);
}

function wasTutorialSeen() {
    try {
        return localStorage.getItem(TUTORIAL_STORAGE_KEY) === "true";
    } catch {
        return false;
    }
}

function markTutorialSeen() {
    try {
        localStorage.setItem(TUTORIAL_STORAGE_KEY, "true");
    } catch {
        // Ignore storage errors: closing the dialog should still work.
    }
}
