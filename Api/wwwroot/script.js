const tasks=[
 {label:"Важно / Не срочно",items:[["Задача абобная","2 часа","2 часа"],["Задача абобная","2 часа","2 часа"]]},
 {label:"Важно / Срочно",items:[["Задача абобная","2 часа","2 часа"],["Задача абобная","2 часа","2 часа"]]},
 {label:"Не важно / Не срочно",items:[["Задача абобная","2 часа","2 часа"],["Задача абобная","2 часа","2 часа"]]},
 {label:"Не важно / Срочно",items:[["Задача абобная","2 часа","2 часа"],["Задача абобная","2 часа","2 часа"]]}
];

const matrix=document.querySelector("#taskMatrix");

function row([title,complexity,deadline]){
  const el=document.createElement("article");
  el.className="task-row";
  el.innerHTML=`
    <div class="task-title">${title}</div>
    <div class="divider"></div>
    <div class="task-meta"><span>сложность</span><span>${complexity}</span></div>
    <div class="divider"></div>
    <div class="task-meta"><span>Дедлайн через</span><span>${deadline}</span></div>
  `;
  return el;
}

tasks.forEach(group=>{
  const q=document.createElement("section");
  q.className="quadrant";
  q.dataset.label=group.label;
  group.items.forEach(item=>q.appendChild(row(item)));
  matrix.appendChild(q);
});


const sidebar = document.getElementById("sidebar");
const overlay = document.getElementById("sidebarOverlay");
const openBtn = document.getElementById("openSidebar");
const closeBtn = document.getElementById("closeSidebar");

function openSidebar() {
    sidebar.classList.add("is-open");
    overlay.classList.add("is-open");
    document.body.style.overflow = "hidden";
}

function closeSidebar() {
    sidebar.classList.remove("is-open");
    overlay.classList.remove("is-open");
    document.body.style.overflow = "";
}

openBtn.addEventListener("click", openSidebar);
closeBtn.addEventListener("click", closeSidebar);
overlay.addEventListener("click", closeSidebar);

document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
        closeSidebar();
    }
});
const collapseButtons = document.querySelectorAll("[data-collapse]");

collapseButtons.forEach((button) => {
    button.addEventListener("click", () => {
        const section = button.closest(".sidebar-section");
        section.classList.toggle("is-collapsed");
    });
});