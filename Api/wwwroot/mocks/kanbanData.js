const PROJECTS = {
    hackathon: {
        title: "Kanban",
        name: "ХАХАТОН",
        color: "#ef6a35",
        meta: "active",

        teams: [
            { name: "5 кать", color: "#4668ad" },
            { name: "абоба", color: "#ef6a35" },
            { name: "какуля", color: "#ffe3d8" },
            { name: "пупуня", color: "#e6a0a6" },
        ],

        columns: [
            {
                title: "Todo",
                color: "#ef6a35",
                tasks: [
                    {
                        title: "Сверстать карточку",
                        description: "сложность: 2 часа\nприоритет: 1 // какое-то описание",
                        priority: "HIGH",
                        time: "2 часа",
                        users: 2,
                    },
                    {
                        title: "API задач",
                        description: "очередь / валидация",
                        priority: "MID",
                        time: "1 час",
                        color: "#42609f",
                        users: 3,
                    },
                ],
            },
            {
                title: "In progress",
                color: "#ef6a35",
                tasks: [
                    {
                        title: "попукать",
                        description: "сложность: 2 часа\nприоритет: 1 // какое-то описание",
                        priority: "HIGH",
                        time: "2 часа",
                        users: 2,
                    },
                    {
                        title: "намазать мясо масло",
                        description: "очередь / валидация",
                        priority: "MID",
                        time: "1 час",
                        color: "#42609f",
                        users: 3,
                    },
                ],
            },
            {
                title: "Done",
                color: "#407d52",
                done: true,
                tasks: [
                    {
                        title: "Поахуевать",
                        description: "сложность: 2 часа\nприоритет: 1 // какое-то описание",
                        priority: "HIGH",
                        time: "2 часа",
                        users: 2,
                    },
                    {
                        title: "кикикикиик",
                        description: "очередь / валидация",
                        priority: "MID",
                        time: "1 час",
                        users: 3,
                    },
                ],
            },
        ],
    },

    vibik: {
        title: "Vibik board",
        name: "Vibik",
        color: "#4668ad",
        meta: "4 участника",

        teams: [
            { name: "Frontend", color: "#4668ad" },
            { name: "Backend", color: "#ef6a35" },
            { name: "QA", color: "#e6a0a6" },
        ],

        columns: [
            {
                title: "Backlog",
                color: "#4668ad",
                tasks: [
                    {
                        title: "Сделать авторизацию",
                        description: "JWT, роли, базовая защита роутов",
                        priority: "MID",
                        time: "3 часа",
                        users: 2,
                    },
                ],
            },
            {
                title: "Review",
                color: "#ef6a35",
                tasks: [
                    {
                        title: "Починить API",
                        description: "валидация ответа и обработка ошибок",
                        priority: "HIGH",
                        time: "1 час",
                        users: 1,
                    },
                ],
            },
            {
                title: "Done",
                color: "#407d52",
                done: true,
                tasks: [
                    {
                        title: "Собрать layout",
                        description: "страница проекта готова",
                        priority: "LOW",
                        time: "30 мин",
                        users: 2,
                    },
                ],
            },
        ],
    },

    blueprint: {
        title: "Blueprint board",
        name: "Blueprint",
        color: "#ffe3d8",
        meta: "5 участников",

        teams: [
            { name: "Design", color: "#ffe3d8" },
            { name: "QA", color: "#e6a0a6" },
        ],

        columns: [
            {
                title: "Ideas",
                color: "#ef6a35",
                tasks: [
                    {
                        title: "Собрать дизайн-систему",
                        description: "цвета, кнопки, карточки, состояния",
                        priority: "HIGH",
                        time: "4 часа",
                        users: 3,
                    },
                ],
            },
            {
                title: "Testing",
                color: "#42609f",
                tasks: [
                    {
                        title: "Проверить адаптив",
                        description: "mobile, tablet, desktop",
                        priority: "MID",
                        time: "2 часа",
                        users: 2,
                    },
                ],
            },
            {
                title: "Done",
                color: "#407d52",
                done: true,
                tasks: [
                    {
                        title: "Подготовить палитру",
                        description: "основные цвета утверждены",
                        priority: "LOW",
                        time: "1 час",
                        users: 1,
                    },
                ],
            },
        ],
    },
};